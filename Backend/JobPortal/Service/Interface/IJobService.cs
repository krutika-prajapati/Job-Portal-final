using Service.DTO;
using Shared.DTO;

namespace Service.Interface
{
    public interface IJobService
    {
        Task<BaseResponseDTO> CreateJob(JobDTO jobDTO);
        Task<BaseResponseDTO> GetJobsCreatedByUser();
        Task<BaseResponseDTO> GetAllJobs();
        Task<BaseResponseDTO> DeleteJob(int jobId);
        Task<BaseResponseDTO> updateJob(UpdateJob updatedJob);
    }
}
