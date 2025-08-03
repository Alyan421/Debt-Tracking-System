using Debt_Tracking_System.Models;
using Debt_Tracking_System.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OfficeOpenXml;
using System.ComponentModel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

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


        public async Task<Transaction?> UpdateTransactionAsync(Transaction transaction)
        {
            var existingTransaction = await _transactionRepository.GetByIdAsync(transaction.Id);
            if (existingTransaction == null)
                return null;

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

            return existingTransaction;
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


        public async Task<byte[]> GenerateBillAsync(int customerId, DateTime startDate, DateTime endDate)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            // Get customer information
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null)
                throw new Exception("Customer not found");

            // Get transactions for the customer in the date range
            var transactions = await FilterByCustomerAndDateRangeAsync(customerId, startDate, endDate);

            var transactionList = transactions.OrderBy(t => t.Date).ToList();

            decimal totalDebits = transactionList.Where(t => t.Type == "Debit").Sum(t => t.Amount);
            decimal totalCredits = transactionList.Where(t => t.Type == "Credit").Sum(t => t.Amount);
            var balance = totalDebits - totalCredits;

            // Generate PDF using QuestPDF
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header()
                        .Height(80)
                        .Background(Colors.Blue.Lighten3)
                        .Padding(10)
                        .Row(row =>
                        {
                            row.RelativeItem()
                                .Column(column =>
                                {
                                    column.Item().Text("F.F Fancy Collection")
                                        .FontSize(18)
                                        .SemiBold()
                                        .FontColor(Colors.Blue.Darken2);

                                    column.Item().Text($"Customer: {customer.Name}")
                                        .FontSize(14)
                                        .SemiBold()
                                        .FontColor(Colors.Blue.Darken1);

                                    if (!string.IsNullOrEmpty(customer.Phone))
                                        column.Item().Text($"Phone: {customer.Phone}")
                                            .FontSize(14)
                                            .SemiBold()
                                            .FontColor(Colors.Blue.Darken1);
                                });

                            row.ConstantItem(140)
                                .Column(column =>
                                {
                                    column.Item().Text($"Bill Generated At:")
                                                       .FontSize(14)
                                                       .SemiBold();
                                    column.Item().Text($"{DateTime.Now:MM/dd/yyyy}")
                                        .FontSize(14);
                                });
                        });

                    page.Content()
                        .PaddingVertical(0.5f, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Spacing(8);

                            // Billing Period - Centered with larger font
                            x.Item().Text($"Billing Period: {startDate:MM/dd/yyyy} to {endDate:MM/dd/yyyy}")
                                .FontSize(16)
                                .SemiBold()
                                .FontColor(Colors.Blue.Darken2)
                                .AlignCenter();

                            // Transactions Table
                            x.Item().Text("Transaction Details")
                                .FontSize(14)
                                .SemiBold()
                                .FontColor(Colors.Blue.Darken2);

                            x.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2); // Date
                                    columns.RelativeColumn(1.5f); // Type
                                    columns.RelativeColumn(3); // Description
                                    columns.RelativeColumn(1.5f); // Amount
                                });

                                // Header row
                                table.Cell()
                                    .Background(Colors.Blue.Medium)
                                    .Padding(6)
                                    .Text("Date")
                                    .FontColor(Colors.White)
                                    .SemiBold();

                                table.Cell()
                                    .Background(Colors.Blue.Medium)
                                    .Padding(6)
                                    .Text("Type")
                                    .FontColor(Colors.White)
                                    .SemiBold();

                                table.Cell()
                                    .Background(Colors.Blue.Medium)
                                    .Padding(6)
                                    .Text("Description")
                                    .FontColor(Colors.White)
                                    .SemiBold();

                                table.Cell()
                                    .Background(Colors.Blue.Medium)
                                    .Padding(6)
                                    .Text("Amount")
                                    .FontColor(Colors.White)
                                    .SemiBold();

                                // Data rows with reduced padding
                                foreach (var transaction in transactionList)
                                {
                                    table.Cell()
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .Padding(4)
                                        .Text(transaction.Date.ToString("MM/dd/yyyy"));

                                    table.Cell()
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .Padding(4)
                                        .Text(transaction.Type)
                                        .FontColor(transaction.Type == "Debit" ? Colors.Red.Medium : Colors.Green.Medium)
                                        .SemiBold();

                                    table.Cell()
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .Padding(4)
                                        .Text(transaction.Description ?? "-");

                                    table.Cell()
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .Padding(4)
                                        .Text($"Rs {transaction.Amount:F2}")
                                        .FontColor(transaction.Type == "Debit" ? Colors.Red.Medium : Colors.Green.Medium)
                                        .SemiBold();
                                }
                            });

                            // Summary Section - Left aligned
                            x.Item().PaddingTop(10)
                                .AlignLeft()
                                .Width(300)
                                .Background(Colors.Grey.Lighten4)
                                .Padding(15)
                                .Column(column =>
                                {
                                    column.Item().Text("Summary")
                                        .FontSize(16)
                                        .SemiBold()
                                        .FontColor(Colors.Blue.Darken2);

                                    column.Item().PaddingTop(8).Row(summaryRow =>
                                    {
                                        summaryRow.RelativeItem().Text("Total Debits:");
                                        summaryRow.ConstantItem(80).Text($"Rs {totalDebits:F2}")
                                            .FontColor(Colors.Red.Medium).SemiBold().AlignRight();
                                    });

                                    column.Item().Row(summaryRow =>
                                    {
                                        summaryRow.RelativeItem().Text("Total Credits:");
                                        summaryRow.ConstantItem(80).Text($"Rs {totalCredits:F2}")
                                            .FontColor(Colors.Green.Medium).SemiBold().AlignRight();
                                    });

                                    column.Item().PaddingTop(5).Row(summaryRow =>
                                    {
                                        summaryRow.RelativeItem().Text("Current Balance:")
                                            .FontSize(12).SemiBold();
                                        summaryRow.ConstantItem(80).Text($"Rs {Math.Abs(balance):F2}")
                                            .FontColor(balance >= 0 ? Colors.Red.Medium : Colors.Green.Medium)
                                            .FontSize(12).SemiBold().AlignRight();
                                    });

                                    column.Item().Text(balance >= 0 ? "(Amount Due)" : "(Credit Balance)")
                                        .FontSize(10)
                                        .FontColor(balance >= 0 ? Colors.Red.Medium : Colors.Green.Medium)
                                        .AlignRight();

                                    column.Item().PaddingTop(5).Row(summaryRow =>
                                    {
                                        summaryRow.RelativeItem().Text("Total Outstanding Debt:")
                                            .FontSize(12).SemiBold();
                                        summaryRow.ConstantItem(80).Text($"Rs {customer.TotalDebt:F2}")
                                            .FontSize(12).SemiBold().AlignRight()
                                            .FontColor(customer.TotalDebt >= 0 ? Colors.Red.Medium : Colors.Green.Medium);
                                    });
                                });

                            // Thank you message
                            x.Item().PaddingTop(20).Text("Thank you for your business!")
                                .FontSize(14)
                                .AlignCenter()
                                .FontColor(Colors.Blue.Medium);
                        });

                    page.Footer()
                        .PaddingVertical(10)
                        .Column(column =>
                        {
                            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);

                            column.Item().PaddingTop(10).Row(row =>
                            {
                                row.RelativeItem()
                                    .Column(leftColumn =>
                                    {
                                        leftColumn.Item().Text("Contact Information")
                                            .FontSize(12)
                                            .SemiBold()
                                            .FontColor(Colors.Blue.Darken2);

                                        leftColumn.Item().Text("M.Faisal Khan: 0345-2597059 | 0323-8237316")
                                            .FontSize(10)
                                            .FontColor(Colors.Black);

                                        leftColumn.Item().Text("M.Farhan Khan: 0333-2268288")
                                            .FontSize(10)
                                            .FontColor(Colors.Black);
                                    });

                                row.RelativeItem()
                                    .Column(rightColumn =>
                                    {
                                        rightColumn.Item().Text("Address")
                                            .FontSize(12)
                                            .SemiBold()
                                            .FontColor(Colors.Blue.Darken2);

                                        rightColumn.Item().Text("Shop 13A-B, Anees Market")
                                            .FontSize(10)
                                            .FontColor(Colors.Black);

                                        rightColumn.Item().Text("New Neham Road, Karachi")
                                            .FontSize(10)
                                            .FontColor(Colors.Black);
                                    });
                            });

                            column.Item().PaddingTop(10)
                                .AlignCenter()
                                .DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Medium))
                                .Text(x =>
                                {
                                    x.Span("Page ");
                                    x.CurrentPageNumber();
                                    x.Span(" of ");
                                    x.TotalPages();
                                });
                        });
                });
            });

            return document.GeneratePdf();
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