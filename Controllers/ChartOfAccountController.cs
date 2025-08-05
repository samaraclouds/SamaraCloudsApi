using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SamaraCloudsApi.Models;
using SamaraCloudsApi.Services;

namespace SamaraCloudsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChartOfAccountController : ControllerBase
    {
        private readonly IChartOfAccountService _coaService;

        public ChartOfAccountController(IChartOfAccountService coaService)
        {
            _coaService = coaService;
        }

        [Authorize]
        [HttpGet("view-all")]
        public async Task<IActionResult> ViewAll(
            [FromQuery] int customerId,
            [FromQuery] int? branchId = null,
            [FromQuery] string? search = null,
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null)
        {
            try
            {
                var data = await _coaService.ViewAllAsync(customerId, branchId, search, dateFrom, dateTo);
                return Ok(new
                {
                    success = true,
                    message = "Chart of Account list retrieved successfully.",
                    count = data.Count(),
                    data
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    success = false,
                    error = "internal_error",
                    message = "An unexpected error occurred while fetching Chart of Account."
                });
            }
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] ChartOfAccountCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var newId = await _coaService.CreateAsync(dto);
                return Ok(new
                {
                    success = true,
                    message = "Chart of Account created successfully.",
                    newAccountId = newId
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    success = false,
                    error = "internal_error",
                    message = "An unexpected error occurred while creating Chart of Account."
                });
            }
        }

        [Authorize]
        [HttpPut("update/{accountId}")]
        public async Task<IActionResult> Update(int accountId, [FromBody] ChartOfAccountUpdateDto dto)
        {
            if (accountId != dto.AccountId)
                return BadRequest("AccountId in URL and body do not match.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _coaService.UpdateAsync(dto);
                return Ok(new
                {
                    success = true,
                    message = "Chart of Account updated successfully."
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    success = false,
                    error = "internal_error",
                    message = "An unexpected error occurred while updating Chart of Account."
                });
            }
        }

        [Authorize]
        [HttpDelete("delete/{accountId}")]
        public async Task<IActionResult> Delete(int accountId, [FromQuery] int deletedBy)
        {
            try
            {
                await _coaService.DeleteAsync(accountId, deletedBy);
                return Ok(new
                {
                    success = true,
                    message = "Chart of Account deleted successfully."
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    success = false,
                    error = "internal_error",
                    message = "An unexpected error occurred while deleting Chart of Account."
                });
            }
        }
    }
}
