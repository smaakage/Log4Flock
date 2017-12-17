using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Log4Flock.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "StackTrace", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Default-Stacktrace",
                url: "StackTrace/{action}/{stackTrace}",
                defaults: new { controller = "StackTrace", action = "Index", stackTrace = UrlParameter.Optional }
            );
        }
    }
}
