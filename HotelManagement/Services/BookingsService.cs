using System.Linq.Expressions;
using HotelManagement.Constants;
using HotelManagement.Data;
using HotelManagement.Data.Entities;
using HotelManagement.Models;
using HotelManagement.Models.Public;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Services;

public interface IBookingsService
{
    Task<MethodResult<Ulid>> MakeBookingAsync(BookingModel bookingModel, string userId);
    Task<PagedResult<GetBookingModel>> GetBookingsAsync(int startIndex, int pageSize);
    Task<MethodResult> ApproveBookingAsync(Ulid bookingId);
    Task<MethodResult> CancelBookingAsync(Ulid bookingId, string reason, string? userId = null);
    Task<PagedResult<GetBookingModel>> GetGuestBookingsAsync(string guestId, BookingType bookingType, int startIndex, int pageSize);
}
public class BookingsService(IDbContextFactory<ApplicationDbContext> contextFactory, 
                            IRoomTypeService roomTypeService, 
                            ILogger<BookingsService> logger) : IBookingsService
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

    public async Task<PagedResult<GetBookingModel>> GetBookingsAsync(int startIndex, int pageSize)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var query = context.Bookings;

        var totalCount = await query.CountAsync();

        var bookings = await query.OrderByDescending(b => b.CheckInDate)
                                                  .Select(b => new GetBookingModel
                                                  {
                                                      Id = b.Id,
                                                      GuestId = b.GuestId,
                                                      GuestName = b.Guest.FullName,
                                                      RoomTypeId = b.RoomTypeId,
                                                      RoomTypeName = b.RoomType.Name,
                                                      SpecialRequest = b.SpecialRequest,
                                                      Status = b.Status,
                                                      BookedOn = b.BookedOn,
                                                      RoomId = b.RoomId,
                                                      RoomNumber = (b.RoomId == null || b.RoomId == Ulid.Empty) ? "" : b.Room.RoomNumber,
                                                      Adult = b.Adult,
                                                      Children = b.Children,
                                                      CheckInDate = b.CheckInDate,
                                                      CheckOutDate = b.CheckOutDate,
                                                      Remarks = b.Remarks,
                                                      TotalAmount = b.TotalAmount
                                                  })
                                                  .Skip(startIndex)
                                                  .Take(pageSize)
                                                  .ToArrayAsync();

        return new PagedResult<GetBookingModel>(totalCount, bookings);
    }

    public async Task<MethodResult> ApproveBookingAsync(Ulid bookingId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        var booking = await context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking is null)
        {
            return "Invalid request";
        }

        switch (booking.Status)
        {
            case BookingStatus.Booked:
                return "Already booked";
            case BookingStatus.Cancelled:
                return "Booking is cancelled.";
            case BookingStatus.PaymentSuccess:
                booking.Status = BookingStatus.Booked;
                booking.ModifiedOn = DateTime.UtcNow;
                break;
            default:
                return "Booking can be approved only after payment.";
        }

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<MethodResult> CancelBookingAsync(Ulid bookingId, string reason, string? userId = null)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        var booking = await context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking is null)
        {
            return "Invalid request";
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            return "Already cancelled.";
        }

        booking.Status = BookingStatus.Cancelled;
        booking.Remarks += Environment.NewLine + 
                           $"Cancelled by {(userId == booking.GuestId ? "Guest." : "Staff/Admin.")} Reason: {reason}";
        booking.ModifiedOn = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<PagedResult<GetBookingModel>> GetGuestBookingsAsync(string guestId, BookingType bookingType, int startIndex, int pageSize)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var query = context.Bookings.Where(b => b.GuestId == guestId);

        var now = DateOnly.FromDateTime(DateTime.Now);

        query = bookingType switch
        {
            BookingType.Upcoming => query.Where(b => b.CheckInDate > now),
            BookingType.Ongoing => query.Where(b => b.CheckInDate == now  || b.CheckOutDate == now),
            BookingType.Past => query.Where(b => b.CheckInDate < now),
        };

        var totalCount = await query.CountAsync();

        var bookings = await query.OrderByDescending(b => b.CheckInDate)
            .Select(_bookingModelSelector)
            .Skip(startIndex)
            .Take(pageSize)
            .ToArrayAsync();

        return new PagedResult<GetBookingModel>(totalCount, bookings);
    }

    private static Expression<Func<Booking, GetBookingModel>> _bookingModelSelector =
        b => new GetBookingModel
        {
            Id = b.Id,
            GuestId = b.GuestId,
            GuestName = b.Guest.FullName,
            RoomTypeId = b.RoomTypeId,
            RoomTypeName = b.RoomType.Name,
            SpecialRequest = b.SpecialRequest,
            Status = b.Status,
            BookedOn = b.BookedOn,
            RoomId = b.RoomId,
            RoomNumber = (b.RoomId == null || b.RoomId == Ulid.Empty) ? "" : b.Room.RoomNumber,
            Adult = b.Adult,
            Children = b.Children,
            CheckInDate = b.CheckInDate,
            CheckOutDate = b.CheckOutDate,
            Remarks = b.Remarks,
            TotalAmount = b.TotalAmount
        };
}