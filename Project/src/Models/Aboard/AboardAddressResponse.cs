using System.Text.Json.Serialization;

namespace AboardKleerIntegration.Models.Aboard
{      
    public class AboardAddressResponse
    {
        [JsonPropertyName("data")]
        public List<AboardAddress> Data { get; set; } = new();
    }      

        public class AboardAddress
    {
        [JsonPropertyName("id")]
        public string Id{ get; set; } = string.Empty;
        [JsonPropertyName("attributes")]
        public AboardAddressAttributes Attributes { get; set; } = new();
    }
        public class AboardAddressAttributes
    {        
        [JsonPropertyName("start-date")]
        public string StartDate { get; set; } = string.Empty;
        [JsonPropertyName("address-line-1")]

        public string AddressLine1{ get; set; } = string.Empty;
        [JsonPropertyName("address-line-2")]
        public string? AddressLine2{ get; set; }
        [JsonPropertyName("city")]
        public string City { get; set; } = string.Empty;
        [JsonPropertyName("zip-code")]
        public string ZipCode { get; set; } = string.Empty;
        [JsonPropertyName("country-code")]
        public string CountryCode { get; set; } = string.Empty;
    }
}