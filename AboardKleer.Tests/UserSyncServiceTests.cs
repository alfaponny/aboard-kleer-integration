using AboardKleerIntegration.Models;
using AboardKleerIntegration.Models.Kleer;
using AboardKleerIntegration.Services;
using AboardKleerIntegration.Sync;
using Moq;
using Microsoft.Extensions.Logging;

namespace AboardKleerIntegration.Tests;

[TestClass]
public class UserSyncServiceTests
{
    private Mock<IKleerService> _kleerService = null!;
    private Mock<IAboardService> _aboardService = null!;
    private Mock<ISlackService> _slackService = null!;
    private Mock<ILogger<UserSyncService>> _logger = null!;
    private UserSyncService _syncService = null!;

    [TestInitialize]
    public void Setup()
    {
        _kleerService = new Mock<IKleerService>();
        _aboardService = new Mock<IAboardService>();
        _slackService = new Mock<ISlackService>();
        _logger = new Mock<ILogger<UserSyncService>>();
        _syncService = new UserSyncService(_kleerService.Object, 
            _aboardService.Object, 
            _slackService.Object, 
            _logger.Object);
    }

    [TestMethod]
    public async Task SyncUsersAsync_MatchesUserWithChanges_UpdatesKleer()
    {
        var aboardUser = CreateUser("Thomasinho", "Domherresson", "thomasinho@fejka.nu", "123");
        var KleerUser = new KleerUser { Id = 1, Name = "Thomasinho Old", Email = "thomasinho@fejka.nu" };

        _aboardService.Setup(s => s.GetUsersAsync()).ReturnsAsync([aboardUser]);
        _aboardService.Setup(s => s.GetEmployeeAddressesAsync("123")).ReturnsAsync([CreateAddress()]);
        _kleerService.Setup(s => s.GetUsersAsync()).ReturnsAsync([KleerUser]);
        _kleerService.Setup(s => s.GetPayrollAsync(1)).ReturnsAsync(CreatePayroll());

        await _syncService.SyncUsersAsync();

        _kleerService.Verify(s => s.UpdateProfileAsync(aboardUser, 1), Times.Once);
        _kleerService.Verify(s => s.UpdateEmployeeDataAsync(aboardUser, It.IsAny<UserAddress>(), 1), Times.Once);
    }

    [TestMethod]
    public async Task SyncUsersAsync_MatchesUserWithoutChanges_DoesNotUpdateKleer()
    {
 
        var aboardUser = CreateUser("Håkaninho", "Sparvsson", "hakaninho@fejka.nu", "456");
        var KleerUser = new KleerUser { Id = 1, Name = "Håkaninho Sparvsson", Email = "hakaninho@fejka.nu" };
        var address = CreateAddress();
        var payroll = new KleerPayrollResponse
        {
            // Aboard has no "c/o" concept, so an unchanged record has an empty Address1
            // and the Aboard address merged into Address2 (matching the source's HasChanges logic).
            Address1 = "",
            Address2 = address.GetMergedAddress(),
            ZipCode = address.ZipCode,
            CountryCode = address.CountryCode,
            State = address.City,
            Phone = aboardUser.WorkPhone
        };

        _aboardService.Setup(s => s.GetUsersAsync()).ReturnsAsync([aboardUser]);
        _aboardService.Setup(s => s.GetEmployeeAddressesAsync("456")).ReturnsAsync([address]);
        _kleerService.Setup(s => s.GetUsersAsync()).ReturnsAsync([KleerUser]);
        _kleerService.Setup(s => s.GetPayrollAsync(1)).ReturnsAsync(payroll);

        await _syncService.SyncUsersAsync();

        _kleerService.Verify(s => s.UpdateProfileAsync(aboardUser, 1), Times.Never);
        _kleerService.Verify(s => s.UpdateEmployeeDataAsync(aboardUser, It.IsAny<UserAddress>(), 1), Times.Never);
    }

