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

        [Authorize]
        [HttpGet("view-all")]
        public async Task<IActionResult> ViewAll(
            [FromQuery] int customerId = 0,
            [FromQuery] string? search = null,
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null)
        {
            var data = await _productService.ViewAllAsync(customerId, search, dateFrom, dateTo);

            // Karena sudah ada ApiResponseWrapperFilter global,
            // kita cukup return data mentahnya saja,
            // nanti filter akan bungkus response secara otomatis.
            return Ok(data);
        }
    }
}
