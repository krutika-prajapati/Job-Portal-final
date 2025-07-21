using Service.DTO;
using Shared.DTO;

namespace Service.Interface
{
    public interface IApplicationService
    {
        Task<BaseResponseDTO> CreateNewApplication(CreateApplicationRequestDTO createApplicationRequest);
        Task<BaseResponseDTO> GetApplicationsByUserId();
        Task<BaseResponseDTO> DeleteApplicationById(int applicationId);
        Task<(string? filePath, string? contentType)> GetResumeFilePath(int applicationId);
        Task<BaseResponseDTO> UpdateApplicationStatus(UpdateApplicationStatus updateApplicationStatus);
    }
}
