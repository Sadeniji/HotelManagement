using HotelManagement.Models.Public;
using Stripe.Checkout;

namespace HotelManagement.Services;

public interface IPaymentService
{
    Task<string> GeneratePaymentUrl(PaymentModel model, string userId, string domain);
}

public class PaymentService : IPaymentService
{
    public async Task<string> GeneratePaymentUrl(PaymentModel model, string userId, string domain)
    {
        var totalAmount = model.Price * model.NumberOfDays;

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
                        Description = $"Booking a {model.RoomTypeName} Room for {model.NumberOfDays} Days at {model.Price:c}/night"
                    }
                }
            }
        ];
        var sessionCreateOptions = new SessionCreateOptions
        {
            LineItems = lineItems.ToList(),
            Mode = "payment",
            SuccessUrl = domain + "/booking-success?session-id={CHECKOUT_SESSION_ID}",
            CancelUrl = $"{domain}/booking-cancel"
        };
        var sessionService = new SessionService();

        var session = await sessionService.CreateAsync(sessionCreateOptions);

        return session.Url;
    }
}