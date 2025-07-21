using Microsoft.AspNetCore.Http;
using Repository.Enum;
using Repository.Interface;
using Repository.Model;
using Service.DTO;
using Service.Interface;
using Shared.DTO;

namespace Service.Services
{
    public class CompanyService:ICompanyService
    {
        private readonly IUserRepo userRepo;
        private readonly ICompanyRepo companyRepo;
        private readonly IHttpContextAccessor httpContextAccessor;

        public CompanyService(IUserRepo userRepo, ICompanyRepo companyRepo, IHttpContextAccessor httpContextAccessor)
        {
            this.userRepo = userRepo;
            this.companyRepo = companyRepo;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<BaseResponseDTO> CreateCompany(CompanyDTO companyDTO)
        {
            var response = new BaseResponseDTO();
            try
            {
                string UserEmail = null;
                var userEmailClaim = httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "email");

                if (userEmailClaim != null)
                {
                    UserEmail = userEmailClaim.Value;
                }

                if (string.IsNullOrEmpty(companyDTO.CompanyName) ||
                   string.IsNullOrEmpty(companyDTO.Description))
                {
                    response.IsSuccess = false;
                    response.Message = "All fields are required.";
                    response.StatusCode = 400; // Bad Request
                    return response;
                }

                if (await companyRepo.CompanyAlreadyExists(companyDTO.CompanyName))
                {
                    response.IsSuccess = false;
                    response.Message = "Company with this name already exists.";
                    response.StatusCode = 409; // Conflict
                    return response;
                }

                if(await userRepo.CompanyAlreadyAddedByUser(UserEmail))
                {
                    response.IsSuccess = false;
                    response.Message = "You have already added a company.";
                    response.StatusCode = 409; // Conflict
                    return response;
                }

                var company = new Company
                {
                    Name = companyDTO.CompanyName,
                    Description = companyDTO.Description,
                    Website = companyDTO.Website,
                    UserId = (await userRepo.GetUserByEmail(UserEmail)).UserId,
                    Active = true,
                    CreatedAt = DateTime.UtcNow
                };

                if (await companyRepo.CreateCompany(company))
                {
                    response.IsSuccess = true;
                    response.Message = "Company created successfully.";
                    response.StatusCode = 201; // Created
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Failed to create company.";
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

        public async Task<BaseResponseDTO> DeleteCompany(int companyId)
        {
            var response = new BaseResponseDTO();
            try
            {
                string UserEmail = null;
                var userEmailClaim = httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "email");

                if (userEmailClaim != null)
                {
                    UserEmail = userEmailClaim.Value;
                }
                var User = (await userRepo.GetUserByEmail(UserEmail));
                var UserId = User.UserId;
                var UserRole = await userRepo.getUserRoleByEmail(User.Email);
                var isAdmin = UserRole == Role.Admin.ToString();

                if (await companyRepo.DeleteCompany(UserId, companyId , isAdmin))
                {
                    response.StatusCode = 200; // OK
                    response.IsSuccess = true;
                    response.Message = "Company deleted successfully.";
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Failed to delete company or you do not have permission to delete this company.";
                    response.StatusCode = 403; // Forbidden
                }
                return response;
            }
            catch (Exception ex)
            {
                return new BaseResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Error deleting company: {ex.Message}",
                    StatusCode = 500 // Internal Server Error
                };

            }
        }

        public async Task<BaseResponseDTO> GetAllCompanyWithUser()
        {
            var response = new BaseResponseDTO();
            try
            {
                var companies = await companyRepo.GetAllCompanyWithUserDetails();
                if (companies != null && companies.Any())
                {
                    response.IsSuccess = true;
                    response.Message = "Companies retrieved successfully.";
                    response.StatusCode = 200; // OK
                    response.Data = companies;
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "No companies found.";
                    response.StatusCode = 404; // Not Found
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Error retrieving companies: {ex.Message}";
                response.StatusCode = 500; // Internal Server Error
            }
            return response;
        }

        public async Task<BaseResponseDTO> GetCompanyCreatedByUser()
        {
            var response = new BaseResponseDTO();
            try
            {
                var userIdClaim = httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id");

                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    var companies = await companyRepo.GetCompanyCreatedByUser(userId);
                    if (companies != null && companies.Any())
                    {
                        response.IsSuccess = true;
                        response.Message = "Companies retrieved successfully.";
                        response.StatusCode = 200; // OK
                        response.Data = companies;
                    }
                    else
                    {
                        response.IsSuccess = false;
                        response.Message = "No companies found.";
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
                response.Message = $"Error retrieving companies: {ex.Message}";
                response.StatusCode = 500; // Internal Server Error
            }
            return response;
        }

        public async Task<BaseResponseDTO> UpdateCompany(UpdateCompany companyDTO)
        {
            var response = new BaseResponseDTO();
            try
            {
                string UserEmail;
                var userEmailClaim = httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "email");

                if (userEmailClaim != null)
                {
                    UserEmail = userEmailClaim.Value;
                    var User = (await userRepo.GetUserByEmail(UserEmail));
                    var UserId = User.UserId;
                    var UserRole = await userRepo.getUserRoleByEmail(User.Email);
                    var isAdmin = UserRole == Role.Admin.ToString();

                    var result = await companyRepo.UpdateCompany(UserId, companyDTO, isAdmin);

                    if (result)
                    {
                        response.IsSuccess = true;
                        response.Message = "Company updated successfully.";
                        response.StatusCode = 200; // OK
                    }
                    else
                    {
                        response.IsSuccess = false;
                        response.Message = "Failed to update company or you do not have permission to update this company.";
                        response.StatusCode = 403; // Forbidden
                    }
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Invalid or missing user email.";
                    response.StatusCode = 400; // Bad Request
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Error retrieving companies: {ex.Message}";
                response.StatusCode = 500; // Internal Server Error
            }
            return response;
        }
    }
}
