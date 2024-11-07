namespace HotelManagement.Models.Public;

public record PaymentModel(Ulid BookingId, string RoomTypeName, int NumberOfDays, decimal TotalAmount);