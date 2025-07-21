using Microsoft.AspNetCore.Http;

namespace Service.DTO
{
    public class CreateApplicationRequestDTO
    {
        public int JobId { get; set; }
        public IFormFile ResumeFile { get; set; }
    }
}
