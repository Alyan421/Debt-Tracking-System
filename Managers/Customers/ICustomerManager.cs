using Debt_Tracking_System.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Debt_Tracking_System.Managers.Customers;

public interface ICustomerManager
{
    Task<Customer> AddCustomerAsync(Customer customer);
    Task<Customer?> UpdateCustomerAsync(Customer updatedCustomer); Task DeleteCustomerAsync(int id);
    Task<Customer?> GetCustomerByIdAsync(int id);
    Task<IEnumerable<Customer>> GetAllCustomersAsync();
}