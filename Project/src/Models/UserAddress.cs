namespace AboardKleerIntegration.Models;
public class UserAddress
{
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;

    public string GetMergedAddress() =>
        (string.IsNullOrWhiteSpace(AddressLine1), string.IsNullOrWhiteSpace(AddressLine2)) switch
        {
            (false, false) => $"{AddressLine1}, {AddressLine2}",
            (false, true)  => AddressLine1,
            (true, false)  => AddressLine2!,
            (true, true)   => ""
        };
}