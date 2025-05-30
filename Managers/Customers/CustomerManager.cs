using Debt_Tracking_System.Models;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Globalization;
using Debt_Tracking_System.Repository;

namespace Debt_Tracking_System.Managers.Customers;

public class CustomerManager : ICustomerManager
{
    private readonly IGenericRepository<Customer> _customerRepository;

    public CustomerManager(IGenericRepository<Customer> customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Customer> AddCustomerAsync(Customer customer)
    {
        customer.TotalDebt = 0;
        customer.CreatedAt = DateTime.UtcNow;

        await _customerRepository.AddAsync(customer);
        return customer;
    }

    public async Task UpdateCustomerAsync(Customer updatedCustomer)
    {
        var customer = await _customerRepository.GetByIdAsync(updatedCustomer.Id);
        if (customer == null) throw new Exception("Customer not found");

        customer.Name = updatedCustomer.Name;
        customer.Phone = updatedCustomer.Phone;
        customer.Address = updatedCustomer.Address;
        await _customerRepository.UpdateAsync(customer);
    }

    public async Task DeleteCustomerAsync(int id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null) throw new Exception("Customer not found");

        await _customerRepository.DeleteAsync(customer);
    }

    public async Task<Customer?> GetCustomerByIdAsync(int id)
    {
        return await _customerRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
    {
        return await _customerRepository.GetAllAsync();
    }
}