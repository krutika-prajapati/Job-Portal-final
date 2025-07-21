using Repository.Model;
using Shared.DTO;

namespace Repository.Interface
{
    public interface IJobRepo
    {
        Task<bool> CreateJob(Job job);
        Task<List<JobCretedByUser>> GetJobsCreateByUser(int userId);
        Task<List<JobCretedByUser>> GetAllJobs();
        Task<bool> DeleteJobById(int jobId, int userId);
        Task<bool> UpdateJob(int userId, UpdateJob updatedJob);
    }
}
