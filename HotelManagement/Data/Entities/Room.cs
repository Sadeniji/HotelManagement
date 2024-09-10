using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Data.Entities;

public class Room
{
    [Key]
    public Ulid Id { get; set; }

    [Required]
    public Ulid RoomTypeId { get; set; }

    [Required, MaxLength(25), Unicode(false)]
    public string RoomNumber { get; set; }

    public bool IsAvailable { get; set; }

    public bool IsDeleted { get; set; }

    public virtual RoomType RoomType { get; set; }
}