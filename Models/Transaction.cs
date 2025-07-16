namespace Debt_Tracking_System.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        public string Type { get; set; } = "Credit"; // or "Debit"
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }

}
