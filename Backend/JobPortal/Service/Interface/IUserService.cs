using Service.DTO;
using Shared.DTO;

namespace Service.Interface
{
    public interface IUserService
    {
        Task<BaseResponseDTO> GetUserDetails();
        Task<BaseResponseDTO> GetAllUserDetails();
        Task<BaseResponseDTO> DeleteUser(int userId);
        Task<BaseResponseDTO> UpdateUserProfile(UpdateUserProfile userProfile);
    }
}
