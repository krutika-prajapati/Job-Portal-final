namespace Shared.DTO
{
    public class CompanyWithUser
    {
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public string Description { get; set; }
        public string? Website { get; set; }
        public string EmployerEmail { get; set; }
        public string EmployerName { get; set; }
        public string? EmployerPhone { get; set; }
    }
}
