using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SamaraCloudsApi.Models;
using SamaraCloudsApi.Services;
using System.Net;

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
            var data = await _coaService.ViewAllAsync(customerId, branchId, search, dateFrom, dateTo);
            var response = new ApiResponse<IEnumerable<ChartOfAccountViewDto>>
            {
                Status = (int)HttpStatusCode.OK,
                Code = "SUCCESS",
                Message = "Chart of Account list retrieved successfully.",
                Errors = null,
                Data = data,
                Count = data.Count()
            };
            return Ok(response);
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] ChartOfAccountCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errorResponse = new ApiResponse<object>
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Code = "VALIDATION_ERROR",
                    Message = "Invalid input data.",
                    Errors = ModelState,
                    Data = null
                };
                return BadRequest(errorResponse);
            }

            var newId = await _coaService.CreateAsync(dto);

            var response = new ApiResponse<int>
            {
                Status = (int)HttpStatusCode.OK,
                Code = "SUCCESS",
                Message = "Chart of Account created successfully.",
                Errors = null,
                Data = newId
            };
            return Ok(response);
        }

        [Authorize]
        [HttpPut("update/{accountId}")]
        public async Task<IActionResult> Update(int accountId, [FromBody] ChartOfAccountUpdateDto dto)
        {
            if (accountId != dto.AccountId)
            {
                var errorResponse = new ApiResponse<object>
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Code = "VALIDATION_ERROR",
                    Message = "AccountId in URL and body do not match.",
                    Errors = null,
                    Data = null
                };
                return BadRequest(errorResponse);
            }

            if (!ModelState.IsValid)
            {
                var errorResponse = new ApiResponse<object>
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Code = "VALIDATION_ERROR",
                    Message = "Invalid input data.",
                    Errors = ModelState,
                    Data = null
                };
                return BadRequest(errorResponse);
            }

            await _coaService.UpdateAsync(dto);

            var response = new ApiResponse<object>
            {
                Status = (int)HttpStatusCode.OK,
                Code = "SUCCESS",
                Message = "Chart of Account updated successfully.",
                Errors = null,
                Data = null
            };
            return Ok(response);
        }

        [Authorize]
        [HttpDelete("delete/{accountId}")]
        public async Task<IActionResult> Delete(int accountId, [FromQuery] int deletedBy)
        {
            await _coaService.DeleteAsync(accountId, deletedBy);

            var response = new ApiResponse<object>
            {
                Status = (int)HttpStatusCode.OK,
                Code = "SUCCESS",
                Message = "Chart of Account deleted successfully.",
                Errors = null,
                Data = null
            };
            return Ok(response);
        }
    }
}
