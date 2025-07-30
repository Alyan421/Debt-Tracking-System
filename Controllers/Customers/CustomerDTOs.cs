namespace Debt_Tracking_System.Controllers.Customers
{
    public class CreateCustomerDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal TotalDebt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class UpdateCustomerDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal TotalDebt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }

    public class GetCustomerDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal TotalDebt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }

}
