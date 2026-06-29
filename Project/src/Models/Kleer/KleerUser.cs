using System.Xml.Serialization;
using System.Text.Json.Serialization;

namespace AboardKleerIntegration.Models.Kleer;
[XmlRoot("user")]
    public class KleerUser
        {
            [XmlElement("id")]
            [JsonPropertyName("id")]
            public int Id { get; set; } 
            [XmlElement("internal-id")]
            public string InternalId { get; set; } = string.Empty;
            [XmlElement("name")]
            [JsonPropertyName("name")]
            public string Name { get; set; } = string.Empty;
            [XmlElement("email")]
            [JsonPropertyName("email")]
            public string Email { get; set; } = string.Empty;
            [XmlElement("active")]
            [JsonPropertyName("active")]
            public bool Active { get; set; } 

        }
        [XmlRoot("users")]
        public class KleerUsersResponse
        {
            [XmlElement("user")]
            public List<KleerUser> Users { get; set; } = new();
        }