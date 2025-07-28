using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRental.Models;

[Table("refresh_tokens")]
public class RefreshToken
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }
    
    [Column("token")]
    public string Token { get; set; }
    
    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
    
    [Column("revoked_at")]
    public DateTime? RevokedAt { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }
    public User User { get; set; }   // navigation property
}