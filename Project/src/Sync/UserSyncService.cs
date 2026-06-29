using AboardKleerIntegration.Models;
using AboardKleerIntegration.Services;
using Microsoft.Extensions.Logging;
using AboardKleerIntegration.Models.Kleer;

namespace AboardKleerIntegration.Sync;

    public class UserSyncService
    {
        private readonly IKleerService _kleerService;
        private readonly IAboardService _aboardService;
        private readonly ISlackService _slackService;

        private readonly ILogger<UserSyncService> _logger;


        public UserSyncService(IKleerService kleerService, IAboardService aboardService, ISlackService slackService, ILogger<UserSyncService> logger)
        {
            _kleerService = kleerService;
            _aboardService = aboardService;
            _slackService = slackService;
            _logger = logger;
        }

        public async Task SyncUsersAsync()
        {
            _logger.LogInformation("Starting to sync users between Aboard and Kleer...");
            var aboardUsers = await _aboardService.GetUsersAsync();
            var kleerUsers = await _kleerService.GetUsersAsync();
            var aboardEmails = aboardUsers.Select( u => u.WorkEmail).ToHashSet();
            var matchedCount = 0;
            var updatedCount = 0;
            var unmatchedCount = 0;

            var kleerLookup = kleerUsers.ToDictionary(k => k.Email, k=> k);
            foreach (var user in aboardUsers)
            {
                try
                {
                    var addresses = await _aboardService.GetEmployeeAddressesAsync(user.Id);
                    var address = addresses.FirstOrDefault();

                    if (kleerLookup.TryGetValue(user.WorkEmail, out var kleerUser))
                    {
                        var existingPayroll = await _kleerService.GetPayrollAsync(kleerUser.Id);

                        if (HasChanges(user, kleerUser, address, existingPayroll))  
                        {
                            _logger.LogInformation("Changes detected for {Name}, {Email}, updating ..", user.FirstName, user.WorkEmail);
                            await _kleerService.UpdateProfileAsync(user, kleerUser.Id);

                            await _kleerService.UpdateEmployeeDataAsync(user, address, kleerUser.Id);    
                            updatedCount++;
                            }
                            else
                            {
                            _logger.LogInformation("No changes for {Name} ({Email}), skipping", user.FirstName, user.WorkEmail);
                            }
                         matchedCount++;    
                    }
                else
                {
                    _logger.LogWarning("No Kleer match for {Name} ({Email})", 
                        user.FirstName, user.WorkEmail);
                   // await _slackService.SendMessageAsync($"Message from the integration between Aboard and Kleer: No Kleer match for {user.FirstName} ({user.WorkEmail}) - please check if this user should be added to Kleer.");    
                    unmatchedCount++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync user {Name} ({Email}), continuing...",
                    user.FirstName, user.WorkEmail);
            }
        }

            foreach (var existingKleerUser in kleerUsers)
            {
                if (string.IsNullOrEmpty(existingKleerUser.Email))
                continue;

            if (!aboardEmails.Contains(existingKleerUser.Email))
            {
                _logger.LogWarning("User {Name} ({Email}) exists in Kleer but not in Aboard - possible offboarding?",
                    existingKleerUser.Name, existingKleerUser.Email);
                await _slackService.SendMessageAsync($"Message from the integration between Aboard and Kleer: User {existingKleerUser.Name} ({existingKleerUser.Email}) exists in Kleer but not in Aboard - please check if this is an offboarding case.");    
            }  
        }

        _logger.LogInformation("Sync between Aboard and Kleer completed. {Matched} matched, {Updated} updated, {Unmatched} unmatched",
            matchedCount, updatedCount, unmatchedCount);
    }
    
        private bool HasChanges(User aboardUser, KleerUser kleerUser, UserAddress? address, KleerPayrollResponse? payroll)
            {
                var aboardFullName = aboardUser.FirstName + " " + aboardUser.LastName;
                var hasChanges = false;

                if (kleerUser.Name != aboardFullName)
                    {
                        _logger.LogInformation("Name change: '{OldName}' to '{NewName}'", 
                        kleerUser.Name, aboardFullName);
                        hasChanges = true;
                    }
                if (kleerUser.Email != aboardUser.WorkEmail)
                    {
                        _logger.LogInformation("Email change: '{OldEmail}' to '{NewEmail}'", 
                        kleerUser.Email, aboardUser.WorkEmail);
                        hasChanges = true;
                    }
                if (address != null && payroll != null)
                {
                    if (payroll.Address1 != "")                
                    {
                        _logger.LogInformation("Address c/o change detected for {Name} ({Email})", 
                        aboardUser.FirstName, aboardUser.WorkEmail); 
                        hasChanges = true;
                    }
                    if (payroll.Address2 != address.GetMergedAddress())                
                    {
                        _logger.LogInformation("Address change detected for {Name} ({Email})", 
                            aboardUser.FirstName, aboardUser.WorkEmail);
                        hasChanges = true;
                    }
                    if (payroll.ZipCode != address.ZipCode)                
                    {
                        _logger.LogInformation("Zip code change detected for {Name} ({Email})", 
                            aboardUser.FirstName, aboardUser.WorkEmail);
                        hasChanges = true;
                    }
                    if (payroll.CountryCode != address.CountryCode)                
                    {
                        _logger.LogInformation("Country code change detected for {Name} ({Email})", 
                            aboardUser.FirstName, aboardUser.WorkEmail);
                        hasChanges = true;  
                    }
                    if (payroll.State != address.City)                
                    {
                        _logger.LogInformation("City change detected for {Name} ({Email})", 
                            aboardUser.FirstName, aboardUser.WorkEmail);
                        hasChanges = true;  
                    }
                    if (payroll.Phone != aboardUser.WorkPhone)                
                    {
                        _logger.LogInformation("Phone change detected for {Name} ({Email})", 
                             aboardUser.FirstName, aboardUser.WorkEmail);
                        hasChanges = true;  
                    }        
                }

            return hasChanges;
        }          
    }