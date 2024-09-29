using HotelManagement.Data;
using HotelManagement.Data.Entities;
using HotelManagement.Models;
using HotelManagement.Models.Public;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Services;

public interface IBookingsService
{
    Task<MethodResult<Ulid>> MakeBookingAsync(BookingModel bookingModel, string userId);
}
public class BookingsService(IDbContextFactory<ApplicationDbContext> contextFactory, IRoomTypeService roomTypeService, ILogger<BookingsService> logger) : IBookingsService
{
    public async Task<MethodResult<Ulid>> MakeBookingAsync(BookingModel bookingModel, string userId)
    {
        try
        {
            var booking = new Booking
            {
                Id = Ulid.NewUlid(),
                Adult = bookingModel.NumberOfAdults ?? 0,
                BookedOn = DateTime.Now,
                ModifiedOn = DateTime.Now,
                CheckInDate = bookingModel.CheckInDate, //?? DateOnly.MinValue,
                CheckOutDate = bookingModel.CheckOutDate, //?? DateOnly.MinValue,
                Children = bookingModel.NumberOfChildren ?? 0,
                GuestId = userId,
                RoomTypeId = bookingModel.RoomTypeId,
                SpecialRequest = bookingModel.SpecialRequest,
                Status = BookingStatus.Pending
            };

            var roomType = await roomTypeService.GetRoomTypeInfoAsync(bookingModel.RoomTypeId);

            if (roomType is null)
            {
                logger.LogError($"The room type with Id - {bookingModel.RoomTypeId} has no price at the moment.");
                return MethodResult<Ulid>.Failure("The room type has no price for now.");
            }

            booking.TotalAmount = (bookingModel.CheckOutDate.DayNumber - bookingModel.CheckInDate.DayNumber) * roomType.Price;

            await using var context = await contextFactory.CreateDbContextAsync();
            await context.Bookings.AddAsync(booking);
            await context.SaveChangesAsync();

            logger.LogInformation("Successfully create a new booking.");
            return MethodResult<Ulid>.Success(booking.Id);
        }
        catch (Exception ex)
        {
            return MethodResult<Ulid>.Failure(ex.Message);
        }
    }
}