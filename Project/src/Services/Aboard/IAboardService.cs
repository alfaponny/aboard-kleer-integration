using AboardKleerIntegration.Models;

namespace AboardKleerIntegration.Services
{ 
    public interface IAboardService
    {
         Task<IEnumerable<User>> GetUsersAsync();
         Task<IEnumerable<UserAddress>> GetEmployeeAddressesAsync(string employeeId);
    }

}