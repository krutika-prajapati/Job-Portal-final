using Repository.Model;
using Shared.DTO;

namespace Repository.Interface
{
    public interface ICompanyRepo
    {
        Task<bool> CreateCompany(Company company);
        Task<bool> CompanyAlreadyExists(string companyName);
        Task<bool> DeleteCompany(int userId,int companyId,bool isAdmin);
        Task<List<CompanyWithUser>> GetAllCompanyWithUserDetails();
        Task<List<CompanyWithUser>> GetCompanyCreatedByUser(int userId);
        Task<bool> UpdateCompany(int userId, UpdateCompany updatedCompany, bool isAdmin);
    }
}
