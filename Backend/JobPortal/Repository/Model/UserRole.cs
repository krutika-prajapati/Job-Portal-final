using System.ComponentModel.DataAnnotations;

namespace Repository.Model
{
    public class UserRole
    {
        [Key]
        public int RoleId { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
