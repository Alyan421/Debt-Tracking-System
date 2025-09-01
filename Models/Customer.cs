namespace Debt_Tracking_System.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal TotalDebt { get; set; }
        public DateOnly CreatedAt { get; set; }

        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
