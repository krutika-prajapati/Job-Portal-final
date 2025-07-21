using Microsoft.AspNetCore.Http;
using Repository.Enum;
using Repository.Interface;
using Repository.Model;
using Repository.Repository;
using Service.DTO;
using Service.Interface;
using Shared.DTO;

namespace Service.Services
{
    public class UserService:IUserService
    {
        private readonly IUserRepo userRepo;
        private readonly IHttpContextAccessor httpContextAccessor;

        public UserService(IUserRepo userRepo, IHttpContextAccessor httpContextAccessor)
        {
            this.userRepo = userRepo;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<BaseResponseDTO> GetUserDetails()
        {
            try
            {
                var userIdClaim = httpContextAccessor.HttpContext?.User?.Claims
                    .FirstOrDefault(x => x.Type == "id");

                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return new BaseResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Invalid or missing user ID claim.",
                        StatusCode = 401 // Unauthorized
                    };
                }

                var user = await userRepo.getUserWithRole(userId);

                if (user == null)
                {
                    return new BaseResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User not found.",
                        StatusCode = 404 // Not Found
                    };
                }

                return new BaseResponseDTO
                {
                    IsSuccess = true,
                    Message = "User details retrieved successfully.",
                    StatusCode = 200,
                    Data = user
                };
            }
            catch (Exception ex)
            {
                // Log exception if logging is configured
                return new BaseResponseDTO
                {
                    IsSuccess = false,
                    Message = "An error occurred while retrieving user details.",
                    StatusCode = 500,
                };
            }
        }

        public async Task<BaseResponseDTO> GetAllUserDetails()
        {
            var response = new BaseResponseDTO();
            try
            {
                var users = await userRepo.GetAllUsersWithRole();

                if (users.Count==0)
                {
                    return new BaseResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Users not available.",
                        StatusCode = 404 // Not Found
                    };
                }

                return new BaseResponseDTO
                {
                    IsSuccess = true,
                    Message = "Users details retrieved successfully.",
                    StatusCode = 200,
                    Data = users
                };
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Error creating user: {ex.Message}";
                response.StatusCode = 500; // Internal Server Error
            }
            return response;
        }

        public async Task<BaseResponseDTO> DeleteUser(int userId)
        {
            var response = new BaseResponseDTO();
            try
            {
                if (await userRepo.DeleteUser(userId))
                {
                    response.IsSuccess = true;
                    response.Message = "User deleted successfully.";
                    response.StatusCode = 200; // OK
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Failed to delete user.";
                    response.StatusCode = 404; // Not Found
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Error deleting user: {ex.Message}";
                response.StatusCode = 500; // Internal Server Error
            }
            return response;
        }

        public async Task<BaseResponseDTO> UpdateUserProfile(UpdateUserProfile userProfile)
        {
            var response = new BaseResponseDTO();
            try
            {
                var userEmailClaim = httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "email");

                if (userEmailClaim != null)
                {
                    string userEmail = userEmailClaim.Value;
                    var user = await userRepo.GetUserByEmail(userEmail);

                    if (user == null)
                    {
                        response.IsSuccess = false;
                        response.Message = "User not found.";
                        response.StatusCode = 404;
                        return response;
                    }

                    var userId = user.UserId;
                    var userRole = await userRepo.getUserRoleByEmail(user.Email);
                    var isAdmin = userRole == Role.Admin.ToString();

                    if (userId != userProfile.Id && !isAdmin)
                    {
                        response.IsSuccess = false;
                        response.Message = "You don't have permission to update this user profile.";
                        response.StatusCode = 403;
                        return response;
                    }

                    var result = await userRepo.UpdateUserProfile(userProfile);

                    if (result)
                    {
                        response.IsSuccess = true;
                        response.Message = "User profile updated successfully.";
                        response.StatusCode = 200;
                    }
                    else
                    {
                        response.IsSuccess = false;
                        response.Message = "Failed to update user profile.";
                        response.StatusCode = 400;
                    }
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Invalid or missing user email.";
                    response.StatusCode = 400;
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Error updating user profile: {ex.Message}";
                response.StatusCode = 500;
            }

            return response;
        }

    }
}
