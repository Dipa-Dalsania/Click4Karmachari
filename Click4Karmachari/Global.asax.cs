using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace ClickKarmachari
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
          
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            HttpConfiguration config = GlobalConfiguration.Configuration;

            config.Formatters.JsonFormatter
                        .SerializerSettings
                        .ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;


        }

        //protected void Application_Error()
        //{
        //    var ex = Server.GetLastError();
        //    //Master_Log _log = new Master_Log();
        //    _log.addedon = DateTime.Now;
        //    if (Session["LOGIN_NAME"] == null)
        //        _log.addedby = "NO USER";
        //    else
        //        _log.addedby = Session["LOGIN_NAME"].ToString();
        //    _log.log_type = "APP_ERROR";
        //    _log.log_message = ex.Message.ToString();
        //    account5_invdbEntities db = new account5_invdbEntities();
        //    //db.Master_Log.Add(_log);
        //    db.SaveChangesAsync();
        //}
    }
}
