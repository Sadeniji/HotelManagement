using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Data.Entities;

public class Payment
{
    public Ulid Id { get; set; }

    public Ulid BookingId { get; set; }

    public string? CheckOutSessionId { get; set; }

    [MaxLength(10), Unicode(false)]
    public string Status { get; set; }

    [MaxLength(150), Unicode(false)]
    public string? AdditionalInfo { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime ModifiedOn { get; set; }

    public virtual Booking Booking { get; set; }
}