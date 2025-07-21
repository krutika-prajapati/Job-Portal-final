using Microsoft.AspNetCore.Http;
using Repository.Enum;
using Repository.Interface;
using Repository.Model;
using Service.DTO;
using Service.Interface;
using Shared.DTO;

namespace Service.Services
{
    public class JobService:
        IJobService
    {
        private readonly IJobRepo jobRepo;
        private readonly IHttpContextAccessor httpContextAccessor;

        public JobService(IJobRepo jobRepo, IHttpContextAccessor httpContextAccessor)
        {
            this.jobRepo = jobRepo;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<BaseResponseDTO> CreateJob(JobDTO jobDTO)
        {
            var response = new BaseResponseDTO();
            try
            {
                if (string.IsNullOrEmpty(jobDTO.Title) ||
                   string.IsNullOrEmpty(jobDTO.Description) ||
                   string.IsNullOrEmpty(jobDTO.JobType))
                {
                    response.IsSuccess = false;
                    response.Message = "All fields are required.";
                    response.StatusCode = 400; // Bad Request
                    return response;
                }

                var job = new Job
                {
                    Title = jobDTO.Title,
                    Description = jobDTO.Description,
                    Location = jobDTO.Location,
                    JobType = Enum.Parse<JobType>(jobDTO.JobType, ignoreCase: true),
                    SalaryRange = jobDTO.SalaryRange,
                    PostedDate = DateTime.UtcNow,
                    CompanyId = jobDTO.CompanyId,
                    Active = true
                };

                if (await jobRepo.CreateJob(job))
                {
                    response.IsSuccess = true;
                    response.Message = "Job created successfully.";
                    response.StatusCode = 201; // Created
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Failed to create job.";
                    response.StatusCode = 500; // Internal Server Error

                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Error creating user: {ex.Message}";
                response.StatusCode = 500; // Internal Server Error
            }
            return response;
        }

        public async Task<BaseResponseDTO> GetJobsCreatedByUser()
        {
            var response = new BaseResponseDTO();
            try
            {
                string UserId;
                var userIdClaim = httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id");

                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    var jobs = await jobRepo.GetJobsCreateByUser(userId);
                    if (jobs != null && jobs.Any())
                    {
                        response.IsSuccess = true;
                        response.Message = "Jobs retrieved successfully.";
                        response.StatusCode = 200; // OK
                        response.Data = jobs;
                    }
                    else
                    {
                        response.IsSuccess = false;
                        response.Message = "No Jobs found.";
                        response.StatusCode = 404; // Not Found
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
                response.Message = $"Error retrieving Jobs: {ex.Message}";
                response.StatusCode = 500; // Internal Server Error
            }
            return response;
        }

        public async Task<BaseResponseDTO> GetAllJobs()
        {
            var response = new BaseResponseDTO();
            try
            {
                    var jobs = await jobRepo.GetAllJobs();
                    if (jobs != null && jobs.Any())
                    {
                        response.IsSuccess = true;
                        response.Message = "Jobs retrieved successfully.";
                        response.StatusCode = 200; // OK
                        response.Data = jobs;
                    }
                    else
                    {
                        response.IsSuccess = false;
                        response.Message = "No Jobs found.";
                        response.StatusCode = 404; // Not Found
                    }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Error retrieving Jobs: {ex.Message}";
                response.StatusCode = 500; // Internal Server Error
            }
            return response;
        }

        public async Task<BaseResponseDTO> DeleteJob(int jobId)
        {
            var response = new BaseResponseDTO();
            try
            {
                var userIdClaim = httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id");

                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    var result = await jobRepo.DeleteJobById(jobId, userId);
                    if (result)
                    {
                        response.IsSuccess = true;
                        response.Message = "Job deleted successfully.";
                        response.StatusCode = 200; // OK
                    }
                    else
                    {
                        response.IsSuccess = false;
                        response.Message = "Failed to delete the job";
                        response.StatusCode = 404; // Not Found
                    }
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Invalid or missing user ID.";

                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Error deleting Job: {ex.Message}";
                response.StatusCode = 500; // Internal Server Error
            }
            return response;
        }

        public async Task<BaseResponseDTO> updateJob(UpdateJob updatedJob)
        {
            var response = new BaseResponseDTO();
            try
            {
                var userIdClaim = httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id");

                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    var result = await jobRepo.UpdateJob(userId,updatedJob);
                    if (result)
                    {
                        response.IsSuccess = true;
                        response.Message = "Job updated successfully.";
                        response.StatusCode = 200; // OK
                    }
                    else
                    {
                        response.IsSuccess = false;
                        response.Message = "Failed to update the job";
                        response.StatusCode = 404; // Not Found
                    }
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Invalid or missing user ID.";

                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Error deleting Job: {ex.Message}";
                response.StatusCode = 500; // Internal Server Error
            }
            return response;
        }
    }
}
