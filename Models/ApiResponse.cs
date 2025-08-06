namespace SamaraCloudsApi.Models
{
    public class ApiResponse<T>
    {
        public int Status { get; set; }           // HTTP status code, misal 200
        public string Code { get; set; } = string.Empty;      // "SUCCESS", dll
        public string Message { get; set; } = string.Empty;   // Pesan sukses
        public object? Errors { get; set; }       // null jika sukses
        public T? Data { get; set; }
        public int? Count { get; set; }
    }

    public class ApiErrorResponse
    {
        public int Status { get; set; }           // HTTP status code, misal 404
        public string Code { get; set; } = string.Empty;      // "NOT_FOUND", dll
        public string Message { get; set; } = string.Empty;   // Pesan error
        public object? Errors { get; set; }       // Detail error
    }
}
