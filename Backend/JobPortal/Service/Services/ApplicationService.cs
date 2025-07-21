using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Repository.Enum;
using Repository.Interface;
using Service.DTO;
using Service.Interface;
using Shared.DTO;
using System.Security.Claims;

namespace Service.Services
{
    public class ApplicationService:IApplicationService
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IApplicationRepo applicationRepo;
        private readonly IWebHostEnvironment environment;
        private readonly IEmailService emailService;

        public ApplicationService(IHttpContextAccessor httpContextAccessor,IApplicationRepo applicationRepo, IWebHostEnvironment environment,IEmailService emailService)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.applicationRepo = applicationRepo;
            this.environment = environment;
            this.emailService = emailService;
        }

        public async Task<BaseResponseDTO> CreateNewApplication(CreateApplicationRequestDTO createApplicationRequest)
        {
            var response = new BaseResponseDTO();
            try
            {
                var userIdClaim = httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id");

                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    var alreadyExists = await applicationRepo.ApplicationExists(userId, createApplicationRequest.JobId);

                    if (alreadyExists)
                    {
                        response.IsSuccess = false;
                        response.Message = "You have already applied for this job.";
                        response.StatusCode = 409; // Conflict
                        return response;
                    }

                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "resumes");

                    // Create directory if not exists
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Save file with unique name
                    string uniqueFileName = $"{Guid.NewGuid()}_{createApplicationRequest.ResumeFile.FileName}_{userId}";

                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await createApplicationRequest.ResumeFile.CopyToAsync(stream);
                    }

                    // Create URL path (to be returned or stored in DB)
                    string resumeUrl = $"/uploads/resumes/{uniqueFileName}";

                    // Create application object
                    var application = new Repository.Model.Application
                    {
                        JobId = createApplicationRequest.JobId,
                        UserId = userId,
                        ResumeUrl = resumeUrl,
                        AppliedDate = DateTime.UtcNow,
                        Status = Repository.Enum.ApplicationStatus.Pending,
                        Active = true
                    };

                    // 4. Call repository to save application
                    var success = await applicationRepo.CreateApplication(application);

                    if (success)
                    {
                        response.IsSuccess = true;
                        response.Message = "Application created successfully.";
                        response.StatusCode = 201; // Created
                    }
                    else
                    {
                        response.IsSuccess = false;
                        response.Message = "Failed to save application.";
                        response.StatusCode = 500;
                    }
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Invalid or missing user ID.";
                    response.StatusCode = 400; // Bad Request
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Error creating application: {ex.Message}";
                response.StatusCode = 500; // Internal Server Error
            }
            return response;

        }


        public async Task<BaseResponseDTO> GetApplicationsByUserId()
        {
            var response = new BaseResponseDTO();
            try
            {
                var userIdClaim = httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id");

                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    var result = await applicationRepo.GetApplicationsByUserId(userId);

                    if (result.Count>0)
                    {
                        response.IsSuccess = true;
                        response.Message = "Applications fetched successfully.";
                        response.StatusCode = 200;
                        response.Data = result;
                    }
                    else
                    {
                        response.IsSuccess = false;
                        response.Message = "No applications found.";
                        response.StatusCode = 404;
                    }
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Invalid or missing user ID.";
                    response.StatusCode = 400; // Bad Request
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Error fetching application: {ex.Message}";
                response.StatusCode = 500; // Internal Server Error
            }
            return response;

        }

        public async Task<BaseResponseDTO> DeleteApplicationById(int applicationId)
        {
            var response = new BaseResponseDTO();
            try
            {
                var user = httpContextAccessor.HttpContext?.User;

                var userIdClaim = user?.Claims.FirstOrDefault(c => c.Type == "id");
                var roleClaim = user?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);

                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    response.IsSuccess = false;
                    response.Message = "Unauthorized or invalid user.";
                    response.StatusCode = 401;
                    return response;
                }

                bool isAdmin = roleClaim?.Value == Role.Admin.ToString();

                var result = await applicationRepo.DeleteApplicationById(applicationId, userId, isAdmin);

                if (result)
                {
                    response.IsSuccess = true;
                    response.Message = "Application deleted successfully.";
                    response.StatusCode = 200;
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Application not found or already deleted.";
                    response.StatusCode = 404;
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Error deleting application: {ex.Message}";
                response.StatusCode = 500;
            }

            return response;
        }

        public async Task<(string? filePath, string? contentType)> GetResumeFilePath(int applicationId)
        {
            try
            {
                var userIdClaim = httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id");

                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    var fileName = await applicationRepo.GetResumeUrlByApplicationId(userId, applicationId);
                    if (string.IsNullOrEmpty(fileName))
                        return (null, null);

                    var resumePath = Path.Combine(environment.WebRootPath, fileName.TrimStart('/'));

                    if (!File.Exists(resumePath))
                        return (null, null);

                    var contentType = "application/pdf"; // optionally use a MIME library

                    return (resumePath, contentType);

                }
                else
                {
                    return (null, null);
                }
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                return (null, null);
            }
        }
        public async Task<BaseResponseDTO> UpdateApplicationStatus(UpdateApplicationStatus updateApplicationStatus)
        {
            var response = new BaseResponseDTO();
            try
            {
                var userIdClaim = httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id");
             
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    if (await applicationRepo.UpdateApplicationStatus(updateApplicationStatus,userId))
                    {
                        response.IsSuccess = true;
                        response.StatusCode = 200;
                        response.Message = "Application status updated successfuly.";

                        if (updateApplicationStatus.Status.Equals(ApplicationStatus.Accepted.ToString(), StringComparison.OrdinalIgnoreCase) ||
                            updateApplicationStatus.Status.Equals(ApplicationStatus.Rejected.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            var userEmail = await applicationRepo.GetUserEmailByApplicationId(updateApplicationStatus.ApplicationId);
                            var jobDetails = await applicationRepo.GetJobAndCompanyNameByApplicationId(updateApplicationStatus.ApplicationId);
                            string jobTitle = jobDetails?.JobTitle ?? "Job";
                            string companyName = jobDetails?.CompanyName ?? "Company";
                            if (!string.IsNullOrEmpty(userEmail))
                            {
                                string subject = "Application Status Update";
                                string body = $@"
                                                <!DOCTYPE html>
                                                <html>
                                                <head>
                                                    <style>
                                                        body {{
                                                            font-family: 'Segoe UI', sans-serif;
                                                            background-color: #f4f4f4;
                                                            padding: 20px;
                                                        }}
                                                        .container {{
                                                            background-color: #fff;
                                                            padding: 25px;
                                                            border-radius: 6px;
                                                            box-shadow: 0 2px 6px rgba(0,0,0,0.1);
                                                            max-width: 550px;
                                                            margin: auto;
                                                        }}
                                                        h2 {{
                                                            color: #0056b3;
                                                        }}
                                                        .status {{
                                                            color: {(updateApplicationStatus.Status == "Accepted" ? "#28a745" : "#dc3545")};
                                                            font-weight: bold;
                                                        }}
                                                    </style>
                                                </head>
                                                <body>
                                                    <div class='container'>
                                                        <h2>Application Status Updated</h2>
                                                        <p>Hello,</p>
                                                        <p>Your application for the position <strong>{jobTitle}</strong> at <strong>{companyName}</strong> has been updated.</p>
                                                        <p>New Status: <span class='status'>{updateApplicationStatus.Status}</span></p>
                                                        <p>Thank you for using our job portal.</p>
                                                    </div>
                                                </body>
                                                </html>";
                                await emailService.SendEmail(userEmail, subject, body);
                            }
                        }
                    }
                    else
                    {
                        response.IsSuccess = false;
                        response.StatusCode = 500;
                        response.Message = "Failed to update application status.";
                    }
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Unauthorized or invalid user.";
                    response.StatusCode = 401;
                    return response;
                }
            }
            catch (Exception)
            {
                response.IsSuccess = false;
                response.StatusCode = 500;
                response.Message = "Internal server error.";

            }
            return response;
        }
    }
}
