namespace Shared.DTO
{
    public class UpdateCompany
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string Description { get; set; }
        public string? Website { get; set; }
    }
}
