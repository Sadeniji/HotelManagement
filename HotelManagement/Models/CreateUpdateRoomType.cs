using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using HotelManagement.Data.Entities;

namespace HotelManagement.Models;

public class CreateUpdateRoomType
{
    public Ulid Id { get; set; }
    [Required, MaxLength(100), Unicode(false)]
    public string Name { get; set; }

    [MaxLength(100)]
    public string Image { get; set; }

    [Required, Range(1, double.MaxValue)] 
    public decimal Price { get; set; }

    [Required, MaxLength(200)]
    public string Description { get; set; }

    public int MaxAdults { get; set; }
    public int MaxChildren { get; set; }
    public bool IsActive { get; set; }
    public CreateUpdateRoomTypeAmenity[] Amenities { get; set; } = [];
}

public class CreateUpdateRoomTypeAmenity(Ulid id, int? unit)
{
    public Ulid Id { get; set; } = id;
    public int? Unit { get; set; } = unit;
}
