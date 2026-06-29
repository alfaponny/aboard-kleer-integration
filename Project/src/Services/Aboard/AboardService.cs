using System.Text.Json;
using AboardKleerIntegration.Models;
using Microsoft.Extensions.Options;
using AboardKleerIntegration.Models.Aboard;
using AboardKleerIntegration.Config;
using System.ComponentModel.DataAnnotations;

namespace AboardKleerIntegration.Services
{
    public class AboardService : IAboardService
    {
        private readonly HttpClient _httpClient;

        public AboardService(HttpClient httpClient, IOptions<AboardOptions> options)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(options.Value.BaseUrl);
            _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.Value.ApiKey);
        }

        public async Task<IEnumerable<User>> GetUsersAsync()
        {
           var allEmployees = new List<AboardEmployee>();
           string? nextUrl = "employees";
           while(nextUrl != null) 
           {
      
                var response = await _httpClient.GetAsync(nextUrl);
                    response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var page = JsonSerializer.Deserialize<AboardEmployeeResponse>(json);
                    allEmployees.AddRange(page?.Data ?? []);
                nextUrl = page?.Links?.Next;
           }
                return allEmployees.Select(e => new User
                {
                Id = e.Id,
                FirstName = e.Attributes.FirstName,
                LastName = e.Attributes.LastName,
                WorkEmail = e.Attributes.WorkEmail,
                WorkPhone = e.Attributes.WorkPhoneNumber,
                PersonalPhone = e.Attributes.PersonalPhoneNumber,
                PersonalEmail = e.Attributes.PersonalEmail,
                Role = e.Attributes.Role,
                NationalId = e.Attributes.NationalIdentificationNumber,
                DateOfBirth = e.Attributes.DateOfBirth,
                EmploymentNumber = e.Attributes.EmploymentNumber,
                WeeklyWorkingHours = e.Attributes.WeeklyWorkingHours,
                DailyWorkingHours = e.Attributes.DailyWorkingHours,
                Workdays = e.Attributes.Workdays.Split(';'),
                }) ?? Enumerable.Empty<User>();
        }

        public async Task<IEnumerable<UserAddress>> GetEmployeeAddressesAsync(string employeeId)
        {
            var response = await _httpClient.GetAsync($"employees/{employeeId}/home-addresses");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var addressResponse = JsonSerializer.Deserialize<AboardAddressResponse>(json);
            return addressResponse?.Data.Select(a => new UserAddress
            {
                AddressLine1 = a.Attributes.AddressLine1,
                AddressLine2 = a.Attributes.AddressLine2,
                City = a.Attributes.City,
                ZipCode = a.Attributes.ZipCode,
                CountryCode = a.Attributes.CountryCode
            }) ?? Enumerable.Empty<UserAddress>();
        }
    }
}