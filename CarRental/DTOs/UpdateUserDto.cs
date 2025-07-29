namespace CarRental.DTOs;

public class UpdateUserDto
{
    public string? Name { get; set; }
    public string? Email { get; set; } 
    public string? Phone { get; set; }
    public Guid? CompanyId { get; set; }
    public int? RoleId { get; set; }  
}