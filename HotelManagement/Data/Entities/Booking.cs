using System.ComponentModel.DataAnnotations;
using HotelManagement.Models.Public;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Data.Entities;

public class Booking
{
    [Key]
    public Ulid Id { get; set; }

    public Ulid RoomTypeId { get; set; }
    public Ulid? RoomId { get; set; }
    [Required]
    public string GuestId { get; set; }
    public int Adult { get; set; }
    public int Children { get; set; }
    [Range(1, double.MaxValue)]
    public decimal TotalAmount { get; set; }
    public DateOnly CheckInDate { get; set; }
    public DateOnly CheckOutDate { get; set; }
    public DateTime BookedOn { get; set; }
    public DateTime ModifiedOn { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    [MaxLength(300), Unicode(false)]
    public string? SpecialRequest { get; set; }

    [MaxLength(300), Unicode(false)]
    public string? Remarks { get; set; }

    public virtual Room Room { get; set; }
    public virtual RoomType RoomType { get; set; }
    public virtual ApplicationUser Guest { get; set; }
}