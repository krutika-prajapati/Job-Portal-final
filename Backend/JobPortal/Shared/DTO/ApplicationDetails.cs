namespace Shared.DTO
{
    public class ApplicationDetails
    {
        public int ApplicationId { get; set; }
        public DateOnly AppliedDate { get; set; } 
        public string ApplicationStatus { get; set; }
        public string JobTitle { get; set; }
        public string JobDescription { get; set; }
        public string? JobLocation { get; set; }
        public string JobType { get; set; }
        public string? SalaryRange { get; set; }
        public string CompanyName { get; set; }
        public string CompanyDescription { get; set; }
        public string? CompanyWebsite { get; set; }
        public DateOnly PostedDate { get; set; }
    }
}
