using Dapper;
using SamaraCloudsApi.Data;
using SamaraCloudsApi.Models;

namespace SamaraCloudsApi.Services
{
    /// <summary>
    /// Service implementation for product business logic (View Only, SP-based)
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly SqlConnectionFactory _connectionFactory;

        public ProductService(SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<ProductViewDto>> ViewAllAsync(
            int customerId = 0, 
            string? search = null, 
            DateTime? dateFrom = null, 
            DateTime? dateTo = null)
        {
            using var conn = _connectionFactory.CreateConnection();
            var param = new {
                customer_id = customerId,
                search,
                date_from = dateFrom,
                date_to = dateTo
            };
            var result = await conn.QueryAsync<ProductViewDto>(
                "sp_inv_product_view_all",
                param,
                commandType: System.Data.CommandType.StoredProcedure
            );
            return result;
        }
    }
}
