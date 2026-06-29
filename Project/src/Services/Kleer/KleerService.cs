using AboardKleerIntegration.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AboardKleerIntegration.Config;
using System.Text.Json;
using System.Text;
using AboardKleerIntegration.Models.Kleer;
using System.Xml.Serialization;
using System.IO;
using System.Security;

namespace AboardKleerIntegration.Services;

    public class KleerService : IKleerService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<KleerService> _logger;
        private readonly string _companyId;

        public KleerService(HttpClient httpClient, ILogger<KleerService> logger, IOptions<KleerOptions> options)
        {
            _httpClient = httpClient;
            _logger = logger;
            _companyId = options.Value.CompanyId;
            _httpClient.DefaultRequestHeaders.Add("X-Token", options.Value.ApiKey);
            _httpClient.BaseAddress = new Uri(options.Value.BaseUrl);
        }
        public async Task<IEnumerable<KleerUser>> GetUsersAsync()
        {
            var response = await _httpClient.GetAsync($"company/{_companyId}/user");
            response.EnsureSuccessStatusCode();
            
            var xml = await response.Content.ReadAsStringAsync();
            var serializer = new XmlSerializer(typeof(KleerUsersResponse));
            using var reader = new StringReader(xml);
            var result = (KleerUsersResponse?)serializer.Deserialize(reader);

            return result?.Users ?? new List<KleerUser>();
        }

        public async Task UpdateProfileAsync(User user, int kleerUserId)
        {
            var kleerUser = new KleerUser
            {
                Id = kleerUserId,
                Name = $"{SecurityElement.Escape(user.FirstName)} {SecurityElement.Escape(user.LastName)}",
                Email = user.WorkEmail,
                Active = true
            };

            var xmlString = $"<user><id>{kleerUserId}</id><name>{SecurityElement.Escape(user.FirstName)} {SecurityElement.Escape(user.LastName)}</name><email>{user.WorkEmail}</email><active>true</active></user>";
            var content = new StringContent(xmlString, new UTF8Encoding(false), "application/xml");

            var response = await _httpClient.PostAsync($"company/{_companyId}/user/{kleerUserId}", content);

            if (!response.IsSuccessStatusCode)
                {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Kleer error {StatusCode} for user {Name} ({Email})", response.StatusCode, user.FirstName + " " + user.LastName, user.WorkEmail);
                _logger.LogDebug("Kleer error body: {Body}", errorBody);
                }

            response.EnsureSuccessStatusCode();
           
            _logger.LogInformation("Updated Kleer user {Id}: {Name} ({Email})", 
                kleerUserId, user.FirstName + " "+ user.LastName, user.WorkEmail);
        }
        public async Task UpdateEmployeeDataAsync(User user, UserAddress? address, int kleerUserId)
        {
            if (address == null) return;

            var getResponse = await _httpClient.GetAsync($"company/{_companyId}/payroll/user/{kleerUserId}");
            
            if(!getResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning("No payroll contract for user {Id}, skipping update", kleerUserId);
                return;
            }
            
            var xml = await getResponse.Content.ReadAsStringAsync();
            var serializer = new XmlSerializer(typeof(KleerPayrollResponse));
            using var reader = new StringReader(xml);
            var existingContract = (KleerPayrollResponse?)serializer.Deserialize(reader);

            if (existingContract == null)
            {
                _logger.LogWarning("Could not read payroll contract for user {Id},", kleerUserId);
                return;
            }

            var updateEmployeeData = new KleerPayrollUpdate
            {
                Ssn = user.NationalId,
                EmploymentStartDate = existingContract.EmploymentStartDate,
                Address1 = "",
                Address2 = address.GetMergedAddress(),
                ZipCode = address.ZipCode,
                State = address.City,
                CountryCode = address.CountryCode,
                Phone = user.WorkPhone
            };    
                
            var serializerUpdate = new XmlSerializer(typeof(KleerPayrollUpdate));
            using var stringWriter = new StringWriter();
            using var xmlWriter = System.Xml.XmlWriter.Create(stringWriter, new System.Xml.XmlWriterSettings { OmitXmlDeclaration = true });
                serializerUpdate.Serialize(xmlWriter, updateEmployeeData);
                
            var content = new StringContent(stringWriter.ToString(), new UTF8Encoding(false), "application/xml");


            var postResponse = await _httpClient.PostAsync($"company/{_companyId}/payroll/user/{kleerUserId}", content);
        
            if (!postResponse.IsSuccessStatusCode)
                {
                var errorBody = await postResponse.Content.ReadAsStringAsync();
                _logger.LogError("Error updating employee data for {Id}: {StatusCode} - {Body}", kleerUserId, postResponse.StatusCode, errorBody);
                }

            postResponse.EnsureSuccessStatusCode();

            _logger.LogInformation("Updated employee data for user {Id}", kleerUserId);
        }
        public async Task<KleerPayrollResponse?> GetPayrollAsync(int kleerUserId)
        {
            var response = await _httpClient.GetAsync($"company/{_companyId}/payroll/user/{kleerUserId}");
            
            if(!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("No payroll contract for user {Id}", kleerUserId);
                return null;
            }
            
            var xml = await response.Content.ReadAsStringAsync();
            var serializer = new XmlSerializer(typeof(KleerPayrollResponse));
            using var reader = new StringReader(xml);
            var result = (KleerPayrollResponse?)serializer.Deserialize(reader);

            return result;
        }
    }