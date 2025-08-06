using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SamaraCloudsApi.Models;
using System.Collections;

namespace SamaraCloudsApi.Helpers
{
    public class ApiResponseWrapperFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Tidak perlu modifikasi sebelum action dieksekusi
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Result is ObjectResult objResult && objResult.Value != null)
            {
                // Jangan bungkus lagi jika sudah ApiResponse
                if (objResult.Value is ApiResponse<object> || objResult.Value.GetType().IsGenericType && objResult.Value.GetType().GetGenericTypeDefinition() == typeof(ApiResponse<>))
                    return;

                int? count = null;
                if (objResult.Value is ICollection collection)
                    count = collection.Count;

                var response = new ApiResponse<object>
                {
                    Status = objResult.StatusCode ?? 200,
                    Code = "SUCCESS",
                    Message = "Request processed successfully.",
                    Errors = null,
                    Data = objResult.Value,
                    Count = count
                };

                context.Result = new ObjectResult(response)
                {
                    StatusCode = objResult.StatusCode ?? 200
                };
            }
        }
    }
}
