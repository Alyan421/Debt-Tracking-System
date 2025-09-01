using Debt_Tracking_System.Models;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Globalization;
using Debt_Tracking_System.Repository;

namespace Debt_Tracking_System.Managers.Customers;

public class CustomerManager : ICustomerManager
{
    private readonly IGenericRepository<Customer> _customerRepository;
    private readonly IGenericRepository<Transaction> _transactionRepository;

    public CustomerManager(IGenericRepository<Customer> customerRepository, IGenericRepository<Transaction> transactionRepository)
    {

        _customerRepository = customerRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<Customer> AddCustomerAsync(Customer customer)
    {
        var initialDebt = customer.TotalDebt;

        customer.TotalDebt = 0;

        await _customerRepository.AddAsync(customer);

        if (initialDebt != 0)
        {
            var openingTransaction = new Transaction
            {
                CustomerId = customer.Id,
                Type = initialDebt > 0 ? "Debit" : "Credit",
                Amount = Math.Abs(initialDebt),
                Description = "Opening Balance",
                Date = customer.CreatedAt
            };
            await _transactionRepository.AddAsync(openingTransaction);

            customer.TotalDebt = initialDebt;
            await _customerRepository.UpdateAsync(customer);
        }

        return customer;
    }

    public async Task<Customer?> UpdateCustomerAsync(Customer customer)
    {
        var existingCustomer = await _customerRepository.GetByIdAsync(customer.Id);
        if (existingCustomer == null)
            return null;

        existingCustomer.Name = customer.Name;
        existingCustomer.Phone = customer.Phone;
        existingCustomer.Address = customer.Address;

        await _customerRepository.UpdateAsync(existingCustomer);
        return existingCustomer;
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