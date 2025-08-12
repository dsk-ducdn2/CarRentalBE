using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CarRental.Models
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("email")]
        [MaxLength(255)] 
        public string Email { get; set; } = string.Empty;

        [Column("password_hash")]
        public string PasswordHash { get; set; } = string.Empty;

        [Column("name")]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Column("phone")]
        [MaxLength(50)]
        public string? Phone { get; set; } = string.Empty;
        
        [Column("status")]
        public string? Status { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
        
        [Column("role_id")]
        public int RoleId { get; set; }
        
        public Role Role { get; set; }   
        
        [Column("company_id")]
        public Guid? CompanyId { get; set; }
        
        public Company Company { get; set; }   
        
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        
        public ICollection<VehicleStatusLogs> VehicleStatusLogs { get; set; } = new List<VehicleStatusLogs>();
        
        public ICollection<MaintenanceLog> MaintenanceLogs { get; set; } = new List<MaintenanceLog>();
        
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
