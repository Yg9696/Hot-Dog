using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
namespace ShopProject.Filters
{
    public class CheckSessionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = filterContext.HttpContext.Session;
            if (session.GetString("CurrentAccount") == null)
            {
                // Redirect or handle as necessary
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                    { "controller", "Login" },
                    { "action", "Index" }
                    });
            }
            base.OnActionExecuting(filterContext);
        }
    }
}
