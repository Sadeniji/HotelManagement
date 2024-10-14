using HotelManagement.Models.Public;

namespace HotelManagement.Models;

public class GetBookingModel
{
    public Ulid Id { get; set; }
    public Ulid RoomTypeId { get; set; }
    public string RoomTypeName { get; set; }
    public Ulid? RoomId { get; set; }
    public string? RoomNumber { get; set; }
    public string GuestId { get; set; }
    public string GuestName { get; set; }
    public int Adult { get; set; }
    public int Children { get; set; }
    public decimal TotalAmount { get; set; }
    public DateOnly CheckInDate { get; set; }
    public DateOnly CheckOutDate { get; set; }
    public DateTime BookedOn { get; set; }
    public BookingStatus Status { get; set; }
    public string? SpecialRequest { get; set; }
    public string? Remarks { get; set; }
    public bool IsRoomNumberAssigned => Status is BookingStatus.Booked or BookingStatus.PaymentSuccess;
    public bool CanBeApproved => Status == BookingStatus.PaymentSuccess;
    public bool CanBeCancelled => Status != BookingStatus.PaymentCancelled && Status != BookingStatus.Cancelled;
}