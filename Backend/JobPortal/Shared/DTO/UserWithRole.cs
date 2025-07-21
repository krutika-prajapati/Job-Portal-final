namespace Shared.DTO
{
    public class UserWithRole
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string? Phone { get; set; }
    }
}
