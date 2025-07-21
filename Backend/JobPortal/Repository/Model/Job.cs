using Repository.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Repository.Model
{
    public class Job
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int JobId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [MaxLength(100)]
        public string? Location { get; set; }

        [Required]
        public JobType JobType { get; set; }

        [MaxLength(50)]
        public string? SalaryRange { get; set; }

        [Required]
        public DateTime PostedDate { get; set; } = DateTime.UtcNow;

        [Required]
        public int CompanyId { get; set; } // FK

        public bool Active { get; set; } = true;

        [ForeignKey("CompanyId")]
        public Company Company { get; set; }
    }
}
