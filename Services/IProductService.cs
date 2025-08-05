using SamaraCloudsApi.Models;

namespace SamaraCloudsApi.Services
{
    /// <summary>
    /// Interface untuk operasi bisnis produk (khusus ViewAll, SP-based)
    /// </summary>
    public interface IProductService
    {
        /// <summary>
        /// Ambil semua produk, bisa difilter customer, search, dan rentang tanggal.
        /// </summary>
        /// <param name="customerId">Id customer (optional)</param>
        /// <param name="search">Keyword pencarian produk (optional)</param>
        /// <param name="dateFrom">Tanggal mulai (optional)</param>
        /// <param name="dateTo">Tanggal akhir (optional)</param>
        /// <returns>List ProductViewDto</returns>
        Task<IEnumerable<ProductViewDto>> ViewAllAsync(
            int customerId = 0,
            string? search = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null
        );
    }
}