    [TestMethod]
    public async Task SyncUsersAsync_UnmatchedAboardUser_DoesNotUpdateKleer()
    {
        var aboardUser = CreateUser("Larsinho", "Larsson", "larsinho@fejka.nu", "789");
        _aboardService.Setup(s => s.GetUsersAsync()).ReturnsAsync([aboardUser]);
        _aboardService.Setup(s => s.GetEmployeeAddressesAsync("789")).ReturnsAsync([]);
        _kleerService.Setup(s => s.GetUsersAsync()).ReturnsAsync([]);

        await _syncService.SyncUsersAsync();

        _kleerService.Verify(s => s.UpdateProfileAsync(It.IsAny<User>(), It.IsAny<int>()), Times.Never);
    }    

    [TestMethod]
    public async Task SyncUsersAsync_KleerUserEmptyEmail_SkipsOffboardingCheck()
    {
        var kleerUser = new KleerUser { Id = 1, Name = "Empty Email", Email = "" };
        _aboardService.Setup(s => s.GetUsersAsync()).ReturnsAsync([]);
        _kleerService.Setup(s => s.GetUsersAsync()).ReturnsAsync([kleerUser]);

        await _syncService.SyncUsersAsync();

        _slackService.Verify(s => s.SendMessageAsync(It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task SyncUsersAsync_KleerUserNotInAboard_SendsOffboardingSlackMessage()
    {
        var kleerUser = new KleerUser { Id = 1, Name = "Gunillainho Storksson", Email = "gunillainho@fejka.nu" };
        _aboardService.Setup(s => s.GetUsersAsync()).ReturnsAsync([]);
        _kleerService.Setup(s => s.GetUsersAsync()).ReturnsAsync([kleerUser]);

        await _syncService.SyncUsersAsync();

        _slackService.Verify(
            s => s.SendMessageAsync(It.Is<string>(m => m.Contains("gunillainho@fejka.nu"))),
            Times.Once);
    }

    [TestMethod]
    public async Task SyncUsersAsync_UserSyncFails_ContinuesWithNext()
    {
        var user1 = CreateUser("Fail", "User", "fail@user.com", "1");
        var user2 = CreateUser("Success", "User", "success@user.com", "2");
        var kleerUser2 = new KleerUser { Id = 2, Name = "Success User", Email = "success@user.com" };

        _aboardService.Setup(s => s.GetUsersAsync()).ReturnsAsync([user1, user2]);
        _aboardService.Setup(s => s.GetEmployeeAddressesAsync("1")).ThrowsAsync(new Exception("API error"));
        _aboardService.Setup(s => s.GetEmployeeAddressesAsync("2")).ReturnsAsync([CreateAddress()]);
        _kleerService.Setup(s => s.GetUsersAsync()).ReturnsAsync([kleerUser2]);
        _kleerService.Setup(s => s.GetPayrollAsync(2)).ReturnsAsync(CreatePayroll());
        
        await _syncService.SyncUsersAsync();

        _kleerService.Verify(s => s.UpdateProfileAsync(user2, 2), Times.Once);
        _kleerService.Verify(s => s.UpdateEmployeeDataAsync(user2, It.IsAny<UserAddress>(), 2), Times.Once);
    }
    [TestMethod]
    public async Task SyncUsersAsync_EmptyLists_CompletesWithoutErrors()
    {
        _aboardService.Setup(s => s.GetUsersAsync()).ReturnsAsync([]);
        _kleerService.Setup(s => s.GetUsersAsync()).ReturnsAsync([]);

        await _syncService.SyncUsersAsync();

        _kleerService.Verify(s => s.UpdateProfileAsync(It.IsAny<User>(), It.IsAny<int>()), Times.Never);
        _kleerService.Verify(s => s.UpdateEmployeeDataAsync(It.IsAny<User>(), It.IsAny<UserAddress>(), It.IsAny<int>()), Times.Never);
    }

       
    private static User CreateUser(string firstName, 
        string lastName, string email, string id) => new()
    {
        Id = id,
        FirstName = firstName,
        LastName = lastName,
        WorkEmail = email,
        WorkPhone = "0701234567",
 
    };
    private static UserAddress CreateAddress() => new()
    {
 
        AddressLine1 = "Kronobergsgatanvägsgatan 12", 
        AddressLine2 = null,
        City = "Lund", 
        ZipCode = "14112", 
        CountryCode = "SE" 
        
    };

    private static KleerPayrollResponse CreatePayroll() => new()
    {
        Address1 = "Fiolfikongatan 3",
        Phone = "0707654321",
    };
}