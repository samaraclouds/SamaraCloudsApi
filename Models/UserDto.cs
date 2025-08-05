namespace SamaraCloudsApi.Models
{
    public class UserDto
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public int? BranchId { get; set; }
        public string Username { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;      // WAJIB untuk hash check
        public string? Fullname { get; set; }
        public string? Email { get; set; }
        public bool? IsAdmin { get; set; }
        public bool? IsActive { get; set; }
        public string? CurrentSessionId { get; set; }
        public bool? IsSingleSessionEnabled { get; set; }
        public string? CurrentIp { get; set; }
        public string? CurrentUserAgent { get; set; }
        public string? UniqId { get; set; }
        public int? RoleId { get; set; }
        public string? RoleName { get; set; }
        public string? CustomerName { get; set; }
        public string? PackageName { get; set; }
        public DateTime? ExpiredDate { get; set; }
    }
}
