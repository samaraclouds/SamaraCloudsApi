using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SamaraCloudsApi.Models;
using SamaraCloudsApi.Services;

namespace SamaraCloudsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Get all products (search, filter by customer, date range)
        /// </summary>
        /// <param name="customerId">Customer ID (default 0 = all customers)</param>
        /// <param name="search">Search keyword (optional, product code/name)</param>
        /// <param name="dateFrom">Created date from (optional)</param>
        /// <param name="dateTo">Created date to (optional)</param>
        /// <returns>List of products</returns>
        [Authorize]
        [HttpGet("view-all")]
        public async Task<IActionResult> ViewAll(
            [FromQuery] int customerId = 0,
            [FromQuery] string? search = null,
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null)
        {
            try
            {
                var data = await _productService.ViewAllAsync(customerId, search, dateFrom, dateTo);
                return Ok(new
                {
                    success = true,
                    message = "Product list retrieved successfully.",
                    count = data.Count(),
                    data
                });
            }
            catch (Exception) // <-- Hilangkan "ex"
            {
                return StatusCode(500, new
                {
                    success = false,
                    error = "internal_error",
                    message = "An unexpected error occurred while fetching products."
                });
            }
        }
    }
}
