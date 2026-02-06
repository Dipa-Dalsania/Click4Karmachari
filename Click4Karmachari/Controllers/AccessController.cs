using Antlr.Runtime.Misc;
using ClickKarmachari.Models;
using ClickKarmachari.Models;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Office2010.CustomUI;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.VariantTypes;
using DocumentFormat.OpenXml.Wordprocessing;
using iRely.Common;
using Microsoft.Ajax.Utilities;
using Microsoft.Exchange.WebServices.Data;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Org.BouncyCastle.Ocsp;
using Rotativa;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Validation;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using WebGrease.Activities;
using static System.Net.WebRequestMethods;

namespace ClickKarmachari.Controllers
{
    [CookieAdminAuthorizeAttribute]
    public class AccessController : Controller
    {
        private Prod_Satyamgroup_V1Entities db = new Prod_Satyamgroup_V1Entities();
        CommonClasses _comm = new CommonClasses();
        UserMaster _LogedUser = null;
        static String ImageKit_PUBLICURL = ConfigurationManager.AppSettings["ImageKit_URL"].ToString();
        static String ImageKit_Folder = ConfigurationManager.AppSettings["ImageKit_Folder"].ToString();
        protected override void OnActionExecuting(ActionExecutingContext ctx)
        {

            var routeData = HttpContext.Request.RequestContext.RouteData;
            string controller = routeData.Values["controller"].ToString();
            string action = routeData.Values["action"].ToString();

            

            if ((controller == "Access" && action == "Salarydetail")||(controller == "Access" && action == "DownloadSalarySlipAsPdf")||
                (controller == "Access" && action == "PrintSalarySlip")||(controller == "Access" && action == "ErrorPage")) 
            {
                return;
            }
            else
            {
                try
                {
                    String LOGGED_EMPLOYEE_TOKEN = HttpContext.Request.Cookies.Get("LOGGED_EMPLOYEE_TOKEN").Value.ToString();
                    String LOGGED_EMPLOYEE_UID = HttpContext.Request.Cookies.Get("LOGGED_EMPLOYEE_UID").Value.ToString();
                    String LOGGED_EMPLOYEE_DEVICEADDRESS = HttpContext.Request.Cookies.Get("LOGGED_EMPLOYEE_DEVICEADDRESS").Value.ToString();

                    Master_Token LoggedEmployeeToken = db.Master_Token.Where(x => x.Token == LOGGED_EMPLOYEE_TOKEN && x.UserMaster.User_UID == LOGGED_EMPLOYEE_UID && x.Device_Address == LOGGED_EMPLOYEE_DEVICEADDRESS).FirstOrDefault();
                    if (LoggedEmployeeToken == null)
                    {
                        ctx.Result = new RedirectToRouteResult(new System.Web.Routing.RouteValueDictionary(new { controller = "Home", action = "Signout" }));
                        return;
                    }
                    _LogedUser = LoggedEmployeeToken.UserMaster;
                    ViewBag._LogedUser = _LogedUser;
                    Company_Menu_Rights MenuRights = db.Company_Menu_Rights.Where(x => x.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
                    ViewBag.MenuRights = MenuRights;
                    User_Menu_Rights user_Menu_Rights = db.User_Menu_Rights.Where(x => x.User_ID == _LogedUser.User_ID).FirstOrDefault();
                    ViewBag.UserMenuRights = user_Menu_Rights;
                }
                catch
                {
                    ctx.Result = new RedirectToRouteResult(new System.Web.Routing.RouteValueDictionary(new { controller = "Home", action = "Signout" }));
                    return;
                }
            }
               
        }

        public class CaptchaResponse
        {
            [JsonProperty("Success")]
            public bool Success { get; set; }

            [JsonProperty("error-codes")]
            public bool ErrorMessage { get; set; }
        }

        public ActionResult Index()
        {
            return RedirectToAction("Dashboard");
        }
        public AccessController()
        {
            ViewBag.ImagekitDisplayURL = ImageKit_PUBLICURL + ImageKit_Folder;
        }
        public ActionResult Dashboard()
        {
            Admin_Dashboard_Result DashboardCounter = db.Admin_Dashboard((int)_LogedUser.Company_ID).FirstOrDefault();
            List<Event_Master> Allevent = db.Event_Master.Where(x => x.Company_ID == _LogedUser.Company_ID && (x.Date > DateTime.Now || x.Date == DateTime.Now)).OrderByDescending(x => x.Date).ToList();
            List<NoticeBoard_Master> Allnotice = db.NoticeBoard_Master.Where(x => x.Company_ID == _LogedUser.Company_ID && (x.Date > DateTime.Now || x.Date == DateTime.Now)).OrderByDescending(x => x.Date).ToList();
            List<UserMaster> AllEmployee = db.UserMasters.Where(x => x.Company_ID == _LogedUser.Company_ID & x.User_type != 1 && (x.status == 0 || x.status == 1)).ToList();
            List<UserMaster> Trainee = AllEmployee.Where(x => x.User_type == 5 && x.status == 0).OrderByDescending(x => x.User_ID).ToList();
            List<UserMaster> rEmp = AllEmployee.Where(x => x.User_type != 5 & x.status == 0).OrderByDescending(x => x.User_ID).ToList();
            List<Leave_Master> leave = db.Leave_Master.Where(x => x.Status == 0  && x.UserMaster.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.From_Date).Take(5).ToList();
            List<Request_Master> res = db.Request_Master.Where(x => x.UserMaster.Company_ID == _LogedUser.Company_ID && x.Status == 0).OrderByDescending(x => x.Req_Id).Take(5).ToList();
            List<Voucher_Master> voucher = db.Voucher_Master.Where(x => x.Status == 0 && x.UserMaster.Company_ID == _LogedUser.Company_ID).OrderBy(x => x.Status).ThenByDescending(x => x.Voucher_Id).Take(5).ToList();
            ViewBag.voucher = voucher;
            ViewBag.res = res;
            ViewBag.leaves = leave;
            ViewBag.DashboardCounter = DashboardCounter;
            ViewBag.Allevent = Allevent;
            ViewBag.Allnotice = Allnotice;
            ViewBag.AllEmp = AllEmployee;
            ViewBag.Trainee = Trainee;
            ViewBag.rEmp = rEmp;

            return View();
        }
        public ActionResult PrensentEmployees()
        {

            DateTime dt = DateTime.Now.Date;
            List<UserMaster> _PresentEmployees = db.UserMasters.Where(x => x.User_type != _LogedUser.User_type && x.Company_ID == _LogedUser.Company_ID).ToList();
            List<User_Punch> _todayspunch = db.User_Punch.Where(x => x.Time.Day == dt.Day && x.Time.Month == dt.Month && x.Time.Year == dt.Year && x.PunchType_Id == 1 && x.UserMaster.Company_ID == _LogedUser.Company_ID).ToList();
            List<UserMaster> user = new List<UserMaster>();
            foreach (User_Punch _attendance in _todayspunch)
            {
                foreach (UserMaster _user in _PresentEmployees)
                {
                    if (_attendance.User_Id == _user.User_ID && user.Where(x => x.User_ID == _user.User_ID).Count() <= 0)
                    {
                        user.Add(_user);
                    }
                }
            }
            ViewBag.user = user;
            return View("Employee_All");
        }
        public ActionResult User_Profile()
        {
            ViewBag.PageTitle = "Update Profile";

            List<City_Master> City = db.City_Master.ToList();
            List<Religion_Master> religion = db.Religion_Master.ToList();
            ViewBag.City = City;
            ViewBag.religion = religion;
            return View();
        }
        [HttpPost]
        public ActionResult User_Profile(FormCollection form, HttpPostedFileBase file)
        {

            var _newuser = db.UserMasters.Find(_LogedUser.User_ID);
            string User_Name = form["User_Name"].ToString();
            long User_Mobile = long.Parse(form["User_Mobile"].ToString());
            string PanCard_No = form["PanCard_No"].ToString();
            string User_Email = form["User_Email"].ToString();
            string City_Name = form["City_Name"].ToString();
            string Religion_name = form["Religion_name"].ToString();
            string Company_Address = form["Company_Address"].ToString();
            if (form["RefMobile"] == "")
            {
                _newuser.RefMobile = null;
            }
            else
            {
                long RefMobile = long.Parse(form["RefMobile"].ToString());
                _newuser.RefMobile = RefMobile;
            }
            if (form["Adharcard_No"] == "")
            {
                _newuser.Adharcard_No = null;
            }
            else
            {
                _newuser.Adharcard_No = long.Parse(form["Adharcard_No"].ToString());
            }
            if (file != null)
            {
                //var allowedExtensions = new[] { ".Jpg", ".JPG", ".png", ".jpg", ".jpeg" };
                //var fileName = Path.GetFileName(file.FileName);
                //var ext = Path.GetExtension(file.FileName);
                //if (!allowedExtensions.Contains(ext))
                //{
                //    TempData["failure"] = "Sorry Invalid Files.";
                //    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Sorry Invalid Files." });
                //}
                //string name = Path.GetFileNameWithoutExtension(fileName);
                //string myfile = name + "_" + DateTime.Now.Second.ToString() + ext;
                //var path = Path.Combine(Server.MapPath("~/Uploads/User_Image"), myfile);
                //file.SaveAs(path);
                //_newuser.image_path = path;
                //_newuser.image_name = myfile;

                string _Oldname = _newuser.image_name;
                CommonResponce result = new ImageKiT_ImageProcess().UploadFile("User_Image", file, _Oldname, 20);
                if (result.EnableError == true)
                {
                    _newuser.image_path = $"{ImageKit_PUBLICURL}/{ImageKit_Folder}/User_Image/" +result.ErrorMsg;
                    _newuser.image_name = result.ErrorMsg;
                }
                else
                {
                    if (Request.IsAjaxRequest()) { return this.Json(new { EnableError = false, ErrorTitle = "Failure", ErrorMsg = result.ErrorMsg }); }
                    else { TempData["Result"] = result.ErrorMsg; return RedirectToAction("Dashboard"); }
                }

            }
            _newuser.PanCard_No = PanCard_No;
            _newuser.User_Name = User_Name;
            _newuser.User_Mobile = User_Mobile;
            _newuser.User_Email = User_Email;
            _newuser.City_Name = City_Name;
            _newuser.Religion_name = Religion_name;
            Company_Master cmp = db.Company_Master.Where(x => x.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
            cmp.Company_Email = User_Email;
            cmp.Company_Address = Company_Address;
            db.SaveChanges();
            Session.Remove("LOGIN_USER");
            Session.Add("LOG_STATUS", "TRUE");
            Session.Add("LOGIN_USER", _newuser);
            Session.Add("LOGIN_NAME", _newuser.User_Name.ToString());
            //TempData["Result"] = "Profile Update Successfully.";
            return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Profile Update Successfully.", Refresh = "Dashboard" });

        }
        public ActionResult Company_Profile()
        {
            ViewBag.PageTitle = "Update Company Profile";

            Company_Master company = db.Company_Master.Where(x => x.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
            ViewBag.company = company;
            List<Company_Doc_Master> cmp = db.Company_Doc_Master.Where(x => x.Company_ID == _LogedUser.Company_ID & x.Status != 6).ToList();
            ViewBag.cmp = cmp;
            return View();
        }

        [HttpPost]
        public ActionResult Company_Profile(FormCollection form, HttpPostedFileBase file)
        {

            var _newuser = db.UserMasters.Find(_LogedUser.User_ID);
            long Company_ID = long.Parse(form["Company_ID"].ToString());
            string Company_Name = form["Company_Name"].ToString();
            string Company_Email = form["Company_Email"].ToString();
            string User_Name = form["User_Name"].ToString();
            string Company_Address = form["Company_Address"].ToString();
            long User_Mobile = long.Parse(form["User_Mobile"].ToString());
            Company_Master cmp = db.Company_Master.Where(x => x.Company_ID == Company_ID).FirstOrDefault();
            cmp.Company_Name = Company_Name;
            cmp.Company_Email = Company_Email;
            cmp.Company_Address = Company_Address;
            if (file != null)
            {

                //var allowedExtensions = new[] { ".Jpg", ".JPG", ".png", ".jpg", ".jpeg" };
                //var fileName = Path.GetFileName(file.FileName);
                //var ext = Path.GetExtension(file.FileName);
                //if (!allowedExtensions.Contains(ext))
                //{
                //    TempData["failure"] = "Sorry Invalid Files.";
                //    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Sorry Invalid Files.", Refresh = "Default" });
                //}
                //string name = Path.GetFileNameWithoutExtension(fileName);
                //string myfile = name + "_" + DateTime.Now.Second.ToString() + ext;
                //var path = Path.Combine(Server.MapPath("~/Uploads/User_Image"), myfile);
                //file.SaveAs(path);
                //cmp.Logo_URL = myfile;

                string _Oldname = cmp.Logo_URL;
                CommonResponce result = new ImageKiT_ImageProcess().UploadFile("User_Image", file, _Oldname, 20);
                if (result.EnableError == true)
                {
                    
                    cmp.Logo_URL = result.ErrorMsg;
                }
                else
                {
                    if (Request.IsAjaxRequest()) { return this.Json(new { EnableError = false, ErrorTitle = "Failure", ErrorMsg = result.ErrorMsg }); }
                    else { TempData["Result"] = result.ErrorMsg; return RedirectToAction("Dashboard"); }
                }

            }
            db.SaveChanges();
            Session.Remove("LOGIN_USER");
            Session.Add("LOG_STATUS", "TRUE");
            Session.Add("LOGIN_USER", _newuser);
            Session.Add("LOGIN_NAME", _newuser.User_Name.ToString());
            TempData["Result"] = "Company Profile Update Successfully.";
            return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Company Profile Update Successfully.", Refresh = "Default" });

        }
        [HttpPost]
        public ActionResult Company_Document_Add(FormCollection form, HttpPostedFileBase file)
        {

            long Doc_Id = long.Parse(form["Doc_Id"].ToString());
            string Doc_Name = form["Doc_Name"].ToString();
            string Cmd = form["Command"].ToString();
            if (Cmd == "Save")
            {
                if (Doc_Id == 0)
                {

                    Company_Doc_Master Doc = new Company_Doc_Master();
                    Doc.Company_ID = _LogedUser.Company_ID;
                    Doc.Doc_Name = Doc_Name;
                    Doc.Status = 0;
                    if (file != null)
                    {
                        //var allowedExtensions = new[] { ".Jpg", ".JPG", ".png", ".jpg", ".jpeg", ".pdf", ".PDF" };
                        //var fileName = Path.GetFileName(file.FileName);
                        //var ext = Path.GetExtension(file.FileName);
                        //if (!allowedExtensions.Contains(ext))
                        //{
                        //    TempData["failure"] = "Sorry, Invalid File ";
                        //    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Sorry, Invalid File ", Refresh = "Default" });
                        //}
                        //string name = Path.GetFileNameWithoutExtension(fileName);
                        //string myfile = name + "_" + DateTime.Now.ToString("ddMMyyyhhmmss") + ext;
                        //var path = Path.Combine(Server.MapPath("~/Uploads/User_Document"), myfile);
                        //file.SaveAs(path);
                        //Doc.Doc_Type = myfile;


                        string _Oldname = Doc.Doc_Type;
                        CommonResponce result = new ImageKiT_ImageProcess().UploadFile("User_Document", file, _Oldname, 20);
                        if (result.EnableError == true)
                        {

                            Doc.Doc_Type = result.ErrorMsg;
                        }
                        else
                        {
                            if (Request.IsAjaxRequest()) { return this.Json(new { EnableError = false, ErrorTitle = "Failure", ErrorMsg = result.ErrorMsg }); }
                            else { TempData["Result"] = result.ErrorMsg; return RedirectToAction("Dashboard"); }
                        }
                    }
                    else
                    {
                        TempData["failure"] = "Please upload File ";
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Please upload File ", Refresh = "Default" });
                    }
                    db.Company_Doc_Master.Add(Doc);
                    db.SaveChanges();
                    TempData["Result"] = "Company Document Added Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Company Document Added Successfully !", Refresh = "Default" });
                }
                else
                {
                    Company_Doc_Master Doc = db.Company_Doc_Master.Where(x => x.Cmp_Doc_Id == Doc_Id).FirstOrDefault();
                    if (Doc == null)
                    {
                        TempData["failure"] = "Sorry, Invalid Document ";
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Document !" });
                    }
                    if (Doc.Status == 1)
                    {
                        TempData["failure"] = "Sorry, Document Approved ";
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Sorry, Document Approved " });
                    }
                    Doc.Doc_Name = Doc_Name;
                    if (file != null)
                    {
                        //var allowedExtensions = new[] { ".Jpg", ".JPG", ".png", ".jpg", ".jpeg", ".pdf", ".PDF" };
                        //var fileName = Path.GetFileName(file.FileName);
                        //var ext = Path.GetExtension(file.FileName);
                        //if (!allowedExtensions.Contains(ext))
                        //{
                        //    TempData["failure"] = "Sorry, Invalid File ";
                        //    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Sorry, Invalid File " });
                        //}
                        //string name = Path.GetFileNameWithoutExtension(fileName);
                        //string myfile = name + "_" + DateTime.Now.ToString("ddMMyyyhhmmss") + ext;
                        //var path = Path.Combine(Server.MapPath("~/Uploads/User_Document"), myfile);
                        //file.SaveAs(path);
                        //Doc.Doc_Type = myfile;


                        string _Oldname = Doc.Doc_Type;
                        CommonResponce result = new ImageKiT_ImageProcess().UploadFile("User_Document", file, _Oldname, 20);
                        if (result.EnableError == true)
                        {

                            Doc.Doc_Type = result.ErrorMsg;
                        }
                        else
                        {
                            if (Request.IsAjaxRequest()) { return this.Json(new { EnableError = false, ErrorTitle = "Failure", ErrorMsg = result.ErrorMsg }); }
                            else { TempData["Result"] = result.ErrorMsg; return RedirectToAction("Dashboard"); }
                        }

                    }
                    //else
                    //{
                    //    TempData["failure"] = "Please upload File ";
                    //    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Please upload File " });
                    //}
                    Doc.Company_ID = _LogedUser.Company_ID;
                    db.SaveChanges();
                    TempData["Result"] = "Company Document Updated Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Company Document Updated Successfully !", Refresh = "Default" });
                }
            }
            else if (Cmd == "Delete")
            {
                Company_Doc_Master Doc = db.Company_Doc_Master.Where(x => x.Cmp_Doc_Id == Doc_Id).FirstOrDefault();
                if (Doc == null)
                {
                    TempData["failure"] = "Sorry, Invalid Document ";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Document !", Refresh = "Default" });
                }
                if (Doc.Status == 1)
                {
                    TempData["failure"] = "Sorry, Document Approved ";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Sorry, Document Approved " });
                }
                Doc.Status = 6;
                db.SaveChanges();
                TempData["failure"] = "Company Document Deleted Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Company Document Deleted Successfully !", Refresh = "Default" });
            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !" });
            }
        }
        public ActionResult Change_Password()
        {
            ViewBag.PageTitle = "Change Password";

            List<City_Master> City = db.City_Master.ToList();
            List<Religion_Master> religion = db.Religion_Master.ToList();
            ViewBag.City = City;
            ViewBag.religion = religion;
            return View();
        }
        [HttpPost]
        public ActionResult Change_Password(FormCollection form)
        {

            var _newuser = db.UserMasters.Find(_LogedUser.User_ID);
            //long uid = long.Parse(form["User_ID"].ToString());
            string Old_Password = form["Old_Password"].ToString();
            string New_Password = form["New_Password"].ToString();
            string Confirm_password = form["Confirm_password"].ToString();
            UserMaster _olduser = db.UserMasters.Where(x => x.User_Password == Old_Password && x.User_ID == _newuser.User_ID && x.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
            if (_olduser == null)
            {
                TempData["failure"] = "Old Password Not Match";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Old Password Not Match" });
            }
            if (New_Password != Confirm_password)
            {
                TempData["failure"] = "New Password Not Match Confirm Password";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "New Password Not Match Confirm Password" });
            }
            _olduser.User_Password = Confirm_password;
            db.SaveChanges();
            TempData["Result"] = "Password Changed Successfully.";
            return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Password Changed Successfully.", Refresh = "Dashboard" });

        }
        [HttpPost]
        public ActionResult Emp_Change_Password(FormCollection form)
        {

           // var _newuser = db.UserMasters.Find(_LogedUser.User_ID);
            long uid = long.Parse(form["User_ID"].ToString());
           
            string New_Password = form["New_Password"].ToString();
            string Confirm_password = form["Confirm_password"].ToString();
            UserMaster _olduser = db.UserMasters.Where(x =>  x.User_ID == uid && x.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
           
            if (New_Password != Confirm_password)
            {
                TempData["failure"] = "New Password Not Match Confirm Password";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "New Password Not Match Confirm Password" });
            }
            _olduser.User_Password = Confirm_password;
            db.SaveChanges();
            TempData["Result"] = "Password Changed Successfully.";
            return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Password Changed Successfully.", Refresh = "Dashboard" });

        }
        [HttpPost]
        public ActionResult Change_Emp_Pass(FormCollection form)
        {
            long Emp_Id = long.Parse(form["Emp_Id"].ToString());

            var _newuser = db.UserMasters.Find(Emp_Id);
            string Old_Password = form["Old_Password"].ToString();
            string New_Password = form["New_Password"].ToString();
            string Confirm_password = form["Confirm_password"].ToString();
            UserMaster _olduser = db.UserMasters.Where(x => x.User_Password == Old_Password).FirstOrDefault();
            if (_olduser == null)
            {
                TempData["failure"] = "Old Password Not Match";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Old Password Not Match" });
            }
            if (New_Password != Confirm_password)
            {
                TempData["failure"] = "New_Password Not Match Confirm_password ";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "New_Password Not Match Confirm_password  !" });
            }
            _newuser.User_Password = Confirm_password;
            db.SaveChanges();
            TempData["Result"] = "Password Changed Successfully.";
            return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Password Changed Successfully.", Refresh = "Employee_Update?User_id=" + Emp_Id });
        }

        public ActionResult Employee_All(int Sts = 99, int Usertype_id = 99, int head_id = 99, string searchname = "", string actionType = "search")
        {

            if (actionType == "download")
            {
                // Perform download logic here
                return DownloadEmployeeTemplate(Sts, Usertype_id, head_id, searchname, true);
            }

            long companyId = _LogedUser.Company_ID;

            // Set page metadata
            ViewBag.PageTitle = "All Employees";
            ViewBag.PageDescription = "All Employees Available in the Company";

            // Dropdown data
            ViewBag.U_Type = db.UserTypes.ToList();
            ViewBag.Heads = db.ProjectSalaryGroups.Where(x => x.Company_ID == companyId).ToList();

            // Build user query
            var userQuery = db.UserMasters
                              .Where(x => x.Company_ID == companyId && x.status != 6);

            if (Usertype_id != 99)
                userQuery = userQuery.Where(x => x.User_type == Usertype_id);

            if (Sts != 99)
            {
                userQuery = userQuery.Where(x => x.status == Sts);
            }
            else
            {
                userQuery = userQuery.Where(x => x.status == 0);
            }


            if (head_id != 99)
                userQuery = userQuery.Where(x => x.Project_SalaryGroupID == head_id);

            if (!string.IsNullOrWhiteSpace(searchname))
                userQuery = userQuery.Where(x => x.EmployeeID == searchname || x.User_Name.Contains(searchname));


            var userList = userQuery.ToList();

            // TempData message handling
            ViewBag.Result = TempData["Result"];
            ViewBag.Failure = TempData["Failure"];
            //TempData.Remove("Result");
            //TempData.Remove("failure");

            // Retain filter values in the ViewBag
            ViewBag.Sts = Sts;
            ViewBag.Usertype_id = Usertype_id;
            ViewBag.head_id = head_id;
            ViewBag.searchname = searchname;
            ViewBag.user = userList;

            return View();
        }


        [HttpPost]
        public ActionResult UploadEmployee(HttpPostedFileBase file)
        {
            try
            {
                if (file != null && file.ContentLength > 0)
                {
                    var departments = db.Department_Master
                        .Where(x => x.Company_ID == _LogedUser.Company_ID)
                        .ToDictionary(x => x.Department, x => x.Dept_ID);

                    var designations = db.Designation_Master
                        .Where(x => x.Company_ID == _LogedUser.Company_ID)
                        .ToDictionary(x => x.Designation, x => x.Desg_Id);

                    var reportingPersons = db.UserMasters
                        .Where(x => x.Company_ID == _LogedUser.Company_ID && x.User_type == 1)
                        .ToDictionary(x => x.User_Name, x => x.User_ID);

                    var salaryGroups = db.ProjectSalaryGroups
                        .Where(x => x.Company_ID == _LogedUser.Company_ID)
                        .ToDictionary(x => x.Project_Name, x => x.Project_SalaryGroupID);

                   

                    using (var workbook = new XLWorkbook(file.InputStream))
                    {
                        var worksheet = workbook.Worksheet(1);
                        var rows = worksheet.RowsUsed().Skip(1);
                        var errors = new List<string>();

                        foreach (var row in rows)
                        {
                            int col = 1;

                            var sno = row.Cell(col++).GetString();
                            if (sno == null || sno == "" || !Regex.IsMatch(sno, @"^\d+$"))
                            {
                                // Add comment + highlight
                                row.Cell(1).Style.Fill.BackgroundColor = XLColor.LightPink;
                                row.Cell(1).Value = $"Expected long Value, For New Employee Enter 0.";
                              
                                errors.Add($"Row {row.RowNumber()}: Srno is blank");
                            }
                            col = 6;
                            var SalaryGroup = row.Cell(col++).GetString();
                            if (SalaryGroup == null || SalaryGroup == "" || !salaryGroups.ContainsKey(SalaryGroup))
                            {

                                // Add comment + highlight
                                row.Cell(6).Style.Fill.BackgroundColor = XLColor.LightPink;
                                row.Cell(6).Value = $"SalaryGroup is required.";

                                errors.Add($"Row {row.RowNumber()}: SalaryGroup is blank");
                            }
                            col = 8;
                            var name = row.Cell(col++).GetString();
                            if (name == null || name == "" )
                            {
                                // Add comment + highlight
                                row.Cell(8).Style.Fill.BackgroundColor = XLColor.LightPink;
                                row.Cell(8).Value = $"Name is required.";

                                errors.Add($"Row {row.RowNumber()}: name is blank");
                            }
                            col = 9;
                            var mobile = row.Cell(col++).GetString();
                            if (mobile == null || mobile == "" || !Regex.IsMatch(mobile, @"^\d+$"))
                            {
                                // Add comment + highlight
                                row.Cell(9).Style.Fill.BackgroundColor = XLColor.LightPink;
                                row.Cell(9).Value = $"Expected long Value, For Mobile number is required.";

                                errors.Add($"Row {row.RowNumber()}: mobileno is blank");
                            }
                            col = 11;
                            var adharcard = row.Cell(col++).GetString();
                            if (adharcard == null || adharcard == "" || !Regex.IsMatch(adharcard, @"^\d+$"))
                            {
                                // Add comment + highlight
                                row.Cell(11).Style.Fill.BackgroundColor = XLColor.LightPink;
                                row.Cell(11).Value = $"Expected long Value, For adharcard number is required.";

                                errors.Add($"Row {row.RowNumber()}: adharcard is blank");
                            }
                            col = 14;
                            var bankname = row.Cell(col++).GetString();
                            if (bankname == null || bankname == "" )
                            {
                                // Add comment + highlight
                                row.Cell(14).Style.Fill.BackgroundColor = XLColor.LightPink;
                                row.Cell(14).Value = $"Bankname is required.";

                                errors.Add($"Row {row.RowNumber()}: bankname is blank");
                            }
                            col = 15;
                            var bankacno = row.Cell(col++).GetString();
                            if (bankacno == null || bankacno == "")
                            {
                                // Add comment + highlight
                                row.Cell(15).Style.Fill.BackgroundColor = XLColor.LightPink;
                                row.Cell(15).Value = $"BankAcNo is required.";

                                errors.Add($"Row {row.RowNumber()}: BankAcNo is blank");
                            }
                            col = 16;
                            var IFSC_No = row.Cell(col++).GetString();
                            if (IFSC_No == null || IFSC_No == "")
                            {
                                // Add comment + highlight
                                row.Cell(16).Style.Fill.BackgroundColor = XLColor.LightPink;
                                row.Cell(16).Value = $"IFSC_No is required.";

                                errors.Add($"Row {row.RowNumber()}: IFSC_No is blank");
                            }
                            col = 17;
                            var UAN_No = row.Cell(col++).GetString();
                            if (UAN_No == null || UAN_No == "")
                            {
                                // Add comment + highlight
                                row.Cell(17).Style.Fill.BackgroundColor = XLColor.LightPink;
                                row.Cell(17).Value = $"UAN_No is required.";

                                errors.Add($"Row {row.RowNumber()}: UAN_No is blank");
                            }
                        }
                        if (errors.Any())
                        {
                            using (var stream = new MemoryStream())
                            {
                                workbook.SaveAs(stream);
                                stream.Position = 0;
                                return File(stream.ToArray(),
                                   "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Validation_Errors.xlsx");
                            }
                           
                            
                            
                        }
                        else
                        {
                            foreach (var row in rows)
                            {
                                if (row.Cell(1).IsEmpty()) continue;

                                int col = 1;
                                long.TryParse(row.Cell(col++).GetString(), out long srNo);
                                UserMaster user = srNo != 0
                                    ? db.UserMasters.FirstOrDefault(x => x.User_ID == srNo && x.Company_ID == _LogedUser.Company_ID)
                                    : new UserMaster();

                                var dept = row.Cell(col++).GetString();
                                var desgn = row.Cell(col++).GetString();
                                var skilledTypeStr = row.Cell(col++).GetString();
                                var reporting = row.Cell(col++).GetString();
                                var salaryGroup = row.Cell(col++).GetString();


                                long deptid = departments.ContainsKey(dept) ? departments[dept] : 0;
                                if (deptid == 0)
                                {
                                    user.Depart_id = null;
                                }
                                else
                                {
                                    user.Depart_id = deptid;
                                }
                                long desid = designations.ContainsKey(desgn) ? designations[desgn] : 0;
                                if (desid == 0)
                                {
                                    user.Depart_id = null;
                                }
                                else
                                {
                                    user.Desig_id = desid;
                                }



                                if (skilledTypeStr.ToLower() == "skilled")
                                {
                                    user.SkilledType = 1;
                                }
                                else if (skilledTypeStr.ToLower() == "unskilled")
                                {
                                    user.SkilledType = 2;
                                }
                                else if (skilledTypeStr.ToLower() == "semi skilled")
                                {
                                    user.SkilledType = 3;
                                }

                                long repperson = reportingPersons.ContainsKey(reporting) ? reportingPersons[reporting] : 0;
                                if (repperson == 0)
                                {
                                    user.Reporting_Person = null;
                                }
                                else
                                {
                                    user.Reporting_Person = repperson;
                                }
                                long salarygrp = salaryGroups.ContainsKey(salaryGroup) ? salaryGroups[salaryGroup] : 0;
                                if (repperson == 0)
                                {
                                    user.Project_SalaryGroupID = null;
                                }
                                else
                                {
                                    user.Project_SalaryGroupID = salarygrp;
                                }



                                user.Project_SalaryGroupID = salaryGroups.ContainsKey(salaryGroup) ? salaryGroups[salaryGroup] : 0;

                                // Basic Info
                                user.EmployeeID = row.Cell(col++).GetString();
                                user.User_Name = row.Cell(col++).GetString();
                                user.User_Mobile = long.TryParse(row.Cell(col++).GetString(), out long mobile) ? mobile : 0;
                                user.City_Name = row.Cell(col++).GetString();
                                user.Adharcard_No = long.TryParse(row.Cell(col++).GetString(), out var adhar) ? adhar : (long?)null;
                                user.PanCard_No = row.Cell(col++).GetString();
                                user.User_Email = row.Cell(col++).GetString();

                                // Bank Details
                                user.Bank_Name = row.Cell(col++).GetString();
                                user.Bank_Acc_No = row.Cell(col++).GetString();
                                user.IFSC_No = row.Cell(col++).GetString();
                                user.UAN_No = row.Cell(col++).GetString();
                                user.ESIC_No = row.Cell(col++).GetString();
                                user.Religion_name = row.Cell(col++).GetString();

                                // Dates
                                user.DOB = DateTime.TryParse(row.Cell(col++).GetString(), out DateTime dob) ? dob : DateTime.Now;
                                user.Date_of_join = DateTime.TryParse(row.Cell(col++).GetString(), out DateTime doj) ? doj : DateTime.Now;
                                user.Blood_group = row.Cell(col++).GetString();

                                // Common
                                user.Profile_Lock = false;
                                user.User_Password = "Default@123";

                                // Salary Info
                                if (srNo == 0)
                                {
                                    user.Company_ID = _LogedUser.Company_ID;
                                    user.User_UID = Guid.NewGuid().ToString().Replace("-", "");
                                    user.status = 0;
                                    user.User_type = 30;

                                    var project = ReadProjectSalaryHead(row, col);
                                    project.Company_ID = _LogedUser.Company_ID;
                                    user.ProjectSalaryHeads.Add(project);


                                    UserMaster lastempid = db.UserMasters.OrderByDescending(x => x.User_ID).FirstOrDefault();
                                    if (lastempid != null)
                                    {
                                        string result = lastempid.EmployeeID.Replace("Emp_", "");
                                        long neweid = long.Parse(result.ToString()) + 1;
                                        user.EmployeeID = "Emp_" + neweid.ToString();
                                    }
                                    else
                                    {

                                        user.EmployeeID = "Emp_1";
                                    }
                                    db.UserMasters.Add(user);
                                   
                                    db.SaveChanges();
                                }
                                else
                                {
                                    var project = db.ProjectSalaryHeads.FirstOrDefault(x => x.User_ID == srNo);
                                    if (project != null)
                                    {
                                        UpdateProjectSalaryHead(project, row, col);
                                    }
                                    else
                                    {
                                        var tempproject = ReadProjectSalaryHead(row, col);
                                        project.Company_ID = _LogedUser.Company_ID;
                                        user.ProjectSalaryHeads.Add(tempproject);
                                    }
                                    db.SaveChanges();
                                }
                            }
                        }



                        TempData["Result"] = "Employee Uploaded Successfully!";
                        return RedirectToAction("Employee_All");

                        //return Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "User Uploaded Successfully!", Refresh = "Default" });
                    }
                }
                TempData["Failure"] = "File not found!";
                return RedirectToAction("Employee_All");
                //return Json(new { EnableError = true, ErrorTitle = "F", ToastMsgSuc = "File not found!", Refresh = "Default" });
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                    .SelectMany(x => x.ValidationErrors)
                    .Select(x => $"Property: {x.PropertyName} - Error: {x.ErrorMessage}")
                    .ToList();

                string fullError = string.Join("; ", errorMessages);

                // Log it or return it as part of your response
                return Json(new
                {
                    EnableError = true,
                    ErrorTitle = "Validation Failed",
                    ToastMsgSuc = fullError
                });
            }
            catch (Exception ex)
            {
                // Optionally log ex.Message
                TempData["Failure"] = "Something went wrong!";
                return RedirectToAction("Employee_All");
                //return Json(new { EnableError = true, ErrorTitle = "F", ToastMsgSuc = "Something went wrong!", Refresh = "Default" });
            }
        }

      
        private ProjectSalaryHead ReadProjectSalaryHead(IXLRow row, int startCol)
        {
            string basic = row.Cell(startCol++).GetString();
            if(basic=="" || basic==null)
            {
                basic = "0";
            }
            string hra = row.Cell(startCol++).GetString();
            if (hra == "" || hra == null)
            {
                hra = "0";
            }
            string da = row.Cell(startCol++).GetString();
            if (da == "" || da == null)
            {
                da = "0";
            }
            string ta = row.Cell(startCol++).GetString();
            if (ta == "" || ta == null)
            {
                ta = "0";
            }
            string bonus = row.Cell(startCol++).GetString();
            if (bonus == "" || bonus == null)
            {
                bonus = "0";
            }
            string leave = row.Cell(startCol++).GetString();
            if (leave == "" || leave == null)
            {
                leave = "0";
            }
            string otrate = row.Cell(startCol++).GetString();
            if (otrate == "" || otrate == null)
            {
                otrate = "0";
            }
            string ph = row.Cell(startCol++).GetString();
            if (ph == "" || ph == null)
            {
                ph = "0";
            }
            string sall = row.Cell(startCol++).GetString();
            if (sall == "" || sall == null)
            {
                sall = "0";
            }
            string oall = row.Cell(startCol++).GetString();
            if (oall == "" || oall == null)
            {
                oall = "0";
            }
            string pf = row.Cell(startCol++).GetString();
            if (pf == "" || pf == null)
            {
                pf = "0";
            }
            string pt = row.Cell(startCol++).GetString();
            if (pt == "" || pt == null)
            {
                pt = "0";
            }
            string esic = row.Cell(startCol++).GetString();
            if (esic == "" || esic == null)
            {
                esic = "0";
            }
            string canteen = row.Cell(startCol++).GetString();
            if (canteen == "" || canteen == null)
            {
                canteen = "0";
            }
            string hrad = row.Cell(startCol++).GetString();
            if (hrad == "" || hrad == null)
            {
                hrad = "0";
            }
            string travel = row.Cell(startCol++).GetString();
            if (travel == "" || travel == null)
            {
                travel = "0";
            }
            string advance = row.Cell(startCol++).GetString();
            if (advance == "" || advance == null)
            {
                advance = "0";
            }
            string uniform = row.Cell(startCol++).GetString();
            if (uniform == "" || uniform == null)
            {
                uniform = "0";
            }
            string welfare = row.Cell(startCol++).GetString();
            if (welfare == "" || welfare == null)
            {
                welfare = "0";
            }
            string other = row.Cell(startCol++).GetString();
            if (other == "" || other == null)
            {
                other = "0";
            }

            return new ProjectSalaryHead
            {
                BASIC = basic,
                HRA = hra,
                DA = da,
                TA = ta,
                BONUS = bonus,
                LEAVE = leave,
                OTRate = otrate,
                PH = ph,
                SPECIALALLOWANCE = sall,
                OTHERALLOWANCE = oall,
                PF = pf,
                PT = pt,
                ESIC = esic,
                CANTEEN = canteen,
                HRA_DEDUCT = hrad,
                TRAVEL = travel,
                ADVANCE = advance,
                Uniform = uniform,
                Welfare = welfare,
                Other = other
            };
        }

        private void UpdateProjectSalaryHead(ProjectSalaryHead project, IXLRow row, int startCol)
        {
            project.BASIC = row.Cell(startCol++).GetString();
            project.HRA = row.Cell(startCol++).GetString();
            project.DA = row.Cell(startCol++).GetString();
            project.TA = row.Cell(startCol++).GetString();
            project.BONUS = row.Cell(startCol++).GetString();
            project.LEAVE = row.Cell(startCol++).GetString();
            project.OTRate = row.Cell(startCol++).GetString();
            project.PH = row.Cell(startCol++).GetString();
            project.SPECIALALLOWANCE = row.Cell(startCol++).GetString();
            project.OTHERALLOWANCE = row.Cell(startCol++).GetString();
            project.PF = row.Cell(startCol++).GetString();
            project.PT = row.Cell(startCol++).GetString();
            project.ESIC = row.Cell(startCol++).GetString();
            project.CANTEEN = row.Cell(startCol++).GetString();
            project.HRA_DEDUCT = row.Cell(startCol++).GetString();
            project.TRAVEL = row.Cell(startCol++).GetString();
            project.ADVANCE = row.Cell(startCol++).GetString();
            project.Uniform = row.Cell(startCol++).GetString();
            project.Welfare = row.Cell(startCol++).GetString();
            project.Other = row.Cell(startCol++).GetString();
        }


        public ActionResult DownloadEmployeeTemplate(int Sts = 99, int Usertype_id = 99, int head_id = 99, string searchname = "", bool withdata = false)
        {
            var department = db.Department_Master.Where(x => x.Company_ID == _LogedUser.Company_ID).Select(x => x.Department).ToList();
            var designations = db.Designation_Master.Where(x => x.Company_ID == _LogedUser.Company_ID).Select(x => x.Designation).ToList();
            List<UserMaster> repo = db.UserMasters.Where(x => x.Company_ID == _LogedUser.Company_ID && x.User_type == 1).ToList();
            var reporting_person = repo.Select(x => x.User_Name).ToList();
            var Salarygroup = db.ProjectSalaryGroups.Where(x => x.Company_ID == _LogedUser.Company_ID).Select(x => x.Project_Name).ToList();

            List<string> skilledTypes = new List<string> { "Skilled", "UnSkilled", "Semi Skilled" };
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Employees");

            List<string> allColumns = new List<string>
            {
                "Sr.no",
                "Department",
                "Designation",
                "SkilledType",
                "ReportingPerson",
                "Salary Group",
                "EmployeeID",
                "EmployeeName",
                "Mobile",
                "City",
                "Adharcard_No",
                "PanCard_No",
                "Email",
                "Bank_Name",
                "BankAcNo.",
                "IFSC_No",
                "UAN_No",
                "ESIC_No",
                "Religion",
                "DateofBirth",
                "DateofJoin",
                "Blood_group",
                "BASIC",
                "HRA",
                "DA", "TA", "BONUS", "LEAVE",
                    "OTRate",  "PH", "SPECIALALLOWANCE", "OTHERALLOWANCE",
                   "PF", "PT", "ESIC", "CANTEEN", "HRA_DEDUCT", "TRAVEL", "ADVANCE",
                    "Uniform", "Welfare", "Other"
            };

            for (int i = 0; i < allColumns.Count; i++)
            {
                var cell = worksheet.Cell(1, i + 1);
                cell.Value = allColumns[i];
                cell.Style.Font.Bold = true;

                // Set background color by column group
                if (i == 0)
                    cell.Style.Fill.BackgroundColor = XLColor.Red;
                else if (i >= 1 && i <= 5)
                    cell.Style.Fill.BackgroundColor = XLColor.Yellow;
                else if (i >= 6 && i <= 21)
                    cell.Style.Fill.BackgroundColor = XLColor.LightGreen;
                else if (i >= 22 && i <= 42)
                    cell.Style.Fill.BackgroundColor = XLColor.LightBlue;
            }
            // Auto-adjust all columns based on the header content
            worksheet.Columns(1, allColumns.Count).AdjustToContents();
            var optionsSheet = workbook.Worksheets.Add("Options");



            if (Sts != 99 || Usertype_id != 99 || head_id != 99 || searchname != "")
            {
                // Build user query
                var userQuery = db.UserMasters
                                  .Where(x => x.Company_ID == _LogedUser.Company_ID && x.status != 6);

                if (Usertype_id != 99)
                    userQuery = userQuery.Where(x => x.User_type == Usertype_id);

                if (Sts != 99)
                    userQuery = userQuery.Where(x => x.status == Sts);

                if (head_id != 99)
                    userQuery = userQuery.Where(x => x.Project_SalaryGroupID == head_id);

                if (!string.IsNullOrWhiteSpace(searchname))
                    userQuery = userQuery.Where(x => x.User_Name.Contains(searchname));

                var userList = userQuery.ToList();

                int col = 1;
                int row = 2;
                foreach (var user in userList)
                {
                    col = 1;

                    worksheet.Cell(row, col++).Value = user.User_ID;
                    worksheet.Cell(row, col++).Value = user.Department_Master?.Department;
                    worksheet.Cell(row, col++).Value = user.Designation_Master?.Designation;
                    int skilledIndex = user.SkilledType.HasValue ? user.SkilledType.Value - 1 : -1;
                    string skilledLabel = (skilledIndex >= 0 && skilledIndex < skilledTypes.Count) ? skilledTypes[skilledIndex] : "";
                    worksheet.Cell(row, col++).Value = skilledLabel;

                    worksheet.Cell(row, col++).Value = (repo.FirstOrDefault(y => y.User_ID == user.Reporting_Person)?.User_Name) ?? "";
                    worksheet.Cell(row, col++).Value = user.ProjectSalaryGroup?.Project_Name;

                    worksheet.Cell(row, col++).Value = user.EmployeeID;
                    worksheet.Cell(row, col++).Value = user.User_Name;
                    worksheet.Cell(row, col++).Value = user.User_Mobile.ToString();
                    worksheet.Cell(row, col++).Value = user.City_Name;
                    worksheet.Cell(row, col++).Value = user.Adharcard_No?.ToString() ?? "";
                    worksheet.Cell(row, col++).Value = user.PanCard_No;
                    worksheet.Cell(row, col++).Value = user.User_Email;

                    worksheet.Cell(row, col++).Value = user.Bank_Name;
                    worksheet.Cell(row, col++).Value = user.Bank_Acc_No;
                    worksheet.Cell(row, col++).Value = user.IFSC_No;
                    worksheet.Cell(row, col++).Value = user.UAN_No;
                    worksheet.Cell(row, col++).Value = user.ESIC_No;
                    worksheet.Cell(row, col++).Value = user.Religion_name;
                    worksheet.Cell(row, col++).Value = user.DOB.ToString("dd-MM-yyyy");
                    worksheet.Cell(row, col++).Value = user.Date_of_join.ToString("dd-MM-yyyy");
                    worksheet.Cell(row, col++).Value = user.Blood_group;

                    // Placeholder Salary Components (adjust as per your salary structure or ProjectSalaryHeads)
                    worksheet.Cell(row, col++).Value = GetSalaryHead(user, "BASIC");
                    worksheet.Cell(row, col++).Value = GetSalaryHead(user, "HRA");
                    worksheet.Cell(row, col++).Value = GetSalaryHead(user, "DA");
                    worksheet.Cell(row, col++).Value = GetSalaryHead(user, "TA");
                    worksheet.Cell(row, col++).Value = GetSalaryHead(user, "BONUS");
                    worksheet.Cell(row, col++).Value = user.Total_Leave.ToString("0.00");
                    worksheet.Cell(row, col++).Value = GetSalaryHead(user, "OTRate");
                    worksheet.Cell(row, col++).Value = GetSalaryHead(user, "PH");
                    worksheet.Cell(row, col++).Value = GetSalaryHead(user, "SPECIALALLOWANCE");
                    worksheet.Cell(row, col++).Value = GetSalaryHead(user, "OTHERALLOWANCE");
                    worksheet.Cell(row, col++).Value = GetSalaryHead(user, "PF");
                    worksheet.Cell(row, col++).Value = GetSalaryHead(user, "PT");
                    worksheet.Cell(row, col++).Value = GetSalaryHead(user, "ESIC");
                    worksheet.Cell(row, col++).Value = GetSalaryHead(user, "CANTEEN");
                    worksheet.Cell(row, col++).Value = GetSalaryHead(user, "HRA_DEDUCT");
                    worksheet.Cell(row, col++).Value = GetSalaryHead(user, "TRAVEL");
                    worksheet.Cell(row, col++).Value = GetSalaryHead(user, "ADVANCE");
                    worksheet.Cell(row, col++).Value = GetSalaryHead(user, "Uniform");
                    worksheet.Cell(row, col++).Value = GetSalaryHead(user, "Welfare");
                    worksheet.Cell(row, col++).Value = GetSalaryHead(user, "Other");

                    row++;
                }
            }


            // Fill Department in Column 9 (I)
            for (int i = 0; i < department.Count; i++)
            {
                optionsSheet.Cell(i + 1, 2).Value = department[i]; // H1, H2, ...
            }

            // Fill Designations in Column 9 (I)
            for (int i = 0; i < designations.Count; i++)
            {
                optionsSheet.Cell(i + 1, 3).Value = designations[i]; // I1, I2, ...
            }

            // Fill SkilledType in Column 9 (I)
            for (int i = 0; i < skilledTypes.Count; i++)
            {
                optionsSheet.Cell(i + 1, 4).Value = skilledTypes[i]; // I1, I2, ...
            }

            // Fill reporting_person in Column 9 (I)
            for (int i = 0; i < reporting_person.Count; i++)
            {
                optionsSheet.Cell(i + 1, 5).Value = reporting_person[i]; // I1, I2, ...
            }

            // Fill reporting_person in Column 9 (I)
            for (int i = 0; i < Salarygroup.Count; i++)
            {
                optionsSheet.Cell(i + 1, 6).Value = Salarygroup[i]; // I1, I2, ...
            }


            if (department.Count > 0)
                workbook.NamedRanges.Add("DepartmentList", optionsSheet.Range(1, 2, department.Count, 2)); // H1:H{N}

            if (designations.Count > 0)
                workbook.NamedRanges.Add("DesignationList", optionsSheet.Range(1, 3, designations.Count, 3)); // I1:I{N}

            if (skilledTypes.Count > 0)
                workbook.NamedRanges.Add("SkillTypeList", optionsSheet.Range(1, 4, skilledTypes.Count, 4)); // I1:I{N}

            if (reporting_person.Count > 0)
                workbook.NamedRanges.Add("ReportingPersonList", optionsSheet.Range(1, 5, reporting_person.Count, 5)); // I1:I{N}

            if (Salarygroup.Count > 0)
                workbook.NamedRanges.Add("SalarygroupList", optionsSheet.Range(1, 6, Salarygroup.Count, 6)); // I1:I{N}



            var departmentValidator = worksheet.Range("B2:B9999").CreateDataValidation();
            departmentValidator.List("=DepartmentList");

            var designationValidator = worksheet.Range("C2:C9999").CreateDataValidation();
            designationValidator.List("=DesignationList");

            var skilledTypevalidator = worksheet.Range("D2:D9999").CreateDataValidation();
            skilledTypevalidator.List("=SkillTypeList");

            var reporting_personvalidator = worksheet.Range("E2:E9999").CreateDataValidation();
            reporting_personvalidator.List("=ReportingPersonList");

            var salarygroup_personvalidator = worksheet.Range("F2:F9999").CreateDataValidation();
            salarygroup_personvalidator.List("=SalarygroupList");
            optionsSheet.Hide();


            var memoryStream = new System.IO.MemoryStream();
            workbook.SaveAs(memoryStream);
            memoryStream.Seek(0, System.IO.SeekOrigin.Begin);

            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "EmployeeAdd.xlsx");
        }

        private decimal GetSalaryHead(UserMaster user, string componentName)
        {
            if (user.ProjectSalaryHeads != null && user.ProjectSalaryHeads.Count > 0)
            {
                var salaryHead = user.ProjectSalaryHeads.FirstOrDefault(); // or loop if needed

                if (salaryHead != null)
                {
                    try
                    {
                        var prop = typeof(ProjectSalaryHead).GetProperty(componentName);
                        if (prop != null)
                        {
                            string val = prop.GetValue(salaryHead) as string;
                            decimal result;
                            if (Decimal.TryParse(val, out result))
                                return result;
                        }
                    }
                    catch
                    {
                        // Log if needed
                    }
                }
            }

            return 0;
        }


        public ActionResult Employee_Update(long User_id)
        {
            UserMaster user = db.UserMasters.FirstOrDefault(x => x.User_ID == User_id);
            ViewBag._user = user;
            var companyId = _LogedUser.Company_ID;
            ViewBag.MenuRights = db.Company_Menu_Rights.FirstOrDefault(x => x.Company_ID == companyId);
            ViewBag.U_Type = db.UserTypes.ToList();
            ViewBag.Desg = db.Designation_Master.Where(x => x.Company_ID == companyId).ToList();
            ViewBag.Dept = db.Department_Master.Where(x => x.Company_ID == companyId).ToList();
            ViewBag.religion = db.Religion_Master.OrderByDescending(x => x.Religion_Id).ToList();
            ViewBag.user_detail = db.UserMasters
                                     .Where(x => x.Company_ID == companyId && x.status == 0 && x.User_type == 1)
                                     .ToList();
            ViewBag.AllSalaryGroups = db.ProjectSalaryGroups.Where(x => x.Company_ID == _LogedUser.Company_ID).ToList();
            ViewBag.city = db.City_Master.ToList();
            ViewBag.projectSalaryHead = db.ProjectSalaryHeads.FirstOrDefault(x => x.User_ID == User_id);
            if (ViewBag.projectSalaryHead is null)
            {
                ProjectSalaryHead tempsalaryhead = new ProjectSalaryHead();
                tempsalaryhead.User_ID = user.User_ID;
                tempsalaryhead.Company_ID = user.Company_ID;
                tempsalaryhead.DailyHours = 0;
                tempsalaryhead.OTRate = "0";
                tempsalaryhead.BASIC = "0";
                tempsalaryhead.HRA = "0";
                tempsalaryhead.DA = "0";
                tempsalaryhead.LEAVE = "0";
                tempsalaryhead.BONUS = "0";
                tempsalaryhead.TA = "0";
                tempsalaryhead.PH = "0";
                tempsalaryhead.SPECIALALLOWANCE = "0";
                tempsalaryhead.OTHERALLOWANCE = "0";
                tempsalaryhead.PF = "0";
                tempsalaryhead.PT = "0";
                tempsalaryhead.ESIC = "0";
                tempsalaryhead.CANTEEN = "0";
                tempsalaryhead.HRA_DEDUCT = "0";
                tempsalaryhead.TRAVEL = "0";
                tempsalaryhead.ADVANCE = "0";
                tempsalaryhead.Uniform = "0";
                tempsalaryhead.Welfare = "0";
                tempsalaryhead.Other = "0";
                db.ProjectSalaryHeads.Add(tempsalaryhead);
                db.SaveChanges();
                ViewBag.projectSalaryHead = db.ProjectSalaryHeads.FirstOrDefault(x => x.User_ID == User_id);
            }

            User_Menu_Rights userRights = db.User_Menu_Rights.FirstOrDefault(x => x.User_ID == User_id)
                             ?? new User_Menu_Rights
                             {
                                 Employees = false,
                                 Assets = false,
                                 Task = false,
                                 NoticeBoard = false,
                                 Attendance = false,
                                 Vouchers = false,
                                 Document_Generator = false,
                                 Document_Storage = false,
                                 Salary_Generator = false
                             };
            ViewBag.rights = userRights;
            return View();
        }


        [HttpPost]
        public ActionResult EmployeeSalaryHeadUpdate(FormCollection form)
        {
            long User_ID = long.Parse(form["User_ID"]);
            int DailyHours = int.Parse(form["DailyHours"]);
            string OTRate = form["OTRate"];
            string PHRate = form["PHRate"];
            string MinWagesPerDay = form["MinWagesPerDay"];
            string BASIC = form["BASIC"];
            string HRA = form["HRA"];
            string DA = form["DA"];
            string LEAVE = form["LEAVE"];
            string BONUS = form["BONUS"];
            string TA = form["TA"];
            string PH = form["PH"];
            string SPECIALALLOWANCE = form["SPECIALALLOWANCE"];
            string OTHERALLOWANCE = form["OTHERALLOWANCE"];
            string PF = form["PF"];
            string PT = form["PT"];
            string ESIC = form["ESIC"];
            string CANTEEN = form["CANTEEN"];
            string HRA_DEDUCT = form["HRA_DEDUCT"];
            string TRAVEL = form["TRAVEL"];
            string ADVANCE = form["ADVANCE"];
            string Uniform = form["Uniform"];
            string Welfare = form["Welfare"];
            string Other = form["Other"];
            string Cmd = form["Command"];
            ProjectSalaryHead project = db.ProjectSalaryHeads.FirstOrDefault(x => x.User_ID == User_ID);
            project.DailyHours = DailyHours;
            project.OTRate = OTRate;
            project.BASIC = BASIC;
            project.HRA = HRA;
            project.DA = DA;
            project.LEAVE = LEAVE;
            project.BONUS = BONUS;
            project.TA = TA;
            project.PH = PH;
            project.SPECIALALLOWANCE = SPECIALALLOWANCE;
            project.OTHERALLOWANCE = OTHERALLOWANCE;
            project.PF = PF;
            project.PT = PT;
            project.ESIC = ESIC;
            project.CANTEEN = CANTEEN;
            project.HRA_DEDUCT = HRA_DEDUCT;
            project.TRAVEL = TRAVEL;
            project.ADVANCE = ADVANCE;
            project.Uniform = Uniform;
            project.Welfare = Welfare;
            project.Other = Other;
            db.SaveChanges();
            return Json(new
            {
                EnableError = true,
                ErrorTitle = "S",
                ToastMsgSuc = "Project Designation updated successfully!",
                Refresh = "Default"
            });
        }







        [HttpPost]
        public ActionResult SaveUserRights(FormCollection form)
        {
            try
            {
                long User_Right_Id = long.Parse(form["User_Right_Id"].ToString());
                long User_ID = long.Parse(form["User_ID"].ToString());


                bool Dashboard = form["Dashboard"] == "on"; // Will be false if unchecked or null
                bool Employees = form["Employees"] == "on"; // Will be false if unchecked or null
                bool Assets = form["Assets"] == "on"; // Will be false if unchecked or null
                bool Task = form["Task"] == "on"; // Will be false if unchecked or null
                bool NoticeBoard = form["NoticeBoard"] == "on"; // Will be false if unchecked or null
                bool Attendance = form["Attendance"] == "on"; // Will be false if unchecked or null
                bool Vouchers = form["Vouchers"] == "on"; // Will be false if unchecked or null
                bool Salary_Generator = form["Salary_Generator"] == "on"; // Will be false if unchecked or null
                bool Document_Generator = form["Document_Generator"] == "on"; // Will be false if unchecked or null
                bool Document_Storage = form["Document_Storage"] == "on"; // Will be false if unchecked or null

                User_Menu_Rights dbRights = db.User_Menu_Rights
                   .FirstOrDefault(x => x.User_Right_Id == User_Right_Id && x.User_ID == User_ID);

                if (dbRights != null)
                {
                    dbRights.Employees = Employees ? true : false;
                    dbRights.Assets = Assets ? true : false;
                    dbRights.Task = Task ? true : false;
                    dbRights.NoticeBoard = NoticeBoard ? true : false;
                    dbRights.Attendance = Attendance ? true : false;
                    dbRights.Vouchers = Vouchers ? true : false;
                    dbRights.Salary_Generator = Salary_Generator ? true : false;
                    dbRights.Document_Generator = Document_Generator ? true : false;
                    dbRights.Document_Storage = Document_Storage ? true : false;
                }
                else
                {
                    User_Menu_Rights newuserrights = new User_Menu_Rights();
                    newuserrights.User_ID = User_ID;
                    newuserrights.Employees = Employees ? true : false;
                    newuserrights.Assets = Assets ? true : false;
                    newuserrights.Task = Task ? true : false;
                    newuserrights.NoticeBoard = NoticeBoard ? true : false;
                    newuserrights.Attendance = Attendance ? true : false;
                    newuserrights.Vouchers = Vouchers ? true : false;
                    newuserrights.Salary_Generator = Salary_Generator ? true : false;
                    newuserrights.Document_Generator = Document_Generator ? true : false;
                    newuserrights.Document_Storage = Document_Storage ? true : false;

                    db.User_Menu_Rights.Add(newuserrights);
                }
                db.SaveChanges();

                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Employee Updated Successfully !", Refresh = "Employee_Update?User_id=" + User_ID });
            }
            catch (Exception e)
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Sorry, Unknown Error Occured - " + e.Message.ToString() + ". Please Contact Administrator ! " });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Employee_Add(FormCollection form, HttpPostedFileBase file)
        {
            try
            {
                long user_id = long.Parse(form["User_ID"].ToString());
                if (user_id == 0)
                {
                    string User_Name = form["User_Name"].ToString();
                    long User_Type = long.Parse(form["user_Type"].ToString());
                    long User_Mobile = long.Parse(form["User_Mobile"].ToString());
                    string User_Email = form["User_Email"].ToString();
                    string User_Password = form["User_Password"].ToString();
                    UserMaster u_mobile = db.UserMasters.Where(x => x.User_Mobile == User_Mobile && x.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
                    if (u_mobile != null) { return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Mobile Number Already Registered !" }); }
                    UserMaster _newuser = new UserMaster();
                    _newuser.User_Mobile = User_Mobile;
                    _newuser.User_Name = User_Name;
                    if(User_Type==null || User_Type==0)
                    {
                        _newuser.User_type = 30;
                    }
                    else
                    {
                        _newuser.User_type = User_Type;
                    }
                        
                    _newuser.User_Email = User_Email;
                    _newuser.User_UID = Guid.NewGuid().ToString().Replace("-", "");
                    _newuser.Company_ID = _LogedUser.Company_ID;
                    _newuser.DOB = DateTime.Now;
                    _newuser.Date_of_join = DateTime.Now;
                    if(User_Password=="" || User_Password==null)
                    {
                        _newuser.User_Password = "Default@123";
                    }
                    else
                    {
                        _newuser.User_Password = User_Password;
                    }

                        _newuser.status = 2;

                    UserMaster lastempid = db.UserMasters.Where(x => x.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.User_ID).FirstOrDefault();
                    if(lastempid!=null)
                    {
                        string result = lastempid.EmployeeID.Replace("Emp_", "");
                        long neweid = long.Parse(result.ToString()) + 1;
                        _newuser.EmployeeID = "Emp_" + neweid.ToString();
                    }
                    else
                    {
                      
                        _newuser.EmployeeID = "Emp_1" ;
                    }

                        ProjectSalaryHead tempsalaryhead = new ProjectSalaryHead();
                    tempsalaryhead.Company_ID = _LogedUser.Company_ID;
                    tempsalaryhead.DailyHours = 0;
                    tempsalaryhead.OTRate = "0";
                    tempsalaryhead.BASIC = "0";
                    tempsalaryhead.HRA = "0";
                    tempsalaryhead.DA = "0";
                    tempsalaryhead.LEAVE = "0";
                    tempsalaryhead.BONUS = "0";
                    tempsalaryhead.TA = "0";
                    tempsalaryhead.PH = "0";
                    tempsalaryhead.SPECIALALLOWANCE = "0";
                    tempsalaryhead.OTHERALLOWANCE = "0";
                    tempsalaryhead.PF = "0";
                    tempsalaryhead.PT = "0";
                    tempsalaryhead.ESIC = "0";
                    tempsalaryhead.CANTEEN = "0";
                    tempsalaryhead.HRA_DEDUCT = "0";
                    tempsalaryhead.TRAVEL = "0";
                    tempsalaryhead.ADVANCE = "0";
                    tempsalaryhead.Uniform = "0";
                    tempsalaryhead.Welfare = "0";
                    tempsalaryhead.Other = "0";

                    User_Menu_Rights usremenurights = new User_Menu_Rights
                    {
                        Employees = false,
                        Assets = false,
                        Task = false,
                        NoticeBoard = false,
                        Attendance = false,
                        Vouchers = false,
                        Document_Generator = false,
                        Document_Storage = false,
                        Salary_Generator = false
                    };

                    _newuser.User_Menu_Rights.Add(usremenurights);
                    _newuser.ProjectSalaryHeads.Add(tempsalaryhead);
                    db.UserMasters.Add(_newuser);
                    db.SaveChanges();

                    ///Email Send.....................//
                    string sub = "You are succesfully Registered in " + _LogedUser.Company_Master.Company_Name;
                    string message = "Please Login and Join Satyam Group";
                    var subject = "Regarding Registration " + _LogedUser.Company_Master.Company_Name;
                    var body = "Dear Employee " + User_Name + ", <br/> You are succesfully Registered  Please Login : <br/>User NO: " + User_Mobile + "<br/>Password: " + User_Password + "<br/>  Subject: " + sub + "<br/>Message: " + message + "<br/>Thank You ! <br/> This is System Generated Mail....! ";
                    MailAddress fromAddress = new MailAddress(_LogedUser.Company_Master.Company_Email);
                    MailAddress toAddress = new MailAddress(User_Email);
                    Thread t1 = null;
                    t1 = new Thread(new ThreadStart(() => new CommonClasses().Email_Verify(fromAddress.ToString(), toAddress.ToString(), subject.ToString(), body.ToString())));
                    t1.Start();
                    TempData["Result"] = "User Added Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "User Added Successfully !", Refresh = "Employee_Update?User_id=" + _newuser.User_ID });
                }
                else
                {
                    UserMaster _updateuser = db.UserMasters.Find(user_id);
                    long User_Type = long.Parse(form["user_Type"].ToString());
                    long? Department = null; if (form["Department"] != "") { Department = long.Parse(form["Department"].ToString()); }
                    long? Designation = null; if (form["Designation"] != "") { Designation = long.Parse(form["Designation"].ToString()); }
                    long? RefMobile = null; if (form["RefMobile"] != "") { RefMobile = long.Parse(form["RefMobile"].ToString()); }
                    long? Adharcard_No = null; if (form["Adharcard_No"] != "") { Adharcard_No = long.Parse(form["Adharcard_No"].ToString()); }
                    long Mobile = long.Parse(form["Mobile"].ToString());
                    long? ProjectSalaryHead = long.Parse(form["ProjectSalaryHead"].ToString());

                    string User_Name = form["User_Name"].ToString();
                    string PanCard_No = form["PanCard_No"].ToString();
                    string City_Name = form["City_Name"].ToString();
                    string Religion_name = form["Religion_name"].ToString();
                    string Company_Email = form["Company_Email"].ToString();
                    string Blood_group = form["Blood_group"].ToString();
                    string Posted_At = form["Posted_At"].ToString();
                    string Team_name = form["Team_name"].ToString();
                    string Bank_Acc_No = form["Bank_Acc_No"].ToString();
                    string IFSC_No = form["IFSC_No"].ToString();
                    string Bank_Name = form["Bank_Name"].ToString();
                    long Reporting_Person = long.Parse(form["Reporting_Person"].ToString());

                    string UAN_No = form["UAN_No"].ToString();
                    string ESIC_No = form["ESIC_No"].ToString();
                    string User_Email = form["User_Email"].ToString();
                    int status = int.Parse(form["status"].ToString());
                    DateTime DOB = DateTime.Parse(form["DOB"].ToString());
                    DateTime Date_of_join = DateTime.Parse(form["Date_of_join"].ToString());
                    bool profile_status = int.Parse(form["Profile_Lock"].ToString()) == 1 ? true : false;
                    if (Mobile == RefMobile)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Please enter new mobile number emergency contact number should be diffrent!" });
                    }

                    if (file != null)
                    {
                        if (file.FileName.Length > 10)
                        {
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Sorry, File Name Is Too Long..!" });
                        }
                        else
                        {
                            var allowedExtensions = new[] { ".Jpg", ".JPG", ".png", ".jpg", ".jpeg" };
                            var fileName = Path.GetFileName(file.FileName);
                            var ext = Path.GetExtension(file.FileName);
                            if (!allowedExtensions.Contains(ext))
                            {
                                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Sorry, Invalid File " });
                            }
                            string name = Path.GetFileNameWithoutExtension(fileName);
                            string myfile = name + "_" + DateTime.Now.ToString("ddMMyyyhhmmss") + ext;
                            var path = Path.Combine(Server.MapPath("~/Uploads/User_Image"), myfile);
                            file.SaveAs(path);
                            _updateuser.image_name = myfile;
                        }

                    }
                    _updateuser.Adharcard_No = Adharcard_No;
                    _updateuser.RefMobile = RefMobile;
                    _updateuser.User_Email = User_Email;

                    _updateuser.PanCard_No = PanCard_No;
                    _updateuser.User_Name = User_Name;
                    _updateuser.User_type = User_Type;

                    _updateuser.City_Name = City_Name;
                    _updateuser.Religion_name = Religion_name;

                    _updateuser.User_Mobile = Mobile;

                    _updateuser.Company_Email = Company_Email;
                    _updateuser.Depart_id = Department;
                    _updateuser.Desig_id = Designation;
                    _updateuser.Team_name = Team_name;
                    _updateuser.Date_of_join = Date_of_join;
                    _updateuser.Posted_At = Posted_At;
                    _updateuser.Bank_Name = Bank_Name;
                    _updateuser.Bank_Acc_No = Bank_Acc_No;
                    _updateuser.IFSC_No = IFSC_No;
                    _updateuser.Blood_group = Blood_group;
                    _updateuser.DOB = DOB;
                    _updateuser.Reporting_Person = Reporting_Person;
                    // _updateuser.ProjectSalaryHead = ProjectSalaryHead;

                    _updateuser.UAN_No = UAN_No;
                    _updateuser.ESIC_No = ESIC_No;
                    _updateuser.status = status;
                    _updateuser.Profile_Lock = profile_status;
                    db.SaveChanges();
                    TempData["Result"] = "User Updated Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "User Updated Successfully !", Refresh = "Employee_Update?User_id=" + _updateuser.User_ID });
                }
            }
            catch (Exception e)
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Sorry, Unknown Error Occured - " + e.Message.ToString() + ". Please Contact Administrator ! " });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PersonalDetail_Add(FormCollection form, HttpPostedFileBase file)
        {
            try
            {

                long user_id = long.Parse(form["User_ID"].ToString());
                if (user_id == 0)
                {
                    string User_Name = form["User_Name"].ToString();
                    long User_Type = long.Parse(form["user_Type"].ToString());
                    long User_Mobile = long.Parse(form["User_Mobile"].ToString());
                    //long Mobile = long.Parse(form["Mobile"].ToString());
                    string User_Email = form["User_Email"].ToString();
                    string User_Password = form["User_Password"].ToString();
                    UserMaster u_mobile = db.UserMasters.Where(x => x.User_Mobile == User_Mobile && x.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
                    if (u_mobile != null) { return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Mobile Number Already Registered !" }); }
                    UserMaster _newuser = new UserMaster();
                    _newuser.User_Mobile = User_Mobile;
                    _newuser.User_Name = User_Name;
                    _newuser.User_type = User_Type;
                    _newuser.User_Email = User_Email;
                    _newuser.User_UID = Guid.NewGuid().ToString().Replace("-", "");
                    _newuser.Company_ID = _LogedUser.Company_ID;
                    _newuser.DOB = DateTime.Now;
                    _newuser.Date_of_join = DateTime.Now;
                    _newuser.User_Password = User_Password;
                    //_newuser.User_Mobile = Mobile;
                    _newuser.status = 2;
                    db.UserMasters.Add(_newuser);
                    db.SaveChanges();

                    ///Email Send.....................//
                    string sub = "You are succesfully Registered in " + _LogedUser.Company_Master.Company_Name;
                    string message = "Please Login and Join Satyam Group";
                    var subject = "Regarding Registration " + _LogedUser.Company_Master.Company_Name;
                    var body = "Dear Employee " + User_Name + ", <br/> You are succesfully Registered  Please Login : <br/>User NO: " + User_Mobile + "<br/>Password: " + User_Password + "<br/>  Subject: " + sub + "<br/>Message: " + message + "<br/>Thank You ! <br/> This is System Generated Mail....! ";
                    MailAddress fromAddress = new MailAddress(_LogedUser.Company_Master.Company_Email);
                    MailAddress toAddress = new MailAddress(User_Email);
                    Thread t1 = null;
                    t1 = new Thread(new ThreadStart(() => new CommonClasses().Email_Verify(fromAddress.ToString(), toAddress.ToString(), subject.ToString(), body.ToString())));
                    t1.Start();
                    TempData["Result"] = "Employee Added Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Employee Added Successfully !", Refresh = "Employee_Update?User_id=" + _newuser.User_ID });
                }
                else
                {
                    UserMaster _updateuser = db.UserMasters.Find(user_id);

                    long? RefMobile = null; if (form["RefMobile"] != "") { RefMobile = long.Parse(form["RefMobile"].ToString()); }
                    long? Adharcard_No = null; if (form["Adharcard_No"] != "") { Adharcard_No = long.Parse(form["Adharcard_No"].ToString()); }
                    long Mobile = long.Parse(form["Mobile"].ToString());
                    string User_Name = form["User_Name"].ToString();
                    string PanCard_No = form["PanCard_No"].ToString();
                    string City_Name = form["City_Name"].ToString();
                    string Religion_name = form["Religion_name"].ToString();
                    string Blood_group = form["Blood_group"].ToString();
                    string User_Email = form["User_Email"].ToString();
                    int status = int.Parse(form["status"].ToString());
                    DateTime DOB = DateTime.Parse(form["DOB"].ToString());
                    bool profile_status = int.Parse(form["Profile_Lock"].ToString()) == 1 ? true : false;
                    if (Mobile == RefMobile)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Please enter new mobile number emergency contact number should be diffrent!" });
                    }

                    if (file != null)
                    {
                        if (file.FileName.Length > 10)
                        {
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Sorry, File Name Is Too Long..!" });
                        }
                        else
                        {
                            //var allowedExtensions = new[] { ".Jpg", ".JPG", ".png", ".jpg", ".jpeg" };
                            //var fileName = Path.GetFileName(file.FileName);
                            //var ext = Path.GetExtension(file.FileName);
                            //if (!allowedExtensions.Contains(ext))
                            //{
                            //    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Sorry, Invalid File " });
                            //}
                            //string name = Path.GetFileNameWithoutExtension(fileName);
                            //string myfile = name + "_" + DateTime.Now.ToString("ddMMyyyhhmmss") + ext;
                            //var path = Path.Combine(Server.MapPath("~/Uploads/User_Image"), myfile);
                            //file.SaveAs(path);
                            //_updateuser.image_name = myfile;

                            string _Oldname = _updateuser.image_name;
                            CommonResponce result = new ImageKiT_ImageProcess().UploadFile("User_Image", file, _Oldname, 20);
                            if (result.EnableError == true)
                            {
                                _updateuser.image_path = $"{ImageKit_PUBLICURL}/{ImageKit_Folder}/User_Image/" + result.ErrorMsg;
                                _updateuser.image_name = result.ErrorMsg;
                            }
                            else
                            {
                                if (Request.IsAjaxRequest()) { return this.Json(new { EnableError = false, ErrorTitle = "Failure", ErrorMsg = result.ErrorMsg }); }
                                else { TempData["Result"] = result.ErrorMsg; return RedirectToAction("Dashboard"); }
                            }



                        }

                    }
                    _updateuser.User_Name = User_Name;
                    _updateuser.User_Mobile = Mobile;
                    _updateuser.RefMobile = RefMobile;
                    _updateuser.User_Email = User_Email;
                    _updateuser.DOB = DOB;
                    _updateuser.Blood_group = Blood_group;
                    _updateuser.Adharcard_No = Adharcard_No;
                    _updateuser.PanCard_No = PanCard_No;
                    _updateuser.City_Name = City_Name;
                    _updateuser.Religion_name = Religion_name;
                    _updateuser.status = status;
                    _updateuser.Profile_Lock = profile_status;

                    db.SaveChanges();
                    TempData["Result"] = "Employee Updated Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Employee Updated Successfully !", Refresh = "Employee_Update?User_id=" + _updateuser.User_ID });
                }
            }
            catch (Exception e)
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Sorry, Unknown Error Occured - " + e.Message.ToString() + ". Please Contact Administrator ! " });
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CompanyDetail_Add(FormCollection form, HttpPostedFileBase file)
        {
            try
            {

                long user_id = long.Parse(form["User_ID"].ToString());
                if (user_id == 0)
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "User not found !" });
                }
                else
                {
                    UserMaster _updateuser = db.UserMasters.Find(user_id);
                    long? Department = null; if (form["Department"] != "") { Department = long.Parse(form["Department"].ToString()); }
                    long? Designation = null; if (form["Designation"] != "") { Designation = long.Parse(form["Designation"].ToString()); }
                    long? Project_SalaryGroupID = null; if (form["Project_SalaryGroupID"] != "") { Project_SalaryGroupID = long.Parse(form["Project_SalaryGroupID"].ToString()); }

                    DateTime Date_of_join = DateTime.Parse(form["Date_of_join"].ToString());
                    long Reporting_Person = long.Parse(form["Reporting_Person"].ToString());
                    string Team_name = form["Team_name"].ToString();
                    string Posted_At = form["Posted_At"].ToString();
                    string Company_Email = form["Company_Email"].ToString();
                    long User_Type = long.Parse(form["user_Type"].ToString());
                    //long ProjectSalaryHead = long.Parse(form["ProjectSalaryHead"].ToString());
                    int SkilledType = int.Parse(form["SkilledType"].ToString());
                    //string EmployeeID = form["EmployeeID"].ToString();

                    //_updateuser.EmployeeID = EmployeeID;
                    _updateuser.Project_SalaryGroupID = Project_SalaryGroupID;
                    _updateuser.User_type = User_Type;
                    _updateuser.Company_Email = Company_Email;
                    _updateuser.Depart_id = Department;
                    _updateuser.Desig_id = Designation;
                    _updateuser.Project_SalaryGroupID = Project_SalaryGroupID;
                    _updateuser.Team_name = Team_name;
                    _updateuser.Date_of_join = Date_of_join;
                    _updateuser.Posted_At = Posted_At;
                    if(Reporting_Person==0)
                    {
                        _updateuser.Reporting_Person = null;
                    }
                    else
                    {
                        _updateuser.Reporting_Person = Reporting_Person;
                    }
                    if (SkilledType == 0)
                    {
                        _updateuser.SkilledType = null;
                    }
                    else
                    {
                        _updateuser.SkilledType = SkilledType;
                    }
                   
                    db.SaveChanges();
                    TempData["Result"] = "Company Detail Updated Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Company Detail Updated Successfully !", Refresh = "Employee_Update?User_id=" + _updateuser.User_ID });
                }
            }
            catch (Exception e)
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Sorry, Unknown Error Occured - " + e.Message.ToString() + ". Please Contact Administrator ! " });
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BankDetail_Add(FormCollection form, HttpPostedFileBase file)
        {
            try
            {
                long user_id = long.Parse(form["User_ID"].ToString());
                if (user_id == 0)
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "User not found !" });

                }
                else
                {
                    UserMaster _updateuser = db.UserMasters.Find(user_id);
                    string User_Name1 = form["User_Name1"].ToString();
                    string Bank_Acc_No = form["Bank_Acc_No"].ToString();
                    string IFSC_No = form["IFSC_No"].ToString();
                    string Bank_Name = form["Bank_Name"].ToString();
                    string UAN_No = form["UAN_No"].ToString();
                    string ESIC_No = form["ESIC_No"].ToString();
                    _updateuser.Bank_Name = Bank_Name;
                    _updateuser.User_Name = User_Name1;
                    _updateuser.Bank_Acc_No = Bank_Acc_No;
                    _updateuser.IFSC_No = IFSC_No;
                    _updateuser.UAN_No = UAN_No;
                    _updateuser.ESIC_No = ESIC_No;
                    db.SaveChanges();
                    TempData["Result"] = "Bankdetails Updated Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Bankdetails Updated Successfully !", Refresh = "Employee_Update?User_id=" + _updateuser.User_ID });
                }
            }
            catch (Exception e)
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Sorry, Unknown Error Occured - " + e.Message.ToString() + ". Please Contact Administrator ! " });
            }

        }


        [HttpPost]
        public ActionResult Employee_Delete(FormCollection form)
        {
            long User_Id = long.Parse(form["User_Id"].ToString());
            var _del = db.UserMasters.Find(User_Id);
            _del.Employee_Task_Master.Where(x => x.Status != 6).ToList();
            _del.Address_Master.Where(x => x.Status != 6).ToList();
            _del.Relation_Master.Where(x => x.Status != 6).ToList();
            _del.Experience_Master.Where(x => x.Status != 6).ToList();
            _del.Document_Master.Where(x => x.Status != 6).ToList();
            _del.Voucher_Master.Where(x => x.Status != 6).ToList();
            _del.Request_Master.Where(x => x.Status != 6).ToList();
            _del.Project_Task_Master.Where(x => x.Status != 6).ToList();
            _del.Resignation_Master.Where(x => x.Status != 6).ToList();
            _del.Leave_Master.Where(x => x.Status != 6).ToList();

            if (_del.Employee_Task_Master.Count > 0)
            {
                TempData["failure"] = " User Has Education Detail, Please Delete Education Detail First .";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = " User Has Education Detail, Please Delete Education Detail First .", Refresh = "Default" });
            }

            if (_del.Address_Master.Count > 0)
            {

                TempData["failure"] = " User Has Address Detail, Please Delete Address Detail First .";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = " User Has Address Detail, Please Delete Address Detail First .", Refresh = "Default" });
            }
            if (_del.Relation_Master.Count > 0)
            {
                TempData["failure"] = " User Has Relation Detail, Please Delete Relation Detail First .";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = " User Has Relation Detail, Please Delete Relation Detail First .", Refresh = "Default" });
            }
            if (_del.Experience_Master.Count > 0)
            {
                TempData["failure"] = " User Has Experience Detail, Please Delete Experience Detail First .";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = " User Has Experience Detail, Please Delete Experience Detail First .", Refresh = "Default" });
            }
            if (_del.Document_Master.Count > 0)
            {
                TempData["failure"] = " User Has Document Detail, Please Delete Document Detail First .";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = " User Has Document Detail, Please Delete Document Detail First .", Refresh = "Default" });
            }

            if (_del.Gift_Master.Count > 0)
            {
                TempData["failure"] = " User Gift Detail, Please Delete Gift Detail First .";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = " User Gift Detail, Please Delete Gift Detail First .", Refresh = "Default" });
            }
            if (_del.Voucher_Master.Count > 0)
            {
                TempData["failure"] = " User Has Voucher Detail, Please Delete It First .";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = " User Has Voucher Detail, Please Delete It First .", Refresh = "Default" });
            }
            //if (_del.Task_Master.Count > 0)
            //{
            //    TempData["failure"] = " User Has Task Detail, Please Delete It First .";
            //    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = " User Has Task Detail, Please Delete It First .", Refresh = "Default" });
            //}
            if (_del.User_Punch.Count > 0)
            {
                TempData["failure"] = " User Has User Punch , Please Delete It First .";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = " User Gift Detail, Please Delete Gift Detail First .", Refresh = "Default" });
            }
            if (_del.Request_Master.Count > 0)
            {
                TempData["failure"] = " User Has Request , Please Delete It First .";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = " User Has Request , Please Delete It First .", Refresh = "Default" });
            }
            if (_del.Attendance_Master.Count > 0)
            {
                TempData["failure"] = " User Has Attendance , Please Delete It First .";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = " User Has Attendance , Please Delete It First .", Refresh = "Default" });
            }
            if (_del.Imp_Doc_Master.Count > 0)
            {
                TempData["failure"] = " User Has Doc , Please Delete It First .";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = " User Has Doc , Please Delete It First .", Refresh = "Default" });
            }
            if (_del.Project_Assign_Master.Count > 0)
            {
                TempData["failure"] = " User Has ProjectAssign , Please Delete It First .";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = " User Has ProjectAssign , Please Delete It First .", Refresh = "Default" });
            }
            if (_del.Project_Task_Master.Count > 0)
            {
                TempData["failure"] = " User Has Project Task , Please Delete It First .";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = " User Has Project Task , Please Delete It First .", Refresh = "Default" });
            }
            if (_del.Transaction_Master.Count > 0)
            {
                TempData["failure"] = " User Has Transaction , Please Delete It First .";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = " User Has Transaction , Please Delete It First .", Refresh = "Default" });
            }
            if (_del.Resignation_Master.Count > 0)
            {
                TempData["failure"] = " User Has Resignation , Please Delete It First .";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = " User Has Resignation , Please Delete It First .", Refresh = "Default" });
            }
            if (_del.Asset_Movement.Count > 0)
            {
                TempData["failure"] = " User Has Asset Movement , Please Delete It First .";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = " User Has Asset Movement , Please Delete It First .", Refresh = "Default" });
            }
            if (_del.Leave_Master.Count > 0)
            {
                TempData["failure"] = " User Has Leave Master , Please Delete It First .";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = " User Has Leave Master , Please Delete It First .", Refresh = "Default" });
            }
            if (_del.Master_Token.Count > 0)
            {
                var _delToken = db.Master_Token.Find(User_Id);
                if (_delToken != null) { db.Master_Token.Remove(_delToken); }
            }
            db.UserMasters.Remove(_del);
            db.SaveChanges();
            TempData["failure"] = "User Deleted Successfully.";
            return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "User Deleted Successfully.", Refresh = "Employee_All" });
        }
        [HttpPost]
        public ActionResult Leave_Add(FormCollection form)
        {

            long User_Id = long.Parse(form["User_Id"].ToString());
            string reason = form["reason"].ToString();
            int year = DateTime.Now.Year;
            DateTime From_Date = DateTime.Parse(year + "/01/01 00:00:00 AM");
            DateTime To_Date = DateTime.Parse(year + "/12/31 00:00:00 AM");
            string Leave_Type = form["Leave_Type"].ToString();
            decimal Leave_Days = decimal.Parse(form["leave_days"].ToString());
            string Cmd = form["Command"].ToString();
            if (Cmd == "Save")
            {
                UserMaster user = db.UserMasters.Where(x => x.User_ID == User_Id).FirstOrDefault();
                Leave_Master Leave = new Leave_Master();
                Leave.User_Id = User_Id;
                Leave.From_Date = From_Date;
                Leave.To_Date = To_Date;
                Leave.Leave_Crdr = 1;
                Leave.Reason = reason;
                Leave.ReqDate = DateTime.Now;
                Leave.Leave_Type = Leave_Type;
                Leave.Status = 1;
                Leave.leave_days = Leave_Days;
                Leave.Add_By = _LogedUser.User_ID;
                db.Leave_Master.Add(Leave);
                if (Leave.Leave_Type == "PL")
                {
                    user.Total_PL = (decimal)(user.Total_PL + Leave.leave_days);
                    user.Total_Leave = user.Total_PL + user.Total_Leave;
                }
                if (Leave.Leave_Type == "CL")
                {
                    user.Total_CL = (decimal)(user.Total_CL + Leave.leave_days);
                    user.Total_Leave = user.Total_CL + user.Total_Leave;
                }
                if (Leave.Leave_Type == "EL")
                {
                    user.Total_EL = (decimal)(user.Total_EL + Leave.leave_days);
                    user.Total_Leave = user.Total_EL + user.Total_Leave;
                }
                user.Total_Leave = user.Total_PL + user.Total_CL + user.Total_EL;
                db.SaveChanges();
                TempData["Result"] = "Leave Added Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Leave Added Successfully !", Refresh = "Default" });
            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !" });
            }
        }
        [HttpPost]
        public ActionResult Address_Add(FormCollection form)
        {

            long Add_ID = long.Parse(form["Add_Address_Addid"].ToString());
            string Cmd = form["Command"].ToString();
            if (Cmd == "Save")
            {
                if (Add_ID == 0)
                {
                    long uid = long.Parse(form["Add_Address_Userid"].ToString());
                    string CityName = form["Add_Address_City"].ToString();
                    string address = form["Add_Address_address"].ToString();
                    Address_Master newrAdd = new Address_Master();
                    newrAdd.User_ID = uid;
                    newrAdd.Add_Address = address;
                    newrAdd.CityName = CityName;
                    newrAdd.Status = 1;
                    db.Address_Master.Add(newrAdd);
                    db.SaveChanges();
                    ViewBag.Tab = "Address";
                    TempData["Result"] = "Address Added Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Address Added Successfully!", Refresh = "Default" });
                }
                else
                {
                    Address_Master newAdd = db.Address_Master.Where(x => x.Add_ID == Add_ID).FirstOrDefault();
                    if (newAdd == null)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Address !", Refresh = "Default" });
                    }
                    string CityName = form["Add_Address_City"].ToString();
                    string address = form["Add_Address_address"].ToString();
                    newAdd.Add_Address = address;
                    newAdd.CityName = CityName;
                    db.SaveChanges();
                    TempData["Result"] = "Address Updated Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Address Updated Successfully!", Refresh = "Default" });
                }
            }
            else if (Cmd == "Approve")
            {
                Address_Master newAdd = db.Address_Master.Where(x => x.Add_ID == Add_ID).FirstOrDefault();
                if (newAdd == null)
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Address !" });
                }
                newAdd.Status = 1;
                db.SaveChanges();
                TempData["Result"] = "Address Approved Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Address Approved Successfully!", Refresh = "Default" });
            }
            else if (Cmd == "Rejected")
            {
                Address_Master newAdd = db.Address_Master.Where(x => x.Add_ID == Add_ID).FirstOrDefault();
                if (newAdd == null)
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Address !" });
                }
                newAdd.Status = 2;
                db.SaveChanges();
                TempData["failure"] = "Address Rejected Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Address Rejected Successfully!", Refresh = "Default" });
            }
            else if (Cmd == "Delete")
            {
                Address_Master newAdd = db.Address_Master.Where(x => x.Add_ID == Add_ID).FirstOrDefault();
                if (newAdd == null)
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Address !" });
                }
                newAdd.Status = 6;
                db.SaveChanges();
                TempData["failure"] = "Address Deleted Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Address Deleted Successfully!", Refresh = "Default" });
            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !" });
            }
        }
        [HttpPost]
        public ActionResult Relation_Add(FormCollection form)
        {

            long Relation_Id = long.Parse(form["Add_Relation_Relation_Id"].ToString());
            long User_id = long.Parse(form["Add_Relation_User_ID"].ToString());
            string Relation = form["Add_Relation_Relation_Name"].ToString();
            string Person_Name = form["Add_Relation_Person_Name"].ToString();
            long? Mobile_No = long.Parse(form["Add_Relation_Mobile_No"].ToString());
            string Cmd = form["Command"].ToString();
            if (Cmd == "Save")
            {
                if (Relation_Id == 0)
                {
                    UserMaster u_mobile = db.UserMasters.Where(x => x.User_Mobile == Mobile_No && x.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
                    if (u_mobile != null) { return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Mobile Number Already Registered !" }); }

                    Relation_Master newrel = new Relation_Master();
                    newrel.User_Id = User_id;
                    newrel.Relation_Name = Relation;
                    newrel.Person_Name = Person_Name;
                    newrel.Mobile_No = Mobile_No;
                    newrel.Status = 1;
                    db.Relation_Master.Add(newrel);
                    db.SaveChanges();
                    TempData["Result"] = "Relation Approved Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Relation Added Successfully!", Refresh = "Default" });
                }
                else
                {
                    UserMaster u_mobile = db.UserMasters.Where(x => x.User_Mobile == Mobile_No && x.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
                    if (u_mobile != null) { return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Mobile Number Already Registered !" }); }

                    Relation_Master newrel = db.Relation_Master.Where(x => x.Relation_Id == Relation_Id).FirstOrDefault();
                    if (newrel == null)
                    {
                        TempData["failure"] = "Relation Approved Successfully !";
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Relation !", Refresh = "Default" });
                    }
                    newrel.User_Id = User_id;
                    newrel.Relation_Name = Relation;
                    newrel.Person_Name = Person_Name;
                    newrel.Mobile_No = Mobile_No;
                    db.SaveChanges();
                    TempData["Result"] = "Relation Updated Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Relation Updated Successfully!", Refresh = "Default" });
                }
            }
            else if (Cmd == "Approve")
            {
                Relation_Master newrel = db.Relation_Master.Where(x => x.Relation_Id == Relation_Id).FirstOrDefault();
                if (newrel == null)
                {
                    TempData["failure"] = "Invalid Realtion !";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Realtion !", Refresh = "Default" });
                }
                newrel.Status = 1;
                db.SaveChanges();
                TempData["Result"] = "Relation Approved Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Relation Approved Successfully!", Refresh = "Default" });
            }
            else if (Cmd == "Rejected")
            {
                Relation_Master newrel = db.Relation_Master.Where(x => x.Relation_Id == Relation_Id).FirstOrDefault();
                if (newrel == null)
                {
                    TempData["failure"] = "Invalid Realtion !";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Relation !", Refresh = "Default" });
                }
                newrel.Status = 2;
                db.SaveChanges();
                TempData["failure"] = "Relation Rejected Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Relation Rejected Successfully!", Refresh = "Default" });
            }
            else if (Cmd == "Delete")
            {
                Relation_Master newrel = db.Relation_Master.Where(x => x.Relation_Id == Relation_Id).FirstOrDefault();
                if (newrel == null)
                {
                    TempData["failure"] = "Invalid Realtion !";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Relation !", Refresh = "Default" });
                }
                newrel.Status = 6;
                db.SaveChanges();
                TempData["failure"] = "Relation Deleted Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Relation Deleted Successfully!", Refresh = "Default" });
            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !" });
            }
        }
        [HttpPost]
        public ActionResult Eduation_Add(FormCollection form)
        {


            string Cmd = form["Command"].ToString();
            long Edu_Id = long.Parse(form["Add_Education_Edu_Id"].ToString());
            long User_Id = long.Parse(form["Add_Education_User_ID"].ToString());
            string Edu_Name = form["Add_Education_Edu_Name"].ToString();
            string university = form["Add_Education_university"].ToString();
            string Pass_Out = form["Add_Education_Pass_Out"].ToString();
            string Field = form["Field"].ToString();
            if (Cmd == "Save")
            {
                if (Edu_Id == 0)
                {
                    Education_Master Edu = new Education_Master();
                    Edu.User_Id = User_Id;
                    Edu.Edu_Name = Edu_Name;
                    Edu.Field = Field;
                    Edu.university = university;
                    Edu.Pass_Out = Pass_Out;
                    Edu.Status = 1;
                    db.Education_Master.Add(Edu);
                    db.SaveChanges();
                    TempData["Result"] = "Education Added Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Education Added Successfully!", Refresh = "Default" });
                }
                else
                {
                    Education_Master Eduu = db.Education_Master.Where(x => x.Edu_Id == Edu_Id).FirstOrDefault();
                    if (Eduu == null)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Education !" });
                    }
                    Eduu.User_Id = User_Id;
                    Eduu.Edu_Name = Edu_Name;
                    Eduu.Field = Field;
                    Eduu.university = university;
                    Eduu.Pass_Out = Pass_Out;
                    db.SaveChanges();
                    TempData["Result"] = "Education Updated Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Education Updated Successfully!", Refresh = "Default" });
                }
            }
            else if (Cmd == "Approve")
            {
                Education_Master Edu = db.Education_Master.Where(x => x.Edu_Id == Edu_Id).FirstOrDefault();
                if (Edu == null)
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Education !" });
                }
                Edu.Status = 1;
                db.SaveChanges();
                TempData["Result"] = "Education Approved Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Education Approved Successfully!", Refresh = "Default" });
            }
            else if (Cmd == "Rejected")
            {
                Education_Master Edu = db.Education_Master.Where(x => x.Edu_Id == Edu_Id).FirstOrDefault();
                if (Edu == null)
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Education !" });
                }
                Edu.Status = 2;
                db.SaveChanges();
                TempData["failure"] = "Education Rejected Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Education Rejected Successfully!", Refresh = "Default" });
            }
            else if (Cmd == "Delete")
            {
                Education_Master Edu = db.Education_Master.Where(x => x.Edu_Id == Edu_Id).FirstOrDefault();
                if (Edu == null)
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Education !" });
                }
                Edu.Status = 6;
                db.SaveChanges();
                TempData["failure"] = "Education Deleted Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Education Deleted Successfully!", Refresh = "Default" });
            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !" });
            }
        }
        [HttpPost]
        public ActionResult Experience_Add(FormCollection form)
        {

            long Exp_Id = long.Parse(form["Add_Experience_Edu_Id"].ToString());
            long User_Id = long.Parse(form["Add_Experience_User_ID"].ToString());
            string Last_Company = form["Add_Experience_Last_Company"].ToString();
            string working_years = form["Add_Experience_working_years"].ToString();
            string Last_Year = form["Add_Experience_Last_Year"].ToString();
            string Cmd = form["Command"].ToString();
            if (Cmd == "Save")
            {
                if (Exp_Id == 0)
                {
                    Experience_Master Exp = new Experience_Master();
                    Exp.User_Id = User_Id;
                    Exp.Last_Company = Last_Company;
                    Exp.working_years = working_years;
                    Exp.Last_Year = Last_Year;
                    Exp.Status = 1;
                    db.Experience_Master.Add(Exp);
                    db.SaveChanges();
                    TempData["Result"] = "Experience Added Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Experience Added Successfully!", Refresh = "Default" });
                }
                else
                {
                    Experience_Master Exp = db.Experience_Master.Where(x => x.Exp_Id == Exp_Id).FirstOrDefault();
                    if (Exp == null)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Experience !" });
                    }
                    Exp.User_Id = User_Id;
                    Exp.Last_Company = Last_Company;
                    Exp.working_years = working_years;
                    Exp.Last_Year = Last_Year;
                    db.SaveChanges();
                    TempData["Result"] = "Experience Updated Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Experience Updated Successfully!", Refresh = "Default" });
                }
            }
            else if (Cmd == "Approve")
            {
                Experience_Master Exp = db.Experience_Master.Where(x => x.Exp_Id == Exp_Id).FirstOrDefault();
                if (Exp == null)
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Experience !" });
                }
                Exp.Status = 1;
                db.SaveChanges();
                TempData["Result"] = "Experience Approved Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Experience Approved Successfully!", Refresh = "Default" });
            }
            else if (Cmd == "Rejected")
            {
                Experience_Master Exp = db.Experience_Master.Where(x => x.Exp_Id == Exp_Id).FirstOrDefault();
                if (Exp == null)
                {

                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Experience !" });
                }
                Exp.Status = 2;
                db.SaveChanges();
                TempData["failure"] = "Experience Rejected Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Experience Rejected Successfully!", Refresh = "Default" });
            }
            else if (Cmd == "Delete")
            {
                Experience_Master Exp = db.Experience_Master.Where(x => x.Exp_Id == Exp_Id).FirstOrDefault();
                if (Exp == null)
                {

                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Experience !" });
                }
                Exp.Status = 6;
                db.SaveChanges();
                TempData["failure"] = "Experience Deleted Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Experience Deleted Successfully!", Refresh = "Default" });
            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !" });
            }
        }
        [HttpPost]
        public ActionResult Document_Add(FormCollection form, HttpPostedFileBase file)
        {

            long Doc_Id = long.Parse(form["Add_Document_Doc_Id"].ToString());
            long User_Id = long.Parse(form["Add_Document_User_ID"].ToString());
            string Doc_Name = form["Add_Document_Doc_Name"].ToString();
            string Cmd = form["Command"].ToString();
            if (Cmd == "Save")
            {
                if (Doc_Id == 0)
                {
                    Document_Master dm = db.Document_Master.Where(x => x.User_Id == _LogedUser.User_ID && x.Doc_Name == Doc_Name).FirstOrDefault();
                    if (dm == null)
                    {
                       
                        Document_Master Doc = new Document_Master();
                        Doc.User_Id = User_Id;
                        Doc.Doc_Name = Doc_Name;
                        Doc.Status = 1;
                        if (file != null)
                        {
                            var allowedExtensions = new[] { ".Jpg", ".JPG", ".png", ".jpg", ".jpeg", ".pdf", ".PDF" };
                            var fileName = Path.GetFileName(file.FileName);
                            var ext = Path.GetExtension(file.FileName);
                            if (!allowedExtensions.Contains(ext))
                            {
                                TempData["failure"] = "Sorry, Invalid File ";
                                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Sorry, Invalid File ", Refresh = "Default" });
                            }
                            string name = Path.GetFileNameWithoutExtension(fileName);
                            string myfile = name + "_" + DateTime.Now.ToString("ddMMyyyhhmmss") + ext;
                            var path = Path.Combine(Server.MapPath("~/Uploads/User_Image"), myfile);
                            file.SaveAs(path);
                            Doc.Doc_Path = path;
                            Doc.Doc_Type = myfile;
                        }
                        else
                        {
                            TempData["failure"] = "Please Upload File ";
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Please Upload File " });
                        }
                        Doc.Company_ID = _LogedUser.Company_ID;
                        db.Document_Master.Add(Doc);
                        db.SaveChanges();
                    }
                    else
                    {
                        if (file != null)
                        {
                            var allowedExtensions = new[] { ".Jpg", ".JPG", ".png", ".jpg", ".jpeg", ".pdf", ".PDF" };
                            var fileName = Path.GetFileName(file.FileName);
                            var ext = Path.GetExtension(file.FileName);
                            if (!allowedExtensions.Contains(ext))
                            {
                                TempData["failure"] = "Sorry, Invalid File ";
                                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Sorry, Invalid File ", Refresh = "Default" });
                            }
                            string name = Path.GetFileNameWithoutExtension(fileName);
                            string myfile = name + "_" + DateTime.Now.ToString("ddMMyyyhhmmss") + ext;
                            var path = Path.Combine(Server.MapPath("~/Uploads/User_Image"), myfile);
                            file.SaveAs(path);
                            dm.Doc_Path = path;
                            dm.Doc_Type = myfile;
                        }
                        db.SaveChanges();
                    }
                  
                    TempData["Result"] = "Document Added Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Document Added Successfully!", Refresh = "Default" });
                }
                else
                {
                    Document_Master Doc = db.Document_Master.Where(x => x.Doc_Id == Doc_Id).FirstOrDefault();
                    if (Doc == null)
                    {
                        TempData["failure"] = "Sorry, Invalid Document ";
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Document !", Refresh = "Default" });
                    }
                    Doc.User_Id = User_Id;
                    Doc.Doc_Name = Doc_Name;
                    Doc.Status = 1;
                    if (file != null)
                    {
                        var allowedExtensions = new[] { ".Jpg", ".JPG", ".png", ".jpg", ".jpeg", ".pdf", ".PDF" };
                        var fileName = Path.GetFileName(file.FileName);
                        var ext = Path.GetExtension(file.FileName);
                        if (!allowedExtensions.Contains(ext))
                        {
                            TempData["failure"] = "Sorry, Invalid File ";
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Sorry, Invalid File ", Refresh = "Default" });
                        }
                        string name = Path.GetFileNameWithoutExtension(fileName);
                        string myfile = name + "_" + DateTime.Now.ToString("ddMMyyyhhmmss") + ext;
                        var path = Path.Combine(Server.MapPath("~/Uploads/User_Image"), myfile);
                        file.SaveAs(path);
                        Doc.Doc_Path = path;
                        Doc.Doc_Type = myfile;
                    }
                    else
                    {
                        //TempData["failure"] = "Please Upload File ";
                        //return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Please Upload File ", Refresh = "Default" });
                    }
                    Doc.Company_ID = _LogedUser.Company_ID;
                    db.SaveChanges();
                    TempData["Result"] = "Document Updated Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Document Updated Successfully!", Refresh = "Default" });
                }
            }
            else if (Cmd == "Approve")
            {
                Document_Master Doc = db.Document_Master.Where(x => x.Doc_Id == Doc_Id).FirstOrDefault();
                if (Doc == null)
                {
                    TempData["failure"] = "Sorry, Invalid Document ";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Document !", Refresh = "Default" });
                }
                Doc.Status = 1;
                db.SaveChanges();
                TempData["Result"] = "Document Approved Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Document Approved Successfully!", Refresh = "Default" });
            }
            else if (Cmd == "Rejected")
            {
                Document_Master Doc = db.Document_Master.Where(x => x.Doc_Id == Doc_Id).FirstOrDefault();
                if (Doc == null)
                {
                    TempData["failure"] = "Sorry, Invalid Document ";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Document !", Refresh = "Default" });
                }
                Doc.Status = 2;
                db.SaveChanges();
                TempData["failure"] = "Document Rejected Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Document Rejected Successfully!", Refresh = "Default" });
            }
            else if (Cmd == "Delete")
            {
                Document_Master Doc = db.Document_Master.Where(x => x.Doc_Id == Doc_Id).FirstOrDefault();
                if (Doc == null)
                {
                    TempData["failure"] = "Sorry, Invalid Document ";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Document !", Refresh = "Default" });
                }
                Doc.Status = 6;
                db.SaveChanges();
                TempData["failure"] = "Document Deleted Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Document Deleted Successfully!", Refresh = "Default" });
            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !" });
            }
        }

       
        public ActionResult AssetType_List()
        {

            ViewBag.PageTitle = "All AssetType";
            List<Asset_type> asset = db.Asset_type.Where(x => x.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.Type_id).ToList();
            ViewBag.asset = asset;
            return View();
        }
        [HttpPost]
        public ActionResult AssetType_Add(FormCollection form)
        {


            long Type_id = long.Parse(form["Add_AssetType_Type_id"].ToString());
            string Name = form["Add_AssetType_Name"].ToString();
            string Cmd = form["Command"].ToString();
            if (Cmd == "Save")
            {
                if (Type_id == 0)
                {
                    Asset_type A = db.Asset_type.Where(x => x.Name == Name && x.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
                    if (A != null)
                    {
                        TempData["failure"] = "Asset Type already Exist !";
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Asset Type already Exist !" });

                    }
                    Asset_type asset = new Asset_type();
                    asset.Name = Name;
                    asset.Company_ID = _LogedUser.Company_ID;
                    db.Asset_type.Add(asset);
                    db.SaveChanges();
                    TempData["Result"] = "Asset Type  Added Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Asset Type  Added Successfully !", Refresh = "Default" });
                }
                else
                {
                    Asset_type Ast = db.Asset_type.Where(x => x.Type_id == Type_id).FirstOrDefault();
                    if (Ast == null)
                    {
                        TempData["failure"] = "Invalid Asset Type  !";
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Asset Type  !" });
                    }
                    Ast.Name = Name;
                    db.SaveChanges();
                    TempData["Result"] = "Asset Type  Updated Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Asset Type  Updated Successfully !", Refresh = "Default" });
                }
            }
            else if (Cmd == "Delete")
            {
                Asset_type Ast = db.Asset_type.Where(x => x.Type_id == Type_id).FirstOrDefault();
                var _delt = db.Asset_type.Find(Type_id);
                if (Ast == null)
                {
                    TempData["failure"] = "Invalid Asset Type  !";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Asset Type  !" });
                }
                if (_delt.Asset_Master.Count > 0)
                {
                    TempData["failure"] = "Assets Has AssetType Please Delete First...!";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Assets Has AssetType Please Delete First...!" });
                }
                db.Asset_type.Remove(_delt);
                db.SaveChanges();
                TempData["failure"] = "Asset Type Deleted Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Asset Type Deleted Successfully !", Refresh = "Default" });
            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !" });
            }
        }
        public ActionResult AssetType_Delete(FormCollection form)
        {
            long Type_id = long.Parse(form["ass_type_id"].ToString());
            Asset_type Ast = db.Asset_type.Where(x => x.Type_id == Type_id).FirstOrDefault();
            var _delt = db.Asset_type.Find(Type_id);
            if (Ast == null)
            {
                TempData["failure"] = "Invalid Asset Type  !";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Asset Type  !" });
            }
            if (_delt.Asset_Master.Count > 0)
            {
                TempData["failure"] = "Assets Has AssetType Please Delete First...!";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Assets Has AssetType Please Delete First...!" });
            }
            db.Asset_type.Remove(_delt);
            db.SaveChanges();
            TempData["failure"] = "Asset Type Deleted Successfully !";
            return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Asset Type Deleted Successfully !", Refresh = "Default" });
        }
        public ActionResult EmployeeType_List()
        {
            ViewBag.PageTitle = "All EmployeeType";
            List<UserType> Emp_Type = db.UserTypes.OrderByDescending(x => x.Usertype_id).ToList();
            ViewBag.Emp_Type = Emp_Type;
            return View();
        }
        [HttpPost]
        public ActionResult EmployeeType_Add(FormCollection form)
        {

            long Type_id = long.Parse(form["Add_Emp_Type_id"].ToString());
            string Name = form["Add_EmpType_Name"].ToString();
            string Cmd = form["Command"].ToString();
            if (Cmd == "Save")
            {
                if (Type_id == 0)
                {
                    UserType A = db.UserTypes.Where(x => x.Usertype_name == Name).FirstOrDefault();
                    if (A != null)
                    {
                        TempData["failure"] = "Employee Type already Exist !";
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Employee Type already Exist !" });
                    }
                    UserType u = new UserType();
                    u.Usertype_name = Name;
                    db.UserTypes.Add(u);
                    db.SaveChanges();
                    TempData["Result"] = "Employee Type  Added Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Employee Type  Added Successfully !", Refresh = "Default" });
                }
                else
                {
                    UserType userType = db.UserTypes.Where(x => x.Usertype_id == Type_id).FirstOrDefault();
                    if (userType == null)
                    {
                        TempData["failure"] = "Invalid Employee Type  !";
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Employee Type  !" });
                    }
                    UserType A = db.UserTypes.Where(x => x.Usertype_name == Name && x.Usertype_id != Type_id).FirstOrDefault();
                    if (A != null)
                    {
                        TempData["failure"] = "Employee Type already Exist !";
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Employee Type already Exist !" });
                    }
                    userType.Usertype_name = Name;
                    db.SaveChanges();
                    TempData["Result"] = "Employee Type  Updated Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Employee Type  Updated Successfully !", Refresh = "Default" });
                }
            }
            else if (Cmd == "Delete")
            {
                UserType userType = db.UserTypes.Where(x => x.Usertype_id == Type_id).FirstOrDefault();
                var _delt = db.UserTypes.Find(Type_id);
                if (userType == null)
                {
                    TempData["failure"] = "Invalid Employee Type  !";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Employee Type  !" });
                }
                db.UserTypes.Remove(_delt);
                db.SaveChanges();

                TempData["failure"] = "Employee Type Deleted Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Employee Type Deleted Successfully !", Refresh = "Default" });
            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !" });
            }
        }
        public ActionResult EmployeeType_Delete(FormCollection form)
        {
            long Type_id = long.Parse(form["emp_type_id"].ToString());
            UserType userType = db.UserTypes.Where(x => x.Usertype_id == Type_id).FirstOrDefault();
            var _delt = db.UserTypes.Find(Type_id);
            if (userType == null)
            {
                TempData["failure"] = "Invalid Employee Type  !";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Employee Type  !" });
            }
            //if (_delt.UserMasters.Count > 0)
            //{
            //    TempData["failure"] = "Employee Type Has User Please Delete First...!";
            //    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Employee Type Has User Please Delete First...!" });
            //}
            db.UserTypes.Remove(_delt);
            db.SaveChanges();

            TempData["failure"] = "Employee Type Deleted Successfully !";
            return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Employee Type Deleted Successfully !", Refresh = "Default" });
        }

        public ActionResult City_List()
        {
            ViewBag.PageTitle = "All Cities";
            List<City_Master> city = db.City_Master.OrderByDescending(x => x.Cityid).ToList();
            ViewBag.city = city;
            return View();
        }
        [HttpPost]
        public ActionResult City_Add(FormCollection form)
        {

            long City_id = long.Parse(form["Add_Cityid"].ToString());
            string Name = form["Add_CityName"].ToString();
            string Cmd = form["Command"].ToString();
            if (Cmd == "Save")
            {
                if (City_id == 0)
                {
                    City_Master A = db.City_Master.Where(x => x.CityName == Name).FirstOrDefault();
                    if (A != null)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "City already Exist !" });
                    }
                    City_Master c = new City_Master();
                    c.CityName = Name;
                    db.City_Master.Add(c);
                    db.SaveChanges();
                    TempData["Result"] = "New City Added Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "New City Added Successfully!", Refresh = "Default" });
                }
                else
                {
                    City_Master c = db.City_Master.Where(x => x.Cityid == City_id).FirstOrDefault();
                    if (c == null)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid City  !" });
                    }
                    c.CityName = Name;
                    db.SaveChanges();
                    TempData["Result"] = "City Updated Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "City Updated Successfully !", Refresh = "Default" });
                }
            }
            else if (Cmd == "Delete")
            {
                City_Master c = db.City_Master.Where(x => x.Cityid == City_id).FirstOrDefault();
                var _delt = db.City_Master.Find(City_id);
                if (c == null)
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid City !" });
                }

                db.City_Master.Remove(_delt);
                db.SaveChanges();
                TempData["failure"] = "City Deleted Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "City Deleted Successfully !", Refresh = "Default" });
            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !" });
            }
        }
        public ActionResult City_Delete(FormCollection form)
        {
            long City_id = long.Parse(form["city_id"].ToString());
            City_Master c = db.City_Master.Where(x => x.Cityid == City_id).FirstOrDefault();
            var _delt = db.City_Master.Find(City_id);
            if (c == null)
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid City !" });
            }

            db.City_Master.Remove(_delt);
            db.SaveChanges();
            TempData["failure"] = "City Deleted Successfully !";
            return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "City Deleted Successfully !", Refresh = "Default" });
        }
        public ActionResult Religion_List()
        {
            ViewBag.PageTitle = "All Religion";
            List<Religion_Master> r = db.Religion_Master.OrderByDescending(x => x.Religion_Id).ToList();
            ViewBag.r = r;
            return View();
        }
        [HttpPost]
        public ActionResult Religion_Add(FormCollection form)
        {

            long Religion_Id = long.Parse(form["Add_Religion_Id"].ToString());
            string Name = form["Add_Religion_Name"].ToString();
            string Cmd = form["Command"].ToString();
            if (Cmd == "Save")
            {
                if (Religion_Id == 0)
                {
                    Religion_Master A = db.Religion_Master.Where(x => x.Religion_Name == Name).FirstOrDefault();
                    if (A != null)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Religion already Exist !" });
                    }
                    Religion_Master r = new Religion_Master();
                    r.Religion_Name = Name;
                    db.Religion_Master.Add(r);
                    db.SaveChanges();
                    TempData["Result"] = "New Religion Added Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "New Religion Added Successfully!", Refresh = "Default" });
                }
                else
                {
                    Religion_Master r = db.Religion_Master.Where(x => x.Religion_Id == Religion_Id).FirstOrDefault();
                    if (r == null)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Religion  !" });
                    }
                    r.Religion_Name = Name;
                    db.SaveChanges();
                    TempData["Result"] = "Religion Updated Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Religion Updated Successfully !", Refresh = "Default" });
                }
            }
            else if (Cmd == "Delete")
            {
                Religion_Master r = db.Religion_Master.Where(x => x.Religion_Id == Religion_Id).FirstOrDefault();
                var _delt = db.Religion_Master.Find(Religion_Id);
                if (r == null)
                {
                    TempData["failure"] = "Invalid Religion !";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Religion !" });
                }
                if (_delt.Festival_Master.Count > 0)
                {
                    TempData["failure"] = "Religion Has Festival Please Delete First...!";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Religion Has Festival Please Delete First...!" });
                }
                db.Religion_Master.Remove(_delt);
                db.SaveChanges();
                TempData["failure"] = "Religion Deleted Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Religion Deleted Successfully !", Refresh = "Default" });
            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !" });
            }
        }
        public ActionResult Religion_Delete(FormCollection form)
        {
            long Religion_Id = long.Parse(form["religion_id"].ToString());
            Religion_Master r = db.Religion_Master.Where(x => x.Religion_Id == Religion_Id).FirstOrDefault();
            var _delt = db.Religion_Master.Find(Religion_Id);
            if (r == null)
            {
                TempData["failure"] = "Invalid Religion !";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Religion !" });
            }

            if (_delt.Festival_Master.Count > 0)
            {
                TempData["failure"] = "Religion Has Festival Please Delete First...!";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Religion Has Festival Please Delete First...!" });
            }
            db.Religion_Master.Remove(_delt);
            db.SaveChanges();
            TempData["failure"] = "Religion Deleted Successfully !";
            return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Religion Deleted Successfully !", Refresh = "Default" });
        }
        public ActionResult Festival_List()
        {
            ViewBag.PageTitle = "All Festival";
            List<Festival_Master> f = db.Festival_Master.OrderByDescending(x => x.Fest_Id).ToList();
            List<Religion_Master> r = db.Religion_Master.ToList();
            ViewBag.R = r;
            ViewBag.f = f;
            return View();
        }
        [HttpPost]
        public ActionResult Festival_Add(FormCollection form)
        {

            long Fest_id = long.Parse(form["Add_Fest_Id"].ToString());
            string Name = form["Add_Fest_Name"].ToString();
            long Religion_id = long.Parse(form["Add_Religion_Id"].ToString());
            string Cmd = form["Command"].ToString();
            if (Cmd == "Save")
            {
                if (Fest_id == 0)
                {
                    Festival_Master F = db.Festival_Master.Where(x => x.Fest_Name == Name).FirstOrDefault();
                    if (F != null)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Festival already Exist !" });
                    }
                    Festival_Master f = new Festival_Master();
                    f.Fest_Name = Name;
                    f.Religion_Id = Religion_id;
                    db.Festival_Master.Add(f);
                    db.SaveChanges();
                    TempData["Result"] = "New Festival Added Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "New Festival Added Successfully !", Refresh = "Default" });
                }
                else
                {
                    Festival_Master F = db.Festival_Master.Where(x => x.Fest_Id == Fest_id).FirstOrDefault();
                    if (F == null)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Festival  !" });
                    }
                    F.Fest_Name = Name;
                    F.Religion_Id = Religion_id;
                    db.SaveChanges();
                    TempData["Result"] = "Festival Updated Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Festival Updated Successfully !", Refresh = "Default" });
                }
            }
            else if (Cmd == "Delete")
            {
                Festival_Master F = db.Festival_Master.Where(x => x.Fest_Id == Fest_id).FirstOrDefault();
                var _delt = db.Festival_Master.Find(Fest_id);
                if (F == null)
                {
                    TempData["failure"] = "Invalid Festival !";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Festival !" });
                }
                if (_delt.Gift_Master.Count > 0)
                {
                    TempData["failure"] = "Festival Has Gift Please Delete First...!";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Festival Has Gift Please Delete First...!" });
                }
                db.Festival_Master.Remove(_delt);
                db.SaveChanges();
                TempData["failure"] = "Festival Deleted Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Festival Deleted Successfully !", Refresh = "Default" });
            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !" });
            }
        }
        public ActionResult Festival_Delete(FormCollection form)
        {
            long Fest_id = long.Parse(form["fes_id"].ToString());
            Festival_Master F = db.Festival_Master.Where(x => x.Fest_Id == Fest_id).FirstOrDefault();
            var _delt = db.Festival_Master.Find(Fest_id);
            if (F == null)
            {
                TempData["failure"] = "Invalid Festival !";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Festival !" });
            }
            if (_delt.Gift_Master.Count > 0)
            {
                TempData["failure"] = "Festival Has Gift Please Delete First...!";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Festival Has Gift Please Delete First...!" });
            }
            db.Festival_Master.Remove(_delt);
            db.SaveChanges();
            TempData["failure"] = "Festival Deleted Successfully !";
            return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Festival Deleted Successfully !", Refresh = "Default" });
        }
        public ActionResult CustomDocument_List()
        {

            ViewBag.PageTitle = "All CustomDocuments";
            List<Custom_Document> c = db.Custom_Document.Where(x => x.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.Cust_Id).ToList();
            ViewBag.c = c;
            return View();
        }
        public ActionResult CustomDocument_Add(long Cust_Id)
        {

            ViewBag.PageTitle = "Add CustomDocument";
            Custom_Document c = db.Custom_Document.Where(x => x.Cust_Id == Cust_Id && x.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
            ViewBag.c = c;
            return View();
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult CustomDocument_Add(FormCollection form)
        {

            long Cust_Id = long.Parse(form["Add_Cust_Id"].ToString());
            string Name = form["Add_Doc_Name"].ToString();
            string Desc = form["Add_Doc_Disc"].ToString();
            string Cmd = form["Command"].ToString();
            if (Cmd == "Save")
            {
                if (Cust_Id == 0)
                {
                    Custom_Document A = db.Custom_Document.Where(x => x.Doc_Name == Name && x.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
                    if (A != null)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Document already Exist !" });
                    }
                    Custom_Document c = new Custom_Document();
                    c.Doc_Name = Name;
                    c.Doc_Disc = Desc;
                    c.Status = 1;
                    c.Company_ID = _LogedUser.Company_ID;
                    db.Custom_Document.Add(c);
                    db.SaveChanges();
                    TempData["Result"] = "New Document Added Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "New Document Added Successfully !", Refresh = "CustomDocument_List" });
                }
                else
                {
                    Custom_Document c = db.Custom_Document.Where(x => x.Cust_Id == Cust_Id).FirstOrDefault();
                    if (c == null)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Document  !" });
                    }
                    c.Doc_Name = Name;
                    c.Doc_Disc = Desc;
                    db.SaveChanges();
                    TempData["Result"] = "Document Updated Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Document Updated Successfully !", Refresh = "CustomDocument_List" });
                }
            }
            else if (Cmd == "Delete")
            {
                Custom_Document c = db.Custom_Document.Where(x => x.Cust_Id == Cust_Id).FirstOrDefault();
                var _delt = db.Custom_Document.Find(Cust_Id);
                if (c == null)
                {
                    TempData["failure"] = "Invalid Document !";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Document !", Refresh = "CustomDocument_List" });
                }
                db.Custom_Document.Remove(_delt);
                db.SaveChanges();
                TempData["failure"] = "Document Deleted Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Document Deleted Successfully !", Refresh = "CustomDocument_List" });
            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !" });
            }
        }
        public ActionResult CustomDocument_Delete(FormCollection form)
        {
            long Cust_Id = long.Parse(form["cust_id"].ToString());
            Custom_Document c = db.Custom_Document.Where(x => x.Cust_Id == Cust_Id).FirstOrDefault();
            var _delt = db.Custom_Document.Find(Cust_Id);
            if (c == null)
            {
                TempData["failure"] = "Invalid Document !";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Document !", Refresh = "CustomDocument_List" });
            }
            db.Custom_Document.Remove(_delt);
            db.SaveChanges();
            TempData["failure"] = "Document Deleted Successfully !";
            return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Document Deleted Successfully !", Refresh = "CustomDocument_List" });
        }
        public ActionResult Department_List()
        {

            ViewBag.PageTitle = "All Department";
            List<Department_Master> dept = db.Department_Master.Where(x => x.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.Dept_ID).ToList();
            ViewBag.dept = dept;
            return View();
        }
        [HttpPost]
        public ActionResult Department_Add(FormCollection form)
        {

            long Dept_Id = long.Parse(form["Add_Dept_ID"].ToString());
            string Name = form["Add_Department"].ToString();
            string Cmd = form["Command"].ToString();
            if (Cmd == "Save")
            {
                if (Dept_Id == 0)
                {
                    Department_Master A = db.Department_Master.Where(x => x.Department == Name && x.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
                    if (A != null)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Departmnet already Exist !" });
                    }
                    Department_Master dept = new Department_Master();
                    dept.Department = Name;
                    dept.Company_ID = _LogedUser.Company_ID;
                    db.Department_Master.Add(dept);
                    db.SaveChanges();
                    TempData["Result"] = "New Department Added Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "New Department Added Successfully!", Refresh = "Default" });
                }
                else
                {
                    Department_Master d = db.Department_Master.Where(x => x.Dept_ID == Dept_Id).FirstOrDefault();
                    if (d == null)
                    {
                        TempData["failure"] = "Invalid Department  !";
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Department  !" });
                    }
                    d.Department = Name;
                    db.SaveChanges();
                    TempData["Result"] = "Department Updated Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Department Updated Successfully!", Refresh = "Default" });
                }
            }
            else if (Cmd == "Delete")
            {
                Department_Master d = db.Department_Master.Where(x => x.Dept_ID == Dept_Id).FirstOrDefault();
                var _delt = db.Department_Master.Find(Dept_Id);
                if (d == null)
                {
                    TempData["failure"] = "Invalid Department !";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Department !" });
                }
                if (_delt.UserMasters.Count > 0)
                {
                    TempData["failure"] = "Department Has User Please Delete First...!";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Department Has User Please Delete First...!" });
                }
                db.Department_Master.Remove(_delt);
                db.SaveChanges();
                TempData["failure"] = "Department Deleted Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Department Deleted Successfully!", Refresh = "Default" });
            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !" });
            }
        }
        public ActionResult Department_Delete(FormCollection form)
        {
            long Dept_Id = long.Parse(form["dep_id"].ToString());
            Department_Master d = db.Department_Master.Where(x => x.Dept_ID == Dept_Id).FirstOrDefault();
            var _delt = db.Department_Master.Find(Dept_Id);
            if (d == null)
            {
                TempData["failure"] = "Invalid Department !";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Department !" });
            }
            if (_delt.UserMasters.Count > 0)
            {
                TempData["failure"] = "Department Has User Please Delete First...!";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Department Has User Please Delete First...!" });
            }
            db.Department_Master.Remove(_delt);
            db.SaveChanges();
            TempData["failure"] = "Department Deleted Successfully !";
            return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Department Deleted Successfully!", Refresh = "Default" });
        }
        public ActionResult Designation_List()
        {

            ViewBag.PageTitle = "All Designation";
            List<Designation_Master> desg = db.Designation_Master.Where(x => x.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.Desg_Id).ToList();
            ViewBag.desg = desg;
            return View();
        }
        [HttpPost]
        public ActionResult Designation_Add(FormCollection form)
        {

            long Desg_Id = long.Parse(form["Add_Desg_Id"].ToString());
            string Name = form["Add_Designation"].ToString();
            string Cmd = form["Command"].ToString();
            if (Cmd == "Save")
            {
                if (Desg_Id == 0)
                {
                    Designation_Master A = db.Designation_Master.Where(x => x.Designation == Name && x.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
                    if (A != null)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Designation already Exist !" });
                    }
                    Designation_Master d = new Designation_Master();
                    d.Designation = Name;
                    d.Company_ID = _LogedUser.Company_ID;
                    db.Designation_Master.Add(d);
                    db.SaveChanges();
                    TempData["Result"] = "New Designation Added Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "New Designation Added Successfully!", Refresh = "Default" });
                }
                else
                {
                    Designation_Master d = db.Designation_Master.Where(x => x.Desg_Id == Desg_Id).FirstOrDefault();
                    if (d == null)
                    {
                        TempData["failure"] = "Invalid Designation  !";
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Designation  !" });
                    }
                    d.Designation = Name;
                    db.SaveChanges();
                    TempData["Result"] = "Designation Updated Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Designation Updated Successfully!", Refresh = "Default" });
                }
            }
            else if (Cmd == "Delete")
            {
                Designation_Master d = db.Designation_Master.Where(x => x.Desg_Id == Desg_Id).FirstOrDefault();
                var _delt = db.Designation_Master.Find(Desg_Id);
                if (d == null)
                {
                    TempData["failure"] = "Invalid Designation !";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Designation !" });
                }
                if (_delt.UserMasters.Count > 0)
                {
                    TempData["failure"] = "Designation Has User Please Delete First...!";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Designation Has User Please Delete First...!" });
                }
                db.Designation_Master.Remove(_delt);
                db.SaveChanges();
                TempData["failure"] = "Designation Deleted Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Designation Deleted Successfully!", Refresh = "Default" });
            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !" });
            }
        }
        public ActionResult Designation_Delete(FormCollection form)
        {
            long Desg_Id = long.Parse(form["desg_id"].ToString());
            Designation_Master d = db.Designation_Master.Where(x => x.Desg_Id == Desg_Id).FirstOrDefault();
            var _delt = db.Designation_Master.Find(Desg_Id);
            if (d == null)
            {
                TempData["failure"] = "Invalid Designation !";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Designation !" });
            }
            if (_delt.UserMasters.Count > 0)
            {
                TempData["failure"] = "Designation Has User Please Delete First...!";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Designation Has User Please Delete First...!" });
            }
            db.Designation_Master.Remove(_delt);
            db.SaveChanges();
            TempData["failure"] = "Designation Deleted Successfully !";
            return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Designation Deleted Successfully!", Refresh = "Default" });
        }

        public ActionResult Head()
        {
            ViewBag.PageTitle = "All Head";
            List<Company_Doc_Heads> doc_Heads = db.Company_Doc_Heads.Where(x => x.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.Document_HeadId).ToList();
            ViewBag.doc_Heads = doc_Heads;
            return View();
        }
        [HttpPost]
        public ActionResult Head_Add(FormCollection form)
        {
            long HeadId = long.Parse(form["HeadId"] ?? "0");
            string command = form["Command"] ?? string.Empty;


            if (command == "Save,")
            {
                string HeadName = form["HeadName"]?.Trim();
                if (string.IsNullOrEmpty(HeadName))
                {
                    TempData["Result"] = "Failure^Please Enter Head Name!";
                    return RedirectToAction("Head");
                }
                if (HeadId == 0)
                {

                    var duplicatehead = db.Company_Doc_Heads
                                    .FirstOrDefault(x => x.HeadName == HeadName && x.Document_HeadId != HeadId);

                    if (duplicatehead != null)
                    {
                        TempData["Result"] = "Failure^Main Head Already Exists!";
                        return RedirectToAction("Head");
                    }
                    var newhead = new Company_Doc_Heads
                    {
                        Company_ID = _LogedUser.Company_ID,
                        HeadName = HeadName
                    };

                    db.Company_Doc_Heads.Add(newhead);
                    db.SaveChangesAsync();

                    TempData["Result"] = "New Head Added Successfully.";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "New Head Added Successfully!", Refresh = "Default" });
                }
                else
                {
                    Company_Doc_Heads d = db.Company_Doc_Heads.Where(x => x.Document_HeadId == HeadId).FirstOrDefault();
                    if (d == null)
                    {
                        TempData["failure"] = "Invalid Head  !";
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Head  !" });
                    }
                    d.HeadName = HeadName;
                    db.SaveChanges();
                    TempData["Result"] = "Head Updated Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Head Updated Successfully!", Refresh = "Default" });
                }
            }
            else if (command == "Delete")
            {
                Company_Doc_Heads d = db.Company_Doc_Heads.Where(x => x.Document_HeadId == HeadId).FirstOrDefault();
                var _delt = db.Company_Doc_Heads.Find(HeadId);
                if (d == null)
                {
                    TempData["failure"] = "Invalid Head !";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Head !" });
                }
                if (_delt.Company_Doc_Heads2.Count > 0)
                {
                    TempData["failure"] = "Head Has Head2 Please Delete First...!";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Head2 Has User Please Delete First...!" });
                }
                db.Company_Doc_Heads.Remove(_delt);
                db.SaveChanges();
                TempData["failure"] = "Head Deleted Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Head Deleted Successfully!", Refresh = "Default" });
            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !" });
            }
        }

        public ActionResult Head2(long? HeadId = 0)
        {
            ViewBag.PageTitle = "All Head";
            List<Company_Doc_Heads2> doc_Head2s = db.Company_Doc_Heads2.Where(x => x.Document_HeadId == HeadId).OrderByDescending(x => x.Document_HeadId).ToList();
            ViewBag.HeadId = HeadId;
            ViewBag.doc_Head2s = doc_Head2s;
            return View();
        }
        [HttpPost]
        public ActionResult Head2_Add(FormCollection form)
        {
            long Head2Id = long.Parse(form["Head2Id"] ?? "0");
            long HeadId = long.Parse(form["HeadId"] ?? "0");
            string command = form["Command"] ?? string.Empty;


            if (command == "Save,")
            {
                string HeadName = form["HeadName2"]?.Trim();
                if (string.IsNullOrEmpty(HeadName))
                {
                    TempData["Result"] = "Failure^Please Enter Head Name!";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Please Enter Head Name!" });
                    //return RedirectToAction("Head");
                }
                if (Head2Id == 0)
                {

                    var duplicatehead = db.Company_Doc_Heads2
                                    .FirstOrDefault(x => x.HeadName == HeadName && x.Document_Head2Id != Head2Id);

                    if (duplicatehead != null)
                    {
                        TempData["failure"] = "Head Already Exists!";
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Head Already Exists!" });
                       // return RedirectToAction("Head");
                    }
                    var newhead = new Company_Doc_Heads2
                    {
                        Document_HeadId = HeadId,
                        HeadName = HeadName
                    };

                    db.Company_Doc_Heads2.Add(newhead);
                    db.SaveChangesAsync();

                    TempData["Result"] = "New Head Added Successfully.";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "New Head Added Successfully!", Refresh = "Default" });
                }
                else
                {
                    Company_Doc_Heads2 d = db.Company_Doc_Heads2.Where(x => x.Document_Head2Id == Head2Id).FirstOrDefault();
                    if (d == null)
                    {
                        TempData["failure"] = "Invalid Head2  !";
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Head  !" });
                    }
                    d.HeadName = HeadName;
                    db.SaveChanges();
                    TempData["Result"] = "Head2 Updated Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Head2 Updated Successfully!", Refresh = "Default" });
                }
            }
            else if (command == "Delete")
            {
                Company_Doc_Heads2 d = db.Company_Doc_Heads2.Where(x => x.Document_Head2Id == Head2Id).FirstOrDefault();
                var _delt = db.Company_Doc_Heads2.Find(Head2Id);
                if (d == null)
                {
                    TempData["failure"] = "Invalid Head2 !";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Head !" });
                }
                if (_delt.Company_ImportantDocuments.Count > 0)
                {
                    TempData["failure"] = "Head2 Has Documents Please Delete First...!";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Head2 Has User Please Delete First...!" });
                }
                db.Company_Doc_Heads2.Remove(_delt);
                db.SaveChanges();
                TempData["failure"] = "Head2 Deleted Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Head2 Deleted Successfully!", Refresh = "Default" });
            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !" });
            }
        }

        public ActionResult ProjectDesignation(long? Project_MasterID = 0)
        {

            ViewBag.PageTitle = "All Department";
            List<ProjectSalaryHead> projectSalaries = db.ProjectSalaryHeads
                .Where(x => x.Company_ID == _LogedUser.Company_ID)
                .OrderByDescending(x => x.ProjectSalaryHeadID).ToList();
            ViewBag.Project_Salaries = projectSalaries;
            ViewBag.Project_MasterID = Project_MasterID;
            return View();
        }

        public ActionResult ProjectDesignationEdit(long salaryheadid)
        {
            if (salaryheadid <= 0)
            {
                ViewBag.salaryheadid = salaryheadid;
                return View();
            }
            ProjectSalaryHead objProjectSalaryHead = db.ProjectSalaryHeads.Where(x => x.Company_ID == _LogedUser.Company_ID && x.ProjectSalaryHeadID == salaryheadid).OrderByDescending(x => x.ProjectSalaryHeadID).FirstOrDefault();
            ViewBag.ProjectSalaryHead = objProjectSalaryHead;
            return View();
        }
        public ActionResult ProjectDesignationAdd(long? Project_MasterID = 0)
        {
            ViewBag.Project_MasterID = Project_MasterID;
            return View();
        }



        [HttpGet]
        public ActionResult ProjectSalaryUploads(int month = 99, int year = 99, int Project_SalaryGroupID = 99)
        {
            if (month == 99)
            {
                month = DateTime.Now.Month;
            }
            if (year == 99)
            {
                year = DateTime.Now.Year;
            }
            ViewBag.PageTitle = "All Department";
            List<ProjectSalaryGroup> ProjectSalaryGroups = db.ProjectSalaryGroups.Where(x => x.Company_ID == _LogedUser.Company_ID ).OrderByDescending(x => x.Project_SalaryGroupID).ToList();
            ViewBag.ProjectSalaryGroups = ProjectSalaryGroups;


            ViewBag.Project_Salaries = db.ProjectSalaries.Where(x => x.UserMaster.Project_SalaryGroupID == Project_SalaryGroupID).ToList();

            //ViewBag.Project_Salaries = db.ProjectSalaries.ToList();
            ViewBag.month = month;
            ViewBag.year = year;
            ViewBag.Project_SalaryGroupID = Project_SalaryGroupID;
            return View();
        }

        [HttpGet]
        public ActionResult ProjectSalaryView(int month = 99, int year = 99, int Project_SalaryGroupID = 99)
        {
            if (month == 99)
            {
                month = DateTime.Now.Month;
            }
            if (year == 99)
            {
                year = DateTime.Now.Year;
            }
            ViewBag.PageTitle = "All Department";
            List<ProjectSalaryGroup> ProjectSalaryGroups = db.ProjectSalaryGroups.Where(x => x.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.Project_SalaryGroupID).ToList();
            ViewBag.ProjectSalaryGroups = ProjectSalaryGroups;


            ViewBag.Project_Salaries = db.ProjectSalaries.Where(x => x.UserMaster.Project_SalaryGroupID == Project_SalaryGroupID && x.SalaryMonth.Month==month && x.SalaryMonth.Year== year && x.UserMaster.status != 6).ToList();

            //ViewBag.Project_Salaries = db.ProjectSalaries.ToList();
            ViewBag.month = month;
            ViewBag.year = year;
            ViewBag.Project_SalaryGroupID = Project_SalaryGroupID;
            return View();
        }

        [HttpGet]
        public ActionResult ProjectSalaryCreater(int month = 99, int year = 99, int Project_SalaryGroupID = 99)
        {
            if (month == 99)
            {
                month = DateTime.Now.Month;
            }
            if (year == 99)
            {
                year = DateTime.Now.Year;
            }
            ViewBag.PageTitle = "All Department";
            List<ProjectSalaryGroup> ProjectSalaryGroups = db.ProjectSalaryGroups.Where(x => x.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.Project_SalaryGroupID).ToList();
            ViewBag.ProjectSalaryGroups = ProjectSalaryGroups;


            ViewBag.Project_Salaries = db.ProjectSalaries.Where(x => x.UserMaster.Project_SalaryGroupID == Project_SalaryGroupID && x.SalaryMonth.Month == month && x.SalaryMonth.Year == year && x.UserMaster.status!=6).ToList();

            //ViewBag.Project_Salaries = db.ProjectSalaries.ToList();
            ViewBag.month = month;
            ViewBag.year = year;
            ViewBag.Project_SalaryGroupID = Project_SalaryGroupID;
            return View();
        }

        [HttpPost]
        public ActionResult DownloadSalarySample(FormCollection form, HttpPostedFileBase UploadedFile)
        {
            int month = int.Parse(form["month"].ToString());
            int year = int.Parse(form["year"].ToString());
            int Project_SalaryGroupID = int.Parse(form["Project_SalaryGroupID"].ToString());
            string Command = form["Command"].ToString();
            if (month == 99)
            {
                month = DateTime.Now.Month;
            }
            if (year == 99)
            {
                year = DateTime.Now.Year;
            }
            if (Command == "ShowSalary")
            {
                return RedirectToAction("ProjectSalaryView", new { month = month, year = year, Project_SalaryGroupID = Project_SalaryGroupID });
            }
            else if (Command == "Download")
            {
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Employee");
                List<UserMaster> users = db.UserMasters.Where(x => x.Company_ID == _LogedUser.Company_ID && x.Project_SalaryGroupID == Project_SalaryGroupID && x.status!=6).ToList();
                worksheet.Cell(1, 1).Value = "SrNo";
                worksheet.Cell(1, 2).Value = "EmployeeID";
                worksheet.Cell(1, 3).Value = "Name";
                worksheet.Cell(1, 4).Value = "Designation";
                worksheet.Cell(1, 5).Value = "UAN Number";
                worksheet.Cell(1, 6).Value = "ESIC Number";
                worksheet.Cell(1, 7).Value = "ShiftHours";
                worksheet.Cell(1, 8).Value = "NoOfDays";
                worksheet.Cell(1, 9).Value = "Advance";
                //worksheet.Cell(1, 9).Value = "ActualOT";
                //worksheet.Cell(1, 10).Value = "ActulOTHours";
                //worksheet.Cell(1, 11).Value = "ActualPH";
                int rownumber = 1;
                foreach (UserMaster user in users)
                {
                    rownumber++;
                    worksheet.Cell(rownumber, 1).Value = rownumber - 1;
                    worksheet.Cell(rownumber, 2).Value = user.User_ID;
                    worksheet.Cell(rownumber, 3).Value = user.User_Name;
                    worksheet.Cell(rownumber, 4).Value = user.Designation_Master.Designation;
                    worksheet.Cell(rownumber, 5).Value = user.UAN_No;
                    worksheet.Cell(rownumber, 6).Value = user.ESIC_No;
                    worksheet.Cell(rownumber, 7).Value = 0;
                    worksheet.Cell(rownumber, 8).Value = 0;
                    worksheet.Cell(rownumber, 9).Value = 0;
                    //worksheet.Cell(rownumber, 9).Value = 0;
                    //worksheet.Cell(rownumber, 10).Value = 0;
                    //worksheet.Cell(rownumber, 11).Value = 0;
                }
                var memoryStream = new System.IO.MemoryStream();
                workbook.SaveAs(memoryStream);
                memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
                return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "_" + month.ToString() + "_" + year.ToString() + ".xlsx");
            }
            else if (Command == "DownloadWage")
            {
                string name = "";

              //  List<UserMaster> users = db.UserMasters.Where(x => x.Company_ID == _LogedUser.Company_ID && x.Project_SalaryGroupID == Project_SalaryGroupID).ToList();
                ProjectSalaryGroup SalaryGroup = db.ProjectSalaryGroups.Where(x => x.Company_ID == _LogedUser.Company_ID && x.Project_SalaryGroupID == Project_SalaryGroupID).FirstOrDefault();
                List<ProjectSalary> SalarySlips = db.ProjectSalaries.Where(x => x.Company_ID == _LogedUser.Company_ID && 
                                                    x.UserMaster.Project_SalaryGroupID == Project_SalaryGroupID && x.SalaryMonth.Month == month && x.SalaryMonth.Year == year).ToList();
                name = "Wage_" + SalaryGroup.Project_Name.ToString() + "_";


                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Employee");
                //List<UserMaster> users = db.UserMasters.Where(x => x.ProjectSalaryHead == ProjectSalaryHeadndid).ToList();

                //First Row Blank
                //worksheet.Row(1).Hide();

                worksheet.Range("A2:AI2").Merge();
                worksheet.Cell(2, 1).Value = "Wage Sheet";
                worksheet.Cell(2, 1).Style.Font.Bold = true;
                worksheet.Cell(2, 1).Style.Font.FontSize = 14;
                worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(2, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                worksheet.Range("A3:AI3").Merge();
                worksheet.Cell(3, 1).Value = SalaryGroup.Project_Name;
                worksheet.Cell(3, 1).Style.Font.Bold = true;
                worksheet.Cell(3, 1).Style.Font.FontSize = 18;
                worksheet.Cell(3, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(3, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                worksheet.Range("A4:AI4").Merge();
                worksheet.Cell(4, 1).Value = SalaryGroup.Project_Address;
                worksheet.Cell(4, 1).Style.Font.FontSize = 12;
                worksheet.Cell(4, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(4, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                worksheet.Range("A5:AI5").Merge();
                worksheet.Cell(5, 1).Value = "Month: " + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month) + " " + year;
                worksheet.Cell(5, 1).Style.Font.FontSize = 12;
                worksheet.Cell(5, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(5, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                // Column Headers (starting from the 6th row)
                worksheet.Cell(6, 1).Value = "SrNo";
                worksheet.Cell(6, 2).Value = "EmployeeID";
                worksheet.Cell(6, 3).Value = "Name";
                worksheet.Cell(6, 4).Value = "Designation";
                worksheet.Cell(6, 5).Value = "UAN Number";
                worksheet.Cell(6, 6).Value = "ESIC Number";
                worksheet.Cell(6, 7).Value = "ShiftHours";
                worksheet.Cell(6, 8).Value = "NoOfDays";
                worksheet.Cell(6, 9).Value = "ActualOT";
                worksheet.Cell(6, 10).Value = "ActualOTHours";
                worksheet.Cell(6, 11).Value = "ActualPH";
                worksheet.Cell(6, 12).Value = "MinWagesPerDay";
                worksheet.Cell(6, 13).Value = "BASIC";
                worksheet.Cell(6, 14).Value = "OTRate";
                worksheet.Cell(6, 15).Value = "PHRate";
                worksheet.Cell(6, 16).Value = "HRA";
                worksheet.Cell(6, 17).Value = "DA";
                worksheet.Cell(6, 18).Value = "LEAVE";
                worksheet.Cell(6, 19).Value = "BONUS";
                worksheet.Cell(6, 20).Value = "TA";
                worksheet.Cell(6, 21).Value = "PH";
                worksheet.Cell(6, 22).Value = "SPECIALALLOWANCE";
                worksheet.Cell(6, 23).Value = "OTHERALLOWANCE";
                worksheet.Cell(6, 24).Value = "OTPAYMENT";
                worksheet.Cell(6, 25).Value = "GROSS_INCOME";
                worksheet.Cell(6, 26).Value = "PF";
                worksheet.Cell(6, 27).Value = "PT";
                worksheet.Cell(6, 28).Value = "ESIC";
                worksheet.Cell(6, 29).Value = "CANTEEN";
                worksheet.Cell(6, 30).Value = "HRA_DEDUCT";
                worksheet.Cell(6, 31).Value = "TRAVEL";
                worksheet.Cell(6, 32).Value = "ADVANCE";
                worksheet.Cell(6, 33).Value = "Uniform";
                worksheet.Cell(6, 34).Value = "Welfare";
                worksheet.Cell(6, 35).Value = "Other";
                worksheet.Cell(6, 36).Value = "GROSS_DEDUCTION";
                worksheet.Cell(6, 37).Value = "NET_INCOME";



                //if (designation != null)
                //{
                //    // Add Wage Rate Details
                //    worksheet.Range("AJ2:AK2").Merge();
                //    worksheet.Cell(2, 36).Value = "Wage";
                //    worksheet.Cell(2, 36).Style.Font.FontSize = 10;
                //    worksheet.Cell(2, 36).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                //    worksheet.Cell(2, 36).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                //    worksheet.Cell(2, 36).Style.Font.Bold = true;
                //    worksheet.Cell(2, 36).Value = "Wage Rate";
                //}


                for (int col = 1; col <= 37; col++) // Looping through all columns
                {
                    worksheet.Cell(6, col).Style.Font.Bold = true;
                }
                worksheet.Range("A6:F6").SetAutoFilter();

                // Now populating the rows
                int rownumber = 6;  // Start from the 7th row (index 6)
                foreach (ProjectSalary _salary in SalarySlips)
                {
                    rownumber++;
                    worksheet.Cell(rownumber, 1).Value = rownumber - 6;
                    worksheet.Cell(rownumber, 2).Value = _salary.UserMaster.EmployeeID;
                    worksheet.Cell(rownumber, 3).Value = _salary.UserMaster.User_Name;
                    worksheet.Cell(rownumber,4).Value = _salary.UserMaster.Designation_Master.Designation;
                    worksheet.Cell(rownumber, 5).Value = _salary.UserMaster.UAN_No;
                    worksheet.Cell(rownumber, 6).Value = _salary.UserMaster.ESIC_No;
                    worksheet.Cell(rownumber, 7).Value = _salary.ShiftHours;
                    worksheet.Cell(rownumber, 8).Value = _salary.NoOfDays;
                    worksheet.Cell(rownumber, 9).Value = _salary.NoOfOTShift;
                    worksheet.Cell(rownumber, 10).Value = _salary.NoOfOTHors;
                    worksheet.Cell(rownumber, 11).Value = _salary.NoOfPH;
                    worksheet.Cell(rownumber, 12).Value = _salary.MinWagesPerDay;
                    worksheet.Cell(rownumber, 13).Value = _salary.BASIC;
                    worksheet.Cell(rownumber, 14).Value = _salary.OTRate;
                    worksheet.Cell(rownumber, 15).Value = _salary.PHRate;
                    worksheet.Cell(rownumber, 16).Value = _salary.HRA;
                    worksheet.Cell(rownumber, 17).Value = _salary.DA;
                    worksheet.Cell(rownumber, 18).Value = _salary.LEAVE;
                    worksheet.Cell(rownumber, 19).Value = _salary.BONUS;
                    worksheet.Cell(rownumber, 20).Value = _salary.TA;
                    worksheet.Cell(rownumber, 21).Value = _salary.PH;
                    worksheet.Cell(rownumber, 22).Value = _salary.SPECIALALLOWANCE;
                    worksheet.Cell(rownumber, 23).Value = _salary.OTHERALLOWANCE;
                    worksheet.Cell(rownumber, 24).Value = _salary.OTPAYMENT;
                    worksheet.Cell(rownumber, 25).Value = _salary.GROSS_INCOME;
                    worksheet.Cell(rownumber, 26).Value = _salary.PF;
                    worksheet.Cell(rownumber, 27).Value = _salary.PT;
                    worksheet.Cell(rownumber, 28).Value = _salary.ESIC;
                    worksheet.Cell(rownumber, 29).Value = _salary.CANTEEN;
                    worksheet.Cell(rownumber, 30).Value = _salary.HRA_DEDUCT;
                    worksheet.Cell(rownumber, 31).Value = _salary.TRAVEL;
                    worksheet.Cell(rownumber, 32).Value = _salary.ADVANCE;
                    worksheet.Cell(rownumber, 33).Value = _salary.Uniform;
                    worksheet.Cell(rownumber, 34).Value = _salary.Welfare;
                    worksheet.Cell(rownumber, 35).Value = _salary.Other;
                    worksheet.Cell(rownumber, 36).Value = _salary.GROSS_DEDUCTION;
                    worksheet.Cell(rownumber, 37).Value = _salary.NET_INCOME;
                }
                var memoryStream = new System.IO.MemoryStream();
                workbook.SaveAs(memoryStream);
                memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
                return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", name + "_" + month.ToString() + "_" + year.ToString() + ".xlsx");
            }
            else if (Command == "Downloadsalary")
            {
                string name = "";

                //  List<UserMaster> users = db.UserMasters.Where(x => x.Company_ID == _LogedUser.Company_ID && x.Project_SalaryGroupID == Project_SalaryGroupID).ToList();
                ProjectSalaryGroup SalaryGroup = db.ProjectSalaryGroups.Where(x => x.Company_ID == _LogedUser.Company_ID && x.Project_SalaryGroupID == Project_SalaryGroupID).FirstOrDefault();
                List<ProjectSalary> SalarySlips = db.ProjectSalaries.Where(x => x.Company_ID == _LogedUser.Company_ID &&
                                                    x.UserMaster.Project_SalaryGroupID == Project_SalaryGroupID && x.SalaryMonth.Month == month && x.SalaryMonth.Year == year).ToList();
                name = "SalarySheet_" + SalaryGroup.Project_Name.ToString() + "_";


                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Employee");
                //List<UserMaster> users = db.UserMasters.Where(x => x.ProjectSalaryHead == ProjectSalaryHeadndid).ToList();

                //First Row Blank
                //worksheet.Row(1).Hide();

                worksheet.Range("A2:J2").Merge();
                worksheet.Cell(2, 1).Value = "Salary Sheet";
                worksheet.Cell(2, 1).Style.Font.Bold = true;
                worksheet.Cell(2, 1).Style.Font.FontSize = 14;
                worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(2, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                worksheet.Range("A3:J3").Merge();
                worksheet.Cell(3, 1).Value = SalaryGroup.Project_Name;
                worksheet.Cell(3, 1).Style.Font.Bold = true;
                worksheet.Cell(3, 1).Style.Font.FontSize = 18;
                worksheet.Cell(3, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(3, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                worksheet.Range("A4:J4").Merge();
                worksheet.Cell(4, 1).Value = SalaryGroup.Project_Address;
                worksheet.Cell(4, 1).Style.Font.FontSize = 12;
                worksheet.Cell(4, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(4, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                worksheet.Range("A5:J5").Merge();
                worksheet.Cell(5, 1).Value = "Month: " + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month) + " " + year;
                worksheet.Cell(5, 1).Style.Font.FontSize = 12;
                worksheet.Cell(5, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(5, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                // Column Headers (starting from the 6th row)
                worksheet.Cell(6, 1).Value = "SrNo";
                worksheet.Cell(6, 2).Value = "EmployeeID";
                worksheet.Cell(6, 3).Value = "Employee_Name";
                worksheet.Cell(6, 4).Value = "Bank_Name";
                worksheet.Cell(6, 5).Value = "BankAcNo.";
                worksheet.Cell(6, 6).Value = "IFSC_No"; 
                worksheet.Cell(6, 7).Value = "UAN Number";
                worksheet.Cell(6, 8).Value = "Adharcard_No";
                worksheet.Cell(6, 9).Value = "NET_INCOME";



                //if (designation != null)
                //{
                //    // Add Wage Rate Details
                //    worksheet.Range("AJ2:AK2").Merge();
                //    worksheet.Cell(2, 36).Value = "Wage";
                //    worksheet.Cell(2, 36).Style.Font.FontSize = 10;
                //    worksheet.Cell(2, 36).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                //    worksheet.Cell(2, 36).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                //    worksheet.Cell(2, 36).Style.Font.Bold = true;
                //    worksheet.Cell(2, 36).Value = "Wage Rate";
                //}


                for (int col = 1; col <= 9; col++) // Looping through all columns
                {
                    worksheet.Cell(6, col).Style.Font.Bold = true;
                }
                worksheet.Range("A6:H6").SetAutoFilter();

                // Now populating the rows
                int rownumber = 6;  // Start from the 7th row (index 6)
                foreach (ProjectSalary _salary in SalarySlips)
                {
                    rownumber++;
                    worksheet.Cell(rownumber, 1).Value = rownumber - 6;
                    worksheet.Cell(rownumber, 2).Value = _salary.UserMaster.EmployeeID;
                    worksheet.Cell(rownumber, 3).Value = _salary.UserMaster.User_Name;
                    worksheet.Cell(rownumber, 4).Value = _salary.UserMaster.Bank_Name;
                    worksheet.Cell(rownumber, 5).Value = _salary.UserMaster.Bank_Acc_No;
                    worksheet.Cell(rownumber, 6).Value = _salary.UserMaster.IFSC_No;
                    worksheet.Cell(rownumber, 7).Value = _salary.UserMaster.UAN_No;
                    worksheet.Cell(rownumber, 8).Value = _salary.UserMaster.Adharcard_No;
                    worksheet.Cell(rownumber, 9).Value = _salary.NET_INCOME;
                }
                var memoryStream = new System.IO.MemoryStream();
                workbook.SaveAs(memoryStream);
                memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
                return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", name + "_" + month.ToString() + "_" + year.ToString() + ".xlsx");
            }
            else
            {
                if (UploadedFile == null)
                {
                    TempData["ErrorMessage"] = "Sorry, No File Found.";
                    return RedirectToAction("ProjectSalaryCreater", new { month = month, year = year, Project_SalaryGroupID = Project_SalaryGroupID });
                }
                using (var workbook = new XLWorkbook(UploadedFile.InputStream))
                {
                    var worksheet = workbook.Worksheet(1);
                    var rows = worksheet.RowsUsed().Skip(1); // Skip header row
                    if (rows.Count() == 0)
                    {
                        TempData["ErrorMessage"] = "Please check Data In Excle File.";
                        return RedirectToAction("ProjectSalaryCreater", new { month = month, year = year, Project_SalaryGroupID = Project_SalaryGroupID });
                    }
                    else
                    {
                        try
                        {
                            List<ProjectSalaryHead> salaryheads = db.ProjectSalaryHeads.ToList();
                            ProjectSalaryHead _salaryhead;
                            if (salaryheads == null)
                            {
                                TempData["ErrorMessage"] = "In Valid Salary Head";
                                return RedirectToAction("ProjectSalaryCreater", new { month = month, year = year, Project_SalaryGroupID = Project_SalaryGroupID });
                            }
                            foreach (var row in rows)
                            {
                                _salaryhead = null;
                                int UserId = int.Parse(row.Cell(2).GetValue<string>());
                                int ShiftHours = int.Parse(row.Cell(7).GetValue<string>());
                                int NoOfDays = int.Parse(row.Cell(8).GetValue<string>());
                                decimal advanceamt = decimal.Parse(row.Cell(9).GetValue<string>());
                                if(advanceamt!=0 || advanceamt!=null)
                                {
                                    int lastDay = DateTime.DaysInMonth(year, month);
                                    DateTime lastDate = new DateTime(year, month, lastDay);
                                    Request_Master Request = new Request_Master();
                                    Request.User_Id = UserId;
                                    Request.Requested_Amount = advanceamt;
                                    Request.Place = "";
                                    Request.Parent_Type = "Personal Advance";
                                    Request.Description = "";
                                    Request.A_Date = lastDate;
                                    Request.S_Date = lastDate; ;
                                    Request.E_Date = lastDate;
                                    Request.Granted_Amount = advanceamt;
                                    Request.Status = 1;
                                    Request.ReqDate = DateTime.Now;
                                    Request.Remark = "";
                                    Request.Remark_For_Admin = "";
                                    db.Request_Master.Add(Request);
                                    db.SaveChanges();
                                }
                                //int ActualOT = int.Parse(row.Cell(9).GetValue<string>());
                                //int ActualOTHours = int.Parse(row.Cell(10).GetValue<string>());
                                //int ActualPH = int.Parse(row.Cell(11).GetValue<string>());
                                int ActualOT = 0;
                                int ActualOTHours = 0;
                                int ActualPH = 0;
                                bool IsNew = false;
                                ProjectSalary _salary = db.ProjectSalaries.Where(x => x.User_ID == UserId && x.SalaryMonth.Year == year && x.SalaryMonth.Month == month).FirstOrDefault();
                                UserMaster _user = db.UserMasters.Where(x => x.User_ID == UserId).FirstOrDefault();

                                if (_salary == null)
                                {
                                    IsNew = true;
                                    _salary = new ProjectSalary();
                                }
                                _salaryhead = salaryheads.Where(x => x.User_ID == _user.User_ID).FirstOrDefault();
                                if (_salaryhead == null)
                                {
                                    continue;
                                }
                                _salary.ProjectSalaryHeadID = _salaryhead.ProjectSalaryHeadID;
                                _salary.Company_ID = _LogedUser.Company_ID;
                                _salary.User_ID = _user.User_ID;
                                _salary.SalaryMonth = new DateTime(year, month, 1);
                                _salary.NoOfDays = NoOfDays;
                                _salary.NoOfPH = ActualPH;
                                _salary.NoOfOTHors = ActualOTHours;
                                _salary.ShiftHours = ShiftHours;
                                _salary.NoOfOTShift = ActualOT;

                                _salary.BASIC = SalaryFormulaResolver.ResolveFormula(_salaryhead.BASIC, _salary);
                                _salary.OTRate = SalaryFormulaResolver.ResolveFormula(_salaryhead.OTRate, _salary, 1);
                                _salary.PHRate = SalaryFormulaResolver.ResolveFormula(_salaryhead.PH, _salary, 1);
                                _salary.HRA = SalaryFormulaResolver.ResolveFormula(_salaryhead.HRA, _salary);
                                _salary.DA = SalaryFormulaResolver.ResolveFormula(_salaryhead.DA, _salary);
                                _salary.LEAVE = SalaryFormulaResolver.ResolveFormula(_salaryhead.LEAVE, _salary);
                                _salary.BONUS = SalaryFormulaResolver.ResolveFormula(_salaryhead.BONUS, _salary);
                                _salary.TA = SalaryFormulaResolver.ResolveFormula(_salaryhead.TA, _salary);
                                decimal totalspallowance = new decimal();
                                List<Voucher_Master> vm = db.Voucher_Master.Where(x => x.User_Id == _user.User_ID && x.Voucher_Date.Month == month && x.Voucher_Date.Year == year && x.Status == 1).ToList();
                                foreach (Voucher_Master v in vm)
                                {
                                    if (v.Trans_Type == 1 && v.Payable_Amount != 0)
                                    {
                                        totalspallowance = (decimal.Parse(v.Payable_Amount.ToString())) + decimal.Parse(totalspallowance.ToString());
                                    }
                                }
                                _salary.SPECIALALLOWANCE = totalspallowance + SalaryFormulaResolver.ResolveFormula(_salaryhead.SPECIALALLOWANCE, _salary);
                                _salary.OTHERALLOWANCE = SalaryFormulaResolver.ResolveFormula(_salaryhead.OTHERALLOWANCE, _salary);
                                _salary.OTPAYMENT = Math.Round(ActualOTHours * SalaryFormulaResolver.ResolveFormula(_salaryhead.OTRate, _salary, 1));
                                _salary.PH = Math.Round(ActualPH * SalaryFormulaResolver.ResolveFormula(_salaryhead.PH, _salary, 1));
                                _salary.GROSS_INCOME = _salary.BASIC + _salary.HRA + _salary.DA + _salary.LEAVE + _salary.BONUS + _salary.TA + _salary.PH + _salary.SPECIALALLOWANCE + _salary.OTHERALLOWANCE + _salary.OTPAYMENT;

                                _salary.PF = SalaryFormulaResolver.ResolveFormula(_salaryhead.PF, _salary);
                                _salary.PT = SalaryFormulaResolver.ResolveFormula(_salaryhead.PT, _salary);
                                if (_salary.PT > 100) { _salary.PT = 100; }
                                _salary.ESIC = SalaryFormulaResolver.ResolveFormula(_salaryhead.ESIC, _salary);
                                _salary.CANTEEN = SalaryFormulaResolver.ResolveFormula(_salaryhead.CANTEEN, _salary);
                                _salary.HRA_DEDUCT = SalaryFormulaResolver.ResolveFormula(_salaryhead.HRA_DEDUCT, _salary);
                                _salary.TRAVEL = SalaryFormulaResolver.ResolveFormula(_salaryhead.TRAVEL, _salary);
                                decimal totalad = new decimal();
                                List<Request_Master> rm = db.Request_Master.Where(x => x.User_Id == _user.User_ID && x.Parent_Type == "Personal Advance" && x.S_Date.Month == month && x.S_Date.Year == year && x.Status == 1).ToList();
                                foreach (Request_Master r in rm)
                                {
                                    totalad = decimal.Parse(r.Granted_Amount.ToString()) + totalad;
                                }
                                foreach (Voucher_Master v in vm)
                                {
                                    if (v.Trans_Type == 2 && v.Payable_Amount != 0)
                                    {
                                        totalad = (@Math.Abs(decimal.Parse(v.Payable_Amount.ToString()))) + decimal.Parse(totalad.ToString());
                                    }

                                }
                                _salary.ADVANCE = totalad + SalaryFormulaResolver.ResolveFormula(_salaryhead.ADVANCE, _salary);
                                // _salary.ADVANCE =  _salary.ADVANCE + (approved voucher sum)
                                _salary.Uniform = SalaryFormulaResolver.ResolveFormula(_salaryhead.Uniform, _salary);
                                _salary.Welfare = SalaryFormulaResolver.ResolveFormula(_salaryhead.Welfare, _salary);
                                _salary.Other = SalaryFormulaResolver.ResolveFormula(_salaryhead.Other, _salary);
                                _salary.GROSS_DEDUCTION = _salary.PF + _salary.PT + _salary.ESIC + _salary.CANTEEN + _salary.HRA_DEDUCT + _salary.TRAVEL + _salary.ADVANCE + _salary.Uniform + _salary.Welfare + _salary.Other;
                                _salary.NET_INCOME = _salary.GROSS_INCOME - _salary.GROSS_DEDUCTION;
                                if (IsNew) db.ProjectSalaries.Add(_salary);
                            }
                            db.SaveChanges();
                            return RedirectToAction("ProjectSalaryCreater", new { month = month, year = year, Project_SalaryGroupID = Project_SalaryGroupID });
                        }
                        catch(Exception ex)
                        {
                            //ViewBag.msg = ex.Message;
                            //return View();
                            TempData["ErrorMessage"] = ex.Message;
                            TempData["ToastType"] = "danger";
                            return RedirectToAction("ProjectSalaryCreater", new { month = month, year = year, Project_SalaryGroupID = Project_SalaryGroupID });
                        }
                    }
                }
                return RedirectToAction("ProjectSalaryView", new { month = month, year = year, Project_SalaryGroupID = Project_SalaryGroupID });
            }
        }
        public static bool IsWageFormula(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            // Try to parse it as a decimal
            return !decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out _);
        }
        [HttpPost]
        public ActionResult ProjectSalaryUploadDirect(int month = 99, int year = 99)
        {
            if (month == 99)
            {
                month = DateTime.Now.Month;
            }
            if (year == 99)
            {
                year = DateTime.Now.Year;
            }
            ViewBag.PageTitle = "All Department";

            List<ProjectSalaryHead> salaryheads = db.ProjectSalaryHeads.Where(x => x.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.ProjectSalaryHeadID).ToList();
            ViewBag.salaryheads = salaryheads;

            List<ProjectSalary> projectSalaries = db.ProjectSalaries.Where(x => x.SalaryMonth.Month == month && x.SalaryMonth.Year == year).ToList();
            ViewBag.Project_Salaries = projectSalaries;
            return View();
        }



        public ActionResult SalaryGroups()
        {
            ViewBag.PageTitle = "All Head";
            List<ProjectSalaryGroup> project_Head_Masters = db.ProjectSalaryGroups.Where(x => x.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.Project_SalaryGroupID).ToList();
            ViewBag.project_Head_Masters = project_Head_Masters;
            return View();
        }


        [HttpPost]
        public ActionResult SalaryGroups_Add(FormCollection form)
        {
            long Project_MasterID = long.Parse(form["Project_MasterID"] ?? "0");
            string command = form["Command"] ?? string.Empty;


            if (command == "Save,")
            {
                string Project_Name = form["Project_Name"]?.Trim();
                string Project_Address = form["Project_Address"]?.Trim();
                if (string.IsNullOrEmpty(Project_Name))
                {
                    TempData["Result"] = "Failure^Please Enter Project Name!";
                    return RedirectToAction("Head");
                }
                if (Project_MasterID == 0)
                {
                    var duplicatehead = db.ProjectSalaryGroups
                                    .FirstOrDefault(x => x.Project_Name == Project_Name && x.Project_SalaryGroupID != Project_MasterID);

                    if (duplicatehead != null)
                    {
                        TempData["Result"] = "Failure^Salary Group Already Exists!";
                        return RedirectToAction("Head");
                    }
                    var newprojecthead = new ProjectSalaryGroup
                    {
                        Company_ID = _LogedUser.Company_ID,
                        Project_Name = Project_Name,
                        Project_Address = Project_Address
                    };

                    db.ProjectSalaryGroups.Add(newprojecthead);
                    db.SaveChangesAsync();

                    TempData["Result"] = "New Salary Group Added Successfully.";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "New Salary Group Added Successfully!", Refresh = "Default" });
                }
                else
                {
                    ProjectSalaryGroup d = db.ProjectSalaryGroups.Where(x => x.Project_SalaryGroupID == Project_MasterID).FirstOrDefault();
                    if (d == null)
                    {
                        TempData["failure"] = "Invalid Head  !";
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Salary Group Head!" });
                    }
                    d.Project_Name = Project_Name;
                    d.Project_Address = Project_Address;
                    db.SaveChanges();
                    TempData["Result"] = "Salary Group Updated Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Salary Group Updated Successfully!", Refresh = "Default" });
                }
            }
            else if (command == "Delete")
            {
                ProjectSalaryGroup d = db.ProjectSalaryGroups.Where(x => x.Project_SalaryGroupID == Project_MasterID).FirstOrDefault();
                var _delt = db.ProjectSalaryGroups.Find(Project_MasterID);
                if (d == null)
                {
                    TempData["failure"] = "Invalid Salary Group !";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Salary Group !" });
                }
                db.ProjectSalaryGroups.Remove(_delt);
                db.SaveChanges();
                TempData["failure"] = "Salary Group Deleted Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Salary Group Deleted Successfully!", Refresh = "Default" });
            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !" });
            }
        }




        private string GetExcelColumnName(int columnNumber)
        {
            string columnName = "";

            while (columnNumber > 0)
            {
                int modulo = (columnNumber - 1) % 26;
                columnName = Convert.ToChar(65 + modulo) + columnName;
                columnNumber = (columnNumber - modulo) / 26;
            }

            return columnName;
        }

        public static bool IsSafeFormula(string formula, List<string> allowedVariables)
        {
            if (string.IsNullOrWhiteSpace(formula))
                return false;

            formula = formula.Trim();

            // 1. Basic safety pattern
            var allowedPattern = new Regex(@"^[0-9a-zA-Z\s\+\-\*\/\.\(\)]+$");
            if (!allowedPattern.IsMatch(formula))
                return false;

            // 2. Work with lowercase versions
            string formulaCopy = formula.ToLower();
            var allowedLower = allowedVariables.Select(v => v.ToLower()).ToList();

            // 3. Replace allowed variables with dummy values
            foreach (var variable in allowedLower)
            {
                formulaCopy = Regex.Replace(
                    formulaCopy,
                    $@"\b{Regex.Escape(variable)}\b",
                    "1",
                    RegexOptions.IgnoreCase
                );
            }

            // 4. If formula still contains any letters, it's unsafe
            if (Regex.IsMatch(formulaCopy, "[a-z]"))
                return false;

            // 5. Try evaluating the formula
            try
            {
                var dt = new System.Data.DataTable();
                var result = dt.Compute(formulaCopy, "");
                return double.TryParse(result.ToString(), out _);
            }
            catch
            {
                return false;
            }
        }


        public ActionResult TaskType_List()
        {

            ViewBag.PageTitle = "All TaskType";
            List<TaskType> task = db.TaskTypes.Where(x => x.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.TaskType_Id).ToList();
            ViewBag.task = task;
            return View();
        }
        [HttpPost]
        public ActionResult TaskType_Add(FormCollection form)
        {

            long TaskType_Id = long.Parse(form["TaskType_Id"].ToString());
            string TaskType_Name = form["TaskType_Name"].ToString();
            string Cmd = form["Command"].ToString();
            if (Cmd == "Save")
            {
                if (TaskType_Id == 0)
                {
                    TaskType t = db.TaskTypes.Where(x => x.TaskType_Name == TaskType_Name && x.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
                    if (t != null)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "TaskType already Exist !" });
                    }
                    TaskType task = new TaskType();
                    task.TaskType_Name = TaskType_Name;
                    task.Company_ID = _LogedUser.Company_ID;
                    db.TaskTypes.Add(task);
                    db.SaveChanges();
                    TempData["Result"] = "New TaskType Added Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "New TaskType Added Successfully !", Refresh = "Default" });
                }
                else
                {
                    TaskType taskType = db.TaskTypes.Where(x => x.TaskType_Id == TaskType_Id).FirstOrDefault();
                    if (taskType == null)
                    {
                        TempData["failure"] = "Invalid TaskType  !";
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid TaskType  !" });
                    }

                    taskType.TaskType_Name = TaskType_Name;
                    taskType.Company_ID = _LogedUser.Company_ID;
                    db.SaveChanges();
                    TempData["Result"] = "TaskType Updated Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "TaskType Updated Successfully !", Refresh = "Default" });
                }
            }
            else if (Cmd == "Delete")
            {
                TaskType t = db.TaskTypes.Where(x => x.TaskType_Id == TaskType_Id).FirstOrDefault();
                var _delt = db.TaskTypes.Find(TaskType_Id);
                if (t == null)
                {
                    TempData["failure"] = "Invalid TaskType  !";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid TaskType  !" });
                }

                db.TaskTypes.Remove(_delt);
                db.SaveChanges();
                TempData["failure"] = "TaskType Deleted Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "TaskType Deleted Successfully !", Refresh = "Default" });
            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !" });
            }

        }
        public ActionResult TaskType_Delete(FormCollection form)
        {
            long TaskType_Id = long.Parse(form["tasktype_id"].ToString());
            var _delt = db.TaskTypes.Find(TaskType_Id);
            if (_delt == null)
            {
                TempData["failure"] = "Invalid TaskType  !";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid TaskType  !" });
            }

            db.TaskTypes.Remove(_delt);
            db.SaveChanges();
            TempData["failure"] = "TaskType Deleted Successfully !";
            return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "TaskType Deleted Successfully !", Refresh = "Default" });
        }
        public ActionResult Asset_List()
        {
            ViewBag.PageTitle = "All Assets";
            ViewBag.PageDescription = "All Assets Available in the Company";

            List<Asset_Master> _Asset = db.Asset_Master.Where(x => x.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.Asset_id).ToList();
            List<Asset_type> Ast = db.Asset_type.Where(x => x.Company_ID == _LogedUser.Company_ID).ToList();
            List<Asset_Movement> amt = db.Asset_Movement.OrderByDescending(x => x.Movement_id).ToList();
            ViewBag.amt = amt;
            ViewBag.Ast = Ast;
            ViewBag.Asset = _Asset;
            return View();
        }
        [HttpPost]
        public ActionResult Asset_Add(FormCollection form)
        {

            long Asset_id = long.Parse(form["Asset_id"].ToString());
            string Asset_Name = form["Asset_Name"].ToString();
            long Asset_Type = long.Parse(form["Asset_Type"].ToString());
            string Serial_Number = form["Serial_Number"].ToString();
            DateTime Purchase_Date = DateTime.Parse(form["Purchase_Date"].ToString());
            int Service_Duration = int.Parse(form["Service_Duration"].ToString());
            string Detail = form["Detail"].ToString();
            string Cmd = form["Command"].ToString();
            int Status = int.Parse(form["Status"].ToString());
            if (Cmd == "Save")
            {
                if (Asset_id == 0)
                {
                    Asset_Master asset = new Asset_Master();
                    asset.Asset_Name = Asset_Name;
                    asset.Asset_Type = Asset_Type;
                    asset.Serial_Number = Serial_Number;
                    if (Purchase_Date <= DateTime.Now)
                    {
                        asset.Purchase_Date = Purchase_Date;
                    }
                    else
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = " Asset Not added Date Is After Today....!" });
                    }
                    if (Service_Duration >= 0)
                    {
                        asset.Service_Duration = Service_Duration;
                    }
                    else
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = " Service Duration should be a positive noumer or 0....!" });
                    }
                    asset.Detail = Detail;
                    asset.Status = Status;
                    asset.Company_ID = _LogedUser.Company_ID;
                    db.Asset_Master.Add(asset);
                    db.SaveChanges();
                    TempData["Result"] = "New Asset Added Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "New Asset Added Successfully !", Refresh = "Default" });
                }
                else
                {
                    Asset_Master asset = db.Asset_Master.Where(x => x.Asset_id == Asset_id).FirstOrDefault();
                    if (asset == null)
                    {
                        TempData["failure"] = "Invalid Asset  !";
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Asset  !", Refresh = "Default" });
                    }
                    asset.Asset_Name = Asset_Name;
                    asset.Asset_Type = Asset_Type;
                    asset.Serial_Number = Serial_Number;
                    if (Purchase_Date <= DateTime.Now)
                    {
                        asset.Purchase_Date = Purchase_Date;
                    }
                    else
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = " Asset Not added Date Is After Today....!" });

                    }
                    if (Service_Duration >= 0)
                    {
                        asset.Service_Duration = Service_Duration;
                    }
                    else
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = " Service Duration should be a positive number or 0....!" });
                    }
                    asset.Detail = Detail;
                    asset.Status = Status;
                    asset.Company_ID = _LogedUser.Company_ID;
                    db.SaveChanges();
                    TempData["Result"] = "Asset Updated Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Asset Updated Successfully !", Refresh = "Default" });
                }
            }
            else if (Cmd == "Delete")
            {
                Asset_Master d = db.Asset_Master.Where(x => x.Asset_id == Asset_id).FirstOrDefault();
                var _delt = db.Designation_Master.Find(Asset_id);
                if (d == null)
                {
                    TempData["failure"] = "Invalid Asset !";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Asset !", Refresh = "Default" });
                }
                if (_delt.UserMasters.Count > 0)
                {
                    TempData["failure"] = "Asset Has User Please Delete First...!";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Asset Has User Please Delete First...!", Refresh = "Default" });
                }
                d.Status = 6;
                db.SaveChanges();
                TempData["failure"] = "Asset Deleted Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Asset Deleted Successfully !", Refresh = "Default" });
            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !" });
            }
        }
        public ActionResult Asset_Delete(FormCollection form)
        {
            long Asset_id = long.Parse(form["Assetid"].ToString());
            Asset_Master asset = db.Asset_Master.Where(x => x.Asset_id == Asset_id).FirstOrDefault();
            var _delt = db.Asset_Master.Find(Asset_id);
            if (asset == null)
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Asset  !" });
            }
            db.Asset_Master.Remove(_delt);
            db.SaveChanges();
            TempData["failure"] = "Asset Deleted Successfully !";
            return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Asset Deleted Successfully !", Refresh = "Default" });
        }
        public ActionResult Asset_Movement_List(long Asset_id)
        {
            ViewBag.PageTitle = "All Asset Movement";

            Asset_Master asset = db.Asset_Master.Where(x => x.Asset_id == Asset_id & x.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
            ViewBag.asset = asset;
            List<UserMaster> user = db.UserMasters.Where(x => x.Company_ID == _LogedUser.Company_ID && x.status == 0).ToList();
            ViewBag.user = user;
            List<Asset_Movement> amt = db.Asset_Movement.Where(x => x.Asset_id == Asset_id).OrderByDescending(x => x.Movement_id).ToList();
            if (amt.Count != 0)
            {
                long AssetMovement_id = amt.Max(x => x.Movement_id);
                Asset_Movement Ast = db.Asset_Movement.Where(x => x.Movement_id == AssetMovement_id & x.Asset_id == Asset_id).FirstOrDefault();
                ViewBag.Ast = Ast;
            }
            return View();
        }
        [HttpPost]
        public ActionResult Asset_Movement_Add(FormCollection form)
        {

            long Movement_id = long.Parse(form["Movement_id"].ToString());
            long Asset_id = long.Parse(form["Asset_id"].ToString());
            DateTime mvdt = DateTime.Parse(form["Movement_Date"].ToString());
            long From_id = long.Parse(form["From_id"].ToString());
            long To_id = long.Parse(form["To_id"].ToString());
            string Detail = form["Detail"].ToString();
            string Cmd = form["Command"].ToString();
            if (Cmd == "Save")
            {
                Asset_Master ast = db.Asset_Master.Where(x => x.Asset_id == Asset_id).FirstOrDefault();
                if (ast.Purchase_Date > mvdt)
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Movement Date Not More Than Puchase Date.....!" });
                }
                Asset_Movement _Movement = db.Asset_Movement.Where(x => x.Asset_id == Asset_id).OrderByDescending(y => y.Movement_id).FirstOrDefault();
                if (_Movement != null)
                {
                    if (mvdt < _Movement.Movement_Date)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Movement Date Not Added Before Movement Date." });
                    }

                }
                Asset_Movement movement = new Asset_Movement();
                movement.Asset_id = Asset_id;
                movement.From_id = From_id;
                movement.To_id = To_id;
                movement.Movement_Date = mvdt;
                movement.Detail = Detail;
                db.Asset_Movement.Add(movement);
                db.SaveChanges();
                TempData["Result"] = "New Movement Added Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "New Movement Added Successfully !", Refresh = "Default" });
            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !" });
            }
        }
        public ActionResult Gift_List()
        {

            ViewBag.PageTitle = "All Gift";
            List<Gift_Master> gift = db.Gift_Master.Where(x => x.Company_ID == _LogedUser.Company_ID).ToList();
            ViewBag.gift = gift;
            List<UserMaster> user_detail = db.UserMasters.Where(x => x.User_type != _LogedUser.User_type && x.Company_ID == _LogedUser.Company_ID && x.status == 0).ToList();
            List<Festival_Master> Fest = db.Festival_Master.ToList();
            ViewBag.Fest = Fest;
            ViewBag.user = user_detail;
            return View();
        }
        [HttpPost]
        public ActionResult Gift_Add(FormCollection form)
        {

            long Gift_id = long.Parse(form["Gift_Id"].ToString());
            string Gift_Name = form["Gift_Name"].ToString();
            long User_Id = long.Parse(form["User_Id"].ToString());
            string Gift_Desc = form["Gift_Desc"].ToString();
            DateTime date = DateTime.Parse(form["Date"].ToString());
            string Cmd = form["Command"].ToString();
            if (Cmd == "Save")
            {
                if (Gift_id == 0)
                {
                    Gift_Master gift = new Gift_Master();
                    if (form["Festival_Id"] == "")
                    {

                    }
                    else
                    {
                        gift.Festival_Id = long.Parse(form["Festival_Id"].ToString());
                    }
                    gift.Gift_Name = Gift_Name;
                    gift.User_Id = User_Id;
                    gift.Gift_Desc = Gift_Desc;
                    gift.Date = date;
                    gift.Company_ID = _LogedUser.Company_ID;
                    db.Gift_Master.Add(gift);
                    db.SaveChanges();
                    TempData["Result"] = "New Gift Added Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "New Gift Added Successfully !", Refresh = "Default" });
                }
                else
                {
                    Gift_Master gift = db.Gift_Master.Where(x => x.Gift_Id == Gift_id).FirstOrDefault();
                    if (gift == null)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Gift  !" });
                    }
                    if (form["Festival_Id"] == "")
                    {

                    }
                    else
                    {
                        gift.Festival_Id = long.Parse(form["Festival_Id"].ToString());
                    }
                    gift.Gift_Name = Gift_Name;
                    gift.User_Id = User_Id;
                    gift.Gift_Desc = Gift_Desc;
                    gift.Date = date;
                    gift.Company_ID = _LogedUser.Company_ID;
                    db.SaveChanges();
                    TempData["Result"] = "Gift Updated Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Gift Updated Successfully !", Refresh = "Default" });
                }
            }
            else if (Cmd == "Delete")
            {
                Gift_Master gift = db.Gift_Master.Where(x => x.Gift_Id == Gift_id).FirstOrDefault();
                var _delt = db.Gift_Master.Find(Gift_id);
                if (gift == null)
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Gift  !" });
                }
                db.Gift_Master.Remove(_delt);
                db.SaveChanges();
                TempData["Result"] = "Gift Deleted Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Gift Deleted Successfully !", Refresh = "Default" });
            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !" });
            }
        }
        public ActionResult Gift_Delete(FormCollection form)
        {
            long Gift_id = long.Parse(form["gift_id"].ToString());
            Gift_Master gift = db.Gift_Master.Where(x => x.Gift_Id == Gift_id).FirstOrDefault();
            var _delt = db.Gift_Master.Find(Gift_id);
            if (gift == null)
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Gift  !" });
            }
            db.Gift_Master.Remove(_delt);
            db.SaveChanges();
            TempData["failure"] = "Gift Deleted Successfully !";
            return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Gift Deleted Successfully !", Refresh = "Default" });
        }

        public ActionResult Holiday_List()
        {

            ViewBag.PageTitle = "All Holiday";
            List<Holiday_Master> holiday = db.Holiday_Master.Where(x => x.Company_Id == _LogedUser.Company_ID).ToList();
            ViewBag.holiday = holiday;

            return View();
        }
        [HttpPost]
        public ActionResult Holiday_Add(FormCollection form)
        {

            long Holiday_Id = long.Parse(form["Holiday_Id"].ToString());
            string Holiday_Name = form["Holiday_Name"].ToString();
            DateTime Holiday_Date = DateTime.Parse(form["Holiday_Date"].ToString());
            string Cmd = form["Command"].ToString();
            if (Cmd == "Save")
            {
                if (Holiday_Id == 0)
                {
                    Holiday_Master holiday_check = db.Holiday_Master.Where(x => x.Holiday_Date == Holiday_Date && x.Holiday_Name == Holiday_Name && x.Company_Id == _LogedUser.Company_ID).FirstOrDefault();
                    if (holiday_check != null)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Holiday Already Exits...!" });
                    }
                    Holiday_Master holiday = new Holiday_Master();
                    holiday.Holiday_Name = Holiday_Name;
                    holiday.Holiday_Date = Holiday_Date;
                    holiday.Company_Id = _LogedUser.Company_ID;
                    db.Holiday_Master.Add(holiday);
                    db.SaveChanges();
                    TempData["Result"] = "New holiday Added Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "New holiday Added Successfully !", Refresh = "Default" });
                }
                else
                {
                    Holiday_Master holiday = db.Holiday_Master.Where(x => x.Holiday_Id == Holiday_Id).FirstOrDefault();

                    Holiday_Master holiday_check = db.Holiday_Master.Where(x => x.Holiday_Date == Holiday_Date && x.Holiday_Name == Holiday_Name && x.Company_Id == _LogedUser.Company_ID && x.Holiday_Id != Holiday_Id).FirstOrDefault();
                    if (holiday_check != null)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Holiday Already Exits...!" });
                    }
                    if (holiday == null)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid holiday  !" });
                    }
                    holiday.Holiday_Name = Holiday_Name;
                    holiday.Holiday_Date = Holiday_Date;
                    db.SaveChanges();
                    TempData["Result"] = "holiday Updated Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "holiday Updated Successfully !", Refresh = "Default" });
                }
            }
            else if (Cmd == "Delete")
            {
                Holiday_Master holiday = db.Holiday_Master.Where(x => x.Holiday_Id == Holiday_Id).FirstOrDefault();
                var _delt = db.Holiday_Master.Find(Holiday_Id);
                if (holiday == null)
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid holiday  !" });
                }
                db.Holiday_Master.Remove(_delt);
                db.SaveChanges();
                TempData["failure"] = "holiday Deleted Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "holiday Deleted Successfully !", Refresh = "Default" });
            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !" });
            }
        }
        public ActionResult Holiday_Delete(FormCollection form)
        {
            long Holiday_Id = long.Parse(form["holiday_id"].ToString());
            Holiday_Master holiday = db.Holiday_Master.Where(x => x.Holiday_Id == Holiday_Id).FirstOrDefault();
            var _delt = db.Holiday_Master.Find(Holiday_Id);
            if (holiday == null)
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid holiday  !" });
            }
            db.Holiday_Master.Remove(_delt);
            db.SaveChanges();
            TempData["failure"] = "holiday Deleted Successfully !";
            return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "holiday Deleted Successfully !", Refresh = "Default" });
        }
        public ActionResult Shift_List()
        {

            ViewBag.PageTitle = "All Shift";
            List<Shift_Master> Shift = db.Shift_Master.Where(x => x.Company_ID == _LogedUser.Company_ID).ToList();
            ViewBag.Shift = Shift;

            return View();
        }
        [HttpPost]
        public ActionResult Shift_Add(FormCollection form)
        {

            long Shift_Id = long.Parse(form["Shift_Id"].ToString());
            string Shift_Name = form["Shift_Name"].ToString();
            TimeSpan Shift_From = TimeSpan.Parse(form["Shift_From"].ToString());
            TimeSpan Shift_To = TimeSpan.Parse(form["Shift_To"].ToString());
            string Cmd = form["Command"].ToString();
            if (Cmd == "Save")
            {
                if (Shift_Id == 0)
                {

                    Shift_Master shift_check = db.Shift_Master.Where(x => x.Shift_From == Shift_From && x.Shift_To == Shift_To && x.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
                    if (shift_check != null)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Shift Already Exits...!" });
                    }
                    Shift_Master shift_check_nm = db.Shift_Master.Where(x => x.Shift_Name == Shift_Name && x.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
                    if (shift_check_nm != null)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Shift Already Exits...!" });
                    }
                    Shift_Master shift = new Shift_Master();
                    shift.Shift_Name = Shift_Name;
                    shift.Shift_From = Shift_From;
                    shift.Shift_To = Shift_To;
                    shift.Company_ID = _LogedUser.Company_ID;
                    db.Shift_Master.Add(shift);
                    db.SaveChanges();
                    TempData["Result"] = "New Shift Added Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "New Shift Added Successfully !", Refresh = "Default" });
                }
                else
                {
                    Shift_Master shift = db.Shift_Master.Where(x => x.Shift_Id == Shift_Id).FirstOrDefault();
                    if (shift == null)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Shift  !" });
                    }
                    shift.Shift_Name = Shift_Name;
                    shift.Shift_From = Shift_From;
                    shift.Shift_To = Shift_To;
                    db.SaveChanges();
                    TempData["Result"] = "Shift Updated Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Shift Updated Successfully !", Refresh = "Default" });
                }
            }
            else if (Cmd == "Delete")
            {
                Shift_Master shift = db.Shift_Master.Where(x => x.Shift_Id == Shift_Id).FirstOrDefault();
                var _delt = db.Shift_Master.Find(Shift_Id);
                if (shift == null)
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Shift  !" });
                }
                db.Shift_Master.Remove(_delt);
                db.SaveChanges();
                TempData["Result"] = "Shift Deleted Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Shift Deleted Successfully !", Refresh = "Default" });
            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !" });
            }
        }
        public ActionResult Shift_Delete(FormCollection form)
        {
            long Shift_Id = long.Parse(form["Shift_Id"].ToString());
            Shift_Master shift = db.Shift_Master.Where(x => x.Shift_Id == Shift_Id).FirstOrDefault();
            var _delt = db.Shift_Master.Find(Shift_Id);
            if (shift == null)
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Shift  !" });
            }
            db.Shift_Master.Remove(_delt);
            db.SaveChanges();
            TempData["failure"] = "Shift Deleted Successfully !";
            return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Shift Deleted Successfully !", Refresh = "Default" });
        }


        public ActionResult Project_List(long? id)
        {
            ViewBag.PageTitle = "All Project";

            List<Project_Master> project = db.Project_Master.Where(x => x.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.Project_Id).ToList();
            List<Project_Assign_Master> project_Assigns = db.Project_Assign_Master.OrderByDescending(x => x.Project_Assign_Id).ToList();
            List<UserMaster> users = db.UserMasters.Where(x => x.User_type != _LogedUser.User_type && x.Company_ID == _LogedUser.Company_ID).ToList();
            if (id != null)
            {

                Project_Master _pro = db.Project_Master.Where(x => x.Project_Id == id && x.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
                if (_pro == null)
                {
                    TempData["Result"] = "Sorry, Selected Project Nt Found ! ";
                    return RedirectToAction("Project_List");
                }
                ViewBag.projectdata = _pro;
            }

            List<UserMaster> empnew = db.UserMasters.Where(x => x.User_type != _LogedUser.User_type && x.Company_ID == _LogedUser.Company_ID && x.status == 0).ToList();
            ViewBag.emp = empnew;

            ViewBag.u = users;
            ViewBag.project_assign = project_Assigns;
            ViewBag.project = project;
            ViewBag.proid = id;
            return View();
        }
        public JsonResult emploPRojectboth(long id)
        {
            List<GetProjectsEmployee_Result> employee_Results = db.GetProjectsEmployee(id).ToList();
            return Json(employee_Results, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult dropdownEmploeName(long id)
        {

            Project_Master _pro = db.Project_Master.Where(x => x.Project_Id == id && x.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
            if (_pro == null)
            {
                TempData["Result"] = "Sorry, Selected Project Not Found ! ";
                //return RedirectToAction("Project_List");
            }
            List<UserMaster> emp = db.UserMasters.Where(x => x.User_type != _LogedUser.User_type && x.Company_ID == _LogedUser.Company_ID && x.status == 0).ToList();
            List<UserMaster> _notincdlued = new List<UserMaster>();
            foreach (UserMaster user in emp)
            {
                bool _found = false;
                foreach (Project_Assign_Master _usr in _pro.Project_Assign_Master)
                {
                    if (user.User_ID == _usr.UserMaster.User_ID)
                    {
                        _found = true;
                    }
                }
                if (_found == false)
                {
                    _notincdlued.Add(user);

                }
            }
            return Json(from obj in _notincdlued.ToList() select new { User_Name = obj.User_Name, User_ID = obj.User_ID }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult Project_Add(FormCollection form)
        {

            long Project_Id = long.Parse(form["Project_Id"].ToString());
            string Project_Name = form["Project_Name"].ToString();
            string Cmd = form["Command"].ToString();
            if (Cmd == "Save")
            {
                if (Project_Id == 0)
                {
                    Project_Master t = db.Project_Master.Where(x => x.Project_Name == Project_Name && x.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
                    if (t != null)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Project already Exist !" });
                    }
                    Project_Master project = new Project_Master();
                    project.Project_Name = Project_Name;
                    project.Company_ID = _LogedUser.Company_ID;
                    db.Project_Master.Add(project);
                    db.SaveChanges();
                    TempData["Result"] = "New Project Added Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "New Project Added Successfully !", Refresh = "Default" });
                }
                else
                {
                    Project_Master p = db.Project_Master.Where(x => x.Project_Id == Project_Id).FirstOrDefault();
                    if (p == null)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Project  !" });
                    }
                    p.Project_Name = Project_Name;
                    p.Company_ID = _LogedUser.Company_ID;
                    db.SaveChanges();
                    TempData["Result"] = "Project Updated Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Project Updated Successfully !", Refresh = "Default" });
                }
            }
            else if (Cmd == "Delete")
            {
                Project_Master p = db.Project_Master.Where(x => x.Project_Id == Project_Id).FirstOrDefault();
                var _del = db.Project_Master.Find(Project_Id);
                if (p == null)
                {

                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Project  !" });
                }
                if (_del.Project_Assign_Master.Count > 0)
                {
                    TempData["failure"] = "Project Has Project Assign Please Delete First...!";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Project Has Project Assign Please Delete First...!", Refresh = "Default" });
                }
                if (_del.Project_Task_Master.Where(x => x.Status != 6).ToList().Count > 0)
                {
                    TempData["failure"] = "Project Has Project Task Please Delete First...!";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Project Has Project Task Please Delete First...!", Refresh = "Default" });
                }
                if (_del.Request_Master.Count > 0)
                {
                    TempData["failure"] = "Project Has Request Please Delete First...!";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Project Has Request Please Delete First...!", Refresh = "Default" });
                }
                if (_del.Voucher_Master.Count > 0)
                {
                    TempData["failure"] = "Project Has Voucher Please Delete First...!";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Project Has Voucher Please Delete First...!", Refresh = "Default" });
                }
                db.Project_Master.Remove(_del);
                db.SaveChanges();
                TempData["failure"] = "Project Deleted Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Project Deleted Successfully !", Refresh = "Default" });
            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !" });
            }

        }
        public ActionResult Project_Delete(FormCollection form)
        {

            long Project_Id = long.Parse(form["Proj_Id"].ToString());
            Project_Master p = db.Project_Master.Where(x => x.Project_Id == Project_Id).FirstOrDefault();
            var _del = db.Project_Master.Find(Project_Id);
            if (p == null)
            {

                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Project  !" });
            }
            if (_del.Project_Assign_Master.Count > 0)
            {
                TempData["failure"] = "Project Has Project Assign Please Delete First...!";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Project Has Project Assign Please Delete First...!", Refresh = "Default" });
            }
            if (_del.Project_Task_Master.Count > 0)
            {
                TempData["failure"] = "Project Has Project Task Please Delete First...!";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Project Has Project Task Please Delete First...!", Refresh = "Default" });
            }
            if (_del.Request_Master.Count > 0)
            {
                TempData["failure"] = "Project Has Request Please Delete First...!";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Project Has Request Please Delete First...!", Refresh = "Default" });
            }
            if (_del.Voucher_Master.Count > 0)
            {
                TempData["failure"] = "Project Has Voucher Please Delete First...!";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Project Has Voucher Please Delete First...!", Refresh = "Default" });
            }
            db.Project_Master.Remove(_del);
            db.SaveChanges();
            TempData["failure"] = "Project Deleted Successfully !";
            return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Project Deleted Successfully !", Refresh = "Default" });
        }
        public ActionResult Project_Assign_List(long Project_Id)
        {
            ViewBag.PageTitle = "All Project Assign";

            Project_Master _pro = db.Project_Master.Where(x => x.Project_Id == Project_Id && x.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
            if (_pro == null)
            {
                TempData["Result"] = "Sorry, elected project not found ! ";
                return RedirectToAction("Project_List");
            }
            List<UserMaster> emp = db.UserMasters.Where(x => x.User_type != _LogedUser.User_type && x.Company_ID == _LogedUser.Company_ID && x.status == 0).ToList();
            ViewBag.emp = emp;
            ViewBag.project = _pro;
            return View();
        }
        [HttpPost]
        public ActionResult Project_Emp_Add(FormCollection form)
        {

            long Assign_Id = long.Parse(form["Assign_Id"].ToString());
            long Project_Id = long.Parse(form["Project_Id"].ToString());
            string Cmd = form["Command"].ToString();
            if (Cmd == "Save")
            {
                if (Assign_Id == 0)
                {
                    long? Emp_Id = long.Parse(form["Emp_Id"].ToString());
                    if (form["Emp_Id"] != null)
                    {
                        Project_Assign_Master project1 = db.Project_Assign_Master.Where(x => x.Project_Id == Project_Id && x.Emp_Id == Emp_Id && x.Project_Assign_Id == Assign_Id).FirstOrDefault();
                        if (project1 == null)
                        {
                            Project_Assign_Master project = new Project_Assign_Master();
                            project.Project_Id = Project_Id;
                            project.Emp_Id = (long)Emp_Id;
                            db.Project_Assign_Master.Add(project);
                            db.SaveChanges();
                            TempData["Result"] = "New Employee Added Successfully !";
                            return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "New Employee Added Successfully !", Refresh = "Default" });
                        }
                        else
                        {
                            TempData["failure"] = "Employee Already Assign !";
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Employee Already Assign !", Refresh = "Default" });
                        }
                    }
                    else
                    {
                        TempData["failure"] = "Please Select Employee !";
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Please Select Employee !", Refresh = "Default" });
                    }
                }
                else
                {
                    TempData["failure"] = "Not Valid!";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Not Valid!", Refresh = "Default" });
                }
            }

            else
            {
                TempData["failure"] = "Invalid Command !";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !", Refresh = "Default" });
            }
        }

        public ActionResult Project_Emp_Delete(FormCollection form, long? id)
        {

            long Assign_Id = long.Parse(form["msgp1"].ToString().TrimEnd().TrimStart());

            Project_Assign_Master p = db.Project_Assign_Master.Where(x => x.Project_Assign_Id == Assign_Id).FirstOrDefault();
            var _del = db.Project_Assign_Master.Find(Assign_Id);
            var _delproject = db.Project_Master.Find(_del.Project_Id);

            if (p == null)
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Project  !" });
            }
            if (_delproject.Project_Task_Master.Where(x => x.Status != 6).ToList().Count > 0)
            {
                TempData["failure"] = "Project Has Project Task Please Delete First...!";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Project Has Project Task Please Delete First...!", Refresh = "Default" });
            }
            if (_delproject.Request_Master.Where(x => x.Status != 6).ToList().Count > 0)
            {
                TempData["failure"] = "Project Has Request Please Delete First...!";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Project Has Request Please Delete First...!", Refresh = "Default" });
            }
            if (_delproject.Voucher_Master.Count > 0)
            {
                TempData["failure"] = "Project Has Voucher Please Delete First...!";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Project Has Voucher Please Delete First...!", Refresh = "Default" });
            }
            db.Project_Assign_Master.Remove(_del);
            db.SaveChanges();
            TempData["failure"] = "Employee Deleted Successfully !";
            return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Employee Deleted Successfully !", Refresh = "Default" });
        }
        public ActionResult Project_Task_List(int status = 101)
        {


            if (status == 101)
            {
                List<Project_Task_Master> Task = db.Project_Task_Master.Where(x => x.Status != 6 && x.UserMaster.Company_ID == _LogedUser.Company_ID).OrderBy(x => x.Status).ThenByDescending(x => x.Task_Id).ToList();
                ViewBag.task = Task;
            }
            if (status == 0)
            {
                List<Project_Task_Master> Task = db.Project_Task_Master.Where(x => x.Status == 0 && x.UserMaster.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.Task_Id).ToList();
                ViewBag.task = Task;
            }
            if (status == 4)
            {
                List<Project_Task_Master> Task = db.Project_Task_Master.Where(x => (x.Status == 4 || x.Status == 5) && x.UserMaster.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.Task_Id).ToList();
                ViewBag.task = Task;
            }
            if (status == 7)
            {
                List<Project_Task_Master> Task = db.Project_Task_Master.Where(x => x.Status == 7 && x.UserMaster.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.Task_Id).ToList();
                ViewBag.task = Task;
            }
            if (status == 1)
            {
                List<Project_Task_Master> Task = db.Project_Task_Master.Where(x => (x.Status == 0 || x.Status == 1) && x.UserMaster.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.Task_Id).ToList();
                ViewBag.task = Task;
            }
            return View();
        }
        public ActionResult Project_Task_Add(long TaskID)
        {
            ViewBag.PageTitle = "Add Project Task";

            Project_Task_Master p = db.Project_Task_Master.Where(x => x.Task_Id == TaskID && x.UserMaster.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
            ViewBag._p = p;
            List<Project_Master> projects = db.Project_Master.Where(x => x.Company_ID == _LogedUser.Company_ID).ToList();
            ViewBag.Project = projects;
            List<TaskType> taskTypes = db.TaskTypes.Where(x => x.Company_ID == _LogedUser.Company_ID).ToList();
            ViewBag.t = taskTypes;
            List<Comment_Master> cmt = db.Comment_Master.Where(x => x.Task_Id == TaskID & x.Comment_Type == 1 & x.UserMaster.Company_ID == _LogedUser.Company_ID).ToList();
            ViewBag._c = cmt;
            List<UserMaster> user = db.UserMasters.Where(x => x.User_type != _LogedUser.User_type && x.Company_ID == _LogedUser.Company_ID && x.status == 0).ToList();
            ViewBag.user = user;
            return View();
        }
        [HttpPost]
        public ActionResult Project_Task_Add(FormCollection form)
        {

            long Emp_Id = long.Parse(form["User_Id"].ToString());
            long Task_Id = long.Parse(form["Task_Id"].ToString());
            long Project_Id = long.Parse(form["Project_Id"].ToString());
            string Task_Name = form["Task_Name"].ToString();
            DateTime A_Start_Date = DateTime.Parse(form["A_Start_Date"].ToString());
            DateTime R_End_Date = DateTime.Parse(form["R_End_Date"].ToString());
            string Duration_Unit = form["Duration_Unit"].ToString();
            long Duration = long.Parse(form["Duration"].ToString());
            string Description = form["Description"].ToString();
            long Task_Type = long.Parse(form["Task_Type"].ToString());

            string Cmd = form["Command"].ToString();
            if (Cmd == "Save")
            {
                if (Task_Id == 0)
                {

                    if (A_Start_Date > R_End_Date)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Please Add Proper Date." });
                    }
                    Project_Assign_Master assign = db.Project_Assign_Master.Where(x => x.Emp_Id == Emp_Id).FirstOrDefault();
                    if (assign == null)
                    {
                        Project_Assign_Master project = new Project_Assign_Master();
                        project.Project_Id = Project_Id;
                        project.Emp_Id = Emp_Id;
                        db.Project_Assign_Master.Add(project);
                        db.SaveChanges();
                    }
                    Project_Task_Master Task = new Project_Task_Master();
                    Task.Task_Name = Task_Name;
                    Task.Emp_Id = Emp_Id;
                    Task.Project_Id = Project_Id;
                    Task.Description = Description;
                    Task.Actual_Start_Date = A_Start_Date;
                    Task.Revised_Start_Date = A_Start_Date;
                    Task.Revised_End_Date = R_End_Date;
                    Task.Status = 0;
                    Task.Task_No = 0;
                    Task.Duration = Duration;
                    Task.Task_Type = Task_Type;
                    Task.Duration_Unit = Duration_Unit;
                    db.Project_Task_Master.Add(Task);
                    db.SaveChanges();
                    Comment_Master cmt = new Comment_Master();
                    cmt.Comment_Type = 2;
                    cmt.Comments = "New Task Update !";
                    cmt.Commented_Date = DateTime.Now;
                    cmt.Task_Id = Task.Task_Id;
                    cmt.Emp_Id = Emp_Id;
                    db.Comment_Master.Add(cmt);
                    db.SaveChanges();
                    TempData["Result"] = "New Task Added Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "New Task Added Successfully !", Refresh = "Project_Task_List" });
                }
                else
                {
                    string Comments = form["comments"].ToString();
                    Project_Task_Master Task = db.Project_Task_Master.Where(x => x.Task_Id == Task_Id).FirstOrDefault();
                    if (Task == null)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Task  !" });
                    }
                    if (A_Start_Date > R_End_Date)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Please Add Proper Date." });
                    }
                    Task.Task_Name = Task_Name;
                    Task.Emp_Id = Emp_Id;
                    Task.Project_Id = Project_Id;
                    Task.Description = Description;
                    Task.Actual_Start_Date = A_Start_Date;
                    Task.Revised_Start_Date = A_Start_Date;
                    Task.Revised_End_Date = R_End_Date;
                    Task.Duration = Duration;
                    Task.Task_Type = Task_Type;
                    Task.Duration_Unit = Duration_Unit;
                    db.SaveChanges();
                    Comment_Master cmt = new Comment_Master();
                    cmt.Comment_Type = 2;
                    cmt.Comments = "New Task Update !";
                    cmt.Commented_Date = DateTime.Now;
                    cmt.Emp_Id = Emp_Id;
                    cmt.Task_Id = Task.Task_Id;
                    db.Comment_Master.Add(cmt);
                    db.SaveChanges();
                    TempData["Result"] = "Task Updated Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Task Updated Successfully !", Refresh = "Project_Task_List" });
                }
            }
            else if (Cmd == "Approve")
            {
                Project_Task_Master p = db.Project_Task_Master.Where(x => x.Task_Id == Task_Id).FirstOrDefault();
                if (p == null)
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Task  !" });
                }
                p.Status = 1;
                db.SaveChanges();

                ////////////Email Send.....................
                //UserMaster u = db.UserMasters.Where(x => x.User_ID == Emp_Id).FirstOrDefault();
                //string sub = "Your Task will Approved";
                //string message = "Your Task will Approved";
                //var subject = "Regarding Task";
                //var body = "Dear Employee , <br/> Your Task will Approved <br/>  Subject: " + sub + "<br/>Message: " + message + "<br/>Thank You !  <br/> This is System Generated Mail....! ";
                //MailAddress fromAddress = new MailAddress(_LogedUser.User_Email);
                //MailAddress toAddress = new MailAddress(u.User_Email);
                //Thread t1 = null;
                //t1 = new Thread(new ThreadStart(() => new CommonClasses().Email_Verify(fromAddress.ToString(), toAddress.ToString(), subject.ToString(), body.ToString())));
                //t1.Start();
                TempData["Result"] = "Project Task Approved Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Project Task Approved Successfully !", Refresh = "Project_Task_List" });
            }
            else if (Cmd == "Rejected")
            {
                Project_Task_Master p = db.Project_Task_Master.Where(x => x.Task_Id == Task_Id).FirstOrDefault();
                if (p == null)
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Task  !" });
                }
                p.Status = 2;
                db.SaveChanges();

                ////////////Email Send.....................
                //UserMaster u = db.UserMasters.Where(x => x.User_ID == Emp_Id).FirstOrDefault();
                //string sub = "Your Task will Rejected";
                //string message = "Your Task will Rejected";
                //var subject = "Regarding Task";
                //var body = "Dear Employee , <br/> Your Task will Rejected <br/>  Subject: " + sub + "<br/>Message: " + message + "<br/>Thank You ! ";
                //MailAddress fromAddress = new MailAddress(_LogedUser.User_Email);
                //MailAddress toAddress = new MailAddress(u.User_Email);
                //Thread t1 = null;
                //t1 = new Thread(new ThreadStart(() => new CommonClasses().Email_Verify(fromAddress.ToString(), toAddress.ToString(), subject.ToString(), body.ToString())));
                //t1.Start();

                TempData["failure"] = "Project Task Rejected Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Project Task Rejected Successfully !", Refresh = "Project_Task_List" });
            }
            else if (Cmd == "Delete")
            {
                Project_Task_Master p = db.Project_Task_Master.Where(x => x.Task_Id == Task_Id).FirstOrDefault();
                var _del = db.Project_Task_Master.Find(Task_Id);
                if (p == null)
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Task  !" });
                }
                if (_del.Comment_Master.Count > 0)
                {
                    List<Comment_Master> _delCmt = db.Comment_Master.Where(x => x.Task_Id == Task_Id).ToList();
                    foreach (Comment_Master cm in _delCmt)
                    {
                        db.Comment_Master.Remove(cm);
                    }
                }
                p.Status = 6;
                //db.Project_Task_Master.Remove(_del);
                db.SaveChanges();
                TempData["failure"] = "Project Task Deleted Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Project Task Deleted Successfully!", Refresh = "Project_Task_List" });
            }
            else if (Cmd == "Send")
            {
                if (Task_Id != 0)
                {
                    string Comments = form["comments"].ToString();
                    if (Comments == "")
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Pelase Add Comment !" });
                    }
                    Comment_Master cm = new Comment_Master();
                    cm.Comments = Comments;
                    cm.Task_Id = Task_Id;
                    cm.Comment_Type = 1;
                    cm.Add_By = _LogedUser.User_ID;
                    cm.Add_On = DateTime.Now;
                    cm.Emp_Id = Emp_Id;
                    cm.Commented_Date = DateTime.Now;
                    db.Comment_Master.Add(cm);
                    db.SaveChanges();
                    TempData["Result"] = "Comment Added Successfully.";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Comment Added Successfully.", Refresh = "Default" });
                }
                else
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Task Not Assigned  !" });
                }
            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !" });
            }
        }

        public ActionResult Project_Task_Delete(FormCollection form)
        {
            long Task_Id = long.Parse(form["Task_Id"].ToString());
            Project_Task_Master p = db.Project_Task_Master.Where(x => x.Task_Id == Task_Id).FirstOrDefault();
            var _del = db.Project_Task_Master.Find(Task_Id);
            if (p == null)
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Task  !" });
            }
            if (_del.Comment_Master.Count > 0)
            {
                List<Comment_Master> _delCmt = db.Comment_Master.Where(x => x.Task_Id == Task_Id).ToList();
                foreach (Comment_Master cm in _delCmt)
                {
                    db.Comment_Master.Remove(cm);
                }
            }
            p.Status = 6;
            //db.Project_Task_Master.Remove(_del);
            db.SaveChanges();
            TempData["failure"] = "Project Task Deleted Successfully !";
            return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Project Task Deleted Successfully!", Refresh = "Project_Task_List" });
        }
        [HttpPost]
        public ActionResult Edit_Comment(FormCollection form)
        {

            long Comment_Id = long.Parse(form["Comnt_ID"].ToString());
            string Comments = form["comments"].ToString();
            long Emp_Id = long.Parse(form["Emp_Id"].ToString());
            long Task_Id = long.Parse(form["Task_Id"].ToString());
            string Cmd = form["Command"].ToString();
            if (Cmd == "Save")
            {
                Comment_Master _cmt = db.Comment_Master.Where(x => x.Comnt_ID == Comment_Id & x.UserMaster.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
                if (_cmt == null)
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Comment  !" });
                }
                _cmt.Comments = Comments;
                _cmt.Add_By = _LogedUser.User_ID;
                _cmt.Add_On = DateTime.Now;
                db.SaveChanges();
                TempData["Result"] = "Comment Updated Successfully.";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Comment Updated Successfully.", Refresh = "Project_Task_Add?TaskID=" + Task_Id });
            }
            else if (Cmd == "Delete")
            {
                Comment_Master _del = db.Comment_Master.Find(Comment_Id);
                if (_del == null)
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Comment  !" });
                }
                db.Comment_Master.Remove(_del);
                db.SaveChanges();
                TempData["failure"] = "Comment Deleted Successfully.";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Comment Deleted Successfully.", Refresh = "Project_Task_Add?TaskID=" + Task_Id });
            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !" });
            }

        }
        public ActionResult GetProject(long Emp_Id)
        {
            long rid = new long();
            List<long> project_Id = new List<long>();
            List<string> project_Name = new List<string>();
            List<Project_Assign_Master> projects = db.Project_Assign_Master.Where(x => x.Emp_Id == Emp_Id && x.Project_Master.Company_ID == _LogedUser.Company_ID).ToList();
            int cnt = projects.Count();
            foreach (var p in projects)
            {
                project_Id.Add(p.Project_Id);
                project_Name.Add(p.Project_Master.Project_Name);
            }
            ///GetReq 
            List<long> req_Id = new List<long>();
            List<decimal> req_Amt = new List<decimal>();
            List<string> req_date = new List<string>();
            List<Voucher_Master> voucher = db.Voucher_Master.Where(x => x.User_Id == Emp_Id && x.Status != 6 && x.UserMaster.Company_ID == _LogedUser.Company_ID).ToList();
            List<Request_Master> requests = db.Request_Master.Where(x => x.User_Id == Emp_Id && x.Status == 1 && x.UserMaster.Company_ID == _LogedUser.Company_ID).ToList();
            List<Request_Master> _notincdlued = new List<Request_Master>();
            foreach (Request_Master R in requests)
            {
                bool _found = false;
                
                foreach (Voucher_Master _v in voucher)
                {
                    if (_v.Req_Id == R.Req_Id)
                    {
                        _found = true;
                        rid = long.Parse(_v.Req_Id.ToString());
                    }
                }
                if (_found == false)
                {
                _notincdlued.Add(R);
                }
            }

            int cntreq = _notincdlued.Count();
            foreach (var p in _notincdlued)
            {
                req_Id.Add(p.Req_Id);
                req_Amt.Add((decimal)p.Granted_Amount);

            }

            if (projects != null || requests != null)
            {
                return Json(new { success = true, Project_Id = project_Id, Project_Name = project_Name, resid = rid, Cnt = cnt, Req_Id = req_Id, Req_Amt = req_Amt, Cntreq = cntreq });
            }
            else
            {
                return Json(new { success = false });
            }
        }
        public ActionResult Getreqres(List<long> myList)
        {
            if(myList.Count!=0)
            {
                foreach (var item in myList)
                {
                    Request_Master rm = db.Request_Master.Where(x => x.Req_Id == item).FirstOrDefault();
                    if (rm != null)
                    {
                        rm.Status = 1;
                        db.SaveChanges();
                    }
                }
                return Json(new { success = true, msg = "Request Approved Successfully !" });
            }
            else
            {
                return Json(new { success = false ,msg="Please select request that can be approved!"});
            }
        }
        public ActionResult VoucherGetreqres(List<long> vouchermyList)
        {

            if (vouchermyList.Count != 0)
            {
                foreach (var item in vouchermyList)
                {
                    Voucher_Master rm = db.Voucher_Master.Where(x => x.Voucher_Id == item).FirstOrDefault();
                    if (rm != null)
                    {
                        rm.Status = 1;
                        db.SaveChanges();
                    }
                }
                return Json(new { success = true, msg = "VoucherRequest Approved Successfully !" });
            }
            else
            {
                return Json(new { success = false, msg = "Please select voucherrequest that can be approved!" });
            }
        }
        public ActionResult AdvanceReq_List(int status = 101, int Sts = 99, int Userid = 99, string parenttype = "All", string actionType = "search")
        {
            ViewBag.PageTitle = "All Advance Request";

            if (actionType == "download")
            {
                // Perform download logic here
                return DownloadRequestTemplate(Sts, Userid, parenttype, true);
            }


            if (status == 101)
            {
                //List<Request_Master> req = db.Request_Master.Where(x => x.Status != 6 && x.UserMaster.Company_ID == _LogedUser.Company_ID).OrderBy(x => x.Status).ThenByDescending(x => x.Req_Id).ToList();
                List<Project_Master> projects = db.Project_Master.Where(x => x.Company_ID == _LogedUser.Company_ID).ToList();
                List<UserMaster> user = db.UserMasters.Where(x => x.User_type != _LogedUser.User_type && x.Company_ID == _LogedUser.Company_ID && x.status == 0).ToList();
                ViewBag.Project = projects;
                ViewBag.user = user;
                //ViewBag.req = req;

                var requestQuery = db.Request_Master
                                .Where(x => x.UserMaster.Company_ID == _LogedUser.Company_ID && x.Status != 6);

                if (Userid != 99)
                    requestQuery = requestQuery.Where(x => x.User_Id == Userid);

                if (Sts != 99)
                    requestQuery = requestQuery.Where(x => x.Status == Sts);

                if (parenttype != "All")
                    requestQuery = requestQuery.Where(x => x.Parent_Type == parenttype);
                var requestList = requestQuery.ToList();
                ViewBag.req = requestList;

            }
            if (status == 0)
            {
                //List<Request_Master> req = db.Request_Master.Where(x => x.Status == 0 && x.UserMaster.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.Req_Id).ToList();
                List<Project_Master> projects = db.Project_Master.Where(x => x.Company_ID == _LogedUser.Company_ID).ToList();
                List<UserMaster> user = db.UserMasters.Where(x => x.User_type != _LogedUser.User_type && x.Company_ID == _LogedUser.Company_ID).ToList();
                ViewBag.Project = projects;
                ViewBag.user = user;
                //ViewBag.req = req;

                var requestQuery = db.Request_Master
                                 .Where(x => x.UserMaster.Company_ID == _LogedUser.Company_ID && x.Status == 0);

                if (Userid != 99)
                    requestQuery = requestQuery.Where(x => x.User_Id == Userid);

                if (Sts != 99)
                    requestQuery = requestQuery.Where(x => x.Status == Sts);

                if (parenttype != "All")
                    requestQuery = requestQuery.Where(x => x.Parent_Type == parenttype);
                var requestList = requestQuery.ToList();
                ViewBag.req = requestList;
            }


            ViewBag.Sts = Sts;
            ViewBag.Userid = Userid;
            ViewBag.parenttype = parenttype;


            return View();
        }

        public ActionResult DownloadRequestTemplate(int Sts = 99, int Userid = 99, string parenttype = "All", bool withdata = false)
        {

            var user = db.UserMasters.Where(x => x.Company_ID == _LogedUser.Company_ID && x.User_type != _LogedUser.User_type && x.status==0).Select(x => x.User_Name).ToList();
            var eid = db.UserMasters.Where(x => x.Company_ID == _LogedUser.Company_ID && x.User_type != _LogedUser.User_type && x.status == 0).Select(x => x.EmployeeID).ToList();
            List<string> satusTypes = new List<string> { "Pending", "Approve" };
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Employees");

            List<string> allColumns = new List<string>
            {
                "Sr.no",
                "EmployeeID",
                //"EmployeeName",
                "Requested_Amount",
                "Granted_Amount",
                "Purpose_to_Visit",
                "Remark_for_Administrator",
                "Remark_By_Administrator",
                "Status"
            };

            for (int i = 0; i < allColumns.Count; i++)
            {
                var cell = worksheet.Cell(1, i + 1);
                cell.Value = allColumns[i];
                cell.Style.Font.Bold = true;

                // Set background color by column group
                if (i == 0)
                    cell.Style.Fill.BackgroundColor = XLColor.Red;
                else if (i == 1)
                    cell.Style.Fill.BackgroundColor = XLColor.Yellow;
                else if (i >= 2 && i <= 6)
                    cell.Style.Fill.BackgroundColor = XLColor.LightGreen;
                else if (i == 7)
                    cell.Style.Fill.BackgroundColor = XLColor.LightBlue;


            }
            // Auto-adjust all columns based on the header content
            worksheet.Columns(1, allColumns.Count).AdjustToContents();
            var optionsSheet = workbook.Worksheets.Add("Options");



            if (Sts != 99 || Userid != 99 || parenttype != "All")
            {
                // Build user query
                var requestQuery = db.Request_Master
                                  .Where(x => x.UserMaster.Company_ID == _LogedUser.Company_ID && x.Status != 6);

                if (Userid != 99)
                    requestQuery = requestQuery.Where(x => x.User_Id == Userid);

                if (Sts != 99)
                    requestQuery = requestQuery.Where(x => x.Status == Sts);

                if (parenttype != "All")
                    requestQuery = requestQuery.Where(x => x.Parent_Type == parenttype);


                var requestList = requestQuery.ToList();

                int col = 1;
                int row = 2;
                foreach (var request_Master in requestList)
                {
                    col = 1;

                    worksheet.Cell(row, col++).Value = request_Master.Req_Id;
                    worksheet.Cell(row, col++).Value = request_Master.UserMaster.EmployeeID;
                    //worksheet.Cell(row, col++).Value = request_Master.UserMaster.User_Name;
                    worksheet.Cell(row, col++).Value = request_Master.Requested_Amount;
                    worksheet.Cell(row, col++).Value = request_Master.Granted_Amount;
                    worksheet.Cell(row, col++).Value = request_Master.Description;
                    worksheet.Cell(row, col++).Value = request_Master.Remark;
                    worksheet.Cell(row, col++).Value = request_Master.Remark_For_Admin;
                    worksheet.Cell(row, col++).Value = request_Master.Status;
                    row++;
                }
            }

            for (int i = 0; i < eid.Count; i++)
            {
                optionsSheet.Cell(i + 1, 2).Value = eid[i];
            }

            // Fill Employeename in Column 9 (I)
            //for (int i = 0; i < user.Count; i++)
            //{
            //    optionsSheet.Cell(i + 1, 3).Value = user[i]; 
            //}

            //Fill statusType in Column 9(I)
            for (int i = 0; i < satusTypes.Count; i++)
            {
                optionsSheet.Cell(i + 1, 8).Value = satusTypes[i]; 
            }

            if (eid.Count > 0)
                workbook.NamedRanges.Add("EmployeeList", optionsSheet.Range(1, 2, eid.Count, 2)); // H1:H{N}

            //if (user.Count > 0)
            //    workbook.NamedRanges.Add("UsertList", optionsSheet.Range(1, 3, user.Count, 3)); // H1:H{N}

            if (satusTypes.Count > 0)
                workbook.NamedRanges.Add("StatusList", optionsSheet.Range(1, 8, satusTypes.Count, 8)); // I1:I{N}


            var employeeValidator = worksheet.Range("B2:B9999").CreateDataValidation();
            employeeValidator.List("=EmployeeList");

            //var userValidator = worksheet.Range("C2:C9999").CreateDataValidation();
            //userValidator.List("=UsertList");

            var statusvalidator = worksheet.Range("H2:I9999").CreateDataValidation();
            statusvalidator.List("=StatusList");


            optionsSheet.Hide();


            var memoryStream = new System.IO.MemoryStream();
            workbook.SaveAs(memoryStream);
            memoryStream.Seek(0, System.IO.SeekOrigin.Begin);

            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "AdvanceRequestAdd.xlsx");
        }

        [HttpPost]
        public ActionResult UploadRequest(HttpPostedFileBase file)
        {
            try
            {
                if (file != null && file.ContentLength > 0)
                {
                    var Employeeid = db.UserMasters
                        .Where(x => x.Company_ID == _LogedUser.Company_ID)
                        .ToDictionary(x => x.EmployeeID, x => x.EmployeeID);

                    var employeename = db.UserMasters
                        .Where(x => x.Company_ID == _LogedUser.Company_ID && x.User_type == 1)
                        .ToDictionary(x => x.User_Name, x => x.User_ID);

                    using (var workbook = new XLWorkbook(file.InputStream))
                    {
                        var worksheet = workbook.Worksheet(1);
                        var rows = worksheet.RowsUsed().Skip(1);

                        foreach (var row in rows)
                        {
                            if (row.Cell(1).IsEmpty()) continue;

                            int col = 1;
                            long.TryParse(row.Cell(col++).GetString(), out long srNo);

                            Request_Master req = srNo != 0
                                ? db.Request_Master.FirstOrDefault(x => x.Req_Id == srNo && x.UserMaster.Company_ID == _LogedUser.Company_ID)
                                : new Request_Master();

                            var eid = row.Cell(col++).GetString();
                            //var ename = row.Cell(col++).GetString();

                            var uid = db.UserMasters.Where(x => x.EmployeeID == eid).Select(x => x.User_ID).FirstOrDefault();
                            // Basic Info
                            req.User_Id = uid;
                            req.Requested_Amount = long.TryParse(row.Cell(col++).GetString(), out var Requested_Amount) ? Requested_Amount : 0;
                            req.Granted_Amount = long.TryParse(row.Cell(col++).GetString(), out long Granted_Amount) ? Granted_Amount : 0;
                            req.Description = row.Cell(col++).GetString();
                            req.Remark_For_Admin = row.Cell(col++).GetString();
                            req.Remark = row.Cell(col++).GetString();
                            var status = row.Cell(col++).GetString();
                            if (status == "Pending")
                            {
                                req.Status = 0;
                            }
                            else if (status == "Approve")
                            {
                                req.Status = 1;
                            }
                            if (srNo == 0)
                            {
                                req.Parent_Type = "Personal Advance";
                                req.A_Date = DateTime.Now;
                                req.S_Date = DateTime.Now;
                                req.E_Date = DateTime.Now;
                                req.ReqDate = DateTime.Now;
                                db.Request_Master.Add(req);
                            }
                        }
                        db.SaveChanges();
                        return Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Request Uploaded Successfully!", Refresh = "Default" });
                    }
                }
                return Json(new { EnableError = true, ErrorTitle = "F", ToastMsgSuc = "File not found!", Refresh = "Default" });
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                    .SelectMany(x => x.ValidationErrors)
                    .Select(x => $"Property: {x.PropertyName} - Error: {x.ErrorMessage}")
                    .ToList();

                string fullError = string.Join("; ", errorMessages);
                // Log it or return it as part of your response
                return Json(new
                {
                    EnableError = true,
                    ErrorTitle = "Validation Failed",
                    ToastMsgSuc = fullError
                });
            }
            catch (Exception ex)
            {
                // Optionally log ex.Message
                return Json(new { EnableError = true, ErrorTitle = "F", ToastMsgSuc = "Something went wrong!", Refresh = "Default" });
            }
        }

        [HttpPost]
        public ActionResult AdvanceReq_Add(FormCollection form)
        {

            long Req_Id = long.Parse(form["Req_Id"].ToString());
            long User_Id = long.Parse(form["User_Id"].ToString());
            string Description = form["Description"].ToString();
            decimal Requested_Amount = decimal.Parse(form["Requested_Amount"].ToString());
            decimal Granted_Amount = decimal.Parse(form["Granted_Amount"].ToString());
            string Place = "";
            if (form["Place"] == null || form["Place"] == "")
            {
                
            }
            else
            {
                Place = form["Place"].ToString();
                
            }
            string Parent_Type = form["Parent_Type"].ToString();
            DateTime S_Date = DateTime.Now;
            DateTime E_Date = DateTime.Now;
            if (Parent_Type== "Opportunity" || Parent_Type == "Project")
            {
                 S_Date = DateTime.Parse(form["S_Date"].ToString());
                 E_Date = DateTime.Parse(form["E_Date"].ToString());
            }
            else
            {
                
            }
                
            string Remark = form["Remark"].ToString();
            string Remark_For_Admin = form["Remark_For_Admin"].ToString();
            string Cmd = form["Command"].ToString();
            if (Cmd == "Save")
            {
                if (Req_Id == 0)
                {
                    if (Parent_Type == "Opportunity" || Parent_Type == "Project")
                    {
                        if (S_Date > E_Date)
                        {
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Please Add Proper Date.", Refresh = "Default" });
                        }
                    }
                    Request_Master Request = new Request_Master();
                    if (form["Place"] == null || form["Place"] == "")
                    {
                        Request.Place = null;
                    }
                    else
                    {
                        string Placeadd = form["Place"].ToString();
                        Request.Place = Placeadd;
                    }
                    if (form["Project_Id"] == null || form["Project_Id"] == "")
                    {
                        Request.Project_Id = null;
                    }
                    else
                    {
                        long? Project_Id = long.Parse(form["Project_Id"].ToString());
                       // Request.Project_Id = null;
                        Request.Project_Id = Project_Id;
                    }
                    Request.User_Id = User_Id;
                    Request.Requested_Amount = Requested_Amount;
                    //Request.Place = Place;
                    Request.Parent_Type = Parent_Type;
                    Request.Description = Description;
                    Request.A_Date = DateTime.Now;
                    Request.S_Date = S_Date;
                    Request.E_Date = E_Date;
                    Request.Granted_Amount = Granted_Amount;
                    Request.Status = 0;
                    Request.Description = Description;
                    Request.ReqDate = DateTime.Now;
                    Request.Remark = Remark;
                    Request.Remark_For_Admin = Remark_For_Admin;
                    db.Request_Master.Add(Request);
                    db.SaveChanges();
                    TempData["Result"] = "New Request Added Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "New Request Added Successfully!", Refresh = "Default" });
                }
                else
                {
                    Request_Master Request = db.Request_Master.Where(x => x.Req_Id == Req_Id).FirstOrDefault();
                    if (Request == null)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Request  !" });
                    }
                    if (Parent_Type == "Opportunity" || Parent_Type == "Project")
                    {
                        if (S_Date > E_Date)
                        {
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Please Add Proper Date.", Refresh = "Default" });
                        }
                    }
                    if (form["Place"] == null || form["Place"] == "")
                    {
                        Request.Place = null;
                    }
                    else
                    {
                        string Placeed = form["Place"].ToString();
                        Request.Place = Placeed;
                    }
                    if (form["Project_Id"] == null || form["Project_Id"] == "")
                    {
                        Request.Project_Id = null;
                    }
                    else
                    {
                        long? Project_Id = long.Parse(form["Project_Id"].ToString());
                        Request.Project_Id = Project_Id;
                    }

                    Request.User_Id = User_Id;
                    Request.Requested_Amount = Requested_Amount;
                    //Request.Place = Place;
                    Request.Parent_Type = Parent_Type;
                    Request.Description = Description;
                    Request.A_Date = DateTime.Now;
                    Request.S_Date = S_Date;
                    Request.E_Date = E_Date;
                    Request.Granted_Amount = Granted_Amount;
                    Request.Description = Description;
                    Request.ReqDate = DateTime.Now;
                    Request.Remark = Remark;
                    Request.Remark_For_Admin = Remark_For_Admin;
                    db.SaveChanges();
                    TempData["Result"] = "Request Updated Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Request Updated Successfully!", Refresh = "Default" });
                }
            }
            else if (Cmd == "Approve")
            {
                Request_Master p = db.Request_Master.Where(x => x.Req_Id == Req_Id).FirstOrDefault();
                if (p == null)
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Request  !" });
                }
                p.Status = 1;
                p.User_Id = User_Id;
                p.Place = Place;
                p.Parent_Type = Parent_Type;
                p.Description = Description;
                p.A_Date = DateTime.Now;
                p.S_Date = S_Date;
                p.E_Date = E_Date;
                p.Requested_Amount = Requested_Amount;
                p.Granted_Amount = Granted_Amount;
                p.Description = Description;
                p.ReqDate = DateTime.Now;
                p.Remark = Remark;
                p.Remark_For_Admin = Remark_For_Admin;
                //int Trans_Type = int.Parse(form["Trans_Type"].ToString());
                if (Granted_Amount > 0)
                {
                    string Trans_Type_Name = "Credit";
                    int Trans_Type = 1;
                    Transaction_Master t = new Transaction_Master();
                    t.User_id = User_Id;
                    t.Amount = Convert.ToDecimal(p.Granted_Amount);
                    t.Type_id = Trans_Type;
                    t.Type_Name = Trans_Type_Name;
                    t.Ref_Id = "Voucher|" + p.Req_Id;
                    t.Trans_Date = DateTime.Now;
                    db.Transaction_Master.Add(t);
                }

                db.SaveChanges();
                TempData["Result"] = "Request Approved Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Request Approved Successfully!", Refresh = "Default" });
            }
            else if (Cmd == "Rejected")
            {
                Request_Master p = db.Request_Master.Where(x => x.Req_Id == Req_Id).FirstOrDefault();
                if (p == null)
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Request  !" });
                }
                p.Status = 2;
                db.SaveChanges();
                TempData["failure"] = "Request Rejected Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Request Rejected Successfully!", Refresh = "Default" });
            }
            else if (Cmd == "Delete")
            {
                Request_Master p = db.Request_Master.Where(x => x.Req_Id == Req_Id).FirstOrDefault();
                var _del = db.Request_Master.Find(Req_Id);
                if (p == null)
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Request  !" });
                }
                if (_del.Voucher_Master.Count > 0)
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Request Has Voucher Detail so Delete First." });
                }
                p.Status = 6;
                db.SaveChanges();
                TempData["failure"] = "Request Deleted Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Request Deleted Successfully!", Refresh = "Default" });
            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !", Refresh = "Default" });
            }

        }

        public ActionResult AdvanceReq_Delete(FormCollection form)
        {
            long Req_Id = long.Parse(form["request_id"].ToString());
            Request_Master p = db.Request_Master.Where(x => x.Req_Id == Req_Id).FirstOrDefault();
            var _del = db.Request_Master.Find(Req_Id);
            if (p == null)
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Request  !" });
            }
            if (_del.Voucher_Master.Count > 0)
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Request Has Voucher Detail so Delete First." });
            }
            p.Status = 6;
            db.SaveChanges();
            TempData["failure"] = "Request Deleted Successfully!";
            return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Request Deleted Successfully!", Refresh = "Default" });
        }
        public ActionResult VoucherReq_List(int status = 101)
        {
            ViewBag.PageTitle = "All Voucher Request";

            if (status == 101)
            {
                List<Voucher_Master> voucher = db.Voucher_Master.Where(x => x.Status != 6 && x.UserMaster.Company_ID == _LogedUser.Company_ID).OrderBy(x => x.Status).ThenByDescending(x => x.Voucher_Id).ToList();
                ViewBag.voucher = voucher;
            }
            else
            {
                List<Voucher_Master> voucher = db.Voucher_Master.Where(x => x.Status == 0 && x.UserMaster.Company_ID == _LogedUser.Company_ID).OrderBy(x => x.Status).ThenByDescending(x => x.Voucher_Id).ToList();
                ViewBag.voucher = voucher;
            }

            return View();
        }
        public ActionResult VoucherReq_Add(long Voucher_Id)
        {
            ViewBag.PageTitle = "Add Voucher";

            List<Request_Master> req = db.Request_Master.Where(x => x.Status == 1 && x.UserMaster.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.ReqDate).ToList();
            List<Project_Master> projects = db.Project_Master.Where(x => x.Company_ID == _LogedUser.Company_ID).ToList();
            List<UserMaster> user = db.UserMasters.Where(x => x.User_type != _LogedUser.User_type && x.Company_ID == _LogedUser.Company_ID && x.status == 0).ToList();
            List<Voucher_Master> voucher = db.Voucher_Master.Where(x => x.Status != 6 && x.UserMaster.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.Voucher_Id).ToList();
            Voucher_Master v = db.Voucher_Master.Where(x => x.Voucher_Id == Voucher_Id && x.UserMaster.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
            ViewBag.v = v;
            ViewBag.voucher = voucher;
            ViewBag.Project = projects;
            ViewBag.Project = projects;
            ViewBag.user = user;
            ViewBag.req = req;
            return View();
        }
        [HttpPost]
        public ActionResult VoucherReq_Add(FormCollection form, HttpPostedFileBase file)
        {

            long Emp_Id = long.Parse(form["User_Id"].ToString());
            long Voucher_Id = long.Parse(form["Voucher_Id"].ToString());
            string Description = form["Description"].ToString();
            string Place = form["Place"].ToString();
            string Remark = form["Remark"].ToString();
            string Remark_By_Admin = form["RemarkByAdmin"].ToString();
            decimal Petrol = decimal.Parse(form["Petrol"].ToString());
            decimal Total_Amount = 0;
            if (form["Total_Amount"].ToString() == null)
            {
                Total_Amount = decimal.Parse(form["Total_Amount"].ToString());
            }
            decimal Payable_Amount = decimal.Parse(form["Payable_Amount"].ToString());
            decimal Deduction_Amount = decimal.Parse(form["Deduction_Amount"].ToString());
            decimal Travelling = decimal.Parse(form["Travelling"].ToString());
            decimal Mobile = decimal.Parse(form["Mobile"].ToString());
            decimal Conveyance = decimal.Parse(form["Conveyance"].ToString());
            string Parent_Type = form["Parent_Type"].ToString();
            DateTime v_date = DateTime.Parse(form["Voucher_Date"].ToString());
            string Cmd = form["Command"].ToString();
            decimal deduction_amount = decimal.Parse(form["Deduction_Amount"].ToString());
            int Trans_Type = int.Parse(form["Trans_Type"].ToString());
            if (Petrol > 0 || Mobile > 0 || Conveyance > 0 || Travelling > 0)
            {
            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Please enter some amount !", Refresh = "Default" });
            }
            if (Cmd == "Save")
            {
                if (Voucher_Id == 0)
                {
                    Voucher_Master voucher = new Voucher_Master();
                    if (form["Req_Id"] == null || form["Req_Id"] == "")
                    {
                        voucher.Req_Id = null;
                    }
                    else
                    {
                        long Req_Id = long.Parse(form["Req_Id"].ToString());
                        voucher.Req_Id = Req_Id;

                    }
                    if (form["Project_Id"] == null || form["Project_Id"] == "")
                    {
                        voucher.Project_Id = null;
                    }
                    else
                    {
                        long? Project_Id = long.Parse(form["Project_Id"].ToString());
                        if (Project_Id == 0)
                        {
                            voucher.Project_Id = null;
                        }
                        else
                        {
                            voucher.Project_Id = Project_Id;
                        }
                    }
                    voucher.User_Id = Emp_Id;
                    voucher.Voucher_Date = v_date;
                    voucher.Description = Description;
                    voucher.Place = Place;
                    voucher.Remark = Remark;
                    voucher.Remark_For_Administrator = Remark_By_Admin;
                    voucher.Parent_Type = Parent_Type;
                    voucher.Petrol = Petrol;
                    voucher.Mobile = Mobile;
                    voucher.Status = 0;
                    voucher.Travelling = Travelling;
                    voucher.Conveyance = Conveyance;
                    voucher.Total_Amount = voucher.Petrol + voucher.Mobile + voucher.Travelling + voucher.Conveyance;
                    voucher.Payable_Amount = Payable_Amount;
                    voucher.Trans_Type = Trans_Type;
                    voucher.Deduction_Amount = deduction_amount;
                    if (file != null)
                    {
                        //var allowedExtensions = new[] { ".Jpg", ".JPG", ".png", ".jpg", ".jpeg", ".pdf", ".docs" };
                        //var fileName = Path.GetFileName(file.FileName);
                        //var ext = Path.GetExtension(file.FileName);
                        //if (!allowedExtensions.Contains(ext))
                        //{
                        //    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Sorry Invalid Files." });
                        //}
                        //string name = Path.GetFileNameWithoutExtension(fileName);
                        //string myfile = name + "_" + DateTime.Now.Second.ToString() + ext;
                        //var path = Path.Combine(Server.MapPath("~/Uploads/User_Image"), myfile);
                        //file.SaveAs(path);
                        //voucher.Image_Name = myfile;

                        string _Oldname = voucher.Image_Name;
                        CommonResponce result = new ImageKiT_ImageProcess().UploadFile("Voucher_Image", file, _Oldname, 20);
                        if (result.EnableError == true)
                        {
                            //voucher.image_path = $"{ImageKit_PUBLICURL}/{ImageKit_Folder}/Voucher_Image/" + result.ErrorMsg;
                            voucher.Image_Name = result.ErrorMsg;
                        }
                        else
                        {
                            if (Request.IsAjaxRequest()) { return this.Json(new { EnableError = false, ErrorTitle = "Failure", ErrorMsg = result.ErrorMsg }); }
                            else { TempData["Result"] = result.ErrorMsg; return RedirectToAction("Dashboard"); }
                        }



                    }
                    db.Voucher_Master.Add(voucher);
                    db.SaveChanges();

                    TempData["Result"] = "New Voucher Added Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "New Voucher Added Successfully!", Refresh = "VoucherReq_List" });
                }
                else
                {
                    Voucher_Master voucher = db.Voucher_Master.Where(x => x.Voucher_Id == Voucher_Id).FirstOrDefault();
                    //if (form["Req_Id"] != null)
                    //{
                    //    long Req_Id = long.Parse(form["Req_Id"].ToString());
                    //    voucher.Req_Id = Req_Id;
                    //}
                    if (form["Req_Id"] == null || form["Req_Id"] == "")
                    {
                        voucher.Req_Id = null;
                    }
                    else
                    {
                        long Req_Id = long.Parse(form["Req_Id"].ToString());
                        voucher.Req_Id = Req_Id;

                    }
                    if (form["Project_Id"] == null || form["Project_Id"] == "")
                    {
                        voucher.Project_Id = null;
                    }
                    else
                    {
                        long? Project_Id = long.Parse(form["Project_Id"].ToString());
                        voucher.Project_Id = Project_Id;
                    }
                    voucher.User_Id = Emp_Id;
                    voucher.Description = Description;
                    voucher.Place = Place;
                    voucher.Remark = Remark;
                    voucher.Voucher_Date = v_date;
                    voucher.Remark_For_Administrator = Remark_By_Admin;
                    voucher.Parent_Type = Parent_Type;
                    voucher.Petrol = Petrol;
                    voucher.Mobile = Mobile;
                    voucher.Travelling = Travelling;
                    voucher.Conveyance = Conveyance;
                    voucher.Total_Amount = voucher.Petrol + voucher.Mobile + voucher.Travelling + voucher.Conveyance;
                    voucher.Payable_Amount = Payable_Amount;
                    voucher.Trans_Type = Trans_Type;
                    voucher.Deduction_Amount = deduction_amount;
                    if (file != null)
                    {
                        //var allowedExtensions = new[] { ".Jpg", ".JPG", ".png", ".jpg", ".jpeg", ".pdf", ".docs" };
                        //var fileName = Path.GetFileName(file.FileName);
                        //var ext = Path.GetExtension(file.FileName);
                        //if (!allowedExtensions.Contains(ext))
                        //{
                        //    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Sorry Invalid Files.", Refresh = "VoucherReq_List" });
                        //}
                        //string name = Path.GetFileNameWithoutExtension(fileName);
                        //string myfile = name + "_" + DateTime.Now.Second.ToString() + ext;
                        //var path = Path.Combine(Server.MapPath("~/Uploads/User_Image"), myfile);
                        //file.SaveAs(path);
                        //voucher.Image_Name = myfile;

                        string _Oldname = voucher.Image_Name;
                        CommonResponce result = new ImageKiT_ImageProcess().UploadFile("Voucher_Image", file, _Oldname, 20);
                        if (result.EnableError == true)
                        {
                            //voucher.image_path = $"{ImageKit_PUBLICURL}/{ImageKit_Folder}/Voucher_Image/" + result.ErrorMsg;
                            voucher.Image_Name = result.ErrorMsg;
                        }
                        else
                        {
                            if (Request.IsAjaxRequest()) { return this.Json(new { EnableError = false, ErrorTitle = "Failure", ErrorMsg = result.ErrorMsg }); }
                            else { TempData["Result"] = result.ErrorMsg; return RedirectToAction("Dashboard"); }
                        }


                    }
                    db.SaveChanges();
                    TempData["Result"] = "Voucher Updated Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Voucher Updated Successfully !", Refresh = "VoucherReq_List" });
                }
            }
            else if (Cmd == "Approve")
            {
                Voucher_Master _v = db.Voucher_Master.Where(x => x.Voucher_Id == Voucher_Id).FirstOrDefault();
                if (_v == null)
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Voucher  !" });
                }
                _v.User_Id = Emp_Id;
                _v.Description = Description;
                _v.Place = Place;
                _v.Remark = Remark;
                _v.Voucher_Date = v_date;
                _v.Remark_For_Administrator = Remark_By_Admin;
                _v.Parent_Type = Parent_Type;
                _v.Petrol = Petrol;
                _v.Mobile = Mobile;
                _v.Travelling = Travelling;
                _v.Conveyance = Conveyance;
                _v.Total_Amount = _v.Petrol + _v.Mobile + _v.Travelling + _v.Conveyance;
                _v.Payable_Amount = Payable_Amount;
                _v.Trans_Type = Trans_Type;
                _v.Deduction_Amount = deduction_amount;
                _v.Status = 1;
                //if (_v.Payable_Amount<)
                //{
                //int Trans_Type = int.Parse(form["Trans_Type"].ToString());
                string Trans_Type_Name = form["Trans_Type_Name"].ToString();
                _v.Trans_Type = Convert.ToInt32(Trans_Type);
                Transaction_Master t = new Transaction_Master();
                t.User_id = Emp_Id;
                t.Amount = Convert.ToDecimal(_v.Payable_Amount);
                t.Type_id = Trans_Type;
                t.Type_Name = Trans_Type_Name;
                t.Ref_Id = "Voucher|" + _v.Voucher_Id;
                t.Trans_Date = DateTime.Now;
                db.Transaction_Master.Add(t);
                //}
                db.SaveChanges();
                TempData["Result"] = "Voucher Approved Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Voucher Approved Successfully!", Refresh = "VoucherReq_List" });
            }
            else if (Cmd == "Rejected")
            {
                Voucher_Master _v = db.Voucher_Master.Where(x => x.Voucher_Id == Voucher_Id).FirstOrDefault();
                if (_v == null)
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Voucher  !" });
                }
                _v.Status = 2;
                db.SaveChanges();
                TempData["failure"] = "Voucher Rejected Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Voucher Rejected Successfully!", Refresh = "VoucherReq_List" });
            }
            else if (Cmd == "Delete")
            {
                Voucher_Master _v = db.Voucher_Master.Where(x => x.Voucher_Id == Voucher_Id).FirstOrDefault();
                var _del = db.Voucher_Master.Find(Voucher_Id);
                if (_v == null)
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Voucher  !" });
                }
                _v.Status = 6;
                db.SaveChanges();
                TempData["failure"] = "Voucher Deleted Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Voucher Deleted Successfully!", Refresh = "VoucherReq_List" });
            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !" });
            }

        }
        public ActionResult VoucherReq_Delete(FormCollection form)
        {
            long Voucher_Id = long.Parse(form["voucher_id"].ToString());
            Voucher_Master _v = db.Voucher_Master.Where(x => x.Voucher_Id == Voucher_Id).FirstOrDefault();
            var _del = db.Voucher_Master.Find(Voucher_Id);
            if (_v == null)
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Voucher  !" });
            }
            _v.Status = 6;
            db.SaveChanges();
            TempData["failure"] = "Voucher Deleted Successfully !";
            return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Voucher Deleted Successfully !", Refresh = "VoucherReq_List" });
        }
        public ActionResult GetReq(long Req_No)
        {

            //string Place, Type;
            //long Project;
            //decimal Requested_Amount, Granted_Amount;
            Request_Master request = db.Request_Master.Where(x => x.Req_Id == Req_No && x.UserMaster.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
            if (request != null)
            {
                if (request.Project_Id != null)
                {
                    return Json(new { success = true, Place = request.Place, Type = request.Parent_Type, Project = request.Project_Master.Project_Name, Project_Id = request.Project_Id, Granted_Amount = request.Granted_Amount });
                }
                else
                {
                    return Json(new { success = true, Place = request.Place, Type = request.Parent_Type, Project = "", Project_Id = 0, Granted_Amount = request.Granted_Amount });
                }
            }
            else
            {
                return Json(new { success = false });
            }
        }
        public ActionResult All_Leave(int status = 101)
        {
            ViewBag.PageTitle = "All Leave Request";

            if (status == 101)
            {
                List<Leave_Master> leave = db.Leave_Master.Where(x => x.Status == 0 & x.Leave_Crdr == 2 && x.UserMaster.Company_ID == _LogedUser.Company_ID & x.From_Date > DateTime.Now).OrderBy(x => x.Status).ThenByDescending(x => x.From_Date).ToList();
                ViewBag.leaves = leave;
                List<UserMaster> user = db.UserMasters.Where(x => x.User_type != _LogedUser.User_type && x.Company_ID == _LogedUser.Company_ID && x.status == 0).ToList();
                ViewBag.user = user;
            }
            if (status == 0)
            {
                List<Leave_Master> leave = db.Leave_Master.Where(x => x.Status == 0 & x.Leave_Crdr == 2 && x.UserMaster.Company_ID == _LogedUser.Company_ID & x.From_Date == DateTime.Now).OrderBy(x => x.From_Date).ToList();
                ViewBag.leaves = leave;
                List<UserMaster> user = db.UserMasters.Where(x => x.User_type != _LogedUser.User_type && x.Company_ID == _LogedUser.Company_ID).ToList();
                ViewBag.user = user;
            }
            return View();
        }
        public ActionResult Leave_List(int status = 101, int Sts = 0)
        {
            ViewBag.PageTitle = "All Leave Request";


            if (status == 0)
            {
                List<Leave_Master> leave = db.Leave_Master.Where(x => x.Status == 0 & x.Leave_Crdr == 2 && x.UserMaster.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.From_Date).ToList();
                ViewBag.leaves = leave;
                List<UserMaster> user = db.UserMasters.Where(x => x.User_type != _LogedUser.User_type && x.Company_ID == _LogedUser.Company_ID).ToList();
                ViewBag.user = user;
            }
            if (status == 101)
            {
                List<Leave_Master> leave = db.Leave_Master.Where(x => x.Status != 6 & x.Leave_Crdr == 2 && x.UserMaster.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.ReqDate).ToList();

                List<Leave_Master> topleave = db.Leave_Master.Where(x => x.Status != 6 & x.Leave_Crdr == 2 && x.UserMaster.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.From_Date).ThenByDescending(x => x.leave_Id).Take(4).ToList();
                ViewBag.topleave = topleave;
                List<UserMaster> user = db.UserMasters.Where(x => x.User_type != _LogedUser.User_type && x.Company_ID == _LogedUser.Company_ID && x.status == 0).ToList();
                ViewBag.user = user;
                if (Sts == 99)
                {

                }
                else
                {
                    leave = leave.Where(x => x.Status == Sts).ToList();
                }
                ViewBag.leaves = leave;
            }


            ViewBag.Sts = Sts;
            return View();
        }

        public ActionResult LeaveGetreqres(List<long> leavemyList)
        {

            if (leavemyList.Count != 0)
            {
                foreach (var item in leavemyList)
                {
                    Leave_Master rm = db.Leave_Master.Where(x => x.leave_Id == item).FirstOrDefault();
                    if (rm != null)
                    {
                        rm.Status = 1;
                        db.SaveChanges();
                    }
                }
                return Json(new { success = true, msg = "LeaveRequest Approved Successfully !" });
            }
            else
            {
                return Json(new { success = false, msg = "Please select LeaveRequest that can be approved!" });
            }
        }

        
             public ActionResult LeaveGetreqresreject(List<long> leavemyListreject)
        {

            if (leavemyListreject.Count != 0)
            {
                foreach (var item in leavemyListreject)
                {
                    Leave_Master rm = db.Leave_Master.Where(x => x.leave_Id == item).FirstOrDefault();
                    if (rm != null)
                    {
                        rm.Status = 2;
                        db.SaveChanges();
                    }
                }
                return Json(new { success = true, msg = "LeaveRequest Rejected Successfully !" });
            }
            else
            {
                return Json(new { success = false, msg = "Please select LeaveRequest that can be Rejected!" });
            }
        }

        [HttpPost]
        public ActionResult Leave_Emp_Add(FormCollection form)
        {

            long Emp_Id = long.Parse(form["Emp_Id"].ToString());
            long Leave_Id = long.Parse(form["leave_Id"].ToString());
            DateTime From_Date = DateTime.Parse(form["From_Date"].ToString());
            DateTime To_Date = DateTime.Parse(form["To_Date"].ToString());
            string Leave_Type = form["Leave_Type"].ToString();
            string reason = form["reason"].ToString();
            string Remark = form["Remark"].ToString();
            List<Holiday_Master> holidays = db.Holiday_Master.ToList();
            int days = (To_Date - From_Date).Days + 1;
            List<DateTime> leaveDays = new List<DateTime>();
            List<DateTime> leaveDaysNew = new List<DateTime>();
            string Cmd = form["Command"].ToString();

            if (Cmd == "Save")
            {

                if (Leave_Id == 0)
                {
                    if (To_Date < From_Date)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "To Date Can't before From Date  !" });
                    }
                    List<Leave_Master> leaves = db.Leave_Master.Where(x => x.Status != 6 && x.User_Id == Emp_Id).ToList();
                    foreach (Leave_Master leave in leaves)
                    {
                        if (leave.From_Date == From_Date)
                        {
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave request already exist !" });
                        }
                    }
                    foreach (Holiday_Master hm in holidays)
                    {
                        for (var day = From_Date.Date; day <= To_Date; day = day.AddDays(1))
                        {
                            if (day.DayOfWeek == DayOfWeek.Sunday || day.Date == hm.Holiday_Date)
                            {
                                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave days contains holiday!" });
                            }
                        }
                    }

                    //if (From_Date < DateTime.Now.Date)
                    //{
                    //    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Yesterday Leave Not Consider!" });
                    //}

                    TimeSpan l = To_Date.Subtract(From_Date);
                    double answer = (To_Date - From_Date).TotalDays;
                    int Days = Convert.ToInt32(answer);
                    decimal _TotalDays = Days;
                    if (form["F_Date"] == "1")
                    {
                        _TotalDays = _TotalDays - decimal.Parse("0.5");
                    }
                    if (form["T_Date"] == "1") { _TotalDays = _TotalDays - decimal.Parse("0.5"); }
                    _TotalDays = _TotalDays + 1;
                    if (_TotalDays <= 0)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Can't Add 0 Leave" });
                    }
                    Leave_Master Leave = new Leave_Master();
                    Leave.User_Id = Emp_Id;
                    Leave.From_Date = From_Date;
                    Leave.To_Date = To_Date;
                    Leave.Leave_Crdr = 2;
                    Leave.Reason = reason;
                    Leave.Remark = Remark;
                    if (form["F_Date"] != null)
                    {
                        int F_Date = int.Parse(form["F_Date"].ToString());
                        Leave.F_Date = F_Date;
                    }
                    else
                    {
                        Leave.F_Date = 0;
                    }
                    if (form["T_Date"] != null)
                    {
                        int T_Date = int.Parse(form["T_Date"].ToString());
                        Leave.T_Date = T_Date;
                    }
                    else
                    {
                        Leave.T_Date = 0;
                    }
                    Leave.ReqDate = DateTime.Now;
                    Leave.Leave_Type = Leave_Type;
                    Leave.Status = 0;
                    Leave.Add_By = _LogedUser.User_ID;
                    Leave.leave_days = _TotalDays;
                    db.Leave_Master.Add(Leave);
                    UserMaster u = db.UserMasters.Where(x => x.User_ID == Emp_Id).FirstOrDefault();
                    if (Leave.Leave_Type == "PL" && Leave.leave_days > u.Total_PL)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "The Employee Does Not Have PL" });
                    }
                    else if (Leave.Leave_Type == "CL" && Leave.leave_days > u.Total_CL)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "The Employee Does Not Have CL" });
                    }
                    else if (Leave.Leave_Type == "EL" && Leave.leave_days > u.Total_EL)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "The Employee Does Not Have EL" });
                    }

                    db.SaveChanges();



                    ///Email Send.....................

                    //string sub = "Please Approved My Leave ";
                    //string message = "Please Approved My Leave";
                    //var subject = "Regarding Leave ";
                    //var body = "Dear Sir , <br/> I Send Leave Request Due : <br/>From Date: " + From_Date + "<br/> To Date: " + To_Date + "<br/>  Subject: " + sub + "<br/>Message: " + message + "<br/>Thank You !  <br/> This is System Generated Mail....!";
                    //MailAddress fromAddress = new MailAddress(_LogedUser.User_Email);
                    //MailAddress toAddress = new MailAddress(_LogedUser.User_Email);
                    //Thread t1 = null;
                    //t1 = new Thread(new ThreadStart(() => new CommonClasses().Email_Verify(fromAddress.ToString(), toAddress.ToString(), subject.ToString(), body.ToString())));
                    //t1.Start();

                    TempData["Result"] = "New leave Added Successfully.";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "New leave Added Successfully !", Refresh = "Default" });

                }
                else
                {

                    if (To_Date < From_Date)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "To Date Can't before From Date  !" });
                    }

                    List<Leave_Master> leaves = db.Leave_Master.Where(x => x.Status != 6 && x.leave_Id != Leave_Id && x.User_Id == Emp_Id).ToList();
                    foreach (Leave_Master leave in leaves)
                    {
                        if (leave.From_Date == From_Date)
                        {


                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave request already exist !" });



                        }
                    }
                    foreach (Holiday_Master hm in holidays)
                    {
                        for (var day = From_Date.Date; day <= To_Date; day = day.AddDays(1))
                        {
                            if (day.DayOfWeek == DayOfWeek.Sunday || day.Date == hm.Holiday_Date)
                            {
                                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave days contains holiday!" });
                            }
                        }
                    }


                    //if (From_Date < DateTime.Now.Date)
                    //{
                    //    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Yesterday Leave Not Consider!" });
                    //}

                    TimeSpan l = To_Date.Subtract(From_Date);
                    double answer = (To_Date - From_Date).TotalDays;
                    int Days = Convert.ToInt32(answer);
                    decimal _TotalDays = Days;
                    if (form["F_Date"] == "1")
                    {
                        _TotalDays = _TotalDays - decimal.Parse("0.5");
                    }
                    if (form["T_Date"] == "1") { _TotalDays = _TotalDays - decimal.Parse("0.5"); }
                    _TotalDays = _TotalDays + 1;
                    if (_TotalDays <= 0)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Can't Add 0 Leave" });
                    }
                    Leave_Master Leave = db.Leave_Master.Where(x => x.leave_Id == Leave_Id).FirstOrDefault();
                    //if (Leave.Status == 1)
                    //{
                    //    TempData["failure"] = "Leave Approved Not Changed!";
                    //    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave Approved Not Changed!" });
                    //}

                    Leave.User_Id = Emp_Id;
                    Leave.From_Date = From_Date;
                    Leave.To_Date = To_Date;
                    Leave.Leave_Crdr = 2;
                    Leave.Reason = reason;
                    Leave.Remark = Remark;
                    if (form["F_Date"] != null)
                    {
                        int F_Date = int.Parse(form["F_Date"].ToString());
                        Leave.F_Date = F_Date;
                    }
                    else
                    {
                        Leave.F_Date = 0;
                    }
                    if (form["T_Date"] != null)
                    {
                        int T_Date = int.Parse(form["T_Date"].ToString());
                        Leave.T_Date = T_Date;
                    }
                    else
                    {
                        Leave.T_Date = 0;
                    }
                    Leave.ReqDate = DateTime.Now;
                    Leave.Leave_Type = Leave_Type;
                    Leave.Status = 0;
                    Leave.Add_By = _LogedUser.User_ID;
                    Leave.leave_days = _TotalDays;
                    UserMaster u = db.UserMasters.Where(x => x.User_ID == Emp_Id).FirstOrDefault();
                    if (Leave.Leave_Type == "PL" && Leave.leave_days > u.Total_PL)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "The Employee Does Not Have PL" });
                    }
                    else if (Leave.Leave_Type == "CL" && Leave.leave_days > u.Total_CL)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "The Employee Does Not Have CL" });
                    }
                    else if (Leave.Leave_Type == "EL" && Leave.leave_days > u.Total_EL)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "The Employee Does Not Have EL" });
                    }
                    db.SaveChanges();
                    TempData["Result"] = "leave Updated Successfully.";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "leave updated Successfully !", Refresh = "Default" });

                }

                //if (From_Date < DateTime.Now.Date)
                //{
                //    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "From Date must be after Today  !" });
                //}
                //if (To_Date < From_Date)
                //{
                //    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "To Date Can't before From Date  !" });
                //}
                //TimeSpan l = To_Date.Subtract(From_Date);
                //double answer = (To_Date - From_Date).TotalDays;
                //int Days = Convert.ToInt32(answer);
                //decimal _TotalDays = Days;
                //if (form["F_Date"] == "1") { _TotalDays = _TotalDays - decimal.Parse("0.5"); }
                //if (form["T_Date"] == "1") { _TotalDays = _TotalDays - decimal.Parse("0.5"); }
                //_TotalDays = _TotalDays + 1;
                //if (_TotalDays <= 0)
                //{
                //    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Can't Add 0 Leave" });
                //}
                //Leave_Master Leave = db.Leave_Master.Where(x => x.leave_Id == Leave_Id).FirstOrDefault();
                //if (Leave.Status == 1)
                //{
                //    TempData["failure"] = "Leave Approved Not Changed!";
                //    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave Approved Not Changed!" });
                //}
                //if (Leave.From_Date < From_Date)
                //{
                //    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Can't Update Before Form Date " });
                //}
                //if (Leave.Status == 1)
                //{
                //    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Can't Update " });
                //}
                //Leave.From_Date = From_Date;
                //Leave.To_Date = To_Date;
                //Leave.Reason = reason;
                //if (form["F_Date"] != null)
                //{
                //    int F_Date = int.Parse(form["F_Date"].ToString());
                //    Leave.F_Date = F_Date;
                //}
                //else
                //{
                //    Leave.F_Date = 0;
                //}
                //if (form["T_Date"] != null)
                //{
                //    int T_Date = int.Parse(form["T_Date"].ToString());
                //    Leave.T_Date = T_Date;
                //}
                //else
                //{
                //    Leave.T_Date = 0;
                //}
                //Leave.ReqDate = DateTime.Now;
                //Leave.Remark = Remark;
                //Leave.Leave_Type = Leave_Type;
                //Leave.leave_days = _TotalDays;
                //UserMaster u = db.UserMasters.Where(x => x.User_ID == _LogedUser.User_ID).FirstOrDefault();
                //if (Leave.Leave_Type == "PL" && Leave.leave_days > u.Total_PL)
                //{
                //    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "The Employee Does Not Have PL" });
                //}
                //else if (Leave.Leave_Type == "CL" && Leave.leave_days > u.Total_CL)
                //{
                //    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "The Employee Does Not Have CL" });
                //}
                //else if (Leave.Leave_Type == "EL" && Leave.leave_days > u.Total_EL)
                //{
                //    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "The Employee Does Not Have EL" });
                //}
                //db.SaveChanges();
                //TempData["Result"] = "leave Updated Successfully.";
                //return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "leave updated Successfully !", Refresh = "Default" });

                //if (Leave_Id == 0)
                //{
                //    if (From_Date < DateTime.Now.Date)
                //    {
                //        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "From Date must be after Today  !" });
                //    }
                //    if (From_Date == DateTime.Now.Date)
                //    {
                //        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Today Leave Not Consider!" });
                //    }
                //    if (To_Date < From_Date)
                //    {
                //        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "To Date Can't before From Date  !" });
                //    }
                //    List<Leave_Master> leave = db.Leave_Master.Where(x => x.From_Date == From_Date).ToList();
                //    if (leave.Count != 0)
                //    {
                //        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave request already exist !" });
                //    }
                //    //for (var day = From_Date.Date; day <= To_Date; day = day.AddDays(1))
                //    //{
                //    //    if (day.DayOfWeek==DayOfWeek.Sunday)
                //    //    {
                //    //        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave days contains holiday!" });
                //    //    }
                //    //}
                //    foreach (Holiday_Master hm in holidays)
                //    {
                //        for (var day = From_Date.Date; day <= To_Date; day = day.AddDays(1))
                //        {
                //            if (day.DayOfWeek == DayOfWeek.Sunday || day.Date==hm.Holiday_Date)
                //            {
                //                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave days contains holiday!" });
                //            }
                //        }
                //    }
                //    TimeSpan l = To_Date.Subtract(From_Date);
                //    double answer = (To_Date - From_Date).TotalDays;
                //    int Days = Convert.ToInt32(answer);
                //    decimal _TotalDays = Days;
                //    if (form["F_Date"] == "1")
                //    {
                //        _TotalDays = _TotalDays - decimal.Parse("0.5");
                //    }
                //    if (form["T_Date"] == "1") { _TotalDays = _TotalDays - decimal.Parse("0.5"); }
                //    _TotalDays = _TotalDays + 1;
                //    if (_TotalDays <= 0)
                //    {
                //        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Can't Add 0 Leave" });
                //    }
                //    Leave_Master Leave = new Leave_Master();
                //    Leave.User_Id = Emp_Id;
                //    Leave.From_Date = From_Date;
                //    Leave.To_Date = To_Date;
                //    Leave.Leave_Crdr = 2;
                //    Leave.Reason = reason;
                //    Leave.Remark = Remark;
                //    if (form["F_Date"] != null)
                //    {
                //        int F_Date = int.Parse(form["F_Date"].ToString());
                //        Leave.F_Date = F_Date;
                //    }
                //    else
                //    {
                //        Leave.F_Date = 0;
                //    }
                //    if (form["T_Date"] != null)
                //    {
                //        int T_Date = int.Parse(form["T_Date"].ToString());
                //        Leave.T_Date = T_Date;
                //    }
                //    else
                //    {
                //        Leave.T_Date = 0;
                //    }
                //    Leave.ReqDate = DateTime.Now;
                //    Leave.Leave_Type = Leave_Type;
                //    Leave.Status = 0;
                //    Leave.Add_By = _LogedUser.User_ID;
                //    Leave.leave_days = _TotalDays;
                //    db.Leave_Master.Add(Leave);
                //    UserMaster u = db.UserMasters.Where(x => x.User_ID == Emp_Id).FirstOrDefault();
                //    if (Leave.Leave_Type == "PL" && Leave.leave_days > u.Total_PL)
                //    {
                //        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "The Employee Does Not Have PL" });
                //    }
                //    else if (Leave.Leave_Type == "CL" && Leave.leave_days > u.Total_CL)
                //    {
                //        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "The Employee Does Not Have CL" });
                //    }
                //    else if (Leave.Leave_Type == "EL" && Leave.leave_days > u.Total_EL)
                //    {
                //        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "The Employee Does Not Have EL" });
                //    }
                //    db.SaveChanges();
                //    TempData["Result"] = "leave Added Successfully.";
                //    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "New leave Added Successfully !", Refresh = "Default" });
                //}
                //else
                //{
                //    if (From_Date < DateTime.Now.Date)
                //    {
                //        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "From Date must be after Today  !" });
                //    }
                //    if (To_Date < From_Date)
                //    {
                //        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "To Date Can't before From Date  !" });
                //    }
                //    TimeSpan l = To_Date.Subtract(From_Date);
                //    double answer = (To_Date - From_Date).TotalDays;
                //    int Days = Convert.ToInt32(answer);
                //    decimal _TotalDays = Days;
                //    if (form["F_Date"] == "1") { _TotalDays = _TotalDays - decimal.Parse("0.5"); }
                //    if (form["T_Date"] == "1") { _TotalDays = _TotalDays - decimal.Parse("0.5"); }
                //    _TotalDays = _TotalDays + 1;

                //    if (_TotalDays <= 0)
                //    {
                //        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Can't Add 0 Leave" });
                //    }
                //    Leave_Master Leave = db.Leave_Master.Where(x => x.leave_Id == Leave_Id).FirstOrDefault();
                //    if (Leave.Status == 1)
                //    {
                //        TempData["failure"] = "Leave Approved Not Changed!";
                //        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave Approved Not Changed!" });
                //    }
                //    if (Leave.From_Date < From_Date)
                //    {
                //        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Can't Update Before Form Date " });
                //    }
                //    if (Leave.Status == 1)
                //    {
                //        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Can't Update " });
                //    }
                //    Leave.User_Id = Emp_Id;
                //    Leave.From_Date = From_Date;
                //    Leave.To_Date = To_Date;
                //    Leave.Reason = reason;
                //    if (form["F_Date"] != null)
                //    {
                //        int F_Date = int.Parse(form["F_Date"].ToString());
                //        Leave.F_Date = F_Date;
                //    }
                //    else
                //    {
                //        Leave.F_Date = 0;
                //    }
                //    if (form["T_Date"] != null)
                //    {
                //        int T_Date = int.Parse(form["T_Date"].ToString());
                //        Leave.T_Date = T_Date;
                //    }
                //    else
                //    {
                //        Leave.T_Date = 0;
                //    }
                //    Leave.Remark = Remark;
                //    Leave.Leave_Type = Leave_Type;
                //    Leave.Add_By = _LogedUser.User_ID;
                //    Leave.leave_days = _TotalDays;
                //    UserMaster u = db.UserMasters.Where(x => x.User_ID == Emp_Id).FirstOrDefault();
                //    if (Leave.Leave_Type == "PL" && Leave.leave_days > u.Total_PL)
                //    {
                //        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "The Employee Does Not Have PL" });
                //    }
                //    else if (Leave.Leave_Type == "CL" && Leave.leave_days > u.Total_CL)
                //    {
                //        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "The Employee Does Not Have CL" });
                //    }
                //    else if (Leave.Leave_Type == "EL" && Leave.leave_days > u.Total_EL)
                //    {
                //        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "The Employee Does Not Have EL" });
                //    }

                //    db.SaveChanges();
                //    TempData["Result"] = "leave Updated Successfully.";
                //    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "leave updated Successfully !", Refresh = "Default" });
                //}
            }
            else if (Cmd == "Approve")
            {
                UserMaster u = db.UserMasters.Where(x => x.User_ID == Emp_Id).FirstOrDefault();
                Leave_Master Leave = db.Leave_Master.Where(x => x.leave_Id == Leave_Id).FirstOrDefault();
                if (Leave == null)
                {
                    TempData["failure"] = "Invalid Leave  !";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Leave  !" });
                }
                if (Leave.Status == 2)
                {
                    TempData["failure"] = " Leave  Rejected Not Changed!";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = " Leave  Rejected Not Changed!" });
                }
                Leave.Status = 1;

                if (Leave.Leave_Type == "PL" & u.Total_PL >= Leave.leave_days)
                {
                    u.Total_PL = (decimal)(u.Total_PL - Leave.leave_days);
                }

                else if (Leave.Leave_Type == "CL" & u.Total_CL >= Leave.leave_days)
                {
                    u.Total_CL = (decimal)(u.Total_CL - Leave.leave_days);
                }

                else if (Leave.Leave_Type == "EL" & u.Total_EL >= Leave.leave_days)
                {
                    u.Total_EL = (decimal)(u.Total_EL - Leave.leave_days);
                }
                else if (Leave.Leave_Type == "LWPS")
                {
                    Leave.Status = 1;
                }
                else
                {
                    TempData["failure"] = "The Employee Does Not Have " + Leave.Leave_Type;
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "The Employee Does Not Have " + Leave.Leave_Type });
                }
                Leave.Approved_By = _LogedUser.User_ID;
                u.Total_Leave = u.Total_PL + u.Total_CL + u.Total_EL;
                db.SaveChanges();

                ////////////Email Send.....................

                //string sub = "Your Leave Will Approved";
                //string message = "Your Leave Will Approved";
                //var subject = "Regarding Leave";
                //var body = "Dear Employee , <br/> Your Leave will Approved  Pelase take : <br/>From : " + From_Date + "<br/>To: " + To_Date + "<br/>  Subject: " + sub + "<br/>Message: " + message + "<br/>Thank You !  <br/> This is System Generated Mail....!";
                //MailAddress fromAddress = new MailAddress(_LogedUser.User_Email);
                //MailAddress toAddress = new MailAddress(u.User_Email);
                //Thread t1 = null;
                //t1 = new Thread(new ThreadStart(() => new CommonClasses().Email_Verify(fromAddress.ToString(), toAddress.ToString(), subject.ToString(), body.ToString())));
                //t1.Start();

                TempData["Result"] = "Leave Approved Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Leave Approved Successfully !", Refresh = "Default" });
            }
            else if (Cmd == "Rejected")
            {
                Leave_Master Leave = db.Leave_Master.Where(x => x.leave_Id == Leave_Id).FirstOrDefault();
                if (Leave == null)
                {
                    TempData["failure"] = "Invalid Leave  !";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Leave  !" });
                }
                if (Leave.Status == 1)
                {
                    //    TempData["failure"] = "Leave Approved Not Changed!";
                    //    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave Approved Not Changed!" });
                    UserMaster um = db.UserMasters.Where(x => x.User_ID == Emp_Id).FirstOrDefault();
                    if (Leave.Leave_Type == "PL")
                    {
                        um.Total_PL = (decimal)(um.Total_PL + Leave.leave_days);
                    }

                    else if (Leave.Leave_Type == "CL")
                    {
                        um.Total_CL = (decimal)(um.Total_CL + Leave.leave_days);
                    }

                    else if (Leave.Leave_Type == "EL")
                    {
                        um.Total_EL = (decimal)(um.Total_EL + Leave.leave_days);
                    }
                    Leave.Approved_By = _LogedUser.User_ID;
                    um.Total_Leave = um.Total_PL + um.Total_CL + um.Total_EL;
                }
                Leave.Approved_By = _LogedUser.User_ID;
                Leave.Status = 2;
                db.SaveChanges();
                UserMaster u = db.UserMasters.Where(x => x.User_ID == Emp_Id).FirstOrDefault();

                ////////////Email Send.....................

                //string sub = "Your Leave will Rejected";
                //string message = "Your Leave will Rejected";
                //var subject = "Regarding Leave";
                //var body = "Dear Employee , <br/> Your Leave will Rejected <br/>  Subject: " + sub + "<br/>Message: " + message + "<br/>Thank You ! ";
                //MailAddress fromAddress = new MailAddress(_LogedUser.User_Email);
                //MailAddress toAddress = new MailAddress(u.User_Email);
                //Thread t1 = null;
                //t1 = new Thread(new ThreadStart(() => new CommonClasses().Email_Verify(fromAddress.ToString(), toAddress.ToString(), subject.ToString(), body.ToString())));
                //t1.Start();

                TempData["failure"] = "Leave Rejected Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave Rejected Successfully !", Refresh = "Default" });
            }
            else if (Cmd == "Delete")
            {
                Leave_Master Leave = db.Leave_Master.Where(x => x.leave_Id == Leave_Id).FirstOrDefault();
                var _del = db.Leave_Master.Find(Leave_Id);
                //if (Leave.Status == 1)
                //{
                //    TempData["failure"] = "Leave Approved Not Changed!";
                //    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave Approved Not Changed!" });
                //}
                if (Leave == null)
                {
                    TempData["failure"] = "Invalid Leave  !";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Leave  !" });
                }
                Leave.Status = 6;
                db.SaveChanges();
                TempData["failure"] = "Leave Deleted Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Leave Deleted Successfully !", Refresh = "Default" });
            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !" });
            }

        }

        public ActionResult Leave_Emp_Delete(FormCollection form)
        {
            long Leave_Id = long.Parse(form["lev_Id"].ToString());
            Leave_Master Leave = db.Leave_Master.Where(x => x.leave_Id == Leave_Id).FirstOrDefault();
            var _del = db.Leave_Master.Find(Leave_Id);
            if (Leave.Status == 1)
            {
                TempData["failure"] = "Leave Approved Not Changed!";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave Approved Not Changed!" });
            }
            if (Leave == null)
            {
                TempData["failure"] = "Invalid Leave  !";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Leave  !" });
            }
            Leave.Status = 6;
            db.SaveChanges();
            TempData["failure"] = "Leave Deleted Successfully !";
            return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Leave Deleted Successfully !", Refresh = "Default" });
        }
        public ActionResult View_Emp_Leave_Transaction(FormCollection form)
        {
            ViewBag.PageTitle = "Leave Transaction";

            List<UserMaster> user = db.UserMasters.Where(x => x.User_type != _LogedUser.User_type && x.Company_ID == _LogedUser.Company_ID && x.status == 0).ToList();
            ViewBag.user = user;
            if (form["Emp_Id"] != null)
            {
                long Emp_Id = long.Parse(form["Emp_Id"].ToString());
                List<Leave_Master> _Leave = db.Leave_Master.Where(x => x.User_Id == Emp_Id && x.UserMaster.Company_ID == _LogedUser.Company_ID).ToList();
                ViewBag._Leave = _Leave;
                UserMaster u = db.UserMasters.Where(x => x.User_ID == Emp_Id && x.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
                ViewBag.u = u;
                ViewBag.user_id = u.User_ID;
                ViewBag.u_name = u.User_Name;
            }
            return View();

        }
        public ActionResult Emp_Leave()
        {
            ViewBag.PageTitle = "Add Leave";

            List<UserMaster> user = db.UserMasters.Where(x => x.User_type != _LogedUser.User_type && x.Company_ID == _LogedUser.Company_ID && x.status == 0).ToList();
            ViewBag.user = user;
            return View();
        }
        [HttpPost]
        public ActionResult Emp_Leave(FormCollection form)
        {

            string Leave_Type = form["Leave_Type"].ToString();
            decimal Leave_Days = decimal.Parse(form["Total_Days"].ToString());
            int year = DateTime.Now.Year;
            DateTime From_Date = DateTime.Parse(year + "/01/01 00:00:00 AM");
            DateTime To_Date = DateTime.Parse(year + "/12/31 00:00:00 AM");
            string reason = form["reason"].ToString();
            if (form["Emp_Id_0"] != null)
            {
                List<UserMaster> u1 = db.UserMasters.Where(x => x.User_type != _LogedUser.User_type && x.Company_ID == _LogedUser.Company_ID & x.status == 0).ToList();
                foreach (UserMaster u in u1)
                {
                    if (u.User_type != _LogedUser.User_type)
                    {
                        Leave_Master Leave = new Leave_Master();
                        Leave.User_Id = u.User_ID;
                        Leave.From_Date = From_Date;
                        Leave.To_Date = To_Date;
                        Leave.Status = 1;
                        Leave.Leave_Crdr = 2;
                        Leave.Reason = reason;
                        Leave.Leave_Type = Leave_Type;
                        Leave.leave_days = Leave_Days;
                        Leave.Add_By = _LogedUser.User_ID;
                        Leave.Leave_Crdr = 1;
                        db.Leave_Master.Add(Leave);
                        if (Leave.Leave_Type == "PL")
                        {
                            u.Total_PL = (decimal)(u.Total_PL + Leave.leave_days);
                        }
                        if (Leave.Leave_Type == "CL")
                        {
                            u.Total_CL = (decimal)(u.Total_CL + Leave.leave_days);
                        }
                        if (Leave.Leave_Type == "EL")
                        {
                            u.Total_EL = (decimal)(u.Total_EL + Leave.leave_days);
                        }
                        u.Total_Leave = u.Total_PL + u.Total_CL + u.Total_EL;
                        if (Leave.Leave_Type == "EL" && Leave_Days > 5)
                        {
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Leave  !" });
                        }
                        u.Total_Leave = u.Total_PL + u.Total_CL + u.Total_EL;
                    }
                }
                db.SaveChanges();
                TempData["Result"] = "leave Added Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "leave Added Successfully !", Refresh = "Default" });
            }
            else
            {
                int t = 0;
                List<UserMaster> u1 = db.UserMasters.Where(x => x.User_type != _LogedUser.User_type && x.Company_ID == _LogedUser.Company_ID).ToList();
                for (int j = 1; j <= u1.Count; j++)
                {
                    if (form["Emp_Id_" + j] != null)
                    {
                        UserMaster u = u1.Where(x => x.User_ID == int.Parse(form["Emp_Id_" + j].ToString()) & x.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
                        if (u.User_type != 21)
                        {
                            Leave_Master Leave = new Leave_Master();
                            Leave.User_Id = u.User_ID;
                            Leave.From_Date = From_Date;
                            Leave.To_Date = To_Date;
                            Leave.Leave_Crdr = 1;
                            Leave.Reason = reason;
                            Leave.Add_By = _LogedUser.User_ID;
                            Leave.Leave_Type = Leave_Type;
                            Leave.Status = 1;
                            Leave.leave_days = Leave_Days;
                            Leave.Leave_Crdr = 1;
                            t++;
                            db.Leave_Master.Add(Leave);
                            if (Leave.Leave_Type == "PL")
                            {
                                u.Total_PL = (decimal)(u.Total_PL + Leave.leave_days);
                            }
                            if (Leave.Leave_Type == "CL")
                            {
                                u.Total_CL = (decimal)(u.Total_CL + Leave.leave_days);
                            }
                            if (Leave.Leave_Type == "EL")
                            {
                                u.Total_EL = (decimal)(u.Total_EL + Leave.leave_days);
                            }
                            u.Total_Leave = u.Total_PL + u.Total_CL + u.Total_EL;
                            if (Leave.Leave_Type == "EL" && Leave_Days > 5)
                            {
                                TempData["failure"] = "Leave_Type EL not added more than 5.";
                                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave_Type EL not added more than 5." });
                            }
                            u.Total_Leave = u.Total_PL + u.Total_CL + u.Total_EL;
                        }
                    }
                }
                if (t == 0)
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgFail = "Please Add Employee." });
                }
                else
                {
                    db.SaveChanges();
                    TempData["Result"] = "leave Added Successfully.";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "leave Added Successfully !", Refresh = "Default" });
                }
            }
        }
        public ActionResult GetEmp(long Emp_Id)
        {
            string DOJ, team_name, Report_Person;
            long desg, dept;
            UserMaster user = db.UserMasters.Where(x => x.User_ID == Emp_Id && x.Company_ID == x.Company_ID).FirstOrDefault();
            if (user != null)
            {
                return Json(new { success = true, DOJ = user.Date_of_join.ToString("dd MMMM yyyy"), team_name = user.Team_name, Report_Person = user.Reporting_Person, desg = user.Desig_id, dept = user.Depart_id });
            }
            else
            {
                return Json(new { success = false });
            }
        }
        public ActionResult Resignation_List()
        {
            ViewBag.PageTitle = "All Resignation";

            List<Resignation_Master> Resignation = db.Resignation_Master.Where(x => x.Status != 6 & x.UserMaster.Company_ID == _LogedUser.Company_ID).OrderBy(x => x.Status).ThenByDescending(x => x.Resignation_Id).ToList();
            ViewBag.Resignation = Resignation;
            List<UserMaster> user = db.UserMasters.Where(x => x.User_type != _LogedUser.User_type && x.Company_ID == _LogedUser.Company_ID && x.status == 0).ToList();
            ViewBag.user = user;
            List<UserMaster> user1 = db.UserMasters.Where(x => x.User_type == _LogedUser.User_type && x.Company_ID == _LogedUser.Company_ID && x.status == 0).ToList();
            ViewBag.user1 = user1;
            List<UserMaster> user2 = db.UserMasters.Where(x => x.Company_ID == _LogedUser.Company_ID && x.status == 0).ToList();
            ViewBag.user2 = user2;
            List<Designation_Master> desg = db.Designation_Master.Where(x => x.Company_ID == _LogedUser.Company_ID).ToList();
            ViewBag.desg = desg;
            List<Department_Master> dept = db.Department_Master.Where(x => x.Company_ID == _LogedUser.Company_ID).ToList();
            ViewBag.dept = dept;
            return View();
        }
        [HttpPost]
        public ActionResult Resignation_Add(FormCollection form)
        {

            long Emp_Id = long.Parse(form["Emp_Id"].ToString());
            long Resignation_Id = long.Parse(form["Resignation_Id"].ToString());
            long Designation = long.Parse(form["Designation"].ToString());
            long Department = long.Parse(form["Department"].ToString());
            string Team_Name = form["Team_Name"].ToString();
            DateTime Date_Of_Join = DateTime.Parse(form["Date_Of_Join"].ToString());
            long? Reporting_Person = long.Parse(form["Reporting_Manager"].ToString());
            DateTime Reliving_Date = DateTime.Parse(form["Reliving_Date"].ToString());
            string Reason_For_Exit = form["Reason_For_Exit"].ToString();
            string Remark_For_Resignation = form["Remark_For_Resignation"].ToString();
            string Suggestion_For_Company = form["Suggestion_For_Company"].ToString();
            long HR_Authority = long.Parse(form["HR_Authority"].ToString());
            string Cmd = form["Command"].ToString();
            List<long> eid = new List<long>();
            List<UserMaster> emp = db.UserMasters.Where(x => x.Company_ID == _LogedUser.Company_ID).ToList();
            foreach (UserMaster employee in emp.Where(x => x.status == 0))
            {
                eid.Add(employee.User_ID);
            }
            if (Cmd == "Save")
            {
                if (Resignation_Id == 0)
                {
                    foreach (long e in eid)
                    {
                        Resignation_Master resg = db.Resignation_Master.Where(x => x.Emp_Id == e).OrderByDescending(x => x.Resignation_Id).FirstOrDefault();
                        ViewBag.resg = resg;
                        if (resg != null)
                        {
                            if (resg.Status == 1)
                            {
                                TempData["failure"] = "Your Resignation Is Approved";
                                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Your Resignation Is Approved" });
                            }
                            if (resg.Status == 0)
                            {
                                TempData["failure"] = "Your Resignation Is Pending";
                                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Your Resignation Is Pending" });
                            }
                        }
                    }
                    if (DateTime.Now > Reliving_Date)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Please Add Proper Date." });
                    }
                    Resignation_Master Resignation = new Resignation_Master();
                    Resignation.Emp_Id = Emp_Id;
                    Resignation.Designation = Designation;
                    Resignation.Team_Name = Team_Name;
                    Resignation.Date_Of_Join = Date_Of_Join;
                    Resignation.Department = Department;
                    Resignation.Date_Of_Resignation = DateTime.Now;
                    Resignation.Reporting_Person = Reporting_Person;
                    Resignation.Reliving_Date = Reliving_Date;
                    Resignation.Reason_For_Exit = Reason_For_Exit;
                    Resignation.Remark_For_Resignation = Remark_For_Resignation;
                    Resignation.Suggestion_For_Company = Suggestion_For_Company;
                    Resignation.HR_Authority = HR_Authority;
                    Resignation.Status = 0;
                    db.Resignation_Master.Add(Resignation);
                    db.SaveChanges();
                    TempData["Result"] = "Resignation Added Successfully.";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Resignation Added Successfully !", Refresh = "Default" });
                }
                else
                {
                    Resignation_Master Resignation = db.Resignation_Master.Where(x => x.Resignation_Id == Resignation_Id).FirstOrDefault();
                    if (Resignation == null)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Resignation  !" });
                    }
                    //if (DateTime.Now < Reliving_Date)
                    //{
                    //    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Please Add Proper Date." });
                    //}
                    Resignation.Emp_Id = Emp_Id;
                    Resignation.Designation = Designation;
                    Resignation.Team_Name = Team_Name;
                    Resignation.Date_Of_Join = Date_Of_Join;
                    Resignation.Department = Department;
                    Resignation.Date_Of_Resignation = DateTime.Now;
                    Resignation.Reporting_Person = Reporting_Person;
                    Resignation.Reliving_Date = Reliving_Date;
                    Resignation.Reason_For_Exit = Reason_For_Exit;
                    Resignation.Remark_For_Resignation = Remark_For_Resignation;
                    Resignation.Suggestion_For_Company = Suggestion_For_Company;
                    Resignation.HR_Authority = HR_Authority;
                    Resignation.Status = 0;
                    db.SaveChanges();
                    TempData["Result"] = "Resignation Updated Successfully.";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Resignation Updated Successfully!", Refresh = "Default" });
                }
            }
            else if (Cmd == "Approve")
            {
                Resignation_Master Resignation = db.Resignation_Master.Where(x => x.Resignation_Id == Resignation_Id).FirstOrDefault();
                if (Resignation == null)
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Resignation  !" });
                }
                Resignation.Status = 1;
                db.SaveChanges();
                ////////////Email Send.....................
                UserMaster u = db.UserMasters.Where(x => x.User_ID == Emp_Id).FirstOrDefault();
                string sub = "Your Resignation will Approved";
                string message = "Your Resignation will Approved";
                var subject = "Regarding Leave";
                var body = "Dear Employee , <br/> Your Resignation will Approved <br/>  Subject: " + sub + "<br/>Message: " + message + "<br/>Thank You !  <br/> This is System Generated Mail....! ";
                MailAddress fromAddress = new MailAddress(_LogedUser.User_Email);
                MailAddress toAddress = new MailAddress(u.User_Email);
                Thread t1 = null;
                t1 = new Thread(new ThreadStart(() => new CommonClasses().Email_Verify(fromAddress.ToString(), toAddress.ToString(), subject.ToString(), body.ToString())));
                t1.Start();
                TempData["Result"] = "Resignation Approved Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Resignation Approved Successfully !", Refresh = "Default" });
            }
            else if (Cmd == "Rejected")
            {
                Resignation_Master Resignation = db.Resignation_Master.Where(x => x.Resignation_Id == Resignation_Id).FirstOrDefault();
                if (Resignation == null)
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Resignation  !" });
                }
                Resignation.Status = 2;
                db.SaveChanges();
                ////////////Email Send.....................
                UserMaster u = db.UserMasters.Where(x => x.User_ID == Emp_Id).FirstOrDefault();
                string sub = "Your Resignation will Rejected";
                string message = "Your Resignation will Rejected";
                var subject = "Regarding Leave";
                var body = "Dear Employee , <br/> Your Resignation will Rejected <br/>  Subject: " + sub + "<br/>Message: " + message + "<br/>Thank You ! ";
                MailAddress fromAddress = new MailAddress(_LogedUser.User_Email);
                MailAddress toAddress = new MailAddress(u.User_Email);
                Thread t1 = null;
                t1 = new Thread(new ThreadStart(() => new CommonClasses().Email_Verify(fromAddress.ToString(), toAddress.ToString(), subject.ToString(), body.ToString())));
                t1.Start();
                TempData["failure"] = "Resignation Rejected Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Resignation Rejected Successfully !", Refresh = "Default" });
            }
            else if (Cmd == "Delete")
            {
                Resignation_Master Resignation = db.Resignation_Master.Where(x => x.Resignation_Id == Resignation_Id).FirstOrDefault();
                var _del = db.Resignation_Master.Find(Resignation_Id);
                if (Resignation == null)
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Resignation  !" });
                }
                Resignation.Status = 6;
                //db.Project_Task_Master.Remove(_del);
                db.SaveChanges();
                TempData["failure"] = "Resignation Deleted Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Resignation Deleted Successfully !", Refresh = "Default" });
            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !" });
            }
        }
        public ActionResult Resignation_Delete(FormCollection form)
        {
            long Resignation_Id = long.Parse(form["Resig_Id"].ToString());
            Resignation_Master Resignation = db.Resignation_Master.Where(x => x.Resignation_Id == Resignation_Id).FirstOrDefault();
            var _del = db.Resignation_Master.Find(Resignation_Id);
            if (Resignation == null)
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Resignation  !" });
            }
            Resignation.Status = 6;
            db.SaveChanges();
            TempData["failure"] = "Resignation Deleted Successfully !";
            return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Resignation Deleted Successfully !", Refresh = "Default" });
        }
        public ActionResult Employee_Punch_List(FormCollection form)
        {
            ViewBag.PageTitle = "All Punch";
            List<User_Punch> user_Punch = new List<User_Punch>();
            List<User_Punch> user_punch1 = new List<User_Punch>();
            UserMaster u = new UserMaster();

            List<UserMaster> user = db.UserMasters.Where(x => x.User_type != _LogedUser.User_type && x.Company_ID == _LogedUser.Company_ID && x.status == 0).ToList();
            ViewBag.user = user;
            if (form["Emp_Id"] != null)
            {
                long Emp_Id = long.Parse(form["Emp_Id"].ToString());
                DateTime dt = DateTime.Parse(form["Punch_date"].ToString());
                List<TimeSpan> times = new List<TimeSpan>();
                List<string> PunchName = new List<string>();
                if (Emp_Id == -1)
                {
                    var punch = db.User_Punch.Where(x => x.UserMaster.Company_ID == _LogedUser.Company_ID && x.Time == dt).GroupBy(x => x.Time);
                    foreach (var d in punch)
                    {
                        foreach (var d1 in d)
                        {
                            String st = dt.ToString("dd MMMM yyyy");
                            DateTime stdt = Convert.ToDateTime(st);
                            string st1 = d1.Time.ToString("dd MMMM yyyy");
                            DateTime stdt1 = Convert.ToDateTime(st1);
                            if (stdt == stdt1)
                            {
                                times.Add(d1.Time.TimeOfDay);
                                PunchName.Add(d1.Punch_Type.Puch_Name);
                            }
                        }
                    }
                }
                else
                {
                    var punch = db.User_Punch.Where(x => x.User_Id == Emp_Id && x.UserMaster.Company_ID == _LogedUser.Company_ID && x.Time == dt).GroupBy(x => x.Time);
                    foreach (var d in punch)
                    {
                        foreach (var d1 in d)
                        {
                            String st = dt.ToString("dd MMMM yyyy");
                            DateTime stdt = Convert.ToDateTime(st);
                            string st1 = d1.Time.ToString("dd MMMM yyyy");
                            DateTime stdt1 = Convert.ToDateTime(st1);
                            if (stdt == stdt1)
                            {
                                times.Add(d1.Time.TimeOfDay);
                                PunchName.Add(d1.Punch_Type.Puch_Name);
                            }

                        }
                    }
                }

                var _u = db.User_Punch.Where(x => x.UserMaster.Company_ID == _LogedUser.Company_ID).GroupBy(x => x.Time).ToList();
                if (Emp_Id == -1)
                {
                    user_punch1 = db.User_Punch.Where(x => x.UserMaster.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.Punch_Id).ToList();
                    u = db.UserMasters.Where(x => x.User_type != _LogedUser.User_type && x.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
                }
                else
                {
                    user_punch1 = db.User_Punch.Where(x => x.User_Id == Emp_Id && x.UserMaster.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.Punch_Id).ToList();
                    u = db.UserMasters.Where(x => x.User_ID == Emp_Id && x.User_type != _LogedUser.User_type && x.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
                }


                foreach (User_Punch up in user_punch1)
                {
                    String st = dt.ToString("dd MMMM yyyy");
                    DateTime stdt = Convert.ToDateTime(st);
                    string st1 = up.Time.ToString("dd MMMM yyyy");
                    DateTime stdt1 = Convert.ToDateTime(st1);
                    if (stdt == stdt1)
                    {
                        user_Punch.Add(up);
                    }
                }

                ViewBag.user_punch = user_Punch;
                ViewBag.user_id = Emp_Id;
                ViewBag.dt = dt;
                ViewBag.u_name = u.User_Name;
                ViewBag.times = times;
                ViewBag._u = _u;
                ViewBag.PunchName = PunchName;

            }
            return View();
        }
        [HttpPost]
        public ActionResult Employee_Punch_Update(FormCollection form)
        {

            long Punch_Id = long.Parse(form["Punch_Id"].ToString());
            DateTime Punchdate = DateTime.Parse(form["Punchdate"].ToString());
            string Cmd = form["Command"].ToString();
            CommonClasses _newobj = new CommonClasses();
            if (Cmd == "Save")
            {
                User_Punch punch = db.User_Punch.Where(x => x.Punch_Id == Punch_Id && x.UserMaster.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
                if (punch == null)
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Punch  !" });
                }
                punch.Time = Punchdate;
                db.SaveChanges();
                _newobj._attendancefordate(Punchdate, punch.User_Id);
                TempData["Result"] = "Punch Updated Successfully.";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Punch Updated Successfully.", Refresh = "Default" });
            }
            else if (Cmd == "Delete")
            {
                User_Punch punch = db.User_Punch.Where(x => x.Punch_Id == Punch_Id && x.UserMaster.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
                if (punch == null)
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Punch  !" });
                }
                var _delt = db.User_Punch.Find(Punch_Id);
                db.User_Punch.Remove(_delt);
                db.SaveChanges();
                TempData["failure"] = "Punch Deleted Successfully.";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Punch Deleted Successfully.", Refresh = "Default" });

            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !" });
            }

        }
        public class Address
        {
            public string Description { get; set; }
            public string Longitude { get; set; }
            public string Latitude { get; set; }
            public string Title { get; set; }
        }

        public ActionResult GetMap(long Emp_Id, DateTime date)
        {

            List<User_Visits> v = db.User_Visits.Where(x => x.Time.Day == date.Day && x.Time.Month == date.Month && x.Time.Year == date.Year && x.User_Id == Emp_Id && x.UserMaster.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.Visit_id).ToList();
            int cnt = v.Count();
            List<string> title = new List<string>();
            List<string> lat = new List<string>();
            List<string> lng = new List<string>();
            List<string> desc = new List<string>();
            var address = new List<Address>();

            foreach (User_Visits _v in v)
            {
                new Address { Description = _v.Location, Longitude = _v.longitude, Latitude = _v.latitude, Title = _v.LocationName };
            }
            foreach (User_Visits _v in v)
            {
                title.Add(_v.LocationName);
                lat.Add(_v.latitude);
                lng.Add(_v.longitude);
                desc.Add(_v.Location);
            }
            int Count = v.Count();
            string markers = "[";
            foreach (User_Visits _v in v)
            {
                markers += "{";
                markers += string.Format("'title': '{0}',", _v.LocationName);
                markers += string.Format("'lat': '{0}',", _v.latitude);
                markers += string.Format("'lng': {0},", _v.longitude);
                markers += string.Format("'description': '{0}'", _v.Location);
                Count--;
                if (Count == 0)
                {
                    markers += "}";
                }
                else
                {
                    markers += "},";
                }
            }
            markers += "];";

            if (v != null)
            {
                return Json(new { success = true, markers = markers, title = title, lat = lat, lng = lng, desc = desc, cnt = cnt, v = address });
            }
            else
            {
                return Json(new { success = false });
            }
        }
        public ActionResult Employee_Visit_List(FormCollection form)
        {
            ViewBag.PageTitle = "All Visit";

            List<UserMaster> user = db.UserMasters.Where(x => x.User_type != _LogedUser.User_type && x.Company_ID == _LogedUser.Company_ID && x.status == 0).ToList();
            ViewBag.user = user;
            if (form["Emp_Id"] != null)
            {
                long Emp_Id = long.Parse(form["Emp_Id"].ToString());
                DateTime Date = DateTime.Parse(form["Date"].ToString());
                List<User_Visits> v = db.User_Visits.Where(x => x.Time.Day == Date.Day && x.Time.Month == Date.Month && x.Time.Year == Date.Year && x.User_Id == Emp_Id && x.UserMaster.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.Visit_id).ToList();
                ViewBag.v = v;
                List<string> str = new List<string>();
                foreach (User_Visits _v in v)
                {
                    string loc = "";
                    loc += "{";
                    loc += string.Format("'title': '{0}',", _v.LocationName);
                    loc += string.Format("'lat': '{0}',", _v.latitude);
                    loc += string.Format("'lng': '{0}',", _v.longitude);
                    loc += string.Format("'description': '{0}'", _v.Location);
                    loc += "},";
                    str.Add(loc);
                }
                User_Visits _V = db.User_Visits.Where(x => x.Time.Day == Date.Day && x.Time.Month == Date.Month && x.Time.Year == Date.Year && x.User_Id == Emp_Id && x.UserMaster.Company_ID == _LogedUser.Company_ID).FirstOrDefault();

                if (_V != null)
                {
                    string v_loc = "";
                    v_loc += "{";
                    v_loc += string.Format("'title': '{0}',", _V.LocationName);
                    v_loc += string.Format("'lat': '{0}',", _V.latitude);
                    v_loc += string.Format("'lng': '{0}',", _V.longitude);
                    v_loc += string.Format("'description': '{0}'", _V.Location);
                    v_loc += "},";
                    ViewBag.Visited_Str1 = v_loc;
                }
                ViewBag.Visited_Str = str;
                UserMaster u = db.UserMasters.Where(x => x.User_ID == Emp_Id && x.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
                ViewBag.user_id = u.User_ID;
                ViewBag.u_name = u.User_Name;
                if (form["Date"] != null)
                {
                    ViewBag.Visited_dt = Date;
                }
                else
                {
                    ViewBag.Visited_dt = DateTime.Now.Date;
                }
            }
            if (form["Date"] == null)
            {
                ViewBag.Visited_dt = DateTime.Now.Date;
            }
            return View();

        }
        public ActionResult Attendance_Report(FormCollection form)
        {
            ViewBag.PageTitle = "Attendance Report";
            if (form["datepicker"] != null)
            {
                DateTime date = DateTime.Parse(form["datepicker"].ToString());
                int Year = date.Year;
                ViewBag.Year = Year;
                int Month = date.Month;
                ViewBag.Month = Month;
                ViewBag.mm = date.ToString("MM-yyyy");
                DateTime today = DateTime.Today;

                DateTime endOfMonth = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
                int day = endOfMonth.Day;
                DateTime now = DateTime.Now;
                int totalSunDays;
                totalSunDays = 0;
                for (int i = 0; i < day; ++i)
                {
                    DateTime d = new DateTime(now.Year, now.Month, i + 1);
                    if (d.DayOfWeek == DayOfWeek.Sunday)
                    {
                        totalSunDays = totalSunDays + 1;
                    }
                }
                ViewBag.TotalSundays = totalSunDays;
                List<Holiday_Master> _holiday = db.Holiday_Master.Where(x => x.Company_Id == _LogedUser.Company_ID && x.Holiday_Date.Value.Month == date.Month && x.Holiday_Date.Value.Year == date.Year).ToList();
                int totalHolidays = _holiday.Count();
                int OffDays = totalSunDays + totalHolidays;
                ViewBag.TotalHoliday = totalHolidays;
                int totalDays = getdays(Year, Month);
                int PresentDays = totalDays - OffDays;
                ViewBag.WorkingDays = PresentDays;
                List<Attendance_Master> _atdn = db.Attendance_Master.Where(x => x.Punch_Date.Month == date.Month && x.Punch_Date.Year == date.Year).ToList();
                List<UserMaster> um = db.UserMasters.Where(x => x.User_type != _LogedUser.User_type && x.Company_ID == _LogedUser.Company_ID && x.status == 0).ToList();
                List<Leave_Master> leave = db.Leave_Master.Where(x => x.ReqDate.Month == date.Month && x.ReqDate.Year == date.Year && x.UserMaster.Company_ID == _LogedUser.Company_ID).ToList();
                ViewBag.user = um;
                ViewBag.attendance = _atdn;
                ViewBag.leave = leave;
                ViewBag.totalDays = totalDays;
            }
            return View();
        }
        public ActionResult Attendance_ReportNew(FormCollection form)
        {
            ViewBag.PageTitle = "Attendance Report";
            if (form["datepicker"] != null)
            {

                DateTime fromDate = DateTime.Parse(form["datepicker"].ToString());
                DateTime today = DateTime.Today;
                DateTime toDate = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
                int Year = fromDate.Year;
                ViewBag.Year = Year;
                int Month = fromDate.Month;
                ViewBag.Month = Month;
                ViewBag.mm = fromDate.ToString("MM-yyyy");
                List<GetCompanyPunchDetails_Result> punchDetails_Results = db.GetCompanyPunchDetails(fromDate, toDate, (int?)_LogedUser.Company_ID, 8, 4).ToList();
                List<GetCompanyPunchDetails_Result> userPunch = new List<GetCompanyPunchDetails_Result>();
                int totalSunDays = 0; int totalHolidays = 0; int totalWorkingDays = 0; int totalPaidLeave = 0;
                if (punchDetails_Results.Count > 0)
                {
                    foreach (GetCompanyPunchDetails_Result punch in punchDetails_Results)
                    {
                        totalSunDays = (int)punch.TotalSunday;
                        totalHolidays = (int)punch.TotalHolidays;
                        totalPaidLeave = totalSunDays + totalHolidays;
                        totalWorkingDays = (int)(punch.TotalDays - totalPaidLeave);

                        var cnt = userPunch.Where(x => x.User_Id == punch.User_Id).Count();
                        if (cnt == 0) { userPunch.Add(punch); }
                    }
                }
                ViewBag.TotalSundays = totalSunDays;
                ViewBag.TotalHoliday = totalHolidays;
                ViewBag.WorkingDays = totalWorkingDays;
                ViewBag.PunchDetail = userPunch;
            }
            return View();
        }

        public ActionResult ExportAllUserPunchToCsv(DateTime fromDate)
        {
            DateTime today = DateTime.Today;
            DateTime toDate = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));


            var punchDetails = db.GetCompanyPunchDetails(fromDate, toDate, (int?)_LogedUser.Company_ID, 8, 4).ToList();

            var csv = new StringBuilder();
            csv.AppendLine("EmployeeID, Name, Date, DayofWeek,Punch_In,Lunch_In,Lunch_Out,Punch_Out, PresentHrs, Detail, AttendanceCount, TotalDaysPresent");

            foreach (var item in punchDetails)
            {
                string dt = item.PunchDate.ToString("dd-MM-yyyy");
                DateTime fdt = DateTime.ParseExact(
                    dt,
                    "dd/MM/yyyy",
                    CultureInfo.InvariantCulture);
                List<User_Punch> up = db.User_Punch.Where(x =>x.User_Id== item.User_Id  && x.Time == fdt).ToList();
                string chkin = "";
                string chkout = "";
                string lunin = "";
                string lunout = "";
                if(up.Count>0)
                {
                    foreach(User_Punch u in up)
                    {
                        if(u.PunchType_Id==1)
                        {
                             chkin = u.Time.ToString("hh:mm:ss:tt");
                        }
                        if (u.PunchType_Id == 2)
                        {
                            chkout = u.Time.ToString("hh:mm:ss:tt");
                        }
                        if (u.PunchType_Id == 3)
                        {
                             lunin = u.Time.ToString("hh:mm:ss:tt");
                        }
                        if (u.PunchType_Id == 4)
                        {
                             lunout = u.Time.ToString("hh:mm:ss:tt");
                        }
                    }
                    csv.AppendLine($"{item.User_Id},{item.UserName},{item.PunchDate.ToString("yyyy-MM-dd")},{item.DayWeek},{chkin},{lunin},{lunout},{chkout},{item.PresentHrs},{item.Detail},{item.AttendanceCount},{item.TotalDaysPresent}");
                }
                else
                {
                    csv.AppendLine($"{item.User_Id},{item.UserName},{item.PunchDate.ToString("yyyy-MM-dd")},{item.DayWeek},{chkin},{lunin},{lunout},{chkout},{item.PresentHrs},{item.Detail},{item.AttendanceCount},{item.TotalDaysPresent}");
                }
                    
            }

            byte[] buffer = Encoding.UTF8.GetBytes(csv.ToString());
            return File(buffer, "text/csv", $"AllUserPunch{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.csv");
        }
        public ActionResult Attendance_historyNew(long User_Id, int Year, int Month)
        {
            ViewBag.PageTitle = "Attendance History";
            DateTime fromDate = new DateTime(Year, Month, 1);
            DateTime toDate = new DateTime(Year, Month, DateTime.DaysInMonth(Year, Month));
            var punchDetails_Results = db.GetUserPunchDetails(fromDate, toDate, (int?)User_Id, 8, 4);
            ViewBag.Punchs = punchDetails_Results;
            ViewBag.User_Id = User_Id;
            ViewBag.Year = Year;
            ViewBag.Month = Month;
            return View();

        }

        public ActionResult ExportUserPunchToCsv(long User_Id, int Year, int Month)
        {
            DateTime fromDate = new DateTime(Year, Month, 1);
            DateTime toDate = new DateTime(Year, Month, DateTime.DaysInMonth(Year, Month));

            var punchDetails = db.GetUserPunchDetails(fromDate, toDate, (int?)User_Id, 8, 4);

            if (punchDetails == null)
                return Content("No data available.");

            var csv = new StringBuilder();
            csv.AppendLine("EmployeeID, Name, Date, DayofWeek, PresentHrs, Detail, AttendanceCount, TotalDaysPresent");

            foreach (var item in punchDetails)
            {
                csv.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7}",
                    item.User_Id,
                    item.UserName,
                    item.PunchDate.ToString("yyyy-MM-dd"),
                    item.DayWeek,
                    item.PresentHrs,
                    item.Detail,
                    item.AttendanceCount,
                    item.TotalDaysPresent
                ));
            }

            byte[] buffer = Encoding.UTF8.GetBytes(csv.ToString());
            return File(buffer, "text/csv", $"UserPunchDetails_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.csv");
        }

        [HttpPost]
        public ActionResult UploadEmployeePunch(HttpPostedFileBase file)
        {
            try
            {
                if (file != null && file.ContentLength > 0)
                {
                    using (var workbook = new XLWorkbook(file.InputStream))
                    {
                        var worksheet = workbook.Worksheet(1);
                        var rows = worksheet.RowsUsed().Skip(1); // Skip header row
                        foreach (var row in rows)
                        {
                            long User_Id = long.Parse(row.Cell(1).GetValue<string>());
                            long PunchType_Id = long.Parse(row.Cell(2).GetValue<string>());
                            DateTime Time = DateTime.Parse(row.Cell(3).GetValue<string>());

                            User_Punch _newpunch = new User_Punch();
                            _newpunch.User_Id = User_Id;
                            _newpunch.PunchType_Id = PunchType_Id;
                            _newpunch.Time = Time;
                            db.User_Punch.Add(_newpunch);
                            db.SaveChanges();

                        }

                    }
                }
                TempData["Result"] = "User Punch Successfully !";
                return RedirectToAction("Attendance_Report");
            }
            catch (Exception e)
            {
                return RedirectToAction("Attendance_Report");
            }

        }
        public ActionResult DownloadPunchTemplate()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Questions");

                // Add header row
                worksheet.Cell(1, 1).Value = "User_Id";
                worksheet.Cell(1, 2).Value = "PunchType_Id";
                worksheet.Cell(1, 3).Value = "Time";

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "EmployeePunchTemplate.xlsx");
                }
            }
        }

        public ActionResult Attendance_history(long User_Id, int Year, int Month)
        {
            ViewBag.PageTitle = "Attendance History";

            ///Get Only Sunday Dates
            List<DateTime> Sundates = new List<DateTime>();
            Sundates = GetSundayDates(Year, Month);
            ViewBag.Sundates = Sundates;

            ///Get All Dates
            List<DateTime> AllDates = new List<DateTime>();
            AllDates = getAllDates(Year, Month);
            ViewBag.AllDates = AllDates;

            ///get Attendance
            List<Attendance_Master> _atdn = db.Attendance_Master.Where(x => x.Punch_Date.Month == Month && x.Punch_Date.Year == Year & x.User_Id == User_Id & x.UserMaster.Company_ID == _LogedUser.Company_ID).ToList();
            ViewBag.attendance = _atdn;

            ///get Leave
            List<Leave_Master> leave = db.Leave_Master.Where(x => x.ReqDate.Month == Month && x.ReqDate.Year == Year && x.User_Id == User_Id && x.UserMaster.Company_ID == _LogedUser.Company_ID && x.Status == 1).ToList();
            ViewBag.leave = leave;

            //.....Total Holidays
            List<Holiday_Master> _holiday = db.Holiday_Master.Where(x => x.Company_Id == _LogedUser.Company_ID && x.Holiday_Date.Value.Month == Month && x.Holiday_Date.Value.Year == Year).ToList();
            ViewBag._holiday = _holiday;




            /////Try
            //List<DateTime> cdt = new List<DateTime>();
            //List<string> str = new List<string>();
            //ViewBag.FinalDt = cdt;
            //ViewBag.FinalStr = str;
            //foreach (DateTime _All in AllDates)
            //{
            //    foreach (Attendance_Master _A in _atdn)
            //    { 
            //        foreach (DateTime _sun in Sundates)
            //        {
            //            foreach (Holiday_Master _h in _holiday)
            //            {
            //                foreach (Leave_Master _l in leave)
            //                {
            //                    if (_All.Date == _A.Punch_Date)
            //                    {
            //                        cdt.Add(_All.Date);
            //                        str.Add("Present");
            //                    }
            //                    else if(_All.Date == _h.Holiday_Date)
            //                    {
            //                        cdt.Add(_All.Date);
            //                        str.Add("Holiday");
            //                    }
            //                    else if (_All.Date == _sun.Date)
            //                    {
            //                        cdt.Add(_All.Date);
            //                        str.Add("Sunday");
            //                    }
            //                    else if (_l.From_Date == _l.To_Date)
            //                    {
            //                        if(_All.Date == _l.From_Date)
            //                        {
            //                            cdt.Add(_All.Date);
            //                            str.Add(_l.Leave_Type);
            //                        }
            //                    }
            //                    else if (_l.From_Date != _l.To_Date)
            //                    {
            //                        int days = (_l.To_Date - _l.From_Date).Days;
            //                        var dates = new List<DateTime>();
            //                        for (var dt = _l.From_Date; dt <= _l.To_Date; dt = dt.AddDays(1))
            //                        {
            //                            dates.Add(dt);
            //                        }
            //                        foreach (DateTime d in dates)
            //                        {
            //                            if (_All.Date == d.Date)
            //                            {
            //                                cdt.Add(_All.Date);
            //                                str.Add(_l.Leave_Type);
            //                            }
            //                        }
            //                    }
            //                    else
            //                    {
            //                        cdt.Add(_All.Date);
            //                        str.Add("LWPS");
            //                    }

            //                }
            //            }
            //        }
            //    }
            //}



            return View();

        }
        public List<DateTime> getAllDates(int Year, int Month)
        {
            var AllDates = new List<DateTime>();
            for (int i = 1; i <= DateTime.DaysInMonth(Year, Month); i++)
            {
                AllDates.Add(new DateTime(Year, Month, i));
            }
            return AllDates;
        }
        static List<DateTime> GetSundayDates(int Year, int Month)
        {
            List<DateTime> dates = new List<DateTime>();
            DayOfWeek day = DayOfWeek.Sunday;
            System.Globalization.CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            for (int i = 1; i <= currentCulture.Calendar.GetDaysInMonth(Year, Month); i++)
            {
                DateTime d = new DateTime(Year, Month, i);
                if (d.DayOfWeek == day)
                {
                    dates.Add(d);
                }
            }
            return dates;

        }
        private int GetSundays(int Year, int Month, DayOfWeek Weekday)
        {
            int ReturnValue = 0;
            DateTime MyDate = new DateTime(Year, Month, 1);
            int Start = 1;
            if (Weekday != MyDate.DayOfWeek)
            {
                Start = -(MyDate.DayOfWeek - Weekday - 1);
                if (Start <= 0)
                {
                    ReturnValue = -1;
                }
            }
            while (Start <= DateTime.DaysInMonth(Year, Month))
            {
                ReturnValue += 1;
                Start += 7;
            }


            return ReturnValue;
        }
        static int getdays(int year, int month)
        {
            int days = DateTime.DaysInMonth(year, month);

            return days;
        }
        public ActionResult Imp_Document_List()
        {
            ViewBag.PageTitle = "ALL Imporatant Document";

            List<UserMaster> user = db.UserMasters.Where(x => x.User_type != _LogedUser.User_type && x.Company_ID == _LogedUser.Company_ID && x.status == 0).ToList();
            ViewBag.user = user;
            List<Imp_Doc_Master> impdoc = db.Imp_Doc_Master.Where(x => x.Company_ID == _LogedUser.Company_ID && x.UserMaster.status == 0).ToList();
            ViewBag.impdoc = impdoc;
            return View();
        }
        [HttpPost]
        public ActionResult Imp_Document_Add(FormCollection form, HttpPostedFileBase file)
        {

            long Emp_Id = long.Parse(form["Emp_Id"].ToString());
            long Type = long.Parse(form["Type"].ToString());
            string Doc_Name = form["Doc_Name"].ToString();
            string Cmd = form["Command"].ToString();
            if (Cmd == "Save")
            {
                Imp_Doc_Master Doc = new Imp_Doc_Master();
                if (Emp_Id != 0)
                {
                    Doc.Emp_Id = Emp_Id;
                    Doc.Doc_Name = Doc_Name;
                    Doc.Type = Type;
                    if (file != null)
                    {
                        //var allowedExtensions = new[] { ".Jpg", ".JPG", ".png", ".jpg", ".jpeg", ".pdf" ,".XLS",".XLSX",".DOC",".PPT",".xls",".xlsx",".txt",".docx",".doc",".pptx",".ppt",".potx",".ppsx"};
                        //var fileName = Path.GetFileName(file.FileName);
                        //var ext = Path.GetExtension(file.FileName);
                        //if (!allowedExtensions.Contains(ext))
                        //{
                        //    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Sorry Invalid Files.!" });
                        //}
                        //string name = Path.GetFileNameWithoutExtension(fileName);
                        //string myfile = name + "_" + DateTime.Now.Second.ToString() + ext;
                        //var path = Path.Combine(Server.MapPath("~/Uploads/User_Document"), myfile);
                        //file.SaveAs(path);
                        //Doc.Doc_Path = path;
                        //Doc.File_Name = myfile;

                        string _Oldname = Doc.File_Name;
                        CommonResponce result = new ImageKiT_ImageProcess().UploadDocument("User_Document", file, _Oldname, 20);
                        if (result.EnableError == true)
                        {
                            Doc.Doc_Path = $"{ImageKit_PUBLICURL}/{ImageKit_Folder}/User_Document/" + result.ErrorMsg;
                            Doc.File_Name = result.ErrorMsg;
                        }
                        else
                        {
                            if (Request.IsAjaxRequest()) { return this.Json(new { EnableError = false, ErrorTitle = "Failure", ErrorMsg = result.ErrorMsg }); }
                            else { TempData["Result"] = result.ErrorMsg; return RedirectToAction("Dashboard"); }
                        }



                    }
                    Doc.Company_ID = _LogedUser.Company_ID;
                    db.Imp_Doc_Master.Add(Doc);
                    db.SaveChanges();
                    TempData["Result"] = "Document Added Successfully.";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Document Added Successfully !", Refresh = "Default" });
                }
                else
                {
                    List<UserMaster> _u = db.UserMasters.Where(x => x.User_type != 21).ToList();
                    foreach (UserMaster user in _u)
                    {
                        Doc.Emp_Id = user.User_ID;
                        Doc.Doc_Name = Doc_Name;
                        Doc.Company_ID = _LogedUser.Company_ID;
                        Doc.Type = Type;
                        if (file != null)
                        {
                            //    var allowedExtensions = new[] { ".Jpg", ".JPG", ".png", ".jpg", ".jpeg", ".pdf", ".XLS", ".XLSX", ".DOC", ".PPT", ".xls", ".xlsx", ".txt", ".docx", ".doc", ".pptx", ".ppt", ".potx", ".ppsx"  }
                            //;
                            //    var fileName = Path.GetFileName(file.FileName);
                            //    var ext = Path.GetExtension(file.FileName);
                            //    if (!allowedExtensions.Contains(ext))
                            //    {
                            //        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Sorry Invalid Files.!" });
                            //    }
                            //    string name = Path.GetFileNameWithoutExtension(fileName);
                            //    string myfile = name + "_" + DateTime.Now.Second.ToString() + ext;
                            //    var path = Path.Combine(Server.MapPath("~/Uploads/User_Document"), myfile);
                            //    file.SaveAs(path);
                            //    Doc.Doc_Path = path;
                            //    Doc.File_Name = myfile;

                            string _Oldname = Doc.File_Name;
                            CommonResponce result = new ImageKiT_ImageProcess().UploadDocument("User_Document", file, _Oldname, 20);
                            if (result.EnableError == true)
                            {
                                Doc.Doc_Path = $"{ImageKit_PUBLICURL}/{ImageKit_Folder}/User_Document/" + result.ErrorMsg;
                                Doc.File_Name = result.ErrorMsg;
                            }
                            else
                            {
                                if (Request.IsAjaxRequest()) { return this.Json(new { EnableError = false, ErrorTitle = "Failure", ErrorMsg = result.ErrorMsg }); }
                                else { TempData["Result"] = result.ErrorMsg; return RedirectToAction("Dashboard"); }
                            }


                        }
                        Doc.Company_ID = _LogedUser.Company_ID;
                        db.Imp_Doc_Master.Add(Doc);
                        db.SaveChanges();
                    }
                    TempData["Result"] = "Document Added Successfully.";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Document Added Successfully !", Refresh = "Default" });
                }
            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !" });
            }

        }
        public ActionResult Imp_Document_Delete(FormCollection form)
        {
            long Imp_Id = long.Parse(form["Imp_Id"].ToString());
            var _del = db.Imp_Doc_Master.Find(Imp_Id);
            db.Imp_Doc_Master.Remove(_del);
            db.SaveChanges();
            TempData["failure"] = "Important Document Deleted Successfully !";
            return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Important Document Deleted Successfully !", Refresh = "Default" });
        }

        public ActionResult Commercial_Documents(FormCollection form)
        {
            if (form["fltdatepicker"] != null)
            {
                DateTime? fltdatepicker = string.IsNullOrEmpty(form["fltdatepicker"])
                      ? (DateTime?)null
                      : DateTime.Parse(form["fltdatepicker"]);

                long? fltHeadId = string.IsNullOrEmpty(form["fltHeadId"]) || form["fltHeadId"] == "0"
                   ? (long?)null
                   : long.Parse(form["fltHeadId"]);

                long? fltHead2Id = string.IsNullOrEmpty(form["fltHead2Id"]) || form["fltHead2Id"] == "0"
                    ? (long?)null
                    : long.Parse(form["fltHead2Id"]);

                ViewBag.PageTitle = "ALL Commercial Document";
                if (fltHeadId != 0 && fltHead2Id != 0)
                {
                    List<GetCommericalDocuments_Result> impdoc = db.GetCommericalDocuments(_LogedUser.Company_ID, fltHeadId, fltHead2Id, fltdatepicker).ToList();
                    ViewBag.impdoc = impdoc;
                }
                ViewBag.fltHeadId = fltHeadId;
                ViewBag.fltHead2Id = fltHead2Id;
                ViewBag.fltdatepicker = fltdatepicker;
            }
            else
            {
                List<GetCommericalDocuments_Result> impdoc = db.GetCommericalDocuments(_LogedUser.Company_ID, null, null, null).ToList();
                ViewBag.impdoc = impdoc;
            }

            List<Company_Doc_Heads> heads = db.Company_Doc_Heads.Where(x => x.Company_ID == _LogedUser.Company_ID).ToList();
            ViewBag.Heads = heads;

            return View();
        }


        public JsonResult GetCompanyDocHeads2(int headId)
        {
            var head2 = db.Company_Doc_Heads2
                          .Where(x => x.Document_HeadId == headId)
                          .Select(x => new
                          {
                              x.Document_Head2Id,
                              x.HeadName
                          }).ToList();

            return Json(head2, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Commercial_Documents_Add(FormCollection form, HttpPostedFileBase file)
        {

            string Cmd = form["Command"] != null ? form["Command"].ToString() : string.Empty;

            if (Cmd == "Save")
            {
                long Document_Id = long.Parse(form["Document_Id"].ToString());
                if (Document_Id != 0)
                {
                    long HeadId = long.Parse(form["HeadId"].ToString());
                    long Head2Id = long.Parse(form["Head2Id"].ToString());
                    string DName = form["DName"].ToString();
                    DateTime fromDate = DateTime.Parse(form["datepicker"].ToString());

                    var Doc = db.Company_ImportantDocuments.Where(x => x.Document_Id == Document_Id).FirstOrDefault();
                    Doc.Document_HeadId = HeadId;
                    Doc.Document_Head2Id = Head2Id;
                    Doc.DName = DName;
                    Doc.DMonth = fromDate;
                    Doc.DAddedon = DateTime.Now;
                    Doc.DUploadby = _LogedUser.User_Name;

                    if (file != null)
                    {
                        //var allowedExtensions = new[] { ".Jpg", ".JPG", ".png", ".jpg", ".jpeg", ".pdf" ,
                        //    ".xls", ".XLS", ".xlsx", ".XLSX" ,".DOC",".PPT",".txt",".docx",".doc",".pptx",".ppt",".potx",".ppsx"// <-- add these
                        //};
                        //var fileName = Path.GetFileName(file.FileName);
                        //var ext = Path.GetExtension(file.FileName);
                        //if (!allowedExtensions.Contains(ext))
                        //{
                        //    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Sorry Invalid Files.!" });
                        //}
                        //string name = Path.GetFileNameWithoutExtension(fileName);
                        //string myfile = name + "_" + DateTime.Now.Second.ToString() + ext;
                        //var path = Path.Combine(Server.MapPath("~/Uploads/Company_Document"), myfile);
                        //file.SaveAs(path);
                        //Doc.DURL = myfile;

                        string _Oldname = Doc.DURL;
                        CommonResponce result = new ImageKiT_ImageProcess().UploadDocument("Company_Document", file, _Oldname, 20);
                        if (result.EnableError == true)
                        {
                           
                            Doc.DURL = result.ErrorMsg;
                        }
                        else
                        {
                            if (Request.IsAjaxRequest()) { return this.Json(new { EnableError = false, ErrorTitle = "Failure", ErrorMsg = result.ErrorMsg }); }
                            else { TempData["Result"] = result.ErrorMsg; return RedirectToAction("Dashboard"); }
                        }


                    }
                    db.SaveChanges();
                    TempData["Result"] = "Document Updated Successfully.";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Document Updated Successfully !", Refresh = "Default" });


                }
                else
                {
                    long HeadId = long.Parse(form["HeadId"].ToString());
                    long Head2Id = long.Parse(form["Head2Id"].ToString());
                    string DName = form["DName"].ToString();
                    DateTime fromDate = DateTime.Parse(form["datepicker"].ToString());
                    Company_ImportantDocuments Doc = new Company_ImportantDocuments();

                    Doc.Company_ID = _LogedUser.Company_ID;
                    Doc.Document_HeadId = HeadId;
                    Doc.Document_Head2Id = Head2Id;
                    Doc.DName = DName;
                    Doc.DMonth = fromDate;
                    if (file != null)
                    {
                        //var allowedExtensions = new[] { ".Jpg", ".JPG", ".png", ".jpg", ".jpeg", ".pdf" ,
                        //    ".xls", ".XLS", ".xlsx", ".XLSX" ,".DOC",".PPT",".txt",".docx",".doc",".pptx",".ppt",".potx",".ppsx"// <-- add these
                        //};
                        //var fileName = Path.GetFileName(file.FileName);
                        //var ext = Path.GetExtension(file.FileName);
                        //if (!allowedExtensions.Contains(ext))
                        //{
                        //    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Sorry Invalid Files.!" });
                        //}
                        //string name = Path.GetFileNameWithoutExtension(fileName);
                        //string myfile = name + "_" + DateTime.Now.Second.ToString() + ext;
                        //var path = Path.Combine(Server.MapPath("~/Uploads/Company_Document"), myfile);
                        //file.SaveAs(path);
                        //Doc.DURL = myfile;

                        string _Oldname = Doc.DURL;
                        CommonResponce result = new ImageKiT_ImageProcess().UploadDocument("Company_Document", file, _Oldname, 20);
                        if (result.EnableError == true)
                        {

                            Doc.DURL = result.ErrorMsg;
                        }
                        else
                        {
                            if (Request.IsAjaxRequest()) { return this.Json(new { EnableError = false, ErrorTitle = "Failure", ErrorMsg = result.ErrorMsg }); }
                            else { TempData["Result"] = result.ErrorMsg; return RedirectToAction("Dashboard"); }
                        }

                    }
                    Doc.DAddedon = DateTime.Now;
                    Doc.DUploadby = _LogedUser.User_Name;

                    db.Company_ImportantDocuments.Add(Doc);
                    db.SaveChanges();
                    TempData["Result"] = "Document Added Successfully.";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Document Added Successfully !", Refresh = "Default" });

                }

            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !" });
            }

        }

        public ActionResult Commercial_Document_Delete(FormCollection form)
        {
            long Document_Id = long.Parse(form["Document_Id"].ToString());
            var _del = db.Company_ImportantDocuments.Find(Document_Id);
            db.Company_ImportantDocuments.Remove(_del);
            db.SaveChanges();
            TempData["failure"] = "Document Deleted Successfully !";
            return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Document Deleted Successfully !", Refresh = "Default" });
        }
        public ActionResult GenerateDoc(FormCollection form)
        {
            ViewBag.PageTitle = "Generate Document";

            List<UserMaster> user = db.UserMasters.Where(x => x.User_type != _LogedUser.UserType.Usertype_id && x.Company_ID == _LogedUser.Company_ID && x.status == 0).ToList();
            ViewBag.user = user;
            List<Custom_Document> custom_s = db.Custom_Document.Where(x => x.Company_ID == _LogedUser.Company_ID).ToList();
            ViewBag.custom_s = custom_s;
            if (form["Emp_Id"] != null && form["Cust_Id"] != null)
            {
                long Emp_Id = long.Parse(form["Emp_Id"].ToString());
                long Cust_Id = long.Parse(form["Cust_Id"].ToString());
                return RedirectToAction("Letter", new { User_Id = Emp_Id, Cust_Id = Cust_Id });
            }
            return View();
        }
        public ActionResult GenerateDocbyemp(long docid, long empid)
        {

            UserMaster user = db.UserMasters.Where(x => x.User_ID == empid && x.Company_ID == _LogedUser.Company_ID && x.status == 0).FirstOrDefault();
            Custom_Document document = db.Custom_Document.Where(x => x.Cust_Id == docid && x.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
            ViewBag.user = user;
            ViewBag.document = document;
            return View();
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult GenerateDocbyemp(FormCollection form)
        {

            long Gen_doc_id = long.Parse(form["Gen_doc_id"].ToString());
            long Gen_doc_empid = long.Parse(form["Gen_doc_empid"].ToString());
            string doc_desc = form["doc_desc"].ToString();
            string Gen_doc_title = form["Gen_doc_title"].ToString();
            if (Gen_doc_id == 0)
            {
                Generated_Documents _newdoc = new Generated_Documents();
                long count11 = db.Generated_Documents.OrderByDescending(x => x.Gen_doc_id).Count();
                string Doc_No = "";
                Doc_No = DateTime.Now.ToString("yyyy") + "/" + DateTime.Now.ToString("MMM") + "/" + (count11 + 1).ToString();
                _newdoc.Gen_doc_empid = Gen_doc_empid;
                _newdoc.Gen_doc_text = doc_desc;
                _newdoc.Gen_doc_no = Doc_No;
                _newdoc.Gen_doc_Title = Gen_doc_title;
                db.Generated_Documents.Add(_newdoc);
                db.SaveChanges();
                return RedirectToAction("PrintDocument", new { Gen_doc_id = _newdoc.Gen_doc_id });
            }
            return RedirectToAction("GenerateDocs");
        }
        public ActionResult GenerateDocs()
        {

            List<Generated_Documents> gendocs = db.Generated_Documents.Where(x => x.UserMaster.Company_ID == _LogedUser.Company_ID && x.UserMaster.status == 0).ToList();
            ViewBag.gendocs = gendocs;
            return View();
        }


        public ActionResult PrintDocument(long Gen_doc_id)
        {
            ViewBag.PageTitle = "Print Documents";

            Generated_Documents gendocs = db.Generated_Documents.Where(x => x.Gen_doc_id == Gen_doc_id && x.UserMaster.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
            ViewBag.gendocs = gendocs;
            return View();
        }
        public ActionResult Employee_Task_weelky(string Taskdate = "", long user_id = 0)
        {

            List<UserMaster> um = db.UserMasters.Where(x => x.User_type != 1 && x.Company_ID == _LogedUser.Company_ID && x.status == 0).ToList();
            if (Taskdate != null & user_id != 0)
            {
                //.....Count Total Sundays

                string[] dates = Taskdate.Split('-');
                IFormatProvider culture = new CultureInfo("en-US", true);
                DateTime date1 = DateTime.ParseExact(dates[0].ToString().Trim(), "MM/dd/yyyy", culture);
                DateTime date2 = DateTime.ParseExact(dates[1].ToString().Trim(), "MM/dd/yyyy", culture);
                string td = date2.ToString();
                List<Employee_Task_Master> em = new List<Employee_Task_Master>();
                //long user_id = long.Parse(form["user_id"].ToString());
                string str = "";
                if (td == "")
                {

                }
                else if (user_id <= 0)
                {

                }
                else
                {
                    DateTime dt = Convert.ToDateTime(date2);
                    em = db.Employee_Task_Master.Where(x => x.Task_Date == dt && x.User_id == user_id && x.Status == 0).ToList();
                    str = "one";
                }
                ViewBag.em = em;
                ViewBag.taskdate = date2;
                ViewBag.str = str;
                ViewBag.userid = user_id;
            }
            ViewBag.User = um;
            return View();
        }
        public ActionResult Employee_Task_weelky_Partial(string start_to_end_date = "", long user_id = 0)
        {

            var datetrim = start_to_end_date.TrimStart().ToString();
            int index = datetrim.TrimStart().IndexOf('-', datetrim.IndexOf('-') + 1);
            var date1 = datetrim.Substring(0, index + 6).TrimEnd().TrimStart();
            var date2 = datetrim.Substring(index + 6 + 1).TrimEnd().TrimStart();

            List<UserMaster> um = db.UserMasters.Where(x => x.User_type != 1 && x.Company_ID == _LogedUser.Company_ID && x.status == 0).ToList();
            if (date1 != null & date2 != null & user_id != 0)
            {
                //.....Count Total Sundays

                //string[] dates = Taskdate.Split('-');
                //IFormatProvider culture = new CultureInfo("en-US", true);
                //DateTime date1 = DateTime.ParseExact(dates[0].ToString().Trim(), "MM/dd/yyyy", culture);
                //DateTime date2 = DateTime.ParseExact(dates[1].ToString().Trim(), "MM/dd/yyyy", culture);
                string td = date2.ToString();
                DateTime fromdate = Convert.ToDateTime(Convert.ToDateTime(date1));
                string fs = fromdate.ToString("yyyy-MM-dd");
                DateTime dfs = Convert.ToDateTime(fs);
                DateTime todate = Convert.ToDateTime(Convert.ToDateTime(date2));
                string ts = todate.ToString("yyyy-MM-dd");
                DateTime dts = Convert.ToDateTime(ts);
                List<Employee_Task_Master> em = new List<Employee_Task_Master>();
                //long user_id = long.Parse(form["user_id"].ToString());
                string str = "";
                //if (td == "")
                //{

                //}
                //else if (user_id <= 0)
                //{

                //}
                //else
                //{

                //    em = db.Employee_Task_Master.Where(x => x.Task_Date >= fromdate && x.Task_Date <= todate && x.User_id == user_id && x.Status == 0).ToList();
                //    str = "one";
                //}\
                if (user_id == -1)
                {
                    List<Employee_Task_Master> edata = db.Employee_Task_Master.Where(x => x.Status == 0 && x.UserMaster.Company_Master.Company_ID == _LogedUser.Company_ID).ToList();
                    foreach (Employee_Task_Master e in edata)
                    {
                        if (e.Task_Date >= dfs && e.Task_Date <= dts)
                        {
                            em.Add(e);
                        }
                    }
                    str = "all";
                }
                else
                {
                    em = db.Employee_Task_Master.Where(x => x.Task_Date >= fromdate && x.Task_Date <= todate && x.User_id == user_id && x.Status == 0).ToList();
                    str = "one";
                }
                ViewBag.em = em;
                ViewBag.user = um;
                ViewBag.taskdate = date2.ToString();
                ViewBag.date1 = fromdate;
                ViewBag.date2 = todate;
                ViewBag.str = str;
                ViewBag.userid = user_id;
            }
            ViewBag.User = um;
            return PartialView();
        }
        public ActionResult Employee_Task(string Taskdate = "", long user_id = 0)
        {

            List<Employee_Task_Master> em = new List<Employee_Task_Master>();

            List<UserMaster> um = db.UserMasters.Where(x => x.User_type != 1 && x.Company_ID == _LogedUser.Company_ID && x.status == 0).ToList();
            string str = "";
            if (Taskdate == "")
            {

            }
            else if (user_id == 0)
            {

            }
            else if (user_id == -1)
            {
                DateTime dt = Convert.ToDateTime(Taskdate);
                em = db.Employee_Task_Master.Where(x => x.Task_Date == dt && x.Status == 0 && x.UserMaster.Company_Master.Company_ID == _LogedUser.Company_ID).ToList();
                str = "all";
            }
            else
            {
                DateTime dt = Convert.ToDateTime(Taskdate);
                em = db.Employee_Task_Master.Where(x => x.Task_Date == dt && x.User_id == user_id && x.Status == 0).ToList();
                str = "one";
            }
            ViewBag.User = um;
            ViewBag.em = em;
            ViewBag.taskdate = Taskdate;
            ViewBag.str = str;
            ViewBag.userid = user_id;
            return View();
        }
        public ActionResult Get_Employee_Weekly_Task_Details(FormCollection form)
        {
            //DateTime Task_date_From = DateTime.Parse(form["Task_date_From"].ToString());
            DateTime Task_date_To = DateTime.Parse(form["end_Date"].ToString());
            long user_id = long.Parse(form["user_id"].ToString());
            string st = Task_date_To.ToString("yyyy-MM-dd");
            return RedirectToAction("Employee_Task", new { Taskdate = st, user_id = user_id });
        }
        public ActionResult Get_Employee_Task_Details(FormCollection form)
        {
            DateTime dt = DateTime.Parse(form["Task_date"].ToString());
            long user_id = long.Parse(form["user_id"].ToString());
            string st = dt.ToString("yyyy-MM-dd");
            return RedirectToAction("Employee_Task", new { Taskdate = st, user_id = user_id });
        }
        public ActionResult delete_task(string Taskdate, long emp_task_id)
        {
            DateTime dt = Convert.ToDateTime(Taskdate);
            string st = dt.ToString("yyyy-MM-dd");
            Employee_Task_Master em = db.Employee_Task_Master.Where(x => x.Employee_Task_id == emp_task_id && x.Status == 0).FirstOrDefault();
            if (em != null)
            {
                db.Employee_Task_Master.Remove(em);
                db.SaveChanges();
                TempData["failure"] = "Task Deleted Successfully..!";
            }

            return RedirectToAction("Employee_Task", new { Taskdate = st, user_id = em.User_id });
        }
        public ActionResult Get_map_data(long eid, DateTime dt)
        {
            List<User_Visits> uv = db.User_Visits.Where(x => x.User_Id == eid).ToList();
            List<string> latitude = new List<string>();
            List<string> longitude = new List<string>();
            List<string> title = new List<string>();
            List<string> description = new List<string>();
            List<string> pname = new List<string>();
            List<string> ipadd = new List<string>();
            List<string> visitdt = new List<string>();
            foreach (User_Visits u in uv)
            {
                string st = dt.ToString("dd-MM-yyyy");
                DateTime uvtime = u.Time;
                string st1 = uvtime.ToString("dd-MM-yyyy");
                if (st == st1)
                {
                    latitude.Add(u.latitude);
                    longitude.Add(u.longitude);
                    title.Add(u.LocationName);
                    description.Add(u.Location);
                    pname.Add(u.Punch_Type.Puch_Name);
                    ipadd.Add(u.IP_Address);
                    visitdt.Add(u.Time.ToString("dd/MM/yyyy hh:mm tt"));
                }

            }
            if (uv.Count > 0)
            {
                return this.Json(new { EnableError = true, ErrorTitle = "S", lati = latitude, longi = longitude, tit = title, desc = description, punchname = pname, ipadd = ipadd, visitdt = visitdt });

            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", });
            }
        }
        public ActionResult Event_list()
        {

            ViewBag.PageTitle = "All Events";
            List<Event_Master> eve = db.Event_Master.Where(x => x.Status != 6 && x.UserMaster.Company_Master.Company_ID == _LogedUser.Company_ID).OrderBy(x => x.Date).ToList();
            ViewBag.Event = eve;

            List<UserMaster> user = db.UserMasters.Where(x => x.Company_ID == _LogedUser.Company_ID & x.status == 0).ToList().OrderBy(x => x.DOB).ToList();
            ViewBag.user = user;

            var day = DateTime.Today;

            var month = day.AddMonths(5);

            List<UserMaster> _userlisttodisplay = new List<UserMaster>();
            for (int i = day.Year; i <= day.Year; i++)
            {

                foreach (UserMaster _user in user.Where(x => x.DOB.Month >= day.Month && x.DOB.Month < month.Month))
                {
                    _userlisttodisplay.Add(_user);
                }
            }

            ViewBag.em = _userlisttodisplay;

            return View();
        }
        [HttpPost]
        public ActionResult Event_Add(FormCollection form)
        {


            long Event_Id = long.Parse(form["Add_Event_ID"].ToString());
            string Title = form["Title"].ToString();
            DateTime Date = DateTime.Parse(form["Date"].ToString()); ;
            string Description = form["Description"].ToString();
            long User_id = long.Parse(form["User_id"].ToString());
            string Cmd = form["Command"].ToString();
            if (Cmd == "Save")
            {
                if (Event_Id == 0)
                {

                    Event_Master eve = new Event_Master();
                    eve.Title = Title;
                    eve.Date = Date;
                    eve.Description = Description;
                    eve.User_id = User_id;
                    eve.Company_ID = _LogedUser.Company_ID;
                    eve.added_by = _LogedUser.User_ID;
                    eve.added_on = DateTime.Now;
                    db.Event_Master.Add(eve);
                    db.SaveChanges();
                    TempData["Result"] = "New Event Added Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "New Event Added Successfully !", Refresh = "Default" });
                }
                else
                {
                    Event_Master evet = db.Event_Master.Where(x => x.Event_Id == Event_Id).FirstOrDefault();
                    if (evet == null)
                    {
                        TempData["failure"] = "Invalid Event  !";
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Event  !" });
                    }
                    evet.Title = Title;
                    evet.Date = Date;
                    evet.Description = Description;
                    evet.User_id = User_id;
                    evet.Company_ID = _LogedUser.Company_ID;
                    //evet.Status = Status;
                    db.SaveChanges();
                    TempData["Result"] = "Event Updated Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Event Updated Successfully !", Refresh = "Default" });
                }
            }
            else if (Cmd == "Delete")
            {
                Event_Master d = db.Event_Master.Where(x => x.Event_Id == Event_Id).FirstOrDefault();
                var _delt = db.Event_Master.Find(Event_Id);

                if (d == null)
                {
                    TempData["failure"] = "Invalid Event !";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Event !" });
                }
                db.Event_Master.Remove(_delt);
                db.SaveChanges();
                TempData["failure"] = "Event Deleted Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Event Deleted Successfully !", Refresh = "Default" });
            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !" });
            }

        }
        public ActionResult Event_Delete(FormCollection form)
        {
            long Event_Id = long.Parse(form["Event_Id"].ToString());
            Event_Master dd = db.Event_Master.Where(x => x.Event_Id == Event_Id).FirstOrDefault();
            var _delt = db.Event_Master.Find(Event_Id);
            if (dd == null)
            {
                TempData["failure"] = "Invalid Event !";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Event !" });
            }
            db.Event_Master.Remove(_delt);
            db.SaveChanges();
            TempData["failure"] = "Event Deleted Successfully !";
            return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Event Deleted Successfully !", Refresh = "Default" });
        }
        public ActionResult NoticeBoard_list()
        {

            ViewBag.PageTitle = "All NoticeBoardList";
            List<NoticeBoard_Master> n = db.NoticeBoard_Master.Where(x => x.Company_Master.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.Notice_Id).ToList();
            ViewBag.notice = n;
            return View();
        }
        [HttpPost]
        public ActionResult NoticeBoard_Add(FormCollection form)
        {

            long Notice_Id = long.Parse(form["Add_Notice_Id"].ToString());
            string Title = form["Title"].ToString();
            DateTime Date = DateTime.Parse(form["Date"].ToString()); ;
            string Description = form["Description"].ToString();
            string Cmd = form["Command"].ToString();
            if (Cmd == "Save")
            {
                if (Notice_Id == 0)
                {
                    NoticeBoard_Master notice = new NoticeBoard_Master();
                    notice.Title = Title;
                    notice.Date = Date;
                    notice.Description = Description;
                    notice.Company_ID = _LogedUser.Company_ID;
                    notice.added_by = _LogedUser.User_ID;
                    notice.added_on = DateTime.Now;
                    db.NoticeBoard_Master.Add(notice);
                    db.SaveChanges();
                    TempData["Result"] = "New Notice on Board Added Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "New Notice on Board Added Successfully !", Refresh = "Default" });
                }
                else
                {
                    NoticeBoard_Master no = db.NoticeBoard_Master.Where(x => x.Notice_Id == Notice_Id).FirstOrDefault();
                    if (no == null)
                    {
                        TempData["failure"] = "Invalid Notice  !";
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Notice  !" });
                    }
                    no.Title = Title;
                    no.Date = Date;
                    no.Description = Description;
                    no.Company_ID = _LogedUser.Company_ID;
                    db.SaveChanges();
                    TempData["Result"] = "Notice on Board Updated Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Notice on Board Updated Successfully !", Refresh = "Default" });
                }
            }
            else if (Cmd == "Delete")
            {
                NoticeBoard_Master d = db.NoticeBoard_Master.Where(x => x.Notice_Id == Notice_Id).FirstOrDefault();
                var _delt = db.NoticeBoard_Master.Find(Notice_Id);

                if (d == null)
                {
                    TempData["failure"] = "Invalid Notice !";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Notice !" });
                }
                db.NoticeBoard_Master.Remove(_delt);
                db.SaveChanges();
                TempData["failure"] = "Notice on Board Deleted Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Notice on Board Deleted Successfully !", Refresh = "Default" });
            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !" });
            }

        }
        public ActionResult NoticeBoard_Delete(FormCollection form)
        {
            long Notice_Id = long.Parse(form["Notice_Id"].ToString());
            NoticeBoard_Master dd = db.NoticeBoard_Master.Where(x => x.Notice_Id == Notice_Id).FirstOrDefault();
            var _delt = db.NoticeBoard_Master.Find(Notice_Id);
            if (dd == null)
            {
                TempData["failure"] = "Invalid Notice !";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Notice !" });
            }
            db.NoticeBoard_Master.Remove(_delt);
            db.SaveChanges();
            TempData["failure"] = "Notice on Board  Deleted Successfully !";
            return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Notice on Board  Deleted Successfully !", Refresh = "Default" });
        }
        public ActionResult InquiryListDetails()
        {
            ViewBag.PageTitle = "inquiry details";

            List<Inquiry_Master> inquirydetails = db.Inquiry_Master.Where(x => x.Company_ID == _LogedUser.Company_ID).ToList();
            ViewBag.InquiryDetails = inquirydetails;
            return View();
        }
        public JsonResult getEmployeeToAssighn()
        {

            List<UserMaster> employeInquiery = db.UserMasters.Where(x => x.Company_ID == _LogedUser.Company_ID && x.status == 0).ToList();
            return Json(from obj in employeInquiery.ToList() select new { User_Name = obj.User_Name, User_ID = obj.User_ID }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult Inquiry_Add(FormCollection form)
        {

            long inquiery_ID = long.Parse(form["Inquiry_Id"]);
            string inquiry_name = form["Inquiry_Name"];
            long mobile_no = long.Parse(form["Mobile_no"]);
            string email_id = form["Email_Id"];
            string services = form["Services"];
            //DateTime previous_meeting = DateTime.Parse(form["Previous_meeting"].ToString());
            // DateTime next_meeting = DateTime.Parse(form["Next_meeting"].ToString());
            string descrip = form["Description"].ToString();
            long assighnto = long.Parse(form["Emp_Id"]);
            int leadstatus = 0;

            string Cmd = form["Command"].ToString();
            if (Cmd == "Save")
            {
                if (inquiery_ID == 0)
                {
                    Inquiry_Master t = db.Inquiry_Master.Where(x => x.Inquiry_Name == inquiry_name && x.Mobile_no == mobile_no && x.Email_Id == email_id && x.Services == services && x.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
                    if (t != null)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "inquiry already Exist !" });
                    }
                    Inquiry_Master inquiry = new Inquiry_Master();
                    inquiry.Inquiry_Name = inquiry_name;
                    inquiry.Mobile_no = mobile_no;
                    inquiry.Email_Id = email_id;
                    inquiry.Services = services;
                    // inquiry.Previous_meeting = previous_meeting;
                    // inquiry.Next_meeting = next_meeting;
                    inquiry.Company_ID = _LogedUser.Company_ID;
                    inquiry.Description = descrip;
                    inquiry.Assign_To = assighnto;
                    inquiry.Lead_Status = leadstatus;
                    inquiry.AddedBy = _LogedUser.User_ID;
                    inquiry.Added_on = DateTime.Now;
                    db.Inquiry_Master.Add(inquiry);
                    db.SaveChanges();
                    var datainq = db.Inquiry_Master.OrderByDescending(x => x.Inquiry_Id).Select(x => x.Inquiry_Id).FirstOrDefault();
                    FollowUp_Master followdet = new FollowUp_Master();
                    followdet.Status = 0;
                    followdet.User_ID = assighnto;
                    followdet.Added_by = _LogedUser.User_ID;
                    followdet.Added_on = DateTime.Now;
                    followdet.Description = descrip;
                    followdet.Inquiry_ID = datainq;
                    db.FollowUp_Master.Add(followdet);
                    db.SaveChanges();
                    TempData["Result"] = "New Inquiry Added Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "New inquiry Added Successfully !", Refresh = "Default" });
                }
                else
                {
                    Inquiry_Master p = db.Inquiry_Master.Where(x => x.Inquiry_Id == inquiery_ID).FirstOrDefault();
                    if (p == null)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid inquiry  !" });
                    }

                    p.Inquiry_Name = inquiry_name;
                    p.Mobile_no = mobile_no;
                    p.Email_Id = email_id;
                    p.Services = services;
                    //  p.Previous_meeting = previous_meeting;
                    //   p.Next_meeting = next_meeting;
                    p.Company_ID = _LogedUser.Company_ID;
                    p.AddedBy = _LogedUser.User_ID;
                    p.Added_on = DateTime.Now;
                    p.Description = descrip;
                    p.Lead_Status = leadstatus;
                    p.Assign_To = assighnto;
                    db.SaveChanges();
                    FollowUp_Master folloupdatelast = db.FollowUp_Master.Where(x => x.Inquiry_ID == inquiery_ID).OrderByDescending(x => x.Inquiry_ID).FirstOrDefault();
                    folloupdatelast.User_ID = assighnto;
                    folloupdatelast.Added_on = DateTime.Now;
                    folloupdatelast.Added_by = _LogedUser.User_ID;
                    folloupdatelast.Status = 0;
                    db.SaveChanges();
                    TempData["Result"] = "inquiry Updated Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Inquiry Updated Successfully !", Refresh = "Default" });
                }
            }
            return View();
        }
        [HttpPost]
        public ActionResult Inquiry_delete(FormCollection form)
        {

            long inqID = long.Parse(form["inqID"].ToString());
            Inquiry_Master p = db.Inquiry_Master.Where(x => x.Inquiry_Id == inqID).FirstOrDefault();
            var _del = db.Project_Master.Find(inqID);
            if (p == null)
            {

                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid inquiry  !" });
            }
            if (p.FollowUp_Master.Count > 0)
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgSuc = "Project Has FollowUp details Delete first !", Refresh = "Default" });
            }
            db.Inquiry_Master.Remove(p);
            db.SaveChanges();
            TempData["failure"] = "Project Deleted Successfully !";
            return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Project Deleted Successfully !", Refresh = "Default" });

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ImportExcel(HttpPostedFileBase excelFile)
        {
            OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            if (excelFile != null && excelFile.ContentLength > 0)
            {
                var extension = Path.GetExtension(excelFile.FileName);
                if (extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase) || extension.Equals(".xls", StringComparison.OrdinalIgnoreCase))
                {
                    using (var package = new ExcelPackage(excelFile.InputStream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                        int rowCount = worksheet.Dimension.Rows;
                        int colCount = worksheet.Dimension.Columns;

                        // Safely get company name from merged row (row 1)
                        string companyName = "";
                        for (int col = 1; col <= colCount; col++)
                        {
                            companyName = worksheet.Cells[1, col].Text?.Trim();
                            if (!string.IsNullOrWhiteSpace(companyName))
                                break;
                        }

                        if (string.IsNullOrWhiteSpace(companyName))
                        {
                            return Json(new { success = false, message = "Company Name is required in the Excel header (row 1)." });
                        }

                        // Ensure the company exists in the DB
                        var company = db.Company_Master
                                        .FirstOrDefault(c => c.Company_Name.Trim().ToLower() == companyName.ToLower());

                        if (company == null)
                        {
                            return Json(new { success = false, message = $"Company '{companyName}' not found in the database." });
                        }

                        long companyId = company.Company_ID;
                        string companyEmail = company.Company_Email;

                        for (int row = 3; row <= rowCount; row++) // Start reading data from row 3
                        {
                            try
                            {
                                // === City ===
                                string cityName = worksheet.Cells[row, 9].Text?.Trim();
                                string religionName = worksheet.Cells[row, 12].Text?.Trim();
                                string deptName = worksheet.Cells[row, 16].Text?.Trim();
                                long? deptId = null;
                                if (!string.IsNullOrWhiteSpace(deptName))
                                {
                                    var dept = db.Department_Master
                                                 .FirstOrDefault(d => d.Department.Trim().ToLower() == deptName.ToLower()
                                                                   && d.Company_ID == companyId);

                                    if (dept == null)
                                        throw new Exception($"Department '{deptName}' not found for Company '{companyName}' at row {row}.");

                                    deptId = dept.Dept_ID;
                                }
                                else
                                {
                                    throw new Exception($"Department Name is required at row {row}.");
                                }

                                // === Designation ===
                                string desigName = worksheet.Cells[row, 17].Text?.Trim();
                                long? desigId = null;
                                if (!string.IsNullOrWhiteSpace(desigName))
                                {
                                    var desig = db.Designation_Master
                                                  .FirstOrDefault(d => d.Designation.Trim().ToLower() == desigName.ToLower() && d.Company_ID == companyId);

                                    if (desig == null)
                                        throw new Exception($"Designation '{desigName}' not found for Company ID {companyId} at row {row}.");

                                    desigId = desig.Desg_Id;
                                }
                                else
                                {
                                    throw new Exception($"Designation is required at row {row}.");
                                }

                                // === UserType ===
                                string userTypeName = worksheet.Cells[row, 7].Text?.Trim();
                                long? userTypeId = null;

                                if (!string.IsNullOrWhiteSpace(userTypeName))
                                {
                                    var userType = db.UserTypes.FirstOrDefault(u => u.Usertype_name.Trim().ToLower() == userTypeName.ToLower());
                                    if (userType == null)
                                        throw new Exception($"User Type '{userTypeName}' not found at row {row}.");

                                    userTypeId = userType.Usertype_id;
                                }
                                else
                                {
                                    throw new Exception($"User Type is required at row {row}.");
                                }

                                // === Status ===
                                int status = 0;
                                string statusText = worksheet.Cells[row, 8].Text?.Trim().ToLower();

                                if (!string.IsNullOrWhiteSpace(statusText))
                                {
                                    if (statusText == "deactive" || statusText == "1") status = 1;
                                    else if (statusText == "active" || statusText == "0") status = 0;
                                    else if (statusText == "delete" || statusText == "6") status = 6;
                                    else int.TryParse(statusText, out status); // fallback
                                }

                                // === Reporting Person ===
                                string reportingName = worksheet.Cells[row, 30].Text;
                                long? reportingPersonId = null;

                                if (!string.IsNullOrWhiteSpace(reportingName))
                                {
                                    var reportingUser = db.UserMasters
                                        .FirstOrDefault(u => u.User_Name == reportingName && u.Company_ID == companyId);
                                    if (reportingUser != null)
                                        reportingPersonId = reportingUser.User_ID;
                                }

                                // === Create and Add User ===
                                var user = new UserMaster
                                {
                                    Company_ID = companyId,
                                    User_UID = Guid.NewGuid().ToString("N"),
                                    User_Name = worksheet.Cells[row, 3].Text,
                                    User_Mobile = Convert.ToInt64(worksheet.Cells[row, 4].Text),
                                    User_Email = worksheet.Cells[row, 5].Text,
                                    User_Password = worksheet.Cells[row, 6].Text,
                                    User_type = userTypeId,
                                    status = status,
                                    City_Name = cityName,
                                    Adharcard_No = string.IsNullOrWhiteSpace(worksheet.Cells[row, 10].Text) ? null : (long?)Convert.ToInt64(worksheet.Cells[row, 13].Text),
                                    PanCard_No = worksheet.Cells[row, 11].Text,
                                    Religion_name = religionName,
                                    Total_PL = 0,
                                    Total_CL = 0,
                                    Total_EL = 0,
                                    Total_Leave = 0,
                                    DOB = DateTime.Parse(worksheet.Cells[row, 13].Text),
                                    Company_Email = companyEmail,
                                    Date_of_join = DateTime.Parse(worksheet.Cells[row, 14].Text),
                                    Blood_group = worksheet.Cells[row, 15].Text,
                                    Depart_id = deptId,
                                    Desig_id = desigId,
                                    Posted_At = worksheet.Cells[row, 18].Text,
                                    Team_name = worksheet.Cells[row, 19].Text,
                                    Bank_Acc_No = worksheet.Cells[row, 20].Text,
                                    IFSC_No = worksheet.Cells[row, 21].Text,
                                    Bank_Name = worksheet.Cells[row, 22].Text,
                                    EC_No = worksheet.Cells[row, 23].Text,
                                    UAN_No = worksheet.Cells[row, 24].Text,
                                    ESIC_No = worksheet.Cells[row, 25].Text,
                                    PF_No = worksheet.Cells[row, 26].Text,
                                    last_otp = string.IsNullOrWhiteSpace(worksheet.Cells[row, 27].Text) ? null : (long?)Convert.ToInt64(worksheet.Cells[row, 27].Text),
                                    last_otp_expire = string.IsNullOrWhiteSpace(worksheet.Cells[row, 28].Text) ? null : (DateTime?)Convert.ToDateTime(worksheet.Cells[row, 28].Text),
                                    Reporting_Person = reportingPersonId,
                                };

                                db.UserMasters.Add(user);
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Row {row} error: {ex.Message}");
                                continue;
                            }
                        }

                        db.SaveChanges();
                        TempData["Result"] = "Data Imported Successfully!";
                        return Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Data Imported Successfully.", Refresh = "Employee_All" });
                    }
                }

                return Json(new { success = false, message = "Invalid file format. Only Excel files are allowed." });
            }

            return Json(new { success = false, message = "Please upload a valid Excel file." });
        }
        [AllowAnonymous]
        public ActionResult Salarydetail(string salaryid)
        {
            string originalId = "";
            long finalsid = new long();
            if (salaryid.StartsWith("enc_"))
            {
                string cleanId = salaryid.Replace("enc_", "");
                originalId =_comm.Decrypt(cleanId);
                finalsid = long.Parse(originalId);
                // Proceed with originalId
            }
            else
            {
                //long sid = long.Parse(salaryid);
                finalsid = long.Parse(salaryid);
            }
            
            
            SalarySlipDetail_Result salary = db.SalarySlipDetail(finalsid).FirstOrDefault();
            ViewBag.salary = salary;
            return View();
        }

        public ActionResult DownloadSalarySlipAsPdf(long salaryid = 0)
        {
            SalarySlipDetail_Result salary = db.SalarySlipDetail(salaryid).FirstOrDefault();
            ViewBag.salary = salary;

            return new ViewAsPdf("DownloadSalarySlipAsPdf")
            {
                FileName = $"SalarySlip{salaryid}.pdf",
                PageSize = Rotativa.Options.Size.A4,
                PageOrientation = Rotativa.Options.Orientation.Portrait,
                PageMargins = new Rotativa.Options.Margins(05, 05, 05, 05)
            };
        }
        public ActionResult ErrorPage()
        {
            return View();
        }
        public ActionResult PrintSalarySlip(string Tokens, string User_UID, string Device_Address, int Month, int Year)
        {
            string baseUrl = $"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Content("~")}";
            string redirectUrl = baseUrl + "/Access/ErrorPage";
            long salaryid = new long();
           
            
            try
            {
                UserMaster u = db.UserMasters.Where(x => x.User_UID == User_UID).FirstOrDefault();
                if(u!=null)
                {
                    List<ProjectSalary> ps = db.ProjectSalaries.Where(x => x.User_ID == u.User_ID).ToList();
                    if (ps.Count != 0)
                    {
                        foreach (ProjectSalary p in ps)
                        {
                            int dm = p.SalaryMonth.Month;
                            int dy = p.SalaryMonth.Year;
                            if (dm == Month && dy == Year)
                            {

                                salaryid = p.ProjectSalaryID;
                                string chk = "enc_" + _comm.Encrypt(p.ProjectSalaryID.ToString());
                                //salaryid = long.Parse(Encrypt(p.ProjectSalaryID.ToString()));
                                redirectUrl = baseUrl + "/Access/Salarydetail?salaryid=" + chk;
                                return Redirect(redirectUrl);
                            }
                            else
                            {
                                return Redirect(redirectUrl);
                            }
                        }
                    }
                    else
                    {
                        return Redirect(redirectUrl);
                    }
                }
               
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                return Redirect(redirectUrl);

            }
            //http://localhost:56995/Access/PrintSalarySlip?Tokens=4e3f7b35957a410eaaff8ff54fe304a1&User_UID=d6421818aaa7499695bb80b4ce85c733&Device_Address=2d3982dfe8b54c09a116f938bf91c26f&Month=6&Year=2025
        }
    }
}