using Service.DTO;
using Shared.DTO;

namespace Service.Interface
{
    public interface IAuthService
    {
        Task<BaseResponseDTO> CreateUser(UserDTO userDTO);
        Task<BaseResponseDTO> Login(LoginDTO loginDTO);
    }
}
