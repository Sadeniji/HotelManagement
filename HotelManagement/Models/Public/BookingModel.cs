using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using HotelManagement.Constants;
using HotelManagement.Extensions;

namespace HotelManagement.Models.Public;

public class BookingModel
{
    [Required]
    public string FirstName { get; set; } = string.Empty;

    public string? LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = "";

    [Required, DataType(DataType.PhoneNumber)]
    public string ContactNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = "";

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = "";

    [Required(ErrorMessage = "Check in date is required.")]
    public DateOnly CheckInDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required(ErrorMessage = "Check out date is required.")]
    [DateOnlyGreaterThan("CheckInDate", ErrorMessage = "Check out date must be after Check in date.")]
    public DateOnly CheckOutDate { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

    public int? NumberOfAdults { get; set; } = 0;

    public int? NumberOfChildren { get; set; } = 0;

    [MaxLength(255)]
    public string? SpecialRequest { get; set; }

    public decimal Amount { get; set; }
    public Ulid RoomTypeId { get; set; }

    public void SetDummyValues()
    {
        // Don't worry we will ignore it, (we will not save it in the database, having it just to bypass the DataAnnotations validation
        Email = "blazing@blazing.com";
        ContactNumber = "123456789";

        FirstName = "Blazing";
        LastName = "Hotel";
        Password = ConfirmPassword = "P@ss1234";
    }
    public void SetValuesFromClaimsPrincipal(ClaimsPrincipal principal)
    {
        if (principal?.Identity?.IsAuthenticated == true)
        {
            var userId = principal.GetUserId();
            var roleName = principal.FindFirstValue(AppConstants.CustomClaimType.RoleName);

            var fullName = principal.FindFirstValue(AppConstants.CustomClaimType.FullName);
            Email = principal.FindFirstValue(AppConstants.CustomClaimType.Email);
            ContactNumber = principal.FindFirstValue(AppConstants.CustomClaimType.ContactNumber);

            var parts = fullName.Split(' ');
            FirstName = parts[0];
            LastName = parts.Length > 1 ? parts[1] : null;

            // Don't worry we will ignore it, (we will not save it in the database, having it just to bypass the DataAnnotations validation
            Password = ConfirmPassword = "P@ss1234";

        }
        
    }
}

public class DateOnlyGreaterThanAttribute(string comparisonProperty) : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var currentValue = (DateOnly)value;

        var property = validationContext.ObjectType.GetProperty(comparisonProperty);

        if (property == null)
            throw new ArgumentException("Property with this name not found");

        var comparisonValue = (DateOnly)property.GetValue(validationContext.ObjectInstance);

        if (currentValue <= comparisonValue)
            return new ValidationResult(ErrorMessage);

        return ValidationResult.Success;
    }
}