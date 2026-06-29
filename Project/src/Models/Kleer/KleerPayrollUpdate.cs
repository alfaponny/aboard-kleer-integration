using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace AboardKleerIntegration.Models.Kleer;

    [XmlRoot("payroll-user-writable")]
    public class KleerPayrollUpdate
    {
        [JsonPropertyName("ssn")]
        [XmlElement("ssn")]
        public string Ssn { get; set; } = string.Empty;

        [JsonPropertyName("address1")]
        [XmlElement("address1")]
        public string Address1 { get; set; } = string.Empty;
        
        [JsonPropertyName("address2")]
        [XmlElement("address2")]
        public string? Address2 { get; set; }

        [JsonPropertyName("zip-code")]
        [XmlElement("zip-code")]
        public string ZipCode { get; set; } = string.Empty;      

        [JsonPropertyName("state")]
        [XmlElement("state")]
        public string State { get; set; } = string.Empty;          

        [JsonPropertyName("country-code")]
        [XmlElement("country-code")]
        public string CountryCode { get; set; } = string.Empty;
     

        [JsonPropertyName("phone")]
        [XmlElement("phone")]
        public string Phone { get; set; } = string.Empty;

        [JsonPropertyName("employment-start-date")]
        [XmlElement("employment-start-date")]
        public string EmploymentStartDate { get; set; } = string.Empty;           
    }  