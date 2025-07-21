namespace Service.DTO
{
    public class JobDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string? Location { get; set; }
        public string JobType { get; set; }
        public string? SalaryRange { get; set; }
        public int CompanyId { get; set; }

    }
}
