using Debt_Tracking_System.Models;
using Debt_Tracking_System.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OfficeOpenXml;
using System.ComponentModel;

namespace Debt_Tracking_System.Managers.Transactions
{
    public class TransactionManager : ITransactionManager
    {
        private readonly IGenericRepository<Models.Transaction> _transactionRepository;
        private readonly IGenericRepository<Customer> _customerRepository;

        public TransactionManager(
            IGenericRepository<Models.Transaction> transactionRepository,
            IGenericRepository<Customer> customerRepository)
        {
            _transactionRepository = transactionRepository;
            _customerRepository = customerRepository;
        }

        public async Task<Models.Transaction> AddTransactionAsync(Models.Transaction transaction)
        {
            var customer = await _customerRepository.GetByIdAsync(transaction.CustomerId);
            if (transaction.Type != "Debit" && transaction.Type != "Credit")
                throw new Exception("Invalid transaction type. Use 'Debit' or 'Credit'.");
            if (transaction.Amount <= 0)
                throw new Exception("Transaction amount cannot be negative");
            if (customer == null)
            {
                customer = new Customer
                {
                    Name = "Unknown",
                    Phone = "",
                    Address = "",
                    TotalDebt = transaction.Type == "Debit" ? transaction.Amount : -transaction.Amount,
                    CreatedAt = DateTime.UtcNow
                };
                await _customerRepository.AddAsync(customer);
            }
            else
            {
                customer.TotalDebt += transaction.Type == "Debit" ? transaction.Amount : -transaction.Amount;
                customer.CreatedAt = transaction.Date;
                await _customerRepository.UpdateAsync(customer);
            }

            transaction.CustomerId = customer.Id;

            await _transactionRepository.AddAsync(transaction);
            return transaction;
        }

        public async Task UpdateTransactionAsync(Models.Transaction transaction)
        {
            var existingTransaction = await _transactionRepository.GetByIdAsync(transaction.Id);
            if (existingTransaction == null) throw new Exception("Transaction not found");

            var oldAmount = existingTransaction.Amount;
            var oldType = existingTransaction.Type;

            // Update transaction properties
            existingTransaction.Type = transaction.Type;
            existingTransaction.Amount = transaction.Amount;
            existingTransaction.Description = transaction.Description;
            existingTransaction.CustomerId = transaction.CustomerId;

            await _transactionRepository.UpdateAsync(existingTransaction);

            var customer = await _customerRepository.GetByIdAsync(existingTransaction.CustomerId);
            if (customer != null)
            {
                customer.TotalDebt -= oldType == "Debit" ? oldAmount : -oldAmount;
                customer.TotalDebt += existingTransaction.Type == "Debit" ? existingTransaction.Amount : -existingTransaction.Amount;
                await _customerRepository.UpdateAsync(customer);
            }
        }

        public async Task DeleteTransactionAsync(int id)
        {
            var transaction = await _transactionRepository.GetByIdAsync(id);
            if (transaction == null) throw new Exception("Transaction not found");

            var customer = await _customerRepository.GetByIdAsync(transaction.CustomerId);
            if (customer != null)
            {
                customer.TotalDebt -= transaction.Type == "Debit" ? transaction.Amount : -transaction.Amount;
                await _customerRepository.UpdateAsync(customer);
            }

            await _transactionRepository.DeleteAsync(transaction);
        }

        public async Task<Models.Transaction?> GetTransactionByIdAsync(int id)
        {
            return await _transactionRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Models.Transaction>> GetAllTransactionsAsync()
        {
            return await _transactionRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Models.Transaction>> FilterByCustomerAsync(int customerId)
        {
            return await _transactionRepository.FindAsync(t => t.CustomerId == customerId);
        }

        public async Task<IEnumerable<Models.Transaction>> FilterByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _transactionRepository.FindAsync(t => t.Date.Date >= startDate.Date && t.Date.Date <= endDate.Date);
        }

        public async Task<IEnumerable<Models.Transaction>> FilterByCustomerAndDateRangeAsync(int customerId, DateTime startDate, DateTime endDate)
        {
            return await _transactionRepository.FindAsync(t =>
                t.CustomerId == customerId && t.Date.Date >= startDate.Date && t.Date.Date <= endDate.Date);
        }
        public async Task<byte[]> GenerateTransactionReportAsync(string type, int? customerId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            IEnumerable<Models.Transaction> transactions = type switch
            {
                "customer" => await _transactionRepository.FindAsync(t => t.CustomerId == customerId),
                "date" => await _transactionRepository.FindAsync(t =>
                    startDate.HasValue && endDate.HasValue &&
                    t.Date.Date >= startDate.Value.Date && t.Date.Date <= endDate.Value.Date),
                "both" => await _transactionRepository.FindAsync(t =>
                    t.CustomerId == customerId &&
                    startDate.HasValue && endDate.HasValue &&
                    t.Date.Date >= startDate.Value.Date && t.Date.Date <= endDate.Value.Date),
                _ => await _transactionRepository.GetAllAsync()
            };
            ExcelPackage.License.SetNonCommercialPersonal("Muhammad Alyan"); //This will also set the Author property to the name provided in the argument.
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Transactions");

            worksheet.Cells[1, 1].Value = "ID";
            worksheet.Cells[1, 2].Value = "CustomerId";
            worksheet.Cells[1, 3].Value = "Type";
            worksheet.Cells[1, 4].Value = "Amount";
            worksheet.Cells[1, 5].Value = "Description";
            worksheet.Cells[1, 6].Value = "Date";

            var row = 2;
            foreach (var t in transactions)
            {
                worksheet.Cells[row, 1].Value = t.Id;
                worksheet.Cells[row, 2].Value = t.CustomerId;
                worksheet.Cells[row, 3].Value = t.Type;
                worksheet.Cells[row, 4].Value = t.Amount;
                worksheet.Cells[row, 5].Value = t.Description;
                worksheet.Cells[row, 6].Value = t.Date.ToString("yyyy-MM-dd HH:mm:ss");
                row++;
            }

            return package.GetAsByteArray();
        }

    }
}