using Microsoft.EntityFrameworkCore;
using Repository.Enum;
using Repository.Interface;
using Repository.Model;
using Shared.DTO;

namespace Repository.Repository
{
    public class UserRepo : IUserRepo
    {

        private readonly DBContext context;

        public UserRepo(DBContext context)
        {
            this.context = context;
        }

        public async Task<bool> CreateUser(User user)
        {
            try
            {
                using (context)
                {
                    await context.Users.AddAsync(user);
                    await context.SaveChangesAsync();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UserAlreadyExists(string email)
        {
            try
            {
                using (context)
                {
                    if (await context.Users.AnyAsync(user => user.Email == email))
                    {
                        return true; // User with this email already exists
                    }
                    else
                    {
                        return false; // User with this email does not exist
                    }
                }
            }
            catch (Exception)
            {
                return false; // An error occurred while checking for the user
            }
        }

        public async Task<int?> GetRoleIdByName(string roleName)
        {
            var userRole = await context.UserRoles
                .FirstOrDefaultAsync(r => r.Name.ToLower() == roleName.ToLower());
            return userRole?.RoleId;
        }

        public async Task<User> GetUserByEmail(string email)
        {
            try
            {
                using (context)
                {
                    return await context.Users.FirstOrDefaultAsync(user => user.Email == email && user.Active);
                }
            }
            catch (Exception)
            {
                return null; // An error occurred while retrieving the user
            }
        }

        public async Task<bool> CompanyAlreadyAddedByUser(string email)
        {
            try
            {
                // Step 1: Get the user with role
                var userWithRole = await (
                    from user in context.Users
                    join role in context.UserRoles on user.RoleId equals role.RoleId
                    where user.Email == email && user.Active
                    select new
                    {
                        UserId = user.UserId,
                        RoleName = role.Name
                    }
                ).FirstOrDefaultAsync();

                if (userWithRole == null)
                    return false; // No active user found with the email

                // Step 2: If user is admin, allow multiple companies
                if (userWithRole.RoleName == Role.Admin.ToString())
                    return false;

                // Step 3: For non-admins, check if a company is already added
                return await context.Companies
                    .AnyAsync(company => company.UserId == userWithRole.UserId && company.Active);
            }
            catch (Exception)
            {
                return false; // Error occurred, treat as not added
            }
        }


        public async Task<string> getUserRoleByEmail(string email)
        {
            try
            {
                var roleName = await (
                    from user in context.Users
                    join role in context.UserRoles
                    on user.RoleId equals role.RoleId
                    where user.Email == email
                    select role.Name
                ).FirstOrDefaultAsync();

                return roleName ?? string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public async Task<UserWithRole> getUserWithRole(int userId)
        {
            try
            {
                return await (
                    from user in context.Users
                    join role in context.UserRoles on user.RoleId equals role.RoleId
                    where user.UserId == userId && user.Active
                    select new UserWithRole
                    {
                        Id=user.UserId,
                        FullName = user.FullName,
                        Email = user.Email,
                        Role = role.Name,
                        Phone = user.Phone,
                    }
                ).FirstOrDefaultAsync();
            }
            catch (Exception)
            {
                return null; // An error occurred while retrieving the user with role
            }
        }

        public async Task<List<UserWithRole>> GetAllUsersWithRole()
        {
            try
            {
                return await (
                    from user in context.Users
                    join role in context.UserRoles
                    on user.RoleId equals role.RoleId
                    where user.Active
                    select new UserWithRole
                    {
                        Id = user.UserId,
                        FullName = user.FullName,
                        Email = user.Email,
                        Role = role.Name,
                        Phone = user.Phone
                    }
                ).ToListAsync();
            }
            catch (Exception ex)
            {

                return new List<UserWithRole>(); // or throw if you prefer to let caller handle it
            }
        }

        public async Task<bool> DeleteUser(int userId)
        {
            try
            {
                using (context)
                {
                    var user = await context.Users.FindAsync(userId);

                    if (user == null)
                    {
                        return false; // Company not found
                    }

                    user.Active = false; // Soft delete
                    context.Users.Update(user);
                    await context.SaveChangesAsync();
                    return true;
                }
            }
            catch (Exception)
            {
                return false; // An error occurred while deleting the user
            }
        }

        public async Task<bool> UpdateUserProfile(UpdateUserProfile updatedUser)
        {
            try
            {
                var user = await context.Users.FindAsync(updatedUser.Id);
                if (user == null)
                {
                    return false; // User not found
                }

                // Update the properties
                user.FullName = updatedUser.FullName;
                user.Email = updatedUser.Email;
                user.Phone = updatedUser.Phone;

                context.Users.Update(user);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false; // An error occurred while updating the user profile
            }
        }
    }
}
