using AboardKleerIntegration.Services;
using Microsoft.Extensions.Options;
using AboardKleerIntegration.Config;

namespace AboardKleerIntegration.Tests;
[TestClass]
public class AboardServiceTests
{
    private TestHttpMessageHandler _httpMessageHandler = null!;
    private HttpClient _httpClient = null!;
    private AboardService _aboardService = null!;   

    [TestInitialize]
    public void Setup()
    {
        _httpMessageHandler = new TestHttpMessageHandler();
        _httpClient = new HttpClient(_httpMessageHandler);
        _aboardService = new AboardService(_httpClient, Options.Create(
            new AboardOptions { BaseUrl = "https://fake.api/", ApiKey = "test-key" }));
    }

    [TestMethod]
    public async Task GetUsersAsync_ReturnsUsers()
    {
        var jsonResponse = @"{
            ""data"": [
                {
                    ""id"": ""123"",
                    ""attributes"": {
                        ""first-name"": ""Petrainho"",
                        ""last-name"": ""Skalman"",
                        ""work-email"": ""petrainho.skalman@fejka.nu"",
                        ""work-phone-number"": ""1234567890"",
                        ""personal-phone-number"": ""0987654321"",
                        ""personal-email"": ""petrainho.skalman.personal@fejka.nu"",
                        ""role"": ""Developer"",
                        ""national-identification-number"": ""19000101-0000"",
                        ""date-of-birth"": ""1900-01-01"",
                        ""employment-number"": ""EMP123"",
                        ""weekly-working-hours"": 40,
                        ""daily-working-hours"": 8,
                        ""workdays"": ""Monday;Tuesday;Wednesday;Thursday;Friday""
                    }
                }
            ],
            ""links"": {
                ""next"": null
            }
        }";     

        _httpMessageHandler.EnqueueResponse(jsonResponse);
        var users = await _aboardService.GetUsersAsync();
        Assert.AreEqual(1, users.Count());
        var user = users.First();
        Assert.AreEqual("123", user.Id);
        Assert.AreEqual("Petrainho", user.FirstName);
        Assert.AreEqual("Skalman", user.LastName);
        Assert.AreEqual("petrainho.skalman@fejka.nu", user.WorkEmail);
        Assert.AreEqual("1234567890", user.WorkPhone);
        Assert.AreEqual("0987654321", user.PersonalPhone);
        Assert.AreEqual("petrainho.skalman.personal@fejka.nu", user.PersonalEmail); 
        Assert.AreEqual("Developer", user.Role);
        Assert.AreEqual("19000101-0000", user.NationalId);
    }
    [TestMethod]
    public async Task GetUsersAsync_EmptyResponse_ReturnsEmptyList()
    {
        var jsonResponse = @"{
            ""data"": [],
            ""links"": {
                ""next"": null
            }
        }";     

        _httpMessageHandler.EnqueueResponse(jsonResponse);
        var users = await _aboardService.GetUsersAsync();
        Assert.AreEqual(0, users.Count());
    }

     [TestMethod]
     public async Task GetPagination_WorksCorrectly()
     {
         var firstPageResponse = @"{
            ""data"": [
                {
                    ""id"": ""456"",
                    ""attributes"": 
                    {
                        ""first-name"": ""Andreaninho"",
                        ""last-name"": ""Bamse"",
                        ""work-email"": ""andreaninho.bamse@fejka.nu"",
                        ""work-phone-number"": ""0701740631"",
                        ""personal-phone-number"": ""0987654321"",
                        ""personal-email"": ""andreaninho.bamse.personal@fejka.nu"",
                        ""role"": ""Developer"",
                        ""national-identification-number"": ""19000202-0000"",
                        ""date-of-birth"": ""1900-02-02"",
                        ""employment-number"": ""EMP456"",
                        ""weekly-working-hours"": 40,
                        ""daily-working-hours"": 8,
                        ""workdays"": ""Monday;Tuesday;Wednesday;Thursday;Friday""
                    }
                }
            ],
            ""links"": {
                ""next"": ""employees?page=2""
            }
        }";     

        var secondPageResponse = @"{
            ""data"": [
                { ""id"": ""789"",
                    ""attributes"": 
                    {
                        ""first-name"": ""Lukeinho"",
                        ""last-name"": ""Undulatsson"",
                        ""work-email"": ""lukeinho.undulatsson@fejka.nu"",
                        ""work-phone-number"": ""1234567890"",
                        ""personal-phone-number"": ""0987654321"",
                        ""personal-email"": ""lukeinho.undulatsson.personal@fejka.nu"",
                        ""role"": ""Developer"",
                        ""national-identification-number"": ""19000303-0000"",
                        ""date-of-birth"": ""1900-03-03"",
                        ""employment-number"": ""EMP123"",
                        ""weekly-working-hours"": 40,
                        ""daily-working-hours"": 8,
                        ""workdays"": ""Monday;Tuesday;Wednesday;Thursday;Friday""
                    }  
                }
            ],
            ""links"": {
                ""next"": null
            }
        }";     

        _httpMessageHandler.EnqueueResponse(firstPageResponse);
        _httpMessageHandler.EnqueueResponse(secondPageResponse);

        var users = await _aboardService.GetUsersAsync();
        Assert.AreEqual(2, users.Count());
        Assert.IsTrue(users.Any(u => u.Id == "456" && u.FirstName == "Andreaninho"));
        Assert.IsTrue(users.Any(u => u.Id == "789" && u.FirstName == "Lukeinho"));
     }
     [TestMethod]
     public async Task GetEmployeeAddressesAsync_ReturnsAddresses()
     {
         var jsonResponse = @"{
            ""data"": [
                {
                    ""id"": ""1"",
                    ""attributes"": {
                        ""start-date"": ""2023-01-01"",
                        ""address-line-1"": ""Drottningggatunvägen 18"",
                        ""address-line-2"": ""lgh 324"",
                        ""zip-code"": ""70532"",
                        ""city"": ""Alingsås"",
                        ""country-code"": ""SE""
                    }
                }
            ],
            ""links"": {
                ""next"": null
            }
        }";     

        _httpMessageHandler.EnqueueResponse(jsonResponse);
        var addresses = await _aboardService.GetEmployeeAddressesAsync("123");
        Assert.AreEqual(1, addresses.Count());
        var address = addresses.First();
        Assert.AreEqual("Drottningggatunvägen 18", address.AddressLine1);
        Assert.AreEqual("lgh 324", address.AddressLine2);
        Assert.AreEqual("70532", address.ZipCode);
        Assert.AreEqual("Alingsås", address.City);
        Assert.AreEqual("SE", address.CountryCode);
    }
}