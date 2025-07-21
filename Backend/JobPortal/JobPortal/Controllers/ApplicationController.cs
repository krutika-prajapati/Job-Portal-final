using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.DTO;
using Service.Interface;
using Shared.DTO;

namespace JobPortal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationController : ControllerBase
    {
        private readonly IApplicationService applicationService;

        public ApplicationController(IApplicationService applicationService)
        {
            this.applicationService = applicationService;
        }

        [HttpPost("[action]")]
        [Authorize(Roles = "JobSeeker")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateApplicationRequest([FromForm] CreateApplicationRequestDTO createApplication)
        {
            var response = await applicationService.CreateNewApplication(createApplication);
            return Ok(response);
        }

        [HttpGet("[action]")]
        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> GetApplicationsByUserId()
        {
            var response = await applicationService.GetApplicationsByUserId();
            return Ok(response);
        }

        [HttpDelete("[action]")]
        [Authorize(Roles = "Admin,JobSeeker")]
        public async Task<IActionResult> DeleteApplication([FromBody]int applicationId)
        {
            var response = await applicationService.DeleteApplicationById(applicationId);
            return Ok(response);
        }

        [HttpGet("[action]/{applicationId}")]
        public async Task<IActionResult> DownloadResume(int applicationId)
        {

            var (filePath, contentType) = await applicationService.GetResumeFilePath(applicationId);

            if (filePath == null)
                return NotFound("Resume not found.");

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var fileName = Path.GetFileName(filePath);

            return File(fileBytes, contentType ?? "application/octet-stream", fileName);
        }

        [HttpPut("[action]")]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> UpdateApplicationStatus([FromBody] UpdateApplicationStatus applicationStatus)
        {
            var response = await applicationService.UpdateApplicationStatus(applicationStatus);
            return Ok(response);
        }
    }
}
