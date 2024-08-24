using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        [Required, Unicode(false)]
        public string FirstName { get; set; }
        [Unicode(false)]
        public string? LastName { get; set; }
        [Required, Unicode(false)]
        public string RoleName { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        public string ContactNumber { get; set; }

        [MaxLength(50), Unicode(false)]
        public string? Designation { get; set; }

        [MaxLength(100), Unicode(false)]
        public string? Image { get; set; }

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}".Trim();
    }

}
