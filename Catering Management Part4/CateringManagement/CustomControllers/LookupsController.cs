using Microsoft.AspNetCore.Mvc.Filters;

namespace CateringManagement.CustomControllers
{
    public class LookupsController : CognizantController
    {
        //Have each controller for a "Lookup" value inherit from this
        //so the appropriate return URL is available
        //Note: This assumes that you have a controller named Lookup
        //which manages all lookup values in a tab controller.
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ViewData["returnURL"] = "/Lookup?Tab=" + ControllerName() + "-Tab";
            base.OnActionExecuting(context);
        }

        public override Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            ViewData["returnURL"] = "/Lookup?Tab=" + ControllerName() + "-Tab";
            return base.OnActionExecutionAsync(context, next);
        }
    }
}
