namespace HotelManagement.Models.Public;

public class FilterModel
{
    public FilterModel()
    {
        
    }
    public static FilterModel GetFilterModel(DateOnly? checkInDate, DateOnly? checkOutDate, int? numberOfAdults, int? numberOfChildren)
    {
        return new FilterModel
        {
            CheckInDate = checkInDate,
            CheckOutDate = checkOutDate,
            NumberOfAdults = numberOfAdults,
            NumberOfChildren = numberOfChildren
        };

    }

    public DateOnly? CheckInDate { get; set; } //= DateOnly.FromDateTime(DateTime.Now);
    public DateOnly? CheckOutDate { get; set; } //= DateOnly.FromDateTime(DateTime.Now.AddDays(1));

    public int? NumberOfAdults { get; set; } = 0;
    public int? NumberOfChildren { get; set; } = 0;

    public IReadOnlyDictionary<string, object?> TDictionary() =>
        new Dictionary<string, object?>()
        {
            [nameof(CheckInDate)] = CheckInDate,
            [nameof(CheckOutDate)] = CheckOutDate,
            [nameof(NumberOfAdults)] = NumberOfAdults,
            [nameof(NumberOfChildren)] = NumberOfChildren
        };
}