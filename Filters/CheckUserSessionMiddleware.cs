using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ShopProject.Filters
{
    public class CheckUserSessionMiddleware
    {
        private readonly RequestDelegate _next;

        public CheckUserSessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Session.GetString("UserID") == null
                && !httpContext.Request.Path.Value.StartsWith("/Login/Index"))
            {
                // Redirect if not logged in and not already on the login page
                httpContext.Response.Redirect("/Login/Index");
            }
            else
            {
                await _next(httpContext);
            }
        }
    }

    
}