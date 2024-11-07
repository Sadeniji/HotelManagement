using HotelManagement.Data;
using HotelManagement.Data.Entities;
using HotelManagement.Models;
using HotelManagement.Models.Public;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;

namespace HotelManagement.Services;

public interface IPaymentService
{
    Task<string> GeneratePaymentUrl(PaymentModel model, string userId, string domain);
    Task<MethodResult<string?>> ConfirmPaymentAsync(Ulid paymentId, Ulid bookingId, string checkoutSessionId);
    Task<MethodResult> CancelPaymentAsync(Ulid paymentId, Ulid bookingId, string checkoutSessionId);
}

public class PaymentService(IDbContextFactory<ApplicationDbContext> contextFactory, UserManager<ApplicationUser> userManager) : IPaymentService
{
    private const string StripePaymentInitiated = "initiated";
    private const string StripePaymentSuccess = "paid";
    private const string StripePaymentFail = "unpaid";
    public async Task<string> GeneratePaymentUrl(PaymentModel model, string userId, string domain)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        var paymentEntity = new Payment
        {
            Id = Ulid.NewUlid(),
            BookingId = model.BookingId,
            CreatedOn = DateTime.Now,
            ModifiedOn = DateTime.Now,
            Status = StripePaymentInitiated
            //CheckOutSessionId = "pending"
        };
        await context.Payments.AddAsync(paymentEntity);
        await context.SaveChangesAsync();

        var totalAmount = model.TotalAmount;

        SessionLineItemOptions[] lineItems =
        [
            new SessionLineItemOptions()
            {
                Quantity = 1,
                PriceData = new SessionLineItemPriceDataOptions()
                {
                    Currency = "usd",
                    UnitAmountDecimal = totalAmount * 100,
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = model.RoomTypeName,
                        Description = $"Booking a {model.RoomTypeName} Room for {model.NumberOfDays} Days at total cost {model.TotalAmount:c}/night(s)"
                    }
                }
            }
        ];
        var sessionCreateOptions = new SessionCreateOptions
        {
            LineItems = lineItems.ToList(),
            Mode = "payment",
            SuccessUrl = $"{domain}/bookings/{model.BookingId.ToString()}" + "/success?session-id={CHECKOUT_SESSION_ID}&payment-id="+paymentEntity.Id,
            CancelUrl = $"{domain}/bookings/{model.BookingId.ToString()}/cancel?session-id={{CHECKOUT_SESSION_ID}}&payment-id={paymentEntity.Id}"
        };
        var sessionService = new SessionService();

        var session = await sessionService.CreateAsync(sessionCreateOptions);

        paymentEntity.CheckOutSessionId = session.Id;
        await context.SaveChangesAsync();

        return session.Url;
    }

    public async Task<MethodResult<string?>> ConfirmPaymentAsync(Ulid paymentId, Ulid bookingId, string checkoutSessionId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var paymentEntity = await context.Payments.FirstOrDefaultAsync(p => p.Id == paymentId && p.CheckOutSessionId == checkoutSessionId);

        if (paymentEntity == null)
        {
            return new MethodResult<string?>(false, "Invalid payment id", default);
        }

        var booking = await context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking is null)
        {
            return new MethodResult<string?>(false, "Invalid booking id", default);
        }

        if (paymentEntity.Status != StripePaymentInitiated)
        {

        }
        else
        {
            var sessionService = new SessionService();
            var checkoutSession = await sessionService.GetAsync(checkoutSessionId);

            if (checkoutSession == null)
            {
                return new MethodResult<string?>(false, "Invalid Checkout session", default);
            }

            paymentEntity.Status = checkoutSession.PaymentStatus;
            paymentEntity.AdditionalInfo = $"Name: {checkoutSession.CustomerDetails.Name} Email: {checkoutSession.CustomerDetails.Email}";

            booking.Status = checkoutSession.PaymentStatus == StripePaymentSuccess
                ? BookingStatus.PaymentSuccess
                : BookingStatus.PaymentCancelled;

            await context.SaveChangesAsync();
        }


        var user = await userManager.Users.FirstOrDefaultAsync(u => u.Id == booking.GuestId);

        var guessName = user?.FullName ?? string.Empty;

        return new MethodResult<string?>(true, null, guessName);
    }

    public async Task<MethodResult> CancelPaymentAsync(Ulid paymentId, Ulid bookingId, string checkoutSessionId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var paymentEntity = await context.Payments.FirstOrDefaultAsync(p => p.Id == paymentId && p.CheckOutSessionId == checkoutSessionId);

        if (paymentEntity == null)
        {
            return new MethodResult(false, "Invalid payment id");
        }

        var booking = await context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking is null)
        {
            return new MethodResult(false, "Invalid booking id");
        }

        if (paymentEntity.Status == StripePaymentInitiated)
        {
            var sessionService = new SessionService();
            var checkoutSession = await sessionService.GetAsync(checkoutSessionId);

            if (checkoutSession == null)
            {
                return new MethodResult(false, "Invalid Checkout session");
            }

            paymentEntity.Status = "cancelled";
            paymentEntity.AdditionalInfo = "Payment cancelled by guest";

            booking.Status = BookingStatus.PaymentCancelled;

            await context.SaveChangesAsync();
        }

        return true;
    }
}