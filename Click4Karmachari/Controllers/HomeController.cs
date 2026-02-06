using ClickKarmachari.Models;
using DocumentFormat.OpenXml.Bibliography;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace ClickKarmachari.Controllers
{
    public class CaptchaResponse
    {
        [JsonProperty("Success")]
        public bool Success { get; set; }

        [JsonProperty("error-codes")]
        public bool ErrorMessage { get; set; }
    }
    public class HomeController : Controller
    {
        private Prod_Satyamgroup_V1Entities db = new Prod_Satyamgroup_V1Entities();
        public ActionResult Index()
        {
            
            return RedirectToAction("`  ");
        }
        public ActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Login(FormCollection form)
        {
            TempData["Failure"] = null;
            TempData["Result"] = null;
            string form_umobile = form["User_Mobile"].ToString();
            string form_password = form["User_Password"].ToString();
            long umobile = 0;
            if (form_umobile.ToString().Length != 10)
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Mobile Number !" });
            }
            else
            {
                try
                {
                    umobile = long.Parse(form_umobile.ToString());
                }
                catch (Exception e)
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Mobile Number !" });
                }
            }
            if (form_password.ToString().Length <= 0 || form_password.ToString().Length > 30)
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Password !" });
            }
            UserMaster _user = db.UserMasters.Where(x => x.User_Mobile.ToString() == form_umobile.ToString() && x.User_Password == form_password.ToString()).FirstOrDefault();
            if (_user != null)
            {
                User_Menu_Rights _menuright = db.User_Menu_Rights.Where(x => x.User_ID == _user.User_ID).FirstOrDefault();
                if (_menuright == null)
                {
                    User_Menu_Rights user_Menu_Rights = new User_Menu_Rights();
                    user_Menu_Rights.User_ID = _user.User_ID;
                    user_Menu_Rights.Employees = true;
                    user_Menu_Rights.Assets = true;
                    user_Menu_Rights.Task = true;
                    user_Menu_Rights.NoticeBoard = true;
                    user_Menu_Rights.Attendance = true;
                    user_Menu_Rights.Vouchers = true;
                    user_Menu_Rights.Salary_Generator = true;
                    user_Menu_Rights.Document_Generator = true;
                    user_Menu_Rights.Document_Storage = true;
                    db.User_Menu_Rights.Add(user_Menu_Rights);
                }
                List<Company_Menu_Rights> menuList = db.Company_Menu_Rights.Where(x => x.Company_ID == _user.Company_ID).ToList();// Store in Session (convert to JSON if necessary)
                if (menuList.Count <= 0)
                {
                    Company_Menu_Rights company_Menu_Rights = new Company_Menu_Rights();
                    company_Menu_Rights.Company_ID = _user.Company_ID;
                    company_Menu_Rights.Employees = true;
                    company_Menu_Rights.Assets = true;
                    company_Menu_Rights.Vouchers = true;
                    company_Menu_Rights.Attendance = true;
                    company_Menu_Rights.Task = true;
                    company_Menu_Rights.NoticeBoard = true;
                    company_Menu_Rights.Salary_Generator = true;
                    company_Menu_Rights.Document_Generator = true;
                    company_Menu_Rights.Document_Storage = true;
                    db.Company_Menu_Rights.Add(company_Menu_Rights);
                }
                db.SaveChanges();
                if (_user.status == 0 || _user.status == 2)
                {
                    setCookie(_user, Response);

                    //TempData["Result"] = "Welcome back ! Login Successfull";
                    if (_user.UserType.Usertype_id == 1)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Login Successfully", Refresh = "../Access/Dashboard" });
                    }
                    else
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Login Successfully", Refresh = "../Employee/Dashboard" });
                    }
                }
                else
                {
                    TempData["Failure"] = "Sorry,You Are Deactive/Deleted Right Now!";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Sorry,You Are Deactive/Deleted Right Now!" });
                }
            }
            else
            {
                TempData["Failure"] = "Username & Password incorrect !";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Username & Password incorrect !" });
            }
        }



        public ActionResult signout()
        {
            return RedirectToAction("Logout");

        }
        public ActionResult Forgot_password()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Forgot_Password(FormCollection form)
        {
            string Email = form["Email"].ToString();
            UserMaster _user = db.UserMasters.Where(x => x.User_Email.ToString() == Email.ToString()).FirstOrDefault();
            if (_user != null)
            {
                if (_user.status == 6)
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Sorry, Your Account has been not Available  " + new CommonClasses()._Status_ByNumber(Convert.ToInt16(_user.status)).ToString() + ".So,Please try again Later !", Refresh = "Login" });
                }
                else
                {
                    //string sub = "Forgot Password";
                    string message = "Please Keep this email id for further used of Communication. <br/> Regards,";
                    var subject = "Forgot Password Regarding " + _user.Company_Master.Company_Name + "- Satyam Group";
                    var body = "Dear," + _user.User_Name + "<br/> Thank You for registering. Given below are your login detail you will Need Following detail While Submitting  Activity.<br/> Your Login Details User No:" + _user.User_Mobile + "<br/>  Password: " + _user.User_Password + "<br/>Message: " + message + "<br/>Thank You !  <br/> This is System Generated Mail....! ";
                    MailAddress fromAddress = new MailAddress("noreply@mshere.com");
                    MailAddress toAddress = new MailAddress(Email);
                    try
                    {
                        Thread t1 = null;
                        t1 = new Thread(new ThreadStart(() => new CommonClasses().Email_Verify(fromAddress.ToString(), toAddress.ToString(), subject.ToString(), body.ToString())));
                        t1.Start();

                        TempData["Result"] = "Password has been successfully send !";
                        return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Password has been successfully send !", Refresh = "Login" });

                    }
                    catch (Exception)
                    {
                        TempData["Failure"] = "Sorry, we are facing some technical issue, Please try again or call us !";
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Sorry, we are facing some technical issue, Please try again or call us !", Refresh = "Login" });
                    }

                }
            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid User" });
            }

        }
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Registration(FormCollection form)
        {
            long Company_Mobile = long.Parse(form["Company_Mobile"].ToString());
            string Company_Name = form["Company_Name"].ToString();
            string Company_Pass = form["Company_Pass"].ToString();
            string Company_Email = form["Company_Email"].ToString();
            string Admin_Name = form["Admin_Name"].ToString();
            string Company_Address = form["Company_Address"].ToString();
            UserMaster u_mobile = db.UserMasters.Where(x => x.User_Mobile == Company_Mobile).FirstOrDefault();
            if (u_mobile != null) { return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Mobile Number Already Registered !" }); }
            u_mobile = db.UserMasters.Where(x => x.User_Email == Company_Email).FirstOrDefault();
            if (u_mobile != null) { return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Email Already Registered !" }); }

            Company_Master company = new Company_Master();
            company.Company_Name = Company_Name;
            company.Adminstrator_Name = Admin_Name;
            company.Company_Email = Company_Email;
            company.Company_Address = Company_Address;
            company.Status = 0;
            company.Contact_NO = Company_Mobile;
            db.Company_Master.Add(company);
            db.SaveChanges();
            long Company_ID = company.Company_ID;
            UserMaster _newuser = new UserMaster();
            _newuser.User_Mobile = Company_Mobile;
            _newuser.User_UID = Guid.NewGuid().ToString().Replace("-", "");
            _newuser.Company_ID = Company_ID;
            _newuser.User_Name = Admin_Name;
            _newuser.User_type = 1;
            _newuser.status = 0;
            //_newuser.DOB =_newuser.DOB ;
            UserMaster lastempid = db.UserMasters.Where(x=>x.Company_ID== company.Company_ID).OrderByDescending(x => x.User_ID).FirstOrDefault();
            if (lastempid != null)
            {
                string result = lastempid.EmployeeID.Replace("Emp_", "");
                long neweid = long.Parse(result.ToString()) + 1;
                _newuser.EmployeeID = "Emp_" + neweid.ToString();
            }
            else
            {

                _newuser.EmployeeID = "Emp_1";
            }
            _newuser.Date_of_join = DateTime.Now;
            _newuser.User_Password = Company_Pass;
            db.UserMasters.Add(_newuser);
            db.SaveChanges();

            ///Email Send.....................

            string sub = "Registerd Company";
            string message = "Please Login and Join Our Team Click4Karmachari Group";
            var subject = "New Registration Your Company in " + Company_Name + "- Click4Karmachari Group";
            var body = "Dear" + Admin_Name + ",<br/> Your Company Will Registers Pelase Login : <br/>User NO: " + Company_Mobile + "<br/>Password: " + Company_Pass + "<br/>  Subject: " + sub + "<br/>Message: " + message + "<br/>Thank You ! <br/> <br/> This is System Generated Mail....! ";
            MailAddress fromAddress = new MailAddress("noreply@mshere.com");
            MailAddress toAddress = new MailAddress(Company_Email);
            Thread t1 = null;
            t1 = new Thread(new ThreadStart(() => new CommonClasses().Email_Verify(fromAddress.ToString(), toAddress.ToString(), subject.ToString(), body.ToString())));
            t1.Start();

            TempData["Result"] = "Company Registered Successfully Please Login & Update Company Profile!";

            return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgFail = "Company Registered Successfully Please Login & Update Company Profile!", Refresh = "Login" });

        }
        public string GetCookie11(String CoockieName)
        {
            string CoockieValue = "";
            try
            {
                HttpCookie cookie = Request.Cookies[CoockieName];
                if (cookie != null)
                {
                    CoockieValue = cookie.Value;
                }
            }
            catch { }
            return CoockieValue;
        }
        public void setCookie(UserMaster admin, HttpResponseBase response)
        {
            string deviceAdd = "";
            try
            {
                deviceAdd = GetCookie11("LOGGED_EMPLOYEE_DEVICEADDRESS");
                if (deviceAdd.ToString().Length <= 5)
                {
                    deviceAdd = Guid.NewGuid().ToString().Replace("-", "");
                }
            }
            catch
            {
                deviceAdd = Guid.NewGuid().ToString().Replace("-", "");
            }
            try
            {
                if (response != null && response.Cookies != null)
                {
                    Master_Token _token = db.Master_Token.Where(x => x.User_ID == admin.User_ID && x.Device_Address == deviceAdd).FirstOrDefault();
                    if (_token == null)
                    {
                        List<Master_Token> _tokens = db.Master_Token.Where(x => x.Device_Address == deviceAdd).ToList();
                        if (_tokens.Count > 0)
                        {
                            db.Master_Token.RemoveRange(_tokens);
                        }
                        _token = new Master_Token();
                        _token.User_ID = admin.User_ID;
                        _token.Token = Guid.NewGuid().ToString().Replace("-", "");
                        _token.CreatedOn = DateTime.Now;
                        _token.Expire = DateTime.Now.AddYears(1);
                        _token.Device_Address = deviceAdd;
                        db.Master_Token.Add(_token);
                    }
                    else
                    {
                        _token.Expire = DateTime.Now.AddYears(1);
                    }
                    db.SaveChanges();

                    HttpCookie cookie1 = new HttpCookie("LOGGED_EMPLOYEE_TOKEN");
                    cookie1.Value = _token.Token;
                    cookie1.Expires = DateTime.Now.AddYears(1);
                    response.Cookies.Add(cookie1);

                    HttpCookie cookie2 = new HttpCookie("LOGGED_EMPLOYEE_DEVICEADDRESS");
                    cookie2.Value = _token.Device_Address;
                    cookie2.Expires = DateTime.Now.AddYears(1);
                    response.Cookies.Add(cookie2);

                    HttpCookie cookie3 = new HttpCookie("LOGGED_EMPLOYEE_UID");
                    cookie3.Value = admin.User_UID;
                    cookie3.Expires = DateTime.Now.AddYears(1);
                    response.Cookies.Add(cookie3);

                    HttpCookie cookie4 = new HttpCookie("LOGGED_EMPLOYEE_NAME");
                    cookie4.Value = admin.User_Name;
                    cookie4.Expires = DateTime.Now.AddYears(1);
                    response.Cookies.Add(cookie4);
                }

            }
            catch (Exception)
            {
                throw;
            }
        }
        public ActionResult LogOut()
        {
            string[] cookieNames = {
                "LOGGED_EMPLOYEE_TOKEN",
                "LOGGED_EMPLOYEE_DEVICEADDRESS",
                "LOGGED_EMPLOYEE_UID",
                "LOGGED_EMPLOYEE_NAME"
            };

            foreach (var cookieName in cookieNames)
            {
                if (Request.Cookies[cookieName] != null)
                {
                    var cookie = new HttpCookie(cookieName)
                    {
                        Expires = DateTime.Now.AddDays(-1)
                    };
                    Response.Cookies.Add(cookie);
                }
            }
            if (System.Web.Security.FormsAuthentication.IsEnabled)
            {
                System.Web.Security.FormsAuthentication.SignOut();
            }

            return Redirect("Login");
        }


        public ActionResult PrintSalarySlip(string Tokens, string User_UID, string Device_Address, int Month, int Year)
        {
            try
            {
                Master_Token LoggedEmployeeToken = db.Master_Token.Where(x => x.Token == Tokens && x.UserMaster.User_UID == User_UID && x.Device_Address == Device_Address).FirstOrDefault();
                if (LoggedEmployeeToken == null)
                {
                    return RedirectToAction("Signout", "Home");
                }
                UserMaster _LogedUser = LoggedEmployeeToken.UserMaster;
                ProjectSalary ps = db.ProjectSalaries.FirstOrDefault(x =>
                                     x.User_ID == _LogedUser.User_ID &&
                                     x.SalaryMonth.Month == Month &&
                                     x.SalaryMonth.Year == Year);

                if(ps == null)
                {
                    return RedirectToAction("Signout", "Home");
                }

                SalarySlipDetail_Result salary = db.SalarySlipDetail(ps.ProjectSalaryID).FirstOrDefault();
                ViewBag.salary = salary;
                return View();

            }
            catch (Exception ex)
            {
                return RedirectToAction("Signout", "Home");
            }
        }
        public ActionResult PrintDocument(string Tokens, string User_UID, string Device_Address,long Gen_doc_id)
        {
            ViewBag.PageTitle = "Print Documents";
            try
            {
                Master_Token LoggedEmployeeToken = db.Master_Token.Where(x => x.Token == Tokens && x.UserMaster.User_UID == User_UID && x.Device_Address == Device_Address).FirstOrDefault();
                if (LoggedEmployeeToken == null)
                {
                    return RedirectToAction("Signout", "Home");
                }
                UserMaster _LogedUser = LoggedEmployeeToken.UserMaster;
                Generated_Documents pd = db.Generated_Documents.FirstOrDefault(x =>
                                     x.UserMaster.Company_ID == _LogedUser.Company_ID &&
                                     x.UserMaster.User_ID==_LogedUser.User_ID &&
                                     x.Gen_doc_id == Gen_doc_id );

                if (pd == null)
                {
                    return RedirectToAction("Signout", "Home");
                }

                ViewBag.gendocs = pd;
                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction("Signout", "Home");
            }
        }

    }

}