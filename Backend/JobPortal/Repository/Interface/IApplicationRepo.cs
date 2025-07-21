using Repository.Model;
using Shared.DTO;

namespace Repository.Interface
{
    public interface IApplicationRepo
    {
        Task<bool> CreateApplication(Application application);
        Task<bool> ApplicationExists(int userId, int jobId);
        Task<List<ApplicationDetails>> GetApplicationsByUserId(int userId);
        Task<bool> DeleteApplicationById(int applicationId, int? userId = null, bool isAdmin = false);
        Task<string?> GetResumeUrlByApplicationId(int userId, int applicationId);
        Task<bool> UpdateApplicationStatus(UpdateApplicationStatus updateApplicationStatus,int userId);
        Task<string?> GetUserEmailByApplicationId(int applicationId);
        Task<(string JobTitle, string CompanyName)?> GetJobAndCompanyNameByApplicationId(int applicationId);
    }
}
