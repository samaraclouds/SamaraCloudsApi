using Microsoft.AspNetCore.Mvc;

namespace SamaraCloudsApi.Controllers
{
    /// <summary>
    /// Basic health check and info endpoint for SamaraClouds API.
    /// </summary>
    [ApiController]
    [Route("api/v1")]
    public class HomeController : ControllerBase
    {
        /// <summary>
        /// Returns API status and metadata.
        /// </summary>
        [HttpGet]
        [Route("")]
        public IActionResult GetApiStatus()
        {
            return Ok(new
            {
                status = "ok",
                app = "SamaraClouds API",
                version = "1.0.0", // Ubah jika perlu
                time = DateTime.UtcNow,
                env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
            });
        }

        /// <summary>
        /// Simple health check endpoint.
        /// </summary>
        [HttpGet]
        [Route("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }
    }
}
