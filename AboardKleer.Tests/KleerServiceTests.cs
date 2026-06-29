using AboardKleerIntegration.Services;
using AboardKleerIntegration.Models;
using Microsoft.Extensions.Options;
using AboardKleerIntegration.Config;
using Microsoft.Extensions.Logging;
using Moq;

namespace AboardKleerIntegration.Tests;
[TestClass]
public class KleerServiceTests
{
    private TestHttpMessageHandler _httpMessageHandler = null!;
    private HttpClient _httpClient = null!;
    private KleerService _kleerService = null!;   

    [TestInitialize]
    public void Setup()
    {
        _httpMessageHandler = new TestHttpMessageHandler();
        _httpClient = new HttpClient(_httpMessageHandler);
        var logger = Mock.Of<ILogger<KleerService>>();
        _kleerService = new KleerService(_httpClient, logger, Options.Create(
            new KleerOptions { BaseUrl = "https://fake.api/", ApiKey = "test-key", CompanyId = "123" }));
    }

    [TestMethod]
    public async Task GetUsersAsync_ReturnsUsers()
    {
        var xmlResponse = @"<users>
            <user>
                <id>1</id>
                <name>Amelinho Talgoxesson</name>
                <email>amelinho.talgoxesson@fejka.nu</email>
            </user>
        </users>";     

        _httpMessageHandler.EnqueueResponse(xmlResponse, "application/xml");
        var users = await _kleerService.GetUsersAsync();

        Assert.AreEqual(1, users.Count());
        var user = users.First();
        Assert.AreEqual(1, user.Id);
        Assert.AreEqual("Amelinho Talgoxesson", user.Name);
        Assert.AreEqual("amelinho.talgoxesson@fejka.nu", user.Email);
        }

    [TestMethod]
    public async Task GetPayrollAsync_ReturnsPayroll()
    {
        var xmlResponse = @"<payroll-user-readable>
            <ssn>19000101-0000</ssn>
            <address1>Angeredsgatan 3</address1>
            <address2></address2>
            <zip-code>75784</zip-code>
            <state>Skellefteå</state>
            <country-code>SE</country-code>
            <phone>0701740626</phone>
            <employment-start-date>2015-08-24</employment-start-date>
        </payroll-user-readable>
        ";

        _httpMessageHandler.EnqueueResponse(xmlResponse, "application/xml");
        var payroll = await _kleerService.GetPayrollAsync(1);

        Assert.IsNotNull(payroll);
        Assert.AreEqual("19000101-0000", payroll.Ssn);
        Assert.AreEqual("Angeredsgatan 3", payroll.Address1);
        Assert.AreEqual("", payroll.Address2);
        Assert.AreEqual("75784", payroll.ZipCode);
        Assert.AreEqual("Skellefteå", payroll.State);
        Assert.AreEqual("SE", payroll.CountryCode);
        Assert.AreEqual("0701740626", payroll.Phone);
        Assert.AreEqual("2015-08-24", payroll.EmploymentStartDate);      
    }
    [TestMethod]
    public async Task GetPayrollAsync_NotFound_ReturnsNull()
    {
        _httpMessageHandler.EnqueueResponse(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
        var payroll = await _kleerService.GetPayrollAsync(999);

        Assert.IsNull(payroll);
    }    
        
    [TestMethod]
    public async Task UpdateProfileAsync_SendsCorrectRequest()
    {
        var user = new User
        {
            Id = "123",
            FirstName = "Carolinaiho",
            LastName = "Mullvadsson",
            WorkEmail = "carolinaiho.mullvadsson@fejka.nu"
        };

        _httpMessageHandler.EnqueueResponse("<ok/>", "application/xml");

        await _kleerService.UpdateProfileAsync(user, 1);
        
        var request = _httpMessageHandler.SentRequests.Single();
        Assert.AreEqual(HttpMethod.Post, request.Method);
        StringAssert.Contains(request.RequestUri!.ToString(), "company/123/user/1");

        var content = await request.Content!.ReadAsStringAsync();
        StringAssert.Contains(content, "<name>Carolinaiho Mullvadsson</name>");
        StringAssert.Contains(content, "<email>carolinaiho.mullvadsson@fejka.nu</email>");
    }   
    [TestMethod]
    public async Task UpdateProfileAsync_OnError_ThrowsException()
    {
        var user = new User 
        {
            Id = "123",
            FirstName = "Isabellinho",
            LastName = "Bofinksson",
            WorkEmail = "isabellinho.bofinksson@fejka.nu"
        };

        _httpMessageHandler.EnqueueResponse(new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError));

        await Assert.ThrowsAsync<HttpRequestException>(
            () => _kleerService.UpdateProfileAsync(user, 1));
    }

    [TestMethod]
    public async Task UpdateEmployeeDataAsync_AddressNull_EarlyReturn()
    {
        var user = new User 
        {
            Id = "123",
            FirstName = "Vegainho",
            LastName = "Talgoxesson",
            WorkEmail = "vegainho.talgoxesson@fejka.nu"

        };
        await _kleerService.UpdateEmployeeDataAsync(user, null, 1);
        Assert.IsEmpty(_httpMessageHandler.SentRequests);
    }    
}