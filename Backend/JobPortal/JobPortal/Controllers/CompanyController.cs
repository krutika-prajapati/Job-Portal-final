using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.DTO;
using Service.Interface;
using Shared.DTO;

namespace JobPortal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService companyService;

        public CompanyController(ICompanyService companyService)
        {
            this.companyService = companyService;
        }
        
        [HttpPost("[action]")]
        [Authorize(Roles = "Employer,Admin")]
        public async Task<IActionResult> CreateCompany([FromBody] CompanyDTO companyDTO)
        {
            var response = await companyService.CreateCompany(companyDTO);
            return Ok(response);
        }

        [HttpDelete("[action]")]
        [Authorize(Roles = "Employer,Admin")]
        public async Task<IActionResult> DeleteCompany([FromBody] int companyId)
        {
            var response = await companyService.DeleteCompany(companyId);
            return Ok(response);

        }

        [HttpGet("[action]")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllCompanyDetails()
        {
            var response = await companyService.GetAllCompanyWithUser();
            return Ok(response);

        }

        [HttpGet("[action]")]
        [Authorize(Roles = "Employer,Admin")]
        public async Task<IActionResult> GetCompanyCreatedByUser()
        {
            var response = await companyService.GetCompanyCreatedByUser();
            return Ok(response);

        }

        [HttpPut("[action]")]
        [Authorize(Roles = "Employer,Admin")]
        public async Task<IActionResult> UpdateCompany([FromBody] UpdateCompany companyDTO)
        {
            var response = await companyService.UpdateCompany(companyDTO);
            return Ok(response);
        }
    }
}
