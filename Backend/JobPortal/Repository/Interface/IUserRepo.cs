using Repository.Model;
using Shared.DTO;

namespace Repository.Interface
{
    public interface IUserRepo
    {

       Task<bool> CreateUser(Model.User user);
       Task<bool> UserAlreadyExists(string email);
       Task<int?> GetRoleIdByName(string roleName);
       Task<User> GetUserByEmail(string email);
       Task<bool> CompanyAlreadyAddedByUser(string email);
       Task<string> getUserRoleByEmail(string email);
       Task<UserWithRole> getUserWithRole(int userId);
       Task<List<UserWithRole>> GetAllUsersWithRole();
       Task<bool> DeleteUser(int userId);
       Task<bool> UpdateUserProfile(UpdateUserProfile updatedUser);
    }
}
