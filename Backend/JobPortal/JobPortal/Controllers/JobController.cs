using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.DTO;
using Service.Interface;
using Shared.DTO;

namespace JobPortal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobController : ControllerBase
    {
        public readonly IJobService jobService;
        public JobController(IJobService jobService)
        {
            this.jobService = jobService;
        }

        [HttpPost("[action]")]
        [Authorize(Roles = "Admin,Employer")]
        public async Task<IActionResult> CreateJob([FromBody] JobDTO jobDTO)
        {
            var response = await jobService.CreateJob(jobDTO);
            return Ok(response);
        }

        [HttpGet("[action]")]
        [Authorize(Roles = "Admin,Employer")]
        public async Task<IActionResult> GetJobsCreatedByUser()
        {
            var response = await jobService.GetJobsCreatedByUser();
            return Ok(response);
        }

        [HttpGet("[action]")]
        [Authorize]
        public async Task<IActionResult> GetAllJobs()
        {
            var response = await jobService.GetAllJobs();
            return Ok(response);
        }

        [HttpDelete("[action]")]
        [Authorize(Roles = "Admin,Employer")]
        public async Task<IActionResult> DeleteJob([FromBody]int jobId)
        {
            var response = await jobService.DeleteJob(jobId);
            return Ok(response);
        }

        [HttpPut("[action]")]
        [Authorize(Roles = "Admin,Employer")]
        public async Task<IActionResult> UpdateJob([FromBody] UpdateJob job)
        {
            var response = await jobService.updateJob(job);
            return Ok(response);
        }
    }
}

