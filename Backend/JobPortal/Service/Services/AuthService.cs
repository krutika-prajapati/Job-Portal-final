using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Repository.Interface;
using Repository.Model;
using Service.DTO;
using Service.Interface;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace Service.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepo userRepo;
        private readonly IPasswordHasher<string> passwordHasher;
        private readonly IConfiguration configuration;

        public AuthService(IUserRepo userRepo, IPasswordHasher<string> passwordHasher, IConfiguration configuration)
        {
            this.userRepo = userRepo;
            this.passwordHasher = passwordHasher;
            this.configuration = configuration;
        }

        #region create user
        public async Task<BaseResponseDTO> CreateUser(UserDTO userDTO)
        {
            var response = new BaseResponseDTO();
            try
            {
                if (string.IsNullOrEmpty(userDTO.FullName) ||
                   string.IsNullOrEmpty(userDTO.Email) ||
                   string.IsNullOrEmpty(userDTO.Password) ||
                   string.IsNullOrEmpty(userDTO.Role))
                {
                    response.IsSuccess = false;
                    response.Message = "All fields are required.";
                    response.StatusCode = 400; // Bad Request
                    return response;
                }

                if (!IsValidEmail(userDTO.Email))
                {
                    response.IsSuccess = false;
                    response.Message = "Invalid email format.";
                    response.StatusCode = 400; // Bad Request
                    return response;
                }

                if (await userRepo.UserAlreadyExists(userDTO.Email))
                {
                    response.IsSuccess = false;
                    response.Message = "User with this email already exists.";
                    response.StatusCode = 409; // Conflict
                    return response;
                }

                // Hash the password
                string hashedPassword = passwordHasher.HashPassword("" , userDTO.Password);

                var roleId = await userRepo.GetRoleIdByName(userDTO.Role);

                if (roleId == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Invalid role specified.";
                    response.StatusCode = 400;
                    return response;
                }

                var user = new User
                {
                    FullName = userDTO.FullName,
                    Email = userDTO.Email,
                    Password = hashedPassword,
                    Phone = userDTO.Phone,
                    RoleId = roleId.Value,
                    Active = true,
                    CreatedAt = DateTime.UtcNow
                };

                if (await userRepo.CreateUser(user))
                {
                    response.IsSuccess = true;
                    response.Message = "User created successfully.";
                    response.StatusCode = 201; // Created
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Failed to create user.";
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

        #endregion

        #region Login

        public async Task<BaseResponseDTO> Login(LoginDTO loginDTO)
        {
            var response = new BaseResponseDTO();
            try
            {
                if (string.IsNullOrEmpty(loginDTO.Email) || string.IsNullOrEmpty(loginDTO.Password))
                {
                    response.IsSuccess = false;
                    response.Message = "Email and password are required.";
                    response.StatusCode = 400; // Bad Request
                    return response;
                }

                if (!IsValidEmail(loginDTO.Email))
                {
                    response.IsSuccess = false;
                    response.Message = "Invalid email format.";
                    response.StatusCode = 400; // Bad Request
                    return response;
                }

                var user = await userRepo.GetUserByEmail(loginDTO.Email);
                if (user == null)
                {
                    response.IsSuccess = false;
                    response.Message = "User not found.";
                    response.StatusCode = 404; // Not Found
                    return response;
                }
                var passwordVerificationResult = passwordHasher.VerifyHashedPassword("", user.Password, loginDTO.Password);
                if (passwordVerificationResult == PasswordVerificationResult.Failed)
                {
                    response.IsSuccess = false;
                    response.Message = "Wrong Credentials";
                    response.StatusCode = 401; // Unauthorized
                    return response;
                }

                // Password is correct, generate JWT token
                var token = await GenerateJwtToken(user);

                // Successful login
                response.IsSuccess = true;
                response.Message = "Login successful.";
                response.StatusCode = 200; // OK
                response.Data = token;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Error during login: {ex.Message}";
                response.StatusCode = 500; // Internal Server Error
            }
            return response;
        }

        #region Create Token
        private async Task<string> GenerateJwtToken(User user)
        {
            var userRole = await userRepo.getUserRoleByEmail(user.Email);
            //List of Claims
            List<Claim> claims = new List<Claim>
            {
                new Claim("email", user.Email),
                new Claim("id", user.UserId.ToString()),
                new Claim(ClaimTypes.Role, userRole)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetSection("Appsettings:Token").Value));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
        #endregion

        #endregion

        #region Email format validation
        private bool IsValidEmail(string email)
        {
            string pattern = @"^[^\s@]+@[^\s@]+\.[^\s@]+$";
            return Regex.IsMatch(email, pattern);
        }
        #endregion
    }
}
