using AboardKleerIntegration.Models;
using AboardKleerIntegration.Models.Kleer;

namespace AboardKleerIntegration.Services
{ 
    public interface IKleerService
    {
            Task<IEnumerable<KleerUser>> GetUsersAsync();
            Task UpdateProfileAsync(User user, int kleerUserId);
            Task UpdateEmployeeDataAsync(User user, UserAddress? address, int kleerUserId);
            Task<KleerPayrollResponse?> GetPayrollAsync(int kleerUserId);
    }
    
}