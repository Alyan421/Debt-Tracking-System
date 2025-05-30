using AutoMapper;
using Debt_Tracking_System.Managers.Transactions;
using Microsoft.AspNetCore.Mvc;
using Debt_Tracking_System.Models;

namespace Debt_Tracking_System.Controllers.Transactions;

[ApiController]
[Route("api/[controller]")]
public class TransactionController : ControllerBase
{
    private readonly ITransactionManager _transactionManager;
    private readonly IMapper _mapper;

    public TransactionController(ITransactionManager transactionManager, IMapper mapper)
    {
        _transactionManager = transactionManager;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> AddTransaction([FromBody] CreateTransactionDTO dto)
    {
        try
        {
            var transaction = _mapper.Map<Transaction>(dto);
            var resultModel = await _transactionManager.AddTransactionAsync(transaction);
            var resultDto = _mapper.Map<GetTransactionDTO>(resultModel);
            return Ok(resultDto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateTransaction([FromBody] UpdateTransactionDTO dto)
    {
        try
        {
            var transaction = _mapper.Map<Transaction>(dto);
            await _transactionManager.UpdateTransactionAsync(transaction);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTransaction(int id)
    {
        try
        {
            await _transactionManager.DeleteTransactionAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTransactionById(int id)
    {
        try
        {
            var transactionModel = await _transactionManager.GetTransactionByIdAsync(id);
            if (transactionModel == null) return NotFound();
            var transactionDto = _mapper.Map<GetTransactionDTO>(transactionModel);
            return Ok(transactionDto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTransactions()
    {
        try
        {
            var transactions = await _transactionManager.GetAllTransactionsAsync();
            var transactionDtos = _mapper.Map<List<GetTransactionDTO>>(transactions);
            return Ok(transactionDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpGet("filter-by-customer/{customerId}")]
    public async Task<IActionResult> FilterByCustomer(int customerId)
    {
        try
        {
            var transactions = await _transactionManager.FilterByCustomerAsync(customerId);
            var dtos = _mapper.Map<List<GetTransactionDTO>>(transactions);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("filter-by-date-range")]
    public async Task<IActionResult> FilterByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var transactions = await _transactionManager.FilterByDateRangeAsync(startDate, endDate);
            var dtos = _mapper.Map<List<GetTransactionDTO>>(transactions);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }


    [HttpGet("filter-by-customer-and-date-range")]
    public async Task<IActionResult> FilterByCustomerAndDateRange([FromQuery] int customerId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var transactions = await _transactionManager.FilterByCustomerAndDateRangeAsync(customerId, startDate, endDate);
            var dtos = _mapper.Map<List<GetTransactionDTO>>(transactions);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }


    [HttpGet("report")]
    public async Task<IActionResult> GenerateReport(
        [FromQuery] string type,
        [FromQuery] int? customerId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var reportBytes = await _transactionManager.GenerateTransactionReportAsync(type, customerId, startDate, endDate);
            return File(reportBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TransactionReport.xlsx");
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

}
