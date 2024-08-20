using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Data.Entities;

public class Booking
{
    [Key]
    public Ulid Id { get; set; }
    public Ulid RoomId { get; set; }
    [Required]
    public string GuestId { get; set; }
    public int Adult { get; set; }
    public int Children { get; set; }
    [Range(1, double.MaxValue)]
    public decimal TotalAmount { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public DateTime BookedOn { get; set; }
    public DateTime ModifiedOn { get; set; }

    [MaxLength(300), Unicode(false)]
    public string? Remarks { get; set; }

    public virtual Room Room { get; set; }
    public virtual ApplicationUser Guest { get; set; }
}