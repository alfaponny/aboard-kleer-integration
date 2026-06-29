using System.Xml.Serialization;
namespace AboardKleerIntegration.Models.Kleer;

[XmlRoot("payroll-user-readable")]
public class KleerPayrollResponse
{

    [XmlElement("ssn")]
    public string Ssn { get; set; } = string.Empty;

    [XmlElement("address1")]
    public string Address1 { get; set; } = string.Empty;

    [XmlElement("address2")]
    public string? Address2 { get; set; }

    [XmlElement("zip-code")]
    public string ZipCode { get; set; } = string.Empty;

    [XmlElement("state")]
    public string State { get; set; } = string.Empty;

    [XmlElement("country-code")]
    public string CountryCode { get; set; } = string.Empty;

    [XmlElement("phone")]
    public string Phone { get; set; } = string.Empty;

    [XmlElement("employment-start-date")]
    public string EmploymentStartDate { get; set; } = string.Empty;
}