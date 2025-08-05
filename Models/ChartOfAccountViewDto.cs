namespace SamaraCloudsApi.Models
{
    public class ChartOfAccountViewDto
    {
        public int AccountId { get; set; }
        public int CustomerId { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; } = default!;
        public string AccountCode { get; set; } = default!;
        public string AccountName { get; set; } = default!;
        public string AccountType { get; set; } = default!;
        public string? AccountSubtype { get; set; }
        public string? ClassificationCode { get; set; }
        public int? ParentAccountId { get; set; }
        public int AccountLevel { get; set; }
        public char NormalBalance { get; set; }
        public string CurrencyCode { get; set; } = default!;
        public bool IsActive { get; set; }
        public bool AllowManualPosting { get; set; }
        public bool AllowBudgeting { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? Description { get; set; }
    }
}
