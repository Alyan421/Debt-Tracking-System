using AutoMapper;
using Debt_Tracking_System.Models;
using Debt_Tracking_System.Controllers.Customers;
using Debt_Tracking_System.Controllers.Transactions;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Customer Mapping
        CreateMap<Customer, GetCustomerDTO>().ReverseMap();
        CreateMap<CreateCustomerDTO, Customer>().ReverseMap();
        CreateMap<UpdateCustomerDTO, Customer>().ReverseMap();

        // Transaction Mapping
        CreateMap<Transaction, GetTransactionDTO>().ReverseMap();
        CreateMap<CreateTransactionDTO, Transaction>().ReverseMap();
        CreateMap<UpdateTransactionDTO, Transaction>().ReverseMap();
    }
}
