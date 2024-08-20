using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Data.Entities;

public class Amenity
{
    [Key]
    public Ulid Id { get; set; }

    [Required, MaxLength(25), Unicode(false)]
    public string Name { get; set; }

    [Required, MaxLength(25), Unicode(false)]
    public string Icon { get; set; }
}