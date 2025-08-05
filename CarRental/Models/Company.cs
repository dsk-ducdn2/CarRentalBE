using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRental.Models;

[Table("companies")]
public class Company
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }            

    [Column("name")]
    public string Name { get; set; }        

    [Column("address")]
    public string? Address { get; set; }    

    [Column("phone")]
    public string? Phone { get; set; }      

    [Column("email")]
    public string? Email { get; set; }      

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }   

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; } 
    
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}