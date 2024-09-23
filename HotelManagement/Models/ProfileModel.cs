using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Models;

public class ProfileModel
{
    public string Id { get; set; }
    [Required]
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = "";

    [Required, DataType(DataType.PhoneNumber)]
    public string ContactNumber { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string? Designation { get; set; }
}