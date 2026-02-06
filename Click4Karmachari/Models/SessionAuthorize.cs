using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClickKarmachari.Models
{
    public class CookieAdminAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            try
            {
                // Retrieve the cookies
                HttpCookie tokenCookie = httpContext.Request.Cookies["LOGGED_EMPLOYEE_TOKEN"];
                HttpCookie uidCookie = httpContext.Request.Cookies["LOGGED_EMPLOYEE_UID"];
                HttpCookie deviceAddressCookie = httpContext.Request.Cookies["LOGGED_EMPLOYEE_DEVICEADDRESS"];
                HttpCookie nameCookie = httpContext.Request.Cookies["LOGGED_EMPLOYEE_NAME"];

                if (tokenCookie == null || uidCookie == null || deviceAddressCookie == null || nameCookie == null)
                {
                    return false;
                }

                string token = tokenCookie.Value;
                string uid = uidCookie.Value;
                string deviceAddress = deviceAddressCookie.Value;
                string name = nameCookie.Value;

                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(deviceAddress) || string.IsNullOrEmpty(name))
                {
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectResult("~/Home/Signout");
        }
    }

    //public class SessionAdminAuthorizeAttribute : AuthorizeAttribute
    //{
    //    protected override bool AuthorizeCore(HttpContextBase httpContext)
    //    {
    //        try
    //        {
    //            UserMaster Login_Customer = (UserMaster)httpContext.Session["LOGIN_USER"];
    //            if (Login_Customer!=null) 
    //            {
    //                if (Login_Customer.User_type == 1)
    //                {
    //                    return true;
    //                } 
    //                else { return false; } 
    //            }
    //            else { return false; }

    //        }
    //        catch (Exception e)
    //        { return false; }
    //    }
    //    protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
    //    { filterContext.Result = new RedirectResult("/Home/Login?id=Session Timeout or Invalid authentication !"); }
    //}

    //public class SessionEmployeeAuthorizeAttribute : AuthorizeAttribute
    //{
    //    protected override bool AuthorizeCore(HttpContextBase httpContext)
    //    {
    //        try
    //        {
    //            UserMaster Login_Customer = (UserMaster)httpContext.Session["LOGIN_USER"];
    //            if (Login_Customer!=null)
    //            { 
    //                if (Login_Customer.User_type != 1)
    //                {
    //                    return true;
    //                }
    //                else 
    //                {
                        
    //                        return false;
                        
    //                }
    //            }
    //            return false;
    //        }
    //        catch (Exception e)
    //        { return false; }
    //    }
    //    protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
    //    { filterContext.Result = new RedirectResult("/Home/Login?id=Session Timeout or Invalid authentication !"); }
    //}


}