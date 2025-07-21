using Service.DTO;
using Shared.DTO;

namespace Service.Interface
{
    public interface ICompanyService
    {
        Task<BaseResponseDTO> CreateCompany(CompanyDTO companyDTO);
        Task<BaseResponseDTO> DeleteCompany(int companyId);
        Task<BaseResponseDTO> GetAllCompanyWithUser();
        Task<BaseResponseDTO> GetCompanyCreatedByUser();
        Task<BaseResponseDTO> UpdateCompany(UpdateCompany companyDTO);
    }
}
