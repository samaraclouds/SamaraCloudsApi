namespace SamaraCloudsApi.Models
{
    public class ChartOfAccountCreateDto
    {
        public int CustomerId { get; set; }
        public int BranchId { get; set; }
        public string AccountCode { get; set; } = default!;
        public string AccountName { get; set; } = default!;
        public string AccountType { get; set; } = default!;
        public string? AccountSubtype { get; set; }
        public string? ClassificationCode { get; set; }
        public int? ParentAccountId { get; set; }
        public int AccountLevel { get; set; } = 1;
        public char NormalBalance { get; set; }
        public string CurrencyCode { get; set; } = "IDR";
        public bool IsActive { get; set; } = true;
        public bool AllowManualPosting { get; set; } = true;
        public bool AllowBudgeting { get; set; } = false;
        public int CreatedBy { get; set; }
        public string? Description { get; set; }
    }
}
