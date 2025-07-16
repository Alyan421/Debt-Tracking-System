namespace Debt_Tracking_System.Controllers.Transactions
{
    public class CreateTransactionDTO
    {
        public int CustomerId { get; set; }
        public string Type { get; set; } = "Credit"; // or "Debit"
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }

    }

    public class UpdateTransactionDTO
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string Type { get; set; } = "Credit";
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }

    }

    public class GetTransactionDTO
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string Type { get; set; } = "Credit";
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }

}
