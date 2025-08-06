using Microsoft.AspNetCore.Builder;

namespace SamaraCloudsApi.Extensions
{
    public static class DocumentationRedirectExtensions
    {
        public static void MapDocRedirects(this WebApplication app)
        {
            app.MapGet("/docs/auth/login", ctx =>
            {
                ctx.Response.Redirect("/docs/#post-/api/Auth/login");
                return Task.CompletedTask;
            });

            app.MapGet("/docs/auth/refresh-token", ctx =>
            {
                ctx.Response.Redirect("/docs/#post-/api/Auth/refresh-token");
                return Task.CompletedTask;
            });

            app.MapGet("/docs/auth/logout", ctx =>
            {
                ctx.Response.Redirect("/docs/#post-/api/Auth/logout");
                return Task.CompletedTask;
            });

            app.MapGet("/docs/auth/change-password", ctx =>
            {
                ctx.Response.Redirect("/docs/#post-/api/Auth/change-password");
                return Task.CompletedTask;
            });

            app.MapGet("/docs/finance/chart-of-account", ctx =>
            {
                ctx.Response.Redirect("/docs/#get-/api/ChartOfAccount/view-all");
                return Task.CompletedTask;
            });

            app.MapGet("/docs/inventory/product", ctx =>
            {
                ctx.Response.Redirect("/docs/#get-/api/Product/view-all");
                return Task.CompletedTask;
            });
        }
    }
}
