using System.Text.Json.Serialization;

namespace AboardKleerIntegration.Models.Aboard
{       
        public class AboardEmployeeResponse
        {
                [JsonPropertyName("data")]
                public List<AboardEmployee> Data { get; set; } = new();

                [JsonPropertyName("links")]
                public AboardLinks? Links { get; set; } = new();

        }      
        public class AboardLinks
        {
                [JsonPropertyName("next")]
                public string? Next { get; set; }
        }

        public class AboardEmployee
        {
                [JsonPropertyName("id")]
                public string Id { get; set; } = string.Empty;
                [JsonPropertyName("attributes")] 
                public AboardEmployeeAttributes Attributes { get; set; } = new();
        }
        public class AboardEmployeeAttributes
        {
                [JsonPropertyName("first-name")]
                public string FirstName { get; set; } = string.Empty;
                [JsonPropertyName("last-name")]
                public string LastName { get; set; } = string.Empty;
                [JsonPropertyName("work-email")]
                public string WorkEmail { get; set; } = string.Empty;
                [JsonPropertyName("work-phone-number")]
                public string WorkPhoneNumber { get; set; } = string.Empty;
                [JsonPropertyName("personal-phone-number")]
                public string PersonalPhoneNumber { get; set; } = string.Empty;
                [JsonPropertyName("personal-email")]
                public string PersonalEmail { get; set; } = string.Empty;
                [JsonPropertyName("role")]
                public string Role { get; set; } = string.Empty;
                [JsonPropertyName("national-identification-number")]
                public string NationalIdentificationNumber { get; set; } = string.Empty;
                [JsonPropertyName("date-of-birth")]
                public string DateOfBirth { get; set; } = string.Empty;
                [JsonPropertyName("employment-number")]
                public string EmploymentNumber { get; set; } = string.Empty;
                [JsonPropertyName("weekly-working-hours")]
                public double WeeklyWorkingHours { get; set; } = 0.0;
                [JsonPropertyName("daily-working-hours")]
                public double DailyWorkingHours { get; set; } = 0.0;
                [JsonPropertyName("workdays")]
                public string Workdays { get; set; } = string.Empty;
        }
}
    