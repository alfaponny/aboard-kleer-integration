namespace AboardKleerIntegration.Models;
   
    public class User
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string WorkEmail { get; set; } = string.Empty;
        public string WorkPhone { get; set; } = string.Empty;
        public string PersonalPhone { get; set; } = string.Empty;
        public string PersonalEmail { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string NationalId { get; set; } = string.Empty;
        public string DateOfBirth { get; set; } = string.Empty;
        public string EmploymentNumber { get; set; } = string.Empty;
        public double WeeklyWorkingHours { get; set; } = 0.0;
        public double DailyWorkingHours { get; set; } = 0.0;
        public IEnumerable<string> Workdays { get; set; } = new List<string>();
    }