using AutoMapper;
using Debt_Tracking_System.Controllers.Customers;
using Debt_Tracking_System.Managers.Customers;
using Debt_Tracking_System.Models;
using Microsoft.AspNetCore.Mvc;

namespace Debt_Tracking_System.Controllers.Customers;

[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerManager _customerManager;
    private readonly IMapper _mapper;

    public CustomerController(ICustomerManager customerManager, IMapper mapper)
    {
        _customerManager = customerManager;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> AddCustomer([FromBody] CreateCustomerDTO dto)
    {
        try
        {
            var customer = _mapper.Map<Customer>(dto);
            var resultModel = await _customerManager.AddCustomerAsync(customer);
            var resultDto = _mapper.Map<GetCustomerDTO>(resultModel);
            return Ok(resultDto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateCustomer([FromBody] UpdateCustomerDTO dto)
    {
        try
        {
            var customer = _mapper.Map<Customer>(dto);
            await _customerManager.UpdateCustomerAsync(customer);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        try
        {
            await _customerManager.DeleteCustomerAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCustomerById(int id)
    {
        try
        {
            var customerModel = await _customerManager.GetCustomerByIdAsync(id);
            if (customerModel == null) return NotFound();
            var customerDto = _mapper.Map<GetCustomerDTO>(customerModel);
            return Ok(customerDto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCustomers()
    {
        try
        {
            var customers = await _customerManager.GetAllCustomersAsync();
            var customerDtos = _mapper.Map<List<GetCustomerDTO>>(customers);
            return Ok(customerDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
