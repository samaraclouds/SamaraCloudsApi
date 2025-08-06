using Dapper;
using SamaraCloudsApi.Data;
using SamaraCloudsApi.Models;
using System.Data;

namespace SamaraCloudsApi.Services
{
    public class ChartOfAccountService : IChartOfAccountService
    {
        private readonly SqlConnectionFactory _connectionFactory;

        public ChartOfAccountService(SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<ChartOfAccountViewDto>> ViewAllAsync(
            int customerId,
            int? branchId = null,
            string? search = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null)
        {
            using var conn = _connectionFactory.CreateConnection();
            var param = new
            {
                customer_id = customerId,
                branch_id = branchId,
                search,
                date_from = dateFrom,
                date_to = dateTo
            };
            var result = await conn.QueryAsync<ChartOfAccountViewDto>(
                "sp_fin_chart_of_account_view_all",
                param,
                commandType: CommandType.StoredProcedure);
            return result;
        }

        public async Task<int> CreateAsync(ChartOfAccountCreateDto dto)
        {
            using var conn = _connectionFactory.CreateConnection();
            var param = new DynamicParameters();
            param.Add("customer_id", dto.CustomerId);
            param.Add("branch_id", dto.BranchId);
            param.Add("account_code", dto.AccountCode);
            param.Add("account_name", dto.AccountName);
            param.Add("account_type", dto.AccountType);
            param.Add("account_subtype", dto.AccountSubtype);
            param.Add("classification_code", dto.ClassificationCode);
            param.Add("parent_account_id", dto.ParentAccountId);
            param.Add("account_level", dto.AccountLevel);
            param.Add("normal_balance", dto.NormalBalance);
            param.Add("currency_code", dto.CurrencyCode);
            param.Add("is_active", dto.IsActive);
            param.Add("allow_manual_posting", dto.AllowManualPosting);
            param.Add("allow_budgeting", dto.AllowBudgeting);
            param.Add("created_by", dto.CreatedBy);
            param.Add("description", dto.Description);
            // Output parameter to get new account ID from SP
            param.Add("NewAccountId", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await conn.ExecuteAsync("sp_fin_chart_of_account_create", param, commandType: CommandType.StoredProcedure);

            return param.Get<int>("NewAccountId");
        }

        public async Task UpdateAsync(ChartOfAccountUpdateDto dto)
        {
            using var conn = _connectionFactory.CreateConnection();
            var param = new
            {
                account_id = dto.AccountId,
                customer_id = dto.CustomerId,
                branch_id = dto.BranchId,
                account_code = dto.AccountCode,
                account_name = dto.AccountName,
                account_type = dto.AccountType,
                account_subtype = dto.AccountSubtype,
                classification_code = dto.ClassificationCode,
                parent_account_id = dto.ParentAccountId,
                account_level = dto.AccountLevel,
                normal_balance = dto.NormalBalance,
                currency_code = dto.CurrencyCode,
                is_active = dto.IsActive,
                allow_manual_posting = dto.AllowManualPosting,
                allow_budgeting = dto.AllowBudgeting,
                updated_by = dto.UpdatedBy,
                description = dto.Description
            };

            await conn.ExecuteAsync("sp_fin_chart_of_account_update", param, commandType: CommandType.StoredProcedure);
        }

        public async Task DeleteAsync(int accountId, int deletedBy)
        {
            using var conn = _connectionFactory.CreateConnection();
            var param = new
            {
                account_id = accountId,
                deleted_by = deletedBy
            };

            await conn.ExecuteAsync("sp_fin_chart_of_account_delete", param, commandType: CommandType.StoredProcedure);
        }
    }
}
