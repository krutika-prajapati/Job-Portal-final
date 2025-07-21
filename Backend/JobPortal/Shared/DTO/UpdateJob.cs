namespace Shared.DTO
{
    public class UpdateJob
    {
        public int JobId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string? Location { get; set; }
        public string JobType { get; set; }
        public string? SalaryRange { get; set; }
    }
}
