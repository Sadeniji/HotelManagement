namespace HotelManagement.Extensions;

public static class StringExtensions
{
    public static string Ellipsis(this string str, int maxLength) =>
        (string.IsNullOrWhiteSpace(str) || str.Length <= maxLength) ? str : $"{str[0..97]}...";

    //public static string Ellipsis(this string str, int maxLength) =>
    //    (string.IsNullOrWhiteSpace(str) || str.Length <= maxLength) ? str : str.Substring(0, maxLength);
}