using Debt_Tracking_System.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Debt_Tracking_System.Managers.Transactions
{
    public interface ITransactionManager
    {
        Task<Transaction> AddTransactionAsync(Transaction transaction);
        Task<Transaction?> UpdateTransactionAsync(Transaction updatedTransaction);
        Task DeleteTransactionAsync(int id);
        Task<Transaction?> GetTransactionByIdAsync(int id);
        Task<IEnumerable<Transaction>> GetAllTransactionsAsync();
        Task<IEnumerable<Transaction>> FilterByCustomerAsync(int customerId);
        Task<IEnumerable<Transaction>> FilterByDateRangeAsync(DateOnly startDate, DateOnly endDate);
        Task<IEnumerable<Transaction>> FilterByCustomerAndDateRangeAsync(int customerId, DateOnly startDate, DateOnly endDate);
        Task<byte[]> GenerateBillAsync(int customerId, DateOnly startDate, DateOnly endDate);
        Task<byte[]> GenerateTransactionReportAsync(string type, int? customerId = null, DateOnly? startDate = null, DateOnly? endDate = null);
    }
}
