using ClickKarmachari.Models;
using ClosedXML.Excel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Device.Location;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Contexts;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace ClickKarmachari.Controllers
{

    [CookieAdminAuthorizeAttribute]
    public class EmployeeController : Controller
    {
        private Prod_Satyamgroup_V1Entities db = new Prod_Satyamgroup_V1Entities();
        public string IPAddress { get; private set; }
        CommonClasses _comm = new CommonClasses();
        static String base_Url = ConfigurationManager.AppSettings["base_Url"].ToString();
        UserMaster _LogedUser = null;
        protected override void OnActionExecuting(ActionExecutingContext ctx)
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
                if (_LogedUser != null)
                {
                    List<UserMaster> alluser = db.UserMasters.Where(x => x.status == 0 && x.User_type != 1 && x.Reporting_Person == _LogedUser.User_ID && x.Company_ID == _LogedUser.Company_ID).ToList();
                    ViewBag.alluser = alluser;
                }
                ViewBag._LogedUser = _LogedUser;
                User_Menu_Rights user_Menu_Rights = db.User_Menu_Rights.Where(x => x.User_ID == _LogedUser.User_ID).FirstOrDefault();
                ViewBag.UserMenuRights = user_Menu_Rights;
                Company_Menu_Rights MenuRights = db.Company_Menu_Rights.Where(x => x.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
                ViewBag.MenuRights = MenuRights;

            }
            catch
            {
                ctx.Result = new RedirectToRouteResult(new System.Web.Routing.RouteValueDictionary(new { controller = "Home", action = "Signout" }));
                return;
            }
        }


        public EmployeeController()
        {
            ViewBag.base_Url = base_Url;
        }
        public string GetIPAddress()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName()); // `Dns.Resolve()` method is deprecated.
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            return ipAddress.ToString();
        }
        private string GetLocalIPv4(NetworkInterfaceType type = NetworkInterfaceType.Ethernet)
        {
            return NetworkInterface
                .GetAllNetworkInterfaces()
                .FirstOrDefault(ni =>
                    ni.NetworkInterfaceType == type
                    && ni.OperationalStatus == OperationalStatus.Up
                    && ni.GetIPProperties().GatewayAddresses.FirstOrDefault() != null
                    && ni.GetIPProperties().UnicastAddresses.FirstOrDefault(ip => ip.Address.AddressFamily == AddressFamily.InterNetwork) != null
                )
                ?.GetIPProperties()
                .UnicastAddresses
                .FirstOrDefault(ip => ip.Address.AddressFamily == AddressFamily.InterNetwork)
                ?.Address
                ?.ToString()
                ?? string.Empty;
        }

        public ActionResult Dashboard()
        {

            User_Punch latestPunch = db.User_Punch.Where(x => x.User_Id == _LogedUser.User_ID).OrderByDescending(p => p.Punch_Id).FirstOrDefault();
            if (latestPunch != null)
            {
                ViewBag.punch_detail = latestPunch;
            }
            else
            {
                ViewBag.p = "New_Punch";
            }

            List<Leave_Master> leave = db.Leave_Master.Where(x => x.User_Id == _LogedUser.User_ID & x.Status == 0).ToList();
            List<Voucher_Master> voucher = db.Voucher_Master.Where(x => x.Status == 0 & x.User_Id == _LogedUser.User_ID).ToList();
            List<Project_Task_Master> project_Task = db.Project_Task_Master.Where(x => x.Emp_Id == _LogedUser.User_ID).ToList();
            List<Asset_Movement> movment = db.Asset_Movement.Where(x => x.From_id == _LogedUser.User_ID || x.To_id == _LogedUser.User_ID).ToList();
            List<Asset_Master> assts = db.Asset_Master.Where(x => x.Company_ID == _LogedUser.Company_ID).ToList();
            List<AssetListIO> _myAssets = new List<AssetListIO>();
            foreach (Asset_Master _my in assts)
            {
                movment = db.Asset_Movement.Where(y => y.Asset_id == _my.Asset_id).OrderByDescending(y => y.Movement_id).ToList();
                if (movment.Count != 0)
                {
                    if (movment.FirstOrDefault().To_id == _LogedUser.User_ID)
                    {
                        _myAssets.Add(new AssetListIO(_my));
                    }
                }
            }
            ViewBag.Asset = _myAssets.Count;
            ViewBag.task_N = project_Task.Where(x => x.Status == 0).ToList().Count;
            ViewBag.task_R = project_Task.Where(x => x.Status == 7).ToList().Count;
            ViewBag.task_S = project_Task.Where(x => x.Status == 9).ToList().Count;
            ViewBag.project_Task = project_Task.Count;
            ViewBag.voucher = voucher.Count;
            ViewBag.leave = leave.Count;
            List<Event_Master> Allevent = db.Event_Master.Where(x => x.Company_ID == _LogedUser.Company_ID && (x.Date > DateTime.Now || x.Date == DateTime.Now)).OrderBy(x => x.Date).ToList();
            ViewBag.Allevent = Allevent;

            List<UserMaster> user = db.UserMasters.Where(x => x.Company_ID == _LogedUser.Company_ID & x.status == 0).ToList().OrderBy(x => x.DOB).ToList();
            ViewBag.user = user;

            var day = DateTime.Today;

            var monthh = day.AddMonths(5);

            List<UserMaster> _userlisttodisplay = new List<UserMaster>();
            for (int i = day.Year; i <= day.Year; i++)
            {

                foreach (UserMaster _user in user.Where(x => x.DOB.Month >= day.Month && x.DOB.Month < monthh.Month))
                {
                    _userlisttodisplay.Add(_user);
                }
            }

            ViewBag.em = _userlisttodisplay;

            List<NoticeBoard_Master> Allnotice = db.NoticeBoard_Master.Where(x => x.Company_ID == _LogedUser.Company_ID && (x.Date > DateTime.Now || x.Date == DateTime.Now)).OrderBy(x => x.Date).ToList();
            ViewBag.Allnotice = Allnotice;

            return View();
        }

        //public static string GetServerIP()
        //{
        //    IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());

        //    foreach (IPAddress address in ipHostInfo.AddressList)
        //    {
        //        if (address.AddressFamily == AddressFamily.InterNetwork)
        //            return address.ToString();
        //    }

        //    return string.Empty;
        //}
        [HttpPost]
        public string TakingpublicIP(string id)
        {
            var publicipAddres = id;
            return publicipAddres;
        }

        [HttpPost]
        public ActionResult Dashboard(FormCollection form)
        {
            string device;
            string u = Request.ServerVariables["HTTP_USER_AGENT"];
            System.Text.RegularExpressions.Regex b = new System.Text.RegularExpressions.Regex(@"(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            Regex v = new System.Text.RegularExpressions.Regex(@"1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if ((b.IsMatch(u) || v.IsMatch(u.Substring(0, 4))))
            {
                device = "mobile";
            }
            else
            {
                device = "pc";
            }
            //GetServerIP();

            string takepublicip = form["publicip"].ToString();
            var serverip = GetIPAddress();
            ViewBag.ipadd = serverip;
            Console.WriteLine(serverip);
            Debug.Write(serverip);
            System.Diagnostics.Debug.WriteLine("Test");
            var takeip = GetLocalIPv4();

            User_Punch punch = new User_Punch();
            string longitude = form["log"].ToString();
            string latitude = form["lat"].ToString();
            string loc = form["loc"].ToString();
            string uid = form["uid"].ToString();
            string ipAdd = "";
            ipAdd = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (string.IsNullOrEmpty(ipAdd))
            {
                ipAdd = Request.ServerVariables["REMOTE_ADDR"];
            }

            string IPAddress = Request.ServerVariables["REMOTE_ADDR"];
            string IPadd2 = Request.UserHostAddress;
            long PunchType_Id = 0;
            if (form["PunchType_Id"].ToString() == null)
            {
            }
            else
            {
                PunchType_Id = long.Parse(form["PunchType_Id"].ToString());
            }
            //string location = form["location"].ToString();
            punch.longitude = longitude;
            punch.latitude = latitude;
            punch.Punch_Via = 1;
            punch.unique_id = uid;
            punch.Location = loc;
            //punch.Location = location;
            User_Punch lastpunch = db.User_Punch.Where(x => x.User_Id == _LogedUser.User_ID
                                && x.Time.Day == DateTime.Today.Day && x.Time.Month == DateTime.Today.Month
                                && x.Time.Year == DateTime.Today.Year).OrderByDescending(x => x.Punch_Id).FirstOrDefault();
            CommonClasses _newobj = new CommonClasses();
            ///ADD IP ADDRESS 
            string ip = _newobj.GetIPAddress(IPAddress);
            if (device == "mobile")
            {
                punch.IP_Address = takeip;
            }
            else
            {
                punch.IP_Address = takepublicip;
            }
            //punch.IP_Address = ip;
            punch.IP_Address = takepublicip;
            if (ipAdd != ip)
            {
                ipAdd = ip;
            }
            if (PunchType_Id == 1 && (lastpunch == null || lastpunch.PunchType_Id == 2))
            {
                User_Punch up = db.User_Punch.Where(x => x.PunchType_Id == 1 && x.Time == DateTime.Now && x.User_Id == _LogedUser.User_ID).FirstOrDefault();
                if (up == null)
                {
                    _newobj._attendancefordate(DateTime.Now.AddDays(-1), _LogedUser.User_ID);
                    _newobj._attendancefordate(DateTime.Now.AddDays(-2), _LogedUser.User_ID);
                    punch.PunchType_Id = 1;
                    punch.User_Id = _LogedUser.User_ID;
                    punch.Time = DateTime.Now;
                    if (device == "mobile")
                    {
                        punch.IP_Address = takeip;
                    }
                    else
                    {
                        punch.IP_Address = takepublicip;
                    }
                    db.User_Punch.Add(punch);
                    db.SaveChanges();
                    TempData["Result"] = "Check IN Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Check IN Successfully !", Refresh = "Default" });
                }
                else
                {
                    TempData["failure"] = "Sorry, You are Already Check IN";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Sorry, You are Already Check IN" });
                }
            }
            else if (PunchType_Id == 2 && (lastpunch.PunchType_Id == 1 || lastpunch.PunchType_Id == 4))
            {
                User_Punch up = db.User_Punch.Where(x => x.PunchType_Id == 2 && x.Time == DateTime.Now && x.User_Id == _LogedUser.User_ID).FirstOrDefault();
                if (up == null)
                {
                    punch.PunchType_Id = 2;
                    punch.User_Id = _LogedUser.User_ID;
                    punch.Time = DateTime.Now;
                    if (device == "mobile")
                    {
                        punch.IP_Address = takeip;
                    }
                    else
                    {
                        punch.IP_Address = takepublicip;
                    }
                    //  punch.IP_Address = takepublicip;
                    db.User_Punch.Add(punch);
                    db.SaveChanges();
                    _newobj._attendancefordate(DateTime.Now, _LogedUser.User_ID);
                    TempData["Result"] = "Check Out Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Check Out Successfully !", Refresh = "Default" });
                }
                else
                {
                    TempData["failure"] = "Sorry, You are Already Check Out";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Sorry, You are Already Check Out" });
                }
            }
            else if (PunchType_Id == 3 && (lastpunch.PunchType_Id == 1))
            {
                User_Punch up = db.User_Punch.Where(x => x.PunchType_Id == 3 && x.Time == DateTime.Now && x.User_Id == _LogedUser.User_ID).FirstOrDefault();
                if (up == null)
                {
                    punch.PunchType_Id = 3;
                    punch.User_Id = _LogedUser.User_ID;
                    punch.Time = DateTime.Now;
                    if (device == "mobile")
                    {
                        punch.IP_Address = takeip;
                    }
                    else
                    {
                        punch.IP_Address = takepublicip;
                    }
                    //  punch.IP_Address = takepublicip;
                    db.User_Punch.Add(punch);
                    db.SaveChanges();
                    TempData["Result"] = "Lunch In Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Lunch In Successfully !", Refresh = "Default" });
                }
                else
                {
                    TempData["failure"] = "Sorry, You are Already Lunch In";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Sorry, You are Already Lunch In" });
                }
            }
            else if (PunchType_Id == 4 && (lastpunch.PunchType_Id == 3))
            {
                User_Punch up = db.User_Punch.Where(x => x.PunchType_Id == 4 && x.Time == DateTime.Now && x.User_Id == _LogedUser.User_ID).FirstOrDefault();
                if (up == null)
                {
                    punch.PunchType_Id = 4;
                    punch.User_Id = _LogedUser.User_ID;
                    punch.Time = DateTime.Now;
                    if (device == "mobile")
                    {
                        punch.IP_Address = takeip;
                    }
                    else
                    {
                        punch.IP_Address = takepublicip;
                    }
                    // punch.IP_Address = takepublicip;
                    db.User_Punch.Add(punch);
                    db.SaveChanges();
                    TempData["Result"] = "Lunch Out Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Lunch Out Successfully !", Refresh = "Default" });
                }
                else
                {
                    TempData["failure"] = "Sorry, You are Already Lunch Out";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Sorry, You are Already Lunch Out" });
                }

            }
            else
            {
                TempData["failure"] = "Sorry, You are currently ";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Sorry, You are currently " + _newobj._PunchName_ByNumber(lastpunch.PunchType_Id).ToString() + ". So, You can't " + _newobj._PunchName_ByNumber(PunchType_Id).ToString() + ". !" });
            }
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

            bool profileLock = form["Profile_Lock"] == "true" || form["Profile_Lock"] == "on";
            if (profileLock == false)
            {
                var _newuser = db.UserMasters.Find(_LogedUser.User_ID);
                string User_Name = form["User_Name"].ToString();
                long User_Mobile = long.Parse(form["User_Mobile"].ToString());
                long Religion_Id = long.Parse(form["Religion_Id"].ToString());
                string PanCard_No = form["PanCard_No"].ToString();
                string User_Email = form["User_Email"].ToString();
                string Religion_name = form["Religion_name"].ToString();
                string City_Name = form["City_Name"].ToString();
                long User_City = long.Parse(form["City"].ToString());
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
                    var allowedExtensions = new[] { ".Jpg", ".JPG", ".png", ".jpg", ".jpeg" };
                    var fileName = Path.GetFileName(file.FileName);
                    var ext = Path.GetExtension(file.FileName);
                    if (!allowedExtensions.Contains(ext))
                    {
                        TempData["failure"] = "Sorry Invalid Files.";
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Sorry Invalid Files." });
                    }
                    string name = Path.GetFileNameWithoutExtension(fileName);
                   
                    string myfile = name + "_" + DateTime.Now.Second.ToString() + ext;
                    var path = Path.Combine(Server.MapPath("~/Uploads/User_Image"), myfile);
                    file.SaveAs(path);
                    _newuser.image_path = path;
                    _newuser.image_name = myfile;
                }
                _newuser.PanCard_No = PanCard_No;
                _newuser.User_Name = User_Name;
                _newuser.User_Mobile = User_Mobile;
                _newuser.User_Email = User_Email;
                _newuser.City_Name = City_Name;
                _newuser.Religion_name = Religion_name;


                db.SaveChanges();
                Session.Remove("LOGIN_USER");
                Session.Add("LOG_STATUS", "TRUE");
                Session.Add("LOGIN_USER", _newuser);
                Session.Add("LOGIN_NAME", _newuser.User_Name.ToString());
                TempData["Result"] = "Profile Update Successfully.";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Profile Update Successfully.", Refresh = "Dashboard" });

            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Your Profile is lock !" });

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
            string Old_Password = form["Old_Password"].ToString();
            string New_Password = form["New_Password"].ToString();
            string Confirm_password = form["Confirm_password"].ToString();
            UserMaster _olduser = db.UserMasters.Where(x => x.User_Password == Old_Password && x.User_ID == _LogedUser.User_ID && x.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
            if (_olduser == null)
            {
                TempData["failure"] = "Old Password Not Match";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Old Password Not Match" });
            }
            if (New_Password != Confirm_password)
            {
                TempData["failure"] = "New Password Not Match Confirm Password";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "New Password Not Match Confirm Password !" });
            }
            _newuser.User_Password = Confirm_password;
            Session.Remove("LOGIN_USER");
            Session.Add("LOG_STATUS", "TRUE");
            Session.Add("LOGIN_USER", _LogedUser);
            Session.Add("LOGIN_NAME", _LogedUser.User_Name.ToString());
            db.SaveChanges();
            TempData["Result"] = "Password Changed Successfully.";
            return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Password Changed Successfully.", Refresh = "Dashboard" });

        }

        public ActionResult Employee_Detail()
        {
            ViewBag.PageTitle = "Employee Detail";

            UserMaster user = db.UserMasters.Where(x => x.User_ID == _LogedUser.User_ID).FirstOrDefault();

            List<UserMaster> Reporting_Person = db.UserMasters.Where(x => x.Company_ID == _LogedUser.Company_ID && x.status == 0).ToList();
            List<UserType> U_Type = db.UserTypes.ToList();
            List<Designation_Master> desg = db.Designation_Master.Where(x => x.Company_ID == _LogedUser.Company_ID).ToList();
            List<Department_Master> dept = db.Department_Master.Where(x => x.Company_ID == _LogedUser.Company_ID).ToList();
            List<City_Master> City = db.City_Master.OrderByDescending(x => x.Cityid).ToList();
            List<Religion_Master> religion = db.Religion_Master.OrderByDescending(x => x.Religion_Id).ToList();
            ViewBag.U_Type = U_Type;
            ViewBag.City = City;
            ViewBag.user = user;
            ViewBag.Desg = desg;
            ViewBag.Dept = dept;
            ViewBag.religion = religion;
            ViewBag.user_detail = Reporting_Person;
            ViewBag.Profile_Lock = user.Profile_Lock;
            //my checkin checkout Workdays count logic 
            int punchmonthdate = DateTime.Now.Month;
            int punchyear = DateTime.Now.Year;
            List<User_Punch> upunch = db.User_Punch.Where(x => x.User_Id == _LogedUser.User_ID & x.PunchType_Id == 1 & x.PunchType_Id == 1 & x.Time.Month.Equals(punchmonthdate) & x.Time.Year == punchyear).ToList();

            ViewBag.AttendedDays = upunch.Count();
            //logic end
            DateTime date = DateTime.Now;
            int Year = date.Year;
            int month = date.Month;
            int dayscount = date.Day;
            ViewBag.Year = Year;
            int Month = date.Month;
            ViewBag.Month = Month;

            //my count total attended days by subtracting leave days and sunday from current date from starting month logic 
            List<Leave_Master> loguserleave = db.Leave_Master.Where(x => x.Status != 6 & x.Leave_Crdr == 2 & x.UserMaster.User_ID == _LogedUser.User_ID & x.To_Date.Year == Year & x.From_Date.Month == month).OrderBy(x => x.Status).ThenByDescending(x => x.From_Date).ToList();
            //var fromdat = "";


            // List<Leave_Master> uattdata = loguserleave.Where( x=>x.UserMaster.User_ID == _LogedUser.User_ID & x.From_Date.Month == month).ToList();
            int count = 0;
            List<string> detaildate = new List<string>();
            DateTime ldate = DateTime.Today;
            int testcount = 0;
            // int curentdays = 0;
            foreach (var TAKEDATE in loguserleave)
            {
                detaildate.Add(TAKEDATE.leave_days.ToString());
                if (TAKEDATE.From_Date.Day < ldate.Day)
                {
                    testcount = testcount + Convert.ToInt32(TAKEDATE.leave_days);
                }
                count = count + Convert.ToInt32(TAKEDATE.leave_days);
            }
            ViewBag.leavecount = count;
            List<Leave_Master> leaves = db.Leave_Master.Where(x => x.Status != 6 & x.Leave_Crdr == 2 & x.UserMaster.Company_ID == _LogedUser.Company_ID & x.To_Date.Year == Year).OrderBy(x => x.Status).ThenByDescending(x => x.From_Date).ToList();
            ViewBag.leaves = leaves.Count;

            DateTime today = DateTime.Today;
            DateTime endOfMonth = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
            //get only last day of month
            int day = endOfMonth.Day;
            //my code
            int currentday = today.Day;
            DateTime now = DateTime.Now;
            int totalSunDays;
            totalSunDays = 0;
            //variable of store till cuurent sunday
            int tillcurrentsunday = 0;
            //till current date sunday count
            for (int i = 0; i < currentday; i++)
            {
                DateTime dcode = new DateTime(now.Year, now.Month, i + 1);
                if (dcode.DayOfWeek == DayOfWeek.Sunday)
                {
                    tillcurrentsunday = tillcurrentsunday + 1;
                }
            }
            for (int i = 0; i < day; ++i)
            {
                DateTime d = new DateTime(now.Year, now.Month, i + 1);
                //Compare date with sunday
                if (d.DayOfWeek == DayOfWeek.Sunday)
                {
                    totalSunDays = totalSunDays + 1;
                }
            }
            ViewBag.TotalSundays = totalSunDays;
            int notcameData = tillcurrentsunday + testcount;
            int orignalworkdays = currentday - notcameData;
            //.....Count Total Holidays
            List<Holiday_Master> _holiday = db.Holiday_Master.Where(x => x.Company_Id == _LogedUser.Company_ID && x.Holiday_Date.Value.Month == date.Month && x.Holiday_Date.Value.Year == date.Year).ToList();
            int totalHolidays = _holiday.Count();
            List<Gift_Master> gift = db.Gift_Master.Where(x => x.UserMaster.Company_ID == _LogedUser.Company_ID & x.Date.Year == Year).ToList();
            ViewBag.gift = gift.Count();
            //comment for test
            //int OffDays = totalSunDays + totalHolidays;
            //my code
            // int OffDays = totalSunDays;
            ViewBag.TotalHoliday = totalHolidays;

            //.....Count Total Days
            int totalDays = getdays(Year, Month);

            //.....Count Total Present Days
            int PresentDays = totalDays - totalSunDays;
            //two logic result for test
            ViewBag.WorkingDays = PresentDays;
            ViewBag.attendedday = orignalworkdays;

            List<Attendance_Master> _atdn = db.Attendance_Master.Where(x => x.Punch_Date.Month == date.Month && x.Punch_Date.Year == date.Year).ToList();
            List<Leave_Master> leave = db.Leave_Master.Where(x => x.ReqDate.Month == date.Month && x.ReqDate.Year == date.Year && x.UserMaster.Company_ID == _LogedUser.Company_ID).ToList();
            ViewBag.attend = _atdn;
            ViewBag.totalDays = totalDays;
            ViewBag.leavelist = leave;
            ViewBag.Click = TempData["Click"]?.ToString();
            if (user.Profile_Lock != null)
            {
                if (user.Profile_Lock == false)
                {
                    TempData["Result"] = "Your Profile is lock !";
                }
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Employee_Add(FormCollection form, HttpPostedFileBase file)
        {
            try
            {

                long user_id = long.Parse(form["User_ID"].ToString());
                bool profileLock = form["Profile_Lock"] == "true" || form["Profile_Lock"] == "on";
                if (profileLock == false)
                {
                    if (user_id == 0)
                    {
                        string User_Name = form["User_Name"].ToString();
                        long User_Type = long.Parse(form["user_Type"].ToString());
                        long User_Mobile = long.Parse(form["User_Mobile"].ToString());
                        //long Mobile = long.Parse(form["Mobile"].ToString());
                        string User_Email = form["User_Email"].ToString();
                        string User_Password = form["User_Password"].ToString();
                        string City_Name = form["City_Name"].ToString();
                        string Religion_Name = form["Religion_Name"].ToString();
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
                        _newuser.Religion_name = Religion_Name;
                        _newuser.City_Name = City_Name;
                        //_newuser.User_Mobile = Mobile;
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
                        long? RefMobile = null; if (form["RefMobile"] != "") { RefMobile = long.Parse(form["RefMobile"].ToString()); }
                        long? Adharcard_No = null; if (form["Adharcard_No"] != "") { Adharcard_No = long.Parse(form["Adharcard_No"].ToString()); }
                        long Mobile = long.Parse(form["Mobile"].ToString());
                        string User_Name = form["User_Name"].ToString();
                        string PanCard_No = form["PanCard_No"].ToString();
                        string Blood_group = form["Blood_group"].ToString();
                        string Bank_Acc_No = form["Bank_Acc_No"].ToString();
                        string IFSC_No = form["IFSC_No"].ToString();
                        string Bank_Name = form["Bank_Name"].ToString();
                        string UAN_No = form["UAN_No"].ToString();
                        string ESIC_No = form["ESIC_No"].ToString();
                        string City_Name = form["City_Name"].ToString();
                        string Religion_Name = form["Religion_Name"].ToString();
                        DateTime DOB = DateTime.Parse(form["DOB"].ToString());


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

                        _updateuser.PanCard_No = PanCard_No;
                        _updateuser.User_Name = User_Name;
                        _updateuser.User_Mobile = Mobile;
                        _updateuser.Bank_Name = Bank_Name;
                        _updateuser.Bank_Acc_No = Bank_Acc_No;
                        _updateuser.IFSC_No = IFSC_No;
                        _updateuser.Blood_group = Blood_group;
                        _updateuser.DOB = DOB;
                        _updateuser.UAN_No = UAN_No;
                        _updateuser.ESIC_No = ESIC_No;
                        _updateuser.City_Name = City_Name;
                        _updateuser.Religion_name = Religion_Name;

                        db.SaveChanges();
                        return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "User Updated Successfully !", Refresh = "Employee_Detail?User_id=" + _updateuser.User_ID });
                    }
                }
                else
                {
                    TempData["Result"] = "Your Profile is lock !";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Your Profile is lock !" });

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
                bool profileLock = form["Profile_Lock"] == "true" || form["Profile_Lock"] == "on";
                if (profileLock == false)
                {
                    if (user_id != 0)
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
                        DateTime DOB = DateTime.Parse(form["DOB"].ToString());
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

                        db.SaveChanges();
                        TempData["Result"] = "User Updated Successfully !";
                        return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "User Updated Successfully !", Refresh = "Employee_Detail?User_id=" + _updateuser.User_ID });
                    }
                    else
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "User not found !" });

                    }
                }
                else
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Your Profile is lock !" });

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
                bool profileLock = form["Profile_Lock"] == "true" || form["Profile_Lock"] == "on";
                if (profileLock == false)
                {
                    if (user_id != 0)
                    {
                        UserMaster _updateuser = db.UserMasters.Find(user_id);
                        long? Department = null; if (form["Department"] != "") { Department = long.Parse(form["Department"].ToString()); }
                        long? Designation = null; if (form["Designation"] != "") { Designation = long.Parse(form["Designation"].ToString()); }
                        DateTime Date_of_join = DateTime.Parse(form["Date_of_join"].ToString());
                        long Reporting_Person = long.Parse(form["Reporting_Person"].ToString());
                        string Team_name = form["Team_name"].ToString();
                        string Posted_At = form["Posted_At"].ToString();
                        string Company_Email = form["Company_Email"].ToString();
                        long User_Type = long.Parse(form["user_Type"].ToString());


                        _updateuser.User_type = User_Type;
                        _updateuser.Company_Email = Company_Email;
                        _updateuser.Depart_id = Department;
                        _updateuser.Desig_id = Designation;
                        _updateuser.Team_name = Team_name;
                        _updateuser.Date_of_join = Date_of_join;
                        _updateuser.Posted_At = Posted_At;
                        _updateuser.Reporting_Person = Reporting_Person;
                        db.SaveChanges();
                        TempData["Result"] = "Company Detail Updated Successfully !";
                        return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Company Detail Updated Successfully !", Refresh = "Employee_Detail?User_id=" + _updateuser.User_ID });
                    }
                    else
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "User not found !" });

                    }
                }
                else
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Your Profile is lock !" });

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
                bool profileLock = form["Profile_Lock"] == "true" || form["Profile_Lock"] == "on";
                if (profileLock == false)
                {
                    if (user_id != 0)
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
                        TempData["Result"] = " Bank Detail Updated Successfully !";
                        return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = " Bank Detail Updated Successfully !", Refresh = "Employee_Detail?User_id=" + _updateuser.User_ID });
                    }
                    else
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "User not found !" });

                    }
                }
                else
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Your Profile is lock !" });

                }
            }
            catch (Exception e)
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Sorry, Unknown Error Occured - " + e.Message.ToString() + ". Please Contact Administrator ! " });
            }

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

        [HttpPost]
        public ActionResult Address_Add(FormCollection form)
        {
            string Address_City = form["Address_City"].ToString();
            string address = form["Address_address"].ToString();
            string Cmd = form["Command"].ToString();
            if (Cmd == "Save")
            {
                Address_Master newrAdd = new Address_Master();
                newrAdd.User_ID = _LogedUser.User_ID;
                newrAdd.Add_Address = address;
                newrAdd.CityName = Address_City;
                newrAdd.Status = 0;
                db.Address_Master.Add(newrAdd);
                db.SaveChanges();
                ViewData["IsEnabled"] = false;
                TempData["Click"] = "Address";
                TempData["Result"] = "Address Added Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Address Added Successfully !", Refresh = "Employee_Detail?User_id=" + _LogedUser.User_ID });

            }

            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !" });
            }
        }
        [HttpPost]
        public ActionResult Relation_Add(FormCollection form)
        {

            string Relation = form["Relation_Relation_Name"].ToString();
            string Person_Name = form["Relation_Person_Name"].ToString();
            long? Mobile_No = long.Parse(form["Relation_Mobile_No"].ToString());
            string Cmd = form["Command"].ToString();
            if (Cmd == "Save")
            {
                UserMaster u_mobile = db.UserMasters.Where(x => x.User_Mobile == Mobile_No && x.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
                if (u_mobile != null) { return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Mobile Number Already Registered !" }); }

                Relation_Master newrel = new Relation_Master();
                newrel.User_Id = _LogedUser.User_ID;
                newrel.Relation_Name = Relation;
                newrel.Person_Name = Person_Name;
                newrel.Mobile_No = Mobile_No;
                newrel.Status = 0;
                db.Relation_Master.Add(newrel);
                db.SaveChanges();
                TempData["Result"] = "Relation Added Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Relation Added Successfully !", Refresh = "Default" });
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
            string Edu_Name = form["Education_Edu_Name"].ToString();
            string university = form["Education_university"].ToString();
            string Field = form["Field"].ToString();
            string Pass_Out = form["Education_Pass_Out"].ToString();
            if (Cmd == "Save")
            {

                Education_Master Edu = new Education_Master();
                Edu.User_Id = _LogedUser.User_ID;
                Edu.Edu_Name = Edu_Name;
                Edu.Field = Field;
                Edu.university = university;
                Edu.Pass_Out = Pass_Out;
                Edu.Status = 0;
                db.Education_Master.Add(Edu);
                db.SaveChanges();
                TempData["Result"] = "Education Added Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Education Added Successfully !", Refresh = "Default" });
            }

            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !" });
            }
        }
        [HttpPost]
        public ActionResult Experience_Add(FormCollection form)
        {

            string Last_Company = form["Experience_Last_Company"].ToString();
            string working_years = form["Experience_working_years"].ToString();
            string Last_Year = form["Experience_Last_Year"].ToString();
            string Cmd = form["Command"].ToString();
            if (Cmd == "Save")
            {

                Experience_Master Exp = new Experience_Master();
                Exp.User_Id = _LogedUser.User_ID;
                Exp.Last_Company = Last_Company;
                Exp.working_years = working_years;
                Exp.Last_Year = Last_Year;
                Exp.Status = 0;
                db.Experience_Master.Add(Exp);
                db.SaveChanges();
                TempData["Result"] = "Experience Added Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Experience Added Successfully !", Refresh = "Default" });
            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !" });
            }
        }
        [HttpPost]
        public ActionResult Document_Add(FormCollection form, HttpPostedFileBase file)
        {

            string Doc_Name = form["Document_Doc_Name"].ToString();
            string Cmd = form["Command"].ToString();
            if (Cmd == "Save")
            {

                Document_Master Doc = new Document_Master();
                Doc.User_Id = _LogedUser.User_ID;
                Doc.Doc_Name = Doc_Name;
                Doc.Status = 0;
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
                    var path = Path.Combine(Server.MapPath("~/Uploads/User_Document"), myfile);
                    file.SaveAs(path);
                    Doc.Doc_Path = path;
                    Doc.Doc_Type = myfile;
                }
                Doc.Company_ID = _LogedUser.Company_ID;
                db.Document_Master.Add(Doc);
                db.SaveChanges();
                TempData["Result"] = "Document Added Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Document Added Successfully !", Refresh = "Default" });
            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !" });
            }
        }
        public ActionResult Employee_Statement_List()
        {
            ViewBag.PageTitle = "Employee Statement";

            List<Transaction_Master> Emp_trans = db.Transaction_Master.Where(x => x.User_id == _LogedUser.User_ID).OrderByDescending(x => x.Trans_id).ToList();
            ViewBag.Emp_trans = Emp_trans;
            return View();
        }
        public ActionResult Employee_AssetMovement_List()
        {
            ViewBag.PageTitle = "Employee AssetMovement";
            List<Asset_Movement> movment = db.Asset_Movement.Where(x => x.UserMaster.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.Movement_id).ToList();
            List<Asset_Master> assts = db.Asset_Master.Where(x => x.Company_ID == _LogedUser.Company_ID).ToList();
            ViewBag.assts = assts;
            ViewBag.movment = movment;
            return View();
        }
        public ActionResult Important_Document()
        {
            ViewBag.PageTitle = "Imporatant Documents";

            List<Document_Master> _impdoc = db.Document_Master.Where(x => x.User_Id == _LogedUser.User_ID && x.Company_ID == _LogedUser.Company_ID && x.Status == 0).ToList();
            ViewBag._impdoc = _impdoc;
            return View();
        }
        public ActionResult Salary_Slip()
        {
            ViewBag.PageTitle = "Salary Slip";

            List<Generated_Documents> _impdoc = db.Generated_Documents.Where(x => x.Gen_doc_empid == _LogedUser.User_ID && x.UserMaster.Company_ID == _LogedUser.Company_ID).ToList();
            ViewBag._impdoc = _impdoc;
            return View();
        }
        public ActionResult USerPrintDocument(long Gen_doc_id)
        {
            ViewBag.PageTitle = "view Documents";

            Generated_Documents gendocs = db.Generated_Documents.Where(x => x.Gen_doc_id == Gen_doc_id && x.UserMaster.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
            ViewBag.gendocs = gendocs;
            return View();

        }




        public ActionResult Resignation_List()
        {
            ViewBag.PageTitle = "All Resignation";

            Resignation_Master resg = db.Resignation_Master.Where(x => x.Emp_Id == _LogedUser.User_ID & x.Status == 1).OrderByDescending(x => x.Resignation_Id).FirstOrDefault();
            ViewBag.resg = resg;
            List<Resignation_Master> Resignation = db.Resignation_Master.Where(x => x.Status != 6 && x.Emp_Id == _LogedUser.User_ID).OrderByDescending(x => x.Status).ThenBy(x => x.Date_Of_Resignation).ToList();
            ViewBag.Resignation = Resignation;
            List<UserMaster> user1 = db.UserMasters.Where(x => x.User_type != _LogedUser.User_type & x.Company_ID == _LogedUser.Company_ID).ToList();
            ViewBag.user1 = user1;
            List<UserMaster> user = db.UserMasters.Where(x => x.Company_ID == _LogedUser.Company_ID && x.status == 0).ToList();
            ViewBag.user = user;
            List<Designation_Master> desg = db.Designation_Master.Where(x => x.Company_ID == _LogedUser.Company_ID).ToList();
            List<UserMaster> desguser = db.UserMasters.Where(x => x.Desig_id == x.Designation_Master.Desg_Id && x.User_ID == _LogedUser.User_ID).ToList();
            List<string> designationuser = desguser.Select(x => x.Designation_Master.Designation).ToList();
            ViewBag.desguserdata = designationuser;
            ViewBag.desg = desg;
            List<Department_Master> dept = db.Department_Master.Where(x => x.Company_ID == _LogedUser.Company_ID).ToList();
            ViewBag.dept = dept;
            if (resg != null)
            {
                if (resg.Status == 1)
                {
                    TempData["failure"] = "Your Resignation Is Approved";
                }
            }
            return View();
        }
        [HttpPost]
        public ActionResult Resignation_Add(FormCollection form)
        {

            UserMaster um = db.UserMasters.Where(x => x.User_type == 1 && x.status == 0).FirstOrDefault();

            long Resignation_Id = long.Parse(form["Resignation_Id"].ToString());
            long Designation = long.Parse(form["Designation"].ToString());
            long Department = long.Parse(form["Department"].ToString());
            string Team_Name = form["Team_Name"].ToString();
            DateTime Date_Of_Join = DateTime.Parse(form["Date_Of_Join"].ToString());
            long Reporting_Manager = long.Parse(form["Reporting_Manager"].ToString());
            DateTime Reliving_Date = DateTime.Parse(form["Reliving_Date"].ToString());
            string Reason_For_Exit = form["Reason_For_Exit"].ToString();
            string Remark_For_Resignation = form["Remark_For_Resignation"].ToString();
            string Suggestion_For_Company = form["Suggestion_For_Company"].ToString();
            //long HR_Authority = long.Parse(form["HR_Authority"].ToString());

            string Cmd = form["Command"].ToString();
            if (Cmd == "Save")
            {
                if (Resignation_Id == 0)
                {
                    Resignation_Master resg = db.Resignation_Master.Where(x => x.Emp_Id == _LogedUser.User_ID).OrderByDescending(x => x.Resignation_Id).FirstOrDefault();
                    ViewBag.resg = resg;
                    if (resg != null)
                    {
                        if (resg.Status == 1)
                        {
                            TempData["failure"] = "Your Resignation Is Approved";
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Your Resignation Is Approved" });
                        }
                        else if (resg.Status == 0)
                        {
                            TempData["failure"] = "Your Resignation Is Pending";
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Your Resignation Is Pending" });
                        }
                    }
                    if (DateTime.Now > Reliving_Date)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Please Add Proper Date." });
                    }
                    Resignation_Master Resignation = new Resignation_Master();
                    Resignation.Emp_Id = _LogedUser.User_ID;
                    Resignation.Designation = Designation;
                    Resignation.Team_Name = Team_Name;
                    Resignation.Date_Of_Join = Date_Of_Join;
                    Resignation.Department = Department;
                    Resignation.Date_Of_Resignation = DateTime.Now;
                    Resignation.Reporting_Person = Reporting_Manager;
                    Resignation.Reliving_Date = Reliving_Date;
                    Resignation.Reason_For_Exit = Reason_For_Exit;
                    Resignation.Remark_For_Resignation = Remark_For_Resignation;
                    Resignation.Suggestion_For_Company = Suggestion_For_Company;
                    Resignation.HR_Authority = um.User_ID;
                    Resignation.Status = 0;
                    db.Resignation_Master.Add(Resignation);
                    db.SaveChanges();
                    Designation_Master d = db.Designation_Master.Where(x => x.Desg_Id == Designation).FirstOrDefault();
                    string desg = d.Designation;
                    ///Email Send.....................
                    string sub = "Please Approved My Resignation ";
                    string message = "Please Approved My Resignation.<br/> Reason :" + Resignation.Reason_For_Exit;
                    var subject = "Regarding Resignation ";
                    var body = "Dear Sir , <br/>  I am resigning   : <br/>  Position: " + desg + "<br/>Subject: " + sub + "<br/> Message: " + message + "<br/>Thank You ! <br/> This is System Generated Mail....!";
                    MailAddress fromAddress = new MailAddress(_LogedUser.User_Email);
                    MailAddress toAddress = new MailAddress(_LogedUser.UserMaster2.User_Email);
                    Thread t1 = null;
                    t1 = new Thread(new ThreadStart(() => new CommonClasses().Email_Verify(fromAddress.ToString(), toAddress.ToString(), subject.ToString(), body.ToString())));
                    t1.Start();
                    TempData["Result"] = "Resignation Added Successfully.";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Resignation Added Successfully !", Refresh = "Default" });
                }
                else
                {
                    Resignation_Master Resignation = db.Resignation_Master.Where(x => x.Resignation_Id == Resignation_Id).FirstOrDefault();
                    if (Resignation.Status != 0)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Resignation Not Updated  !" });
                    }
                    if (Resignation == null)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Resignation  !" });
                    }
                    if (DateTime.Now > Reliving_Date)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Please Add Proper Date." });
                    }
                    if (Resignation.Status != 0)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Resignation Not Updated  !" });
                    }
                    Resignation.Emp_Id = _LogedUser.User_ID;
                    Resignation.Designation = Designation;
                    Resignation.Team_Name = Team_Name;
                    Resignation.Date_Of_Join = Date_Of_Join;
                    Resignation.Department = Department;
                    Resignation.Date_Of_Resignation = DateTime.Now;
                    Resignation.Reporting_Person = Reporting_Manager;
                    Resignation.Reliving_Date = Reliving_Date;
                    Resignation.Reason_For_Exit = Reason_For_Exit;
                    Resignation.Remark_For_Resignation = Remark_For_Resignation;
                    Resignation.Suggestion_For_Company = Suggestion_For_Company;
                    Resignation.HR_Authority = um.User_ID;
                    Resignation.Status = 0;
                    db.SaveChanges();
                    TempData["Result"] = "Resignation Updated Successfully.";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Resignation Updated Successfully!", Refresh = "Default" });
                }
            }
            else if (Cmd == "Delete")
            {

                Resignation_Master Resignation = db.Resignation_Master.Where(x => x.Resignation_Id == Resignation_Id).FirstOrDefault();
                var _del = db.Resignation_Master.Find(Resignation_Id);
                if (Resignation == null)
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Resignation  !" });
                }
                if (Resignation.Status != 0)
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Resignation Not Updated  !" });
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
        public ActionResult GetEmp()
        {

            string DOJ, team_name, Report_Person;
            long desg, dept;
            UserMaster user = db.UserMasters.Where(x => x.User_ID == _LogedUser.User_ID).FirstOrDefault();
            if (user != null)
            {
                return Json(new { success = true, DOJ = user.Date_of_join.ToString("dd MMMM yyyy"), team_name = user.Team_name, Report_Person = user.Reporting_Person, desg = user.Desig_id, dept = user.Depart_id });
            }
            else
            {
                return Json(new { success = false });
            }
        }
        public ActionResult Leave_Approve_List()
        {
            ViewBag.PageTitle = "All Leave Request";

            List<Leave_Master> leave = db.Leave_Master.Where(x => x.Status != 6 & x.Leave_Crdr == 2 && x.UserMaster.Reporting_Person == _LogedUser.User_ID && x.UserMaster.status == 0).OrderBy(x => x.Status).ThenByDescending(x => x.From_Date).ToList();
            ViewBag.leaves = leave;

            List<UserMaster> user = db.UserMasters.Where(x => x.Company_ID == _LogedUser.Company_ID && x.status == 0).ToList();
            ViewBag.user = user;
            return View();
        }
        public ActionResult Leave_Emp_Approve(FormCollection form)
        {


            long Emp_Id = long.Parse(form["Emp_Id"].ToString());
            long Leave_Id = long.Parse(form["leave_Id"].ToString());
            DateTime From_Date = DateTime.Parse(form["From_Date"].ToString());
            DateTime To_Date = DateTime.Parse(form["To_Date"].ToString());
            string Leave_Type = form["Leave_Type"].ToString();
            string reason = form["reason"].ToString();
            string Remark = form["Remark"].ToString();
            string Cmd = form["Command"].ToString();
            if (Cmd == "Approve")
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
                if (Leave.UserMaster.Reporting_Person != _LogedUser.User_ID)
                {
                    TempData["failure"] = "Invalid Reporting Person....!";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Reporting Person....!" });
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

                string sub = "Your Leave Will Approved";
                string message = "Your Leave Will Approved";
                var subject = "Regarding Leave";
                var body = "Dear Employee , <br/> Your Leave will Approved  Pelase take : <br/>From : " + From_Date + "<br/>To: " + To_Date + "<br/>  Subject: " + sub + "<br/>Message: " + message + "<br/>Thank You !  <br/> This is System Generated Mail....!";
                MailAddress fromAddress = new MailAddress(_LogedUser.User_Email);
                MailAddress toAddress = new MailAddress(u.User_Email);
                Thread t1 = null;
                t1 = new Thread(new ThreadStart(() => new CommonClasses().Email_Verify(fromAddress.ToString(), toAddress.ToString(), subject.ToString(), body.ToString())));
                t1.Start();

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
                if (Leave.UserMaster.Reporting_Person != _LogedUser.User_ID)
                {
                    TempData["failure"] = "Invalid Reporting Person....!";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Reporting Person....!" });
                }
                if (Leave.Status == 1)
                {
                    TempData["failure"] = "Leave Approved Not Changed!";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave Approved Not Changed!" });
                }
                Leave.Approved_By = _LogedUser.User_ID;
                Leave.Status = 2;
                db.SaveChanges();
                UserMaster u = db.UserMasters.Where(x => x.User_ID == Emp_Id).FirstOrDefault();

                ////////////Email Send.....................

                string sub = "Your Leave will Rejected";
                string message = "Your Leave will Rejected";
                var subject = "Regarding Leave";
                var body = "Dear Employee , <br/> Your Leave will Rejected <br/>  Subject: " + sub + "<br/>Message: " + message + "<br/>Thank You ! ";
                MailAddress fromAddress = new MailAddress(_LogedUser.User_Email);
                MailAddress toAddress = new MailAddress(u.User_Email);
                Thread t1 = null;
                t1 = new Thread(new ThreadStart(() => new CommonClasses().Email_Verify(fromAddress.ToString(), toAddress.ToString(), subject.ToString(), body.ToString())));
                t1.Start();

                TempData["failure"] = "Leave Rejected Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave Rejected Successfully !", Refresh = "Default" });
            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !" });
            }
        }
        public ActionResult Leave_List(int status = 101)
        {
            ViewBag.PageTitle = "All Leave Request";

            if (status == 101)
            {
                List<Leave_Master> leave = db.Leave_Master.Where(x => x.Status != 6 & x.Leave_Crdr == 2 && x.User_Id == _LogedUser.User_ID).OrderByDescending(x => x.leave_Id).ToList();
                ViewBag.leaves = leave;
            }
            if (status == 0)
            {
                List<Leave_Master> leave = db.Leave_Master.Where(x => x.Status == 0 & x.Leave_Crdr == 2 && x.User_Id == _LogedUser.User_ID).ToList();
                ViewBag.leaves = leave;
            }
            if (status == 501)
            {
                UserMaster _user = db.UserMasters.Where(x => x.User_ID == _LogedUser.User_ID).FirstOrDefault();
                ViewBag._user = _user;
            }
            return View();
        }
        [HttpPost]
        public ActionResult Leave_Add(FormCollection form)
        {

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
                    if (Leave_Type == "SL")
                    {

                        if (From_Date < DateTime.Now.Date.AddDays(0))
                        {
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "SL add only one day!" });
                        }

                        if (To_Date < From_Date)
                        {
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "To Date Can't before From Date  !" });
                        }
                        List<Leave_Master> leaves = db.Leave_Master.Where(x => x.Status != 6 && x.User_Id == _LogedUser.User_ID && x.Leave_Crdr == (decimal)2.0).ToList();
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
                        Leave.User_Id = _LogedUser.User_ID;
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
                        db.SaveChanges();
                        TempData["Result"] = "New leave Added Successfully.";
                        return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "New leave Added Successfully !", Refresh = "Default" });
                    }
                    if (days == 1)
                    {
                        if (Leave_Type != "EL" )
                        {
                            if(Leave_Type !="LWPS")
                            {
                                if (From_Date < DateTime.Now.Date.AddDays(1))
                                {
                                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave for 1 to 3 days should be planned before 2 days !" });
                                }
                                if (From_Date == DateTime.Now.Date.AddDays(1))
                                {
                                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave for 1 to 3 days should be planned before 2 days !" });
                                }
                            }
                            
                            if (To_Date < From_Date)
                            {
                                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "To Date Can't before From Date  !" });
                            }
                            List<Leave_Master> leaves = db.Leave_Master.Where(x => x.Status != 6 && x.User_Id == _LogedUser.User_ID && x.Leave_Crdr == (decimal)2.0).ToList();
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
                        }

                        if (From_Date < DateTime.Now.Date)
                        {
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Yesterday Leave Not Consider!" });
                        }

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
                        Leave.User_Id = _LogedUser.User_ID;
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
                        UserMaster u = db.UserMasters.Where(x => x.User_ID == _LogedUser.User_ID).FirstOrDefault();
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
                    else if (days == 2 || days == 3)
                    {

                        if (From_Date < DateTime.Now.Date.AddDays(1))
                        {
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave for 1 to 3 days should be planned before 2 days !" });
                        }
                        if (From_Date == DateTime.Now.Date.AddDays(1))
                        {
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave for 1 to 3 days should be planned before 2 days !" });
                        }
                        if (To_Date < From_Date)
                        {
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "To Date Can't before From Date  !" });
                        }
                        for (var day = From_Date.Date; day <= To_Date; day = day.AddDays(1))
                        {
                            leaveDays.Add(day);
                        }
                        foreach (DateTime d in leaveDays)
                        {
                            List<Leave_Master> leaves = db.Leave_Master.Where(x => x.Status != 6 && x.User_Id == _LogedUser.User_ID && x.Leave_Crdr == (decimal)2.0).ToList();
                            foreach (Leave_Master leave in leaves)
                            {
                                if (leave != null)
                                {
                                    DateTime fd = leave.From_Date;
                                    DateTime td = leave.To_Date;
                                    for (var day = fd.Date; day <= td; day = day.AddDays(1))
                                    {
                                        if (day == d)
                                        {
                                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave request already exist !" });
                                        }
                                        else
                                        {
                                            leaveDaysNew.Add(day);
                                        }
                                    }

                                }
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

                        if (From_Date < DateTime.Now.Date)
                        {
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Yesterday Leave Not Consider!" });
                        }

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
                        Leave.User_Id = _LogedUser.User_ID;
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
                        UserMaster u = db.UserMasters.Where(x => x.User_ID == _LogedUser.User_ID).FirstOrDefault();
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
                    else if (days == 4 || days == 5 || days == 6)
                    {

                        if (From_Date < DateTime.Now.Date.AddDays(4))
                        {
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave for 4 to 6 days should be planned before 5 days !" });
                        }
                        if (From_Date == DateTime.Now.Date.AddDays(4))
                        {
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave for 4 to 6 days should be planned before 5 days !" });
                        }
                        if (To_Date < From_Date)
                        {
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "To Date Can't before From Date  !" });
                        }
                        for (var day = From_Date.Date; day <= To_Date; day = day.AddDays(1))
                        {
                            leaveDays.Add(day);
                        }
                        foreach (DateTime d in leaveDays)
                        {
                            List<Leave_Master> leaves = db.Leave_Master.Where(x => x.Status != 6 && x.User_Id == _LogedUser.User_ID && x.Leave_Crdr == (decimal)2.0).ToList();
                            foreach (Leave_Master leave in leaves)
                            {
                                if (leave != null)
                                {
                                    DateTime fd = leave.From_Date;
                                    DateTime td = leave.To_Date;
                                    for (var day = fd.Date; day <= td; day = day.AddDays(1))
                                    {
                                        if (day == d)
                                        {
                                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave request already exist !" });
                                        }
                                        else
                                        {
                                            leaveDaysNew.Add(day);
                                        }
                                    }

                                }
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

                        if (From_Date < DateTime.Now.Date)
                        {
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Yesterday Leave Not Consider!" });
                        }

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
                        Leave.User_Id = _LogedUser.User_ID;
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
                        UserMaster u = db.UserMasters.Where(x => x.User_ID == _LogedUser.User_ID).FirstOrDefault();
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
                    else if (days >= 6)
                    {
                        if (Leave_Type == "PL" || Leave_Type == "CL" || Leave_Type == "SL")
                        {
                            if (From_Date < DateTime.Now.Date.AddDays(9))
                            {
                                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave for 6 or more days should be planned before 10 days !" });
                            }
                            if (From_Date == DateTime.Now.Date.AddDays(9))
                            {
                                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave for 6 or more days should be planned before 10 days !" });
                            }
                            if (To_Date < From_Date)
                            {
                                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "To Date Can't before From Date  !" });
                            }
                            for (var day = From_Date.Date; day <= To_Date; day = day.AddDays(1))
                            {
                                leaveDays.Add(day);
                            }
                            foreach (DateTime d in leaveDays)
                            {
                                List<Leave_Master> leaves = db.Leave_Master.Where(x => x.Status != 6 && x.User_Id == _LogedUser.User_ID && x.Leave_Crdr == (decimal)2.0).ToList();
                                foreach (Leave_Master leave in leaves)
                                {
                                    if (leave != null)
                                    {
                                        DateTime fd = leave.From_Date;
                                        DateTime td = leave.To_Date;
                                        for (var day = fd.Date; day <= td; day = day.AddDays(1))
                                        {
                                            if (day == d)
                                            {
                                                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave request already exist !" });
                                            }
                                            else
                                            {
                                                leaveDaysNew.Add(day);
                                            }
                                        }

                                    }
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
                        }

                        if (From_Date < DateTime.Now.Date)
                        {
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Yesterday Leave Not Consider!" });
                        }

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
                        Leave.User_Id = _LogedUser.User_ID;
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
                        UserMaster u = db.UserMasters.Where(x => x.User_ID == _LogedUser.User_ID).FirstOrDefault();
                        if (Leave.Leave_Type == "PL" && Leave.leave_days > u.Total_PL)
                        {
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "The Employee Does Not Have PL" });
                        }
                        else if (Leave.Leave_Type == "CL" && Leave.leave_days > u.Total_CL)
                        {
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "The Employee Does Not Have CL" });
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
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Check selected dates" });
                    }
                }
                else
                {
                    if (Leave_Type == "SL")
                    {

                        if (From_Date < DateTime.Now.Date.AddDays(0))
                        {
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "SL add only one day!" });
                        }
                        if (To_Date < From_Date)
                        {
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "To Date Can't before From Date  !" });
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
                        if (Leave.Status == 1)
                        {
                            TempData["failure"] = "Leave Approved Not Changed!";
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave Approved Not Changed!" });
                        }
                        Leave.User_Id = _LogedUser.User_ID;
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
                        db.SaveChanges();
                        TempData["Result"] = "leave Updated Successfully.";
                        return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "leave updated Successfully !", Refresh = "Default" });
                    }

                    if (days == 1)
                    {
                        if (Leave_Type != "EL")
                        {
                            if (From_Date < DateTime.Now.Date.AddDays(1))
                            {
                                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave for 1 to 3 days should be planned before 2 days !" });
                            }
                            if (From_Date == DateTime.Now.Date.AddDays(1))
                            {
                                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave for 1 to 3 days should be planned before 2 days !" });
                            }
                            if (To_Date < From_Date)
                            {
                                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "To Date Can't before From Date  !" });
                            }

                            List<Leave_Master> leaves = db.Leave_Master.Where(x => x.Status != 6 && x.leave_Id != Leave_Id && x.User_Id == _LogedUser.User_ID).ToList();
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
                        }

                        if (From_Date < DateTime.Now.Date)
                        {
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Yesterday Leave Not Consider!" });
                        }

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
                        if (Leave.Status == 1)
                        {
                            TempData["failure"] = "Leave Approved Not Changed!";
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave Approved Not Changed!" });
                        }

                        Leave.User_Id = _LogedUser.User_ID;
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
                        UserMaster u = db.UserMasters.Where(x => x.User_ID == _LogedUser.User_ID).FirstOrDefault();
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
                    else if (days == 2 || days == 3)
                    {

                        if (From_Date < DateTime.Now.Date.AddDays(1))
                        {
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave for 1 to 3 days should be planned before 2 days !" });
                        }
                        if (From_Date == DateTime.Now.Date.AddDays(1))
                        {
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave for 1 to 3 days should be planned before 2 days !" });
                        }
                        if (To_Date < From_Date)
                        {
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "To Date Can't before From Date  !" });
                        }
                        for (var day = From_Date.Date; day <= To_Date; day = day.AddDays(1))
                        {
                            leaveDays.Add(day);
                        }
                        foreach (DateTime d in leaveDays)
                        {
                            List<Leave_Master> leaves = db.Leave_Master.Where(x => x.Status != 6 && x.leave_Id != Leave_Id && x.User_Id == _LogedUser.User_ID && x.Leave_Crdr == (decimal)2.0).ToList();
                            foreach (Leave_Master leave in leaves)
                            {
                                if (leave != null)
                                {
                                    DateTime fd = leave.From_Date;
                                    DateTime td = leave.To_Date;
                                    for (var day = fd.Date; day <= td; day = day.AddDays(1))
                                    {
                                        if (day == d)
                                        {
                                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave request already exist !" });
                                        }
                                        else
                                        {
                                            leaveDaysNew.Add(day);
                                        }
                                    }

                                }
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

                        if (From_Date < DateTime.Now.Date)
                        {
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Yesterday Leave Not Consider!" });
                        }

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
                        if (Leave.Status == 1)
                        {
                            TempData["failure"] = "Leave Approved Not Changed!";
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave Approved Not Changed!" });
                        }

                        Leave.User_Id = _LogedUser.User_ID;
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
                        UserMaster u = db.UserMasters.Where(x => x.User_ID == _LogedUser.User_ID).FirstOrDefault();
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
                    else if (days == 4 || days == 5 || days == 6)
                    {

                        if (From_Date < DateTime.Now.Date.AddDays(4))
                        {
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave for 4 to 6 days should be planned before 5 days !" });
                        }
                        if (From_Date == DateTime.Now.Date.AddDays(4))
                        {
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave for 4 to 6 days should be planned before 5 days !" });
                        }
                        if (To_Date < From_Date)
                        {
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "To Date Can't before From Date  !" });
                        }
                        for (var day = From_Date.Date; day <= To_Date; day = day.AddDays(1))
                        {
                            leaveDays.Add(day);
                        }
                        foreach (DateTime d in leaveDays)
                        {
                            List<Leave_Master> leaves = db.Leave_Master.Where(x => x.Status != 6 && x.leave_Id != Leave_Id && x.User_Id == _LogedUser.User_ID && x.Leave_Crdr == (decimal)2.0 && (x.From_Date == d || x.To_Date == d)).ToList();
                            foreach (Leave_Master leave in leaves)
                            {
                                if (leave != null)
                                {
                                    DateTime fd = leave.From_Date;
                                    DateTime td = leave.To_Date;
                                    for (var day = fd.Date; day <= td; day = day.AddDays(1))
                                    {
                                        if (day == d)
                                        {
                                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave request already exist !" });
                                        }
                                        else
                                        {
                                            leaveDaysNew.Add(day);
                                        }
                                    }

                                }
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

                        if (From_Date < DateTime.Now.Date)
                        {
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Yesterday Leave Not Consider!" });
                        }

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
                        if (Leave.Status == 1)
                        {
                            TempData["failure"] = "Leave Approved Not Changed!";
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave Approved Not Changed!" });
                        }

                        Leave.User_Id = _LogedUser.User_ID;
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
                        UserMaster u = db.UserMasters.Where(x => x.User_ID == _LogedUser.User_ID).FirstOrDefault();
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
                    else if (days >= 6)
                    {
                        if (Leave_Type == "PL" || Leave_Type == "CL" || Leave_Type == "SL")
                        {
                            if (From_Date < DateTime.Now.Date.AddDays(9))
                            {
                                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave for 6 or more days should be planned before 10 days !" });
                            }
                            if (From_Date == DateTime.Now.Date.AddDays(9))
                            {
                                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave for 6 or more days should be planned before 10 days !" });
                            }
                            if (To_Date < From_Date)
                            {
                                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "To Date Can't before From Date  !" });
                            }
                            for (var day = From_Date.Date; day <= To_Date; day = day.AddDays(1))
                            {
                                leaveDays.Add(day);
                            }
                            foreach (DateTime d in leaveDays)
                            {
                                List<Leave_Master> leaves = db.Leave_Master.Where(x => x.Status != 6 && x.leave_Id != Leave_Id && x.User_Id == _LogedUser.User_ID && x.Leave_Crdr == (decimal)2.0).ToList();
                                foreach (Leave_Master leave in leaves)
                                {
                                    if (leave != null)
                                    {
                                        DateTime fd = leave.From_Date;
                                        DateTime td = leave.To_Date;
                                        for (var day = fd.Date; day <= td; day = day.AddDays(1))
                                        {
                                            if (day == d)
                                            {
                                                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave request already exist !" });
                                            }
                                            else
                                            {
                                                leaveDaysNew.Add(day);
                                            }
                                        }

                                    }
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

                        }
                        if (From_Date < DateTime.Now.Date)
                        {
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Yesterday Leave Not Consider!" });
                        }

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
                        if (Leave.Status == 1)
                        {
                            TempData["failure"] = "Leave Approved Not Changed!";
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Leave Approved Not Changed!" });
                        }

                        Leave.User_Id = _LogedUser.User_ID;
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
                        UserMaster u = db.UserMasters.Where(x => x.User_ID == _LogedUser.User_ID).FirstOrDefault();
                        if (Leave.Leave_Type == "PL" && Leave.leave_days > u.Total_PL)
                        {
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "The Employee Does Not Have PL" });
                        }
                        else if (Leave.Leave_Type == "CL" && Leave.leave_days > u.Total_CL)
                        {
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "The Employee Does Not Have CL" });
                        }



                        db.SaveChanges();
                        TempData["Result"] = "leave Updated Successfully.";
                        return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "leave updated Successfully !", Refresh = "Default" });

                    }
                    else
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Check selected dates" });
                    }

                }
            }
            else if (Cmd == "Delete")
            {
                Leave_Master Leave = db.Leave_Master.Where(x => x.leave_Id == Leave_Id).FirstOrDefault();
                var _del = db.Leave_Master.Find(Leave_Id);
                if (Leave.Status == 1 || Leave.Status == 2)
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Can't Update " });
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
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !" });
            }

        }
        public ActionResult Project_Task_List(int status = 101)
        {

            if (status == 101)
            {
                List<Project_Task_Master> Task = db.Project_Task_Master.Where(x => x.Status != 6 && x.Emp_Id == _LogedUser.User_ID & x.Project_Master.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.Task_Id).ToList();
                ViewBag.task = Task;
            }
            if (status == 0)
            {
                List<Project_Task_Master> Task = db.Project_Task_Master.Where(x => x.Status == 0 && x.Emp_Id == _LogedUser.User_ID & x.Project_Master.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.Task_Id).ToList();
                ViewBag.task = Task;
            }
            if (status == 4)
            {
                List<Project_Task_Master> Task = db.Project_Task_Master.Where(x => x.Status == 4 && x.Emp_Id == _LogedUser.User_ID & x.Project_Master.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.Task_Id).ToList();
                ViewBag.task = Task;
            }
            if (status == 5)
            {
                List<Project_Task_Master> Task = db.Project_Task_Master.Where(x => x.Status == 5 && x.Emp_Id == _LogedUser.User_ID & x.Project_Master.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.Task_Id).ToList();
                ViewBag.task = Task;
            }
            return View();
        }
        public ActionResult Project_Task_Add(long TaskID)
        {
            ViewBag.PageTitle = "Add Project Task";

            Project_Task_Master p = db.Project_Task_Master.Where(x => x.Task_Id == TaskID).FirstOrDefault();
            ViewBag._p = p;
            List<Project_Assign_Master> projects = db.Project_Assign_Master.Where(x => x.Emp_Id == _LogedUser.User_ID && x.Project_Master.Company_ID == _LogedUser.Company_ID).ToList();
            ViewBag.Project = projects;
            List<TaskType> taskTypes = db.TaskTypes.Where(x => x.Company_ID == _LogedUser.Company_ID).ToList();
            ViewBag.t = taskTypes;
            List<Comment_Master> cmt = db.Comment_Master.Where(x => x.Task_Id == TaskID & x.Comment_Type == 1 && x.UserMaster.Company_ID == _LogedUser.Company_ID).ToList();
            ViewBag._c = cmt;
            return View();
        }
        [HttpPost]
        public ActionResult Project_Task_Add(FormCollection form)
        {

            long Task_Id = long.Parse(form["Task_Id"].ToString());
            long Project_Id = long.Parse(form["Project_Id"].ToString());
            string Task_Name = form["Task_Name"].ToString();
            DateTime A_Start_Date = DateTime.Parse(form["A_Start_Date"].ToString());
            DateTime R_End_Date = DateTime.Parse(form["R_End_Date"].ToString());
            string Duration_Unit = form["Duration_Unit"].ToString();
            long Duration = long.Parse(form["Duration"].ToString());
            string Description = form["Description"].ToString();
            long Task_Type = long.Parse(form["Task_Type"].ToString());
            string Comments = null;
            //if (form["comments"].ToString()!=null || form["comments"].ToString() != "") {  Comments = form["comments"].ToString(); }

            string Cmd = form["Command"].ToString();
            if (Cmd == "Save")
            {
                if (Task_Id == 0)
                {
                    if (A_Start_Date > R_End_Date)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Please Add Proper Date." });
                    }
                    Project_Assign_Master assign = db.Project_Assign_Master.Where(x => x.Emp_Id == _LogedUser.User_ID).FirstOrDefault();
                    if (assign == null)
                    {
                        Project_Assign_Master project = new Project_Assign_Master();
                        project.Project_Id = Project_Id;
                        project.Emp_Id = _LogedUser.User_ID;
                        db.Project_Assign_Master.Add(project);
                        db.SaveChanges();
                    }
                    Project_Task_Master Task = new Project_Task_Master();
                    Task.Task_Name = Task_Name;
                    Task.Emp_Id = _LogedUser.User_ID;
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
                    cmt.Emp_Id = _LogedUser.User_ID;
                    db.Comment_Master.Add(cmt);
                    db.SaveChanges();


                    ///Email Send.....................

                    string sub = "Please Approved My Task ";
                    string message = "Please Approved My Tak";
                    var subject = "Regarding Project Task ";
                    var body = "Dear Sir , <br/>  I Request To Project Task   : <br/>Subject: " + sub + "<br/> Message: " + message + "<br/>Thank You !  <br/> This is System Generated Mail....!";
                    MailAddress fromAddress = new MailAddress(_LogedUser.User_Email);
                    MailAddress toAddress = new MailAddress(_LogedUser.User_Email);
                    Thread t1 = null;
                    t1 = new Thread(new ThreadStart(() => new CommonClasses().Email_Verify(fromAddress.ToString(), toAddress.ToString(), subject.ToString(), body.ToString())));
                    t1.Start();

                    TempData["Result"] = "New Task Added Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "New Task Added Successfully !", Refresh = "Project_Task_List" });
                }
                else
                {
                    Project_Task_Master Task = db.Project_Task_Master.Where(x => x.Task_Id == Task_Id).FirstOrDefault();
                    if (Task == null)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Task  !" });
                    }
                    Task.Task_Name = Task_Name;
                    Task.Emp_Id = _LogedUser.User_ID;
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
                    cmt.Emp_Id = _LogedUser.User_ID;
                    cmt.Task_Id = Task.Task_Id;
                    db.Comment_Master.Add(cmt);
                    db.SaveChanges();
                    TempData["Result"] = "Task Updated Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Task Updated Successfully !", Refresh = "Project_Task_List" });
                }
            }
            if (Cmd == "Delete")
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
                db.SaveChanges();
                TempData["failure"] = "Project Task Deleted Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Project Task Deleted Successfully!", Refresh = "Project_Task_List" });
            }
            if (Cmd == "Running")
            {
                Project_Task_Master p = db.Project_Task_Master.Where(x => x.Task_Id == Task_Id).FirstOrDefault();
                var _del = db.Project_Task_Master.Find(Task_Id);
                if (p == null)
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Task  !" });
                }
                p.Status = 7;
                db.SaveChanges();
                TempData["Result"] = "Project Task Running !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Project Task Running !", Refresh = "Project_Task_List" });

            }
            if (Cmd == "Stop")
            {
                Project_Task_Master p = db.Project_Task_Master.Where(x => x.Task_Id == Task_Id).FirstOrDefault();
                var _del = db.Project_Task_Master.Find(Task_Id);
                if (p == null)
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Task  !" });
                }
                p.Status = 8;
                db.SaveChanges();
                TempData["Result"] = "Project Task Stop.,";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Project Task Stop.", Refresh = "Project_Task_List" });

            }
            if (Cmd == "Complete")
            {
                Project_Task_Master p = db.Project_Task_Master.Where(x => x.Task_Id == Task_Id).FirstOrDefault();
                var _del = db.Project_Task_Master.Find(Task_Id);
                if (p == null)
                {
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Task  !" });
                }
                p.Status = 9;
                db.SaveChanges();
                TempData["Result"] = "Project Task Complete !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Project Task Complete !", Refresh = "Project_Task_List" });
            }
            if (Cmd == "Send")
            {
                if (Task_Id != 0)
                {
                    if (form["comments"].ToString() == "")
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Pelase Add Comment !" });
                    }
                    Comment_Master cm = new Comment_Master();
                    cm.Comments = form["comments"].ToString();
                    cm.Task_Id = Task_Id;
                    cm.Comment_Type = 1;
                    cm.Add_By = _LogedUser.User_ID;
                    cm.Add_On = DateTime.Now;
                    cm.Emp_Id = _LogedUser.User_ID;
                    cm.Commented_Date = DateTime.Now;
                    db.Comment_Master.Add(cm);
                    db.SaveChanges();
                    TempData["Result"] = "Comment Added Successfully.";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Comment Added Successfully.", Refresh = "Project_Task_List" });

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
                Comment_Master _cmt = db.Comment_Master.Where(x => x.Comnt_ID == Comment_Id).FirstOrDefault();
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
                TempData["Result"] = "Comment Deleted Successfully.";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Comment Deleted Successfully.", Refresh = "Project_Task_Add?TaskID=" + Task_Id });

            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !" });
            }


        }
        public ActionResult AdvanceReq_List()
        {
            ViewBag.PageTitle = "All Advance Request";

            List<Request_Master> req = db.Request_Master.Where(x => x.Status != 6 & x.User_Id == _LogedUser.User_ID).OrderByDescending(x => x.Req_Id).ToList();
            List<Project_Master> projects = db.Project_Master.Where(x => x.Company_ID == _LogedUser.Company_ID).ToList();
            List<UserMaster> user = db.UserMasters.Where(x => x.User_type != _LogedUser.User_type && x.Company_ID == _LogedUser.Company_ID).ToList();
            ViewBag.Project = projects;
            ViewBag.user = user;
            ViewBag.req = req;
            return View();
        }
        [HttpPost]
        public ActionResult AdvanceReq_Add(FormCollection form)
        {

            long Req_Id = long.Parse(form["Req_Id"].ToString());
            string Description = form["Description"].ToString();
            decimal Requested_Amount = decimal.Parse(form["Requested_Amount"].ToString());
            string Place = "";
            if (form["Place"] == null || form["Place"] == "")
            {

            }
            else
            {
                Place = form["Place"].ToString();

            }
            string Parent_Type = form["Parent_Type"].ToString();
            //DateTime S_Date = DateTime.Parse(form["S_Date"].ToString());
            //DateTime E_Date = DateTime.Parse(form["E_Date"].ToString());
            DateTime S_Date = DateTime.Now;
            DateTime E_Date = DateTime.Now;
            if (Parent_Type == "Opportunity" || Parent_Type == "Project")
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
                    Request.User_Id = _LogedUser.User_ID;
                    Request.Requested_Amount = Requested_Amount;
                    //Request.Place = Place;
                    Request.Parent_Type = Parent_Type;
                    Request.Description = Description;
                    Request.A_Date = DateTime.Now;
                    Request.S_Date = S_Date;
                    Request.Granted_Amount = 0;
                    Request.E_Date = E_Date;
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
                    if (Request.Status == 1)
                    {
                        TempData["failure"] = "Request Approved Not Changed!";
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Request Approved Not Changed!" });
                    }
                    if (Request == null)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Request  !" });
                    }
                    if (Request.Status == 1)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Request Cant't Update  !" });
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

                    Request.Requested_Amount = Requested_Amount;
                   // Request.Place = Place;
                    Request.Parent_Type = Parent_Type;
                    Request.Description = Description;
                    Request.A_Date = DateTime.Now;
                    Request.S_Date = S_Date;
                    Request.E_Date = E_Date;
                    Request.Description = Description;
                    Request.ReqDate = DateTime.Now;
                    Request.Remark = Remark;
                    Request.Remark_For_Admin = Remark_For_Admin;
                    db.SaveChanges();
                    TempData["Result"] = "Request Updated Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Request Updated Successfully !", Refresh = "Default" });
                }
            }
            else if (Cmd == "Delete")
            {
                Request_Master p = db.Request_Master.Where(x => x.Req_Id == Req_Id).FirstOrDefault();
                var _del = db.Request_Master.Find(Req_Id);
                if (p.Status == 1)
                {
                    TempData["failure"] = "Request Approved Not Changed!";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Request Approved Not Changed!" });
                }
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
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Request Deleted Successfully !", Refresh = "Default" });
            }
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !", Refresh = "Default" });
            }

        }
        public ActionResult GetReq(long Req_No)
        {
            //Request_Master request = db.Request_Master.Where(x => x.Req_Id == Req_No).FirstOrDefault();
            //if (request != null)
            //{
            //    return Json(new { success = true, Place = request.Place, Type = request.Parent_Type, Project = request.Project_Id, Granted_Amount = request.Granted_Amount });
            //}
            //else
            //{
            //    return Json(new { success = false });
            //}


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
        public ActionResult GetProject()
        {
            long rid = new long();
            List<long> project_Id = new List<long>();
            List<string> project_Name = new List<string>();
            List<Project_Master> projects = db.Project_Master.Where(x => x.Company_ID == _LogedUser.Company_ID).ToList();
            int cnt = projects.Count();

            ///GetReq 
            List<long> req_Id = new List<long>();
            List<decimal> req_Amt = new List<decimal>();
            List<string> req_date = new List<string>();
            List<Voucher_Master> voucher = db.Voucher_Master.Where(x => x.User_Id == _LogedUser.User_ID && x.Status != 6 && x.UserMaster.Company_ID == _LogedUser.Company_ID).ToList();
            List<Request_Master> requests = db.Request_Master.Where(x => x.User_Id == _LogedUser.User_ID && x.Status == 1 && x.UserMaster.Company_ID == _LogedUser.Company_ID).ToList();
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

            if (requests != null)
            {
                return Json(new { success = true, Project_Id = project_Id, Project_Name = project_Name, resid = rid, Cnt = cnt, Req_Id = req_Id, Req_Amt = req_Amt, Cntreq = cntreq });
            }
            else
            {
                return Json(new { success = false });
            }
        }
        public ActionResult VoucherReq_List(int status = 101)
        {
            ViewBag.PageTitle = "All Voucher Request";

            if (status == 101)
            {
                List<Voucher_Master> voucher = db.Voucher_Master.Where(x => x.Status != 6 & x.User_Id == _LogedUser.User_ID).OrderByDescending(x => x.Voucher_Id).ToList();
                ViewBag.voucher = voucher;
            }
            if (status == 0)
            {
                List<Voucher_Master> voucher = db.Voucher_Master.Where(x => x.Status == 0 & x.User_Id == _LogedUser.User_ID).OrderByDescending(x => x.Status).ThenBy(x => x.Voucher_Id).ToList();
                ViewBag.voucher = voucher;
            }

            return View();
        }
        public ActionResult VoucherReq_Add(long Voucher_Id)
        {
            ViewBag.PageTitle = "Add Voucher";

            List<Request_Master> req = db.Request_Master.Where(x => x.Status != 6 & x.User_Id == _LogedUser.User_ID).OrderByDescending(x => x.ReqDate).ToList();
            List<Project_Master> projects = db.Project_Master.Where(x => x.Company_ID == _LogedUser.Company_ID).ToList();
            List<UserMaster> user = db.UserMasters.Where(x => x.User_type != _LogedUser.User_type && x.Company_ID == _LogedUser.Company_ID).ToList();
            List<Voucher_Master> voucher = db.Voucher_Master.Where(x => x.Status != 6 && x.User_Id == _LogedUser.User_ID).OrderByDescending(x => x.Voucher_Id).ToList();
            Voucher_Master v = db.Voucher_Master.Where(x => x.Voucher_Id == Voucher_Id).FirstOrDefault();
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

            long Voucher_Id = long.Parse(form["Voucher_Id"].ToString());
            string Description = form["Description"].ToString();
            string Place = form["Place"].ToString();
            string Remark = form["Remark"].ToString();
            string Remark_By_Admin = form["RemarkByAdmin"].ToString();
            decimal Petrol = decimal.Parse(form["Petrol"].ToString());
            decimal Total_Amount = decimal.Parse(form["Total_Amount"].ToString());
            decimal Payable_Amount = decimal.Parse(form["Payable_Amount"].ToString());
            decimal Deduction_Amount = decimal.Parse(form["Deduction_Amount"].ToString());
            decimal Travelling = decimal.Parse(form["Travelling"].ToString());
            decimal Mobile = decimal.Parse(form["Mobile"].ToString());
            decimal Conveyance = decimal.Parse(form["Conveyance"].ToString());
            string Parent_Type = form["Parent_Type"].ToString();
            int Trans_Type = int.Parse(form["Trans_Type"].ToString());
            DateTime v_date = DateTime.Parse(form["Voucher_Date"].ToString());
            string Cmd = form["Command"].ToString();
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
                        voucher.Project_Id = Project_Id;
                    }
                    voucher.User_Id = _LogedUser.User_ID;
                    voucher.Voucher_Date = v_date;
                    voucher.Description = Description;
                    voucher.Place = Place;
                    voucher.Remark = Remark;
                    voucher.Remark_For_Administrator = Remark_By_Admin;
                    voucher.Parent_Type = Parent_Type;
                    voucher.Trans_Type = Trans_Type;
                    voucher.Petrol = Petrol;
                    voucher.Mobile = Mobile;
                    voucher.Status = 0;
                    voucher.Travelling = Travelling;
                    voucher.Conveyance = Conveyance;
                    voucher.Total_Amount = voucher.Petrol + voucher.Mobile + voucher.Travelling + voucher.Conveyance;
                    voucher.Payable_Amount = Payable_Amount;
                    voucher.Deduction_Amount = Deduction_Amount;
                    if (file != null)
                    {
                        var allowedExtensions = new[] { ".Jpg", ".JPG", ".png", ".jpg", ".jpeg", ".pdf", ".docs" };
                        var fileName = Path.GetFileName(file.FileName);
                        var ext = Path.GetExtension(file.FileName);
                        if (!allowedExtensions.Contains(ext))
                        {
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Sorry Invalid Files." });
                        }
                        string name = Path.GetFileNameWithoutExtension(fileName);
                        string myfile = name + "_" + DateTime.Now.Second.ToString() + ext;
                        var path = Path.Combine(Server.MapPath("~/Uploads/User_Image"), myfile);
                        file.SaveAs(path);
                        voucher.Image_Name = myfile;

                    }
                    db.Voucher_Master.Add(voucher);
                    db.SaveChanges();

                    TempData["Result"] = "New Voucher Added Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "New Voucher Added Successfully !", Refresh = "VoucherReq_List" });
                }
                else
                {
                    Voucher_Master voucher = db.Voucher_Master.Where(x => x.Voucher_Id == Voucher_Id).FirstOrDefault();
                    if (voucher.Status == 1)
                    {
                        TempData["failure"] = "voucher Approved Not Changed!";
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "voucher Approved Not Changed!" });
                    }
                    if (voucher == null)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Voucher  !" });
                    }
                    if (voucher.Status == 1)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Voucher Cant't Update  !" });
                    }
                    if (form["Req_Id"] != null)
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

                    voucher.Description = Description;
                    voucher.Place = Place;
                    voucher.Remark = Remark;
                    voucher.Voucher_Date = v_date;
                    voucher.Remark_For_Administrator = Remark_By_Admin;
                    voucher.Parent_Type = Parent_Type;
                    voucher.Trans_Type = Trans_Type;
                    voucher.Petrol = Petrol;
                    voucher.Mobile = Mobile;
                    voucher.Travelling = Travelling;
                    voucher.Conveyance = Conveyance;
                    voucher.Total_Amount = voucher.Petrol + voucher.Mobile + voucher.Travelling + voucher.Conveyance;
                    voucher.Payable_Amount = Payable_Amount;
                    voucher.Deduction_Amount = Deduction_Amount;
                    if (file != null)
                    {
                        var allowedExtensions = new[] { ".Jpg", ".JPG", ".png", ".jpg", ".jpeg", ".pdf", ".docs" };
                        var fileName = Path.GetFileName(file.FileName);
                        var ext = Path.GetExtension(file.FileName);
                        if (!allowedExtensions.Contains(ext))
                        {
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Sorry Invalid Files." });
                        }
                        string name = Path.GetFileNameWithoutExtension(fileName);
                        string myfile = name + "_" + DateTime.Now.Second.ToString() + ext;
                        var path = Path.Combine(Server.MapPath("~/Uploads/User_Image"), myfile);
                        file.SaveAs(path);
                        voucher.Image_Name = myfile;
                    }
                    db.SaveChanges();
                    TempData["Result"] = "Voucher Updated Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Voucher Updated Successfully !", Refresh = "VoucherReq_List" });
                }
            }
            else if (Cmd == "Delete")
            {
                Voucher_Master _v = db.Voucher_Master.Where(x => x.Voucher_Id == Voucher_Id).FirstOrDefault();
                if (_v.Status == 1)
                {
                    TempData["failure"] = "voucher Approved Not Changed!";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "voucher Approved Not Changed!" });
                }
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
            else
            {
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Command !" });
            }

        }

        //===========================================================================================================================================================
        //public ActionResult Add_Punch()
        //{
        //   

        //    List<User_Punch> panch_List = db.User_Punch.Where(x => x.User_Id == _LogedUser.User_ID).ToList();
        //    if (panch_List.Count == 0)
        //    {
        //        ViewBag.p = "New_Punch";
        //        return View();
        //    }
        //    var max = db.User_Punch.Where(x => x.User_Id == _LogedUser.User_ID).OrderByDescending(p => p.Punch_Id).FirstOrDefault().Punch_Id;
        //    User_Punch panch_detail = db.User_Punch.Where(x => x.Punch_Id == max).FirstOrDefault();
        //    if (panch_detail != null)
        //    {
        //        ViewBag.punch_detail = panch_detail;
        //        return View();
        //    }
        //    else
        //    {
        //        ViewBag.p = "New_Punch";
        //        return View();
        //    }

        //}

        public ActionResult Add_Employee_Task()
        {
            return View();
        }
        public ActionResult Add_task(string[] values)
        {

            var data = new object();
            int i = 0;
            DateTime dt = DateTime.Now;
            string st = dt.ToString("yyyy-MM-dd");
            if (values.Contains(""))
            {
                data = new { rescode = 1, resmsg = "Please Validate Your Input Task Details Can Not Be Blank Enter!" };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            else
            {
                for (i = 0; i < values.Length; i++)
                {
                    Employee_Task_Master etask = new Employee_Task_Master();
                    etask.User_id = _LogedUser.User_ID;
                    etask.Task_Detail = values[i];
                    etask.Task_Date = Convert.ToDateTime(st);
                    etask.Status = 0;
                    db.Employee_Task_Master.Add(etask);
                }
                db.SaveChanges();
                data = new { rescode = 0, resmsg = "Task Details Added Successfully !", date = st };
                return Json(data, JsonRequestBehavior.AllowGet);
            }


        }
        public ActionResult Employee_Task_List(string Taskdate = "")
        {

            List<Employee_Task_Master> em = new List<Employee_Task_Master>();
            string str = "";
            if (Taskdate == "")
            {

            }
            else
            {
                DateTime dt = Convert.ToDateTime(Taskdate);
                em = db.Employee_Task_Master.Where(x => x.Task_Date == dt && x.User_id == _LogedUser.User_ID).ToList();

            }
            ViewBag.em = em;
            ViewBag.taskdate = Taskdate;
            return View();
        }
        public ActionResult Get_Employee_Task_Details(FormCollection form)
        {
            DateTime dt = DateTime.Parse(form["Task_date"].ToString());
            string st = dt.ToString("yyyy-MM-dd");
            return RedirectToAction("Employee_Task_List", new { Taskdate = st });
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
                TempData["Result"] = "Task Deleted Successfully..!";
            }

            return RedirectToAction("Employee_Task_List", new { Taskdate = st });
        }

        public ActionResult Reporting_Employee_Task_List(string Taskdate = "", long userid = 0)
        {

            List<Employee_Task_Master> em = new List<Employee_Task_Master>();
            string str = "";
            if (Taskdate == "")
            {

            }
            else if (userid == 0)
            {

            }
            else if (userid == -1)
            {
                DateTime dt = Convert.ToDateTime(Taskdate);
                em = db.Employee_Task_Master.Where(x => x.Task_Date == dt && x.Status == 0 && x.UserMaster.Company_Master.Company_ID == _LogedUser.Company_ID).ToList();
                str = "all";
            }
            else
            {
                str = "one";
                DateTime dt = Convert.ToDateTime(Taskdate);
                string st = dt.ToString("dd-MM-yyyy");
                //List<Employee_Task_Master> edetails = db.Employee_Task_Master.Where(x => x.User_id == userid && x.UserMaster.status == 0).ToList();
                //foreach (Employee_Task_Master e in edetails)
                //{
                //    DateTime dt1 = Convert.ToDateTime(e.Task_Date);
                //    string st1 = dt1.ToString("dd-MM-yyyy");
                //    if (st == st1)
                //    {
                //        em.Add(e);
                //    }
                //}
                em = db.Employee_Task_Master.Where(x => x.Task_Date == dt && x.User_id == userid && x.Status == 0).ToList();
            }
            List<UserMaster> um = db.UserMasters.Where(x => x.Reporting_Person == _LogedUser.User_ID && x.status == 0).ToList();
            ViewBag.um = um;
            ViewBag.em = em;
            ViewBag.taskdate = Taskdate;
            ViewBag.str = str;
            ViewBag.userid = userid;
            return View();
        }
        public ActionResult Get_Employee_Task_Details_reporting(FormCollection form)
        {
            DateTime dt = DateTime.Parse(form["Task_date"].ToString());
            string st = dt.ToString("yyyy-MM-dd");
            long user_id = long.Parse(form["user_id"].ToString());
            return RedirectToAction("Reporting_Employee_Task_List", new { Taskdate = st, userid = user_id });
        }
        public ActionResult InquiryListDetails()
        {
            ViewBag.PageTitle = "inquiry details";
            List<Inquiry_Master> inquirydetails = db.Inquiry_Master.Where(x => x.Company_ID == _LogedUser.Company_ID && x.Assign_To == _LogedUser.User_ID).ToList();
            ViewBag.InquiryDetails = inquirydetails;
            return View();
        }
        public JsonResult getEmployeeToAssighn()
        {

            List<UserMaster> employeInquiery = db.UserMasters.Where(x => x.Company_ID == _LogedUser.Company_ID && x.status == 0).ToList();
            return Json(from obj in employeInquiery.ToList() select new { User_Name = obj.User_Name, User_ID = obj.User_ID }, JsonRequestBehavior.AllowGet);
        }
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
                    TempData["Result"] = "inquiry Updated Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Inquiry Updated Successfully !", Refresh = "Default" });
                }
            }
            return View();
        }
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
        public ActionResult followup_details()
        {
            ViewBag.PageTitle = "Followup details";

            List<Inquiry_Master> inquiryfollow = db.Inquiry_Master.Where(x => x.Assign_To == _LogedUser.User_ID || x.assighnNextID == _LogedUser.User_ID).ToList();
            List<FollowUp_Master> asigninquiry = db.FollowUp_Master.Where(x => x.Added_by == _LogedUser.User_ID).ToList();
            List<FollowUp_Master> checkAssighn = db.FollowUp_Master.Where(x => x.Added_by == _LogedUser.User_ID && x.User_ID != null).ToList();
            List<FollowUp_Master> sharedFollowupList = db.FollowUp_Master.Where(x => x.User_ID == _LogedUser.User_ID).ToList();

            ViewBag.Sharedfollow = sharedFollowupList;
            ViewBag.chkassighn = checkAssighn.Count();
            ViewBag.inquieryfollowup = inquiryfollow;
            ViewBag.inqAssign = asigninquiry;
            return View();
        }

        public JsonResult viewemployeeAssighn(long id)
        {


            List<FollowUp_Master> asigninquiry = db.FollowUp_Master.Where(x => x.Added_by == _LogedUser.User_ID && x.Inquiry_ID == id).ToList();

            return Json(from obj in asigninquiry select new { User_Name = obj.UserMaster.User_Name, Inquiry_Name = obj.Inquiry_Master.Inquiry_Name, Description = obj.Description, Next_followed_Date = obj.Next_followed_Date.ToString("dd MMMM yyyy"), Status = obj.Status }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult viewSharedemployeeAssighn()
        {


            List<FollowUp_Master> asigninquiry = db.FollowUp_Master.Where(x => x.User_ID == _LogedUser.User_ID).ToList();

            return Json(from obj in asigninquiry select new { User_Name = obj.UserMaster.User_Name, Inquiry_Name = obj.Inquiry_Master.Inquiry_Name, Description = obj.Description, Next_followed_Date = obj.Next_followed_Date.ToString("dd MMMM yyyy"), Status = obj.Status }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ViewAssighnFullDetails(long Inquiry_Id)
        {

            // List<FollowUp_Master> asigninquiry = db.FollowUp_Master.Where(x=>x.Inquiry_ID == Inquiry_Id ).ToList();
            List<Inquiry_Master> inquirydetails = db.Inquiry_Master.Where(x => x.Inquiry_Id == Inquiry_Id).ToList();

            List<FollowUp_Master> followinquiryDetails = db.FollowUp_Master.Where(x => x.Inquiry_ID == Inquiry_Id && x.User_ID != null).ToList();
            List<FollowUp_Master> task = db.FollowUp_Master.Where(x => x.Inquiry_ID == Inquiry_Id && x.User_ID == null).ToList();
            List<Inquiry_Master> firstinquiery = db.Inquiry_Master.Where(x => x.Inquiry_Id == Inquiry_Id).ToList();
            var addedid = firstinquiery.Where(x => x.Inquiry_Id == Inquiry_Id).Select(x => x.Assign_To).SingleOrDefault();
            var addedby = firstinquiery.Where(x => x.Inquiry_Id == Inquiry_Id).Select(x => x.AddedBy).SingleOrDefault();
            List<FollowUp_Master> usertask = db.FollowUp_Master.Where(x => x.Inquiry_ID == Inquiry_Id && x.User_ID == null && x.Added_by == addedby.Value).ToList();
            List<FollowUp_Master> firstemployetask = db.FollowUp_Master.Where(x => x.Added_by == _LogedUser.User_ID && x.User_ID == null).ToList();
            List<UserMaster> uid = db.UserMasters.Where(x => x.User_ID == addedid.Value).ToList();
            List<UserMaster> addedbyuser = db.UserMasters.Where(x => x.User_ID == addedby.Value).ToList();
            //  List<FollowUp_Master> firstempAddedby = db.FollowUp_Master.Where(x => x.Inquiry_ID == Inquiry_Id && x.User_ID==null).ToList();
            // var firstaddedbyid = firstempAddedby.Where(x => x.Inquiry_ID == Inquiry_Id).Select(X => X.Added_by).SingleOrDefault();
            //List<UserMaster> firstempuser=db.UserMasters.Where(x=>x.User_ID== firstaddedbyid.Value).ToList();
            //  ViewBag.firstemp = firstempuser;
            List<long> addedbyuserid = new List<long>();
            int count = 0;
            foreach (var user in followinquiryDetails)
            {

                var adduseridadded = followinquiryDetails.Where(x => x.Inquiry_ID == Inquiry_Id).Select(x => x.Added_by).ToList();
                addedbyuserid.Add(adduseridadded[count].Value);
                count = count + 1;

            }
            List<string> unamelist = new List<string>();
            for (int i = 0; i < followinquiryDetails.Count(); i++)
            {
                long iddd = addedbyuserid[i];
                string USERASSIGHNNAME = db.UserMasters.Where(X => X.User_ID == iddd).Select(x => x.User_Name).SingleOrDefault();
                unamelist.Add(USERASSIGHNNAME.ToString());
            }
            ViewBag.unamelist = unamelist;
            ViewBag.username = uid;
            ViewBag.addedbyuser = addedbyuser;
            ViewBag.USERTASK = usertask;
            ViewBag.firstemptask = firstemployetask;
            ViewBag.task = task;
            ViewBag.followdet = followinquiryDetails;
            ViewBag.inquirydet = inquirydetails;
            // ViewBag.Asigninquiry = asigninquiry;
            return View();
        }
        [HttpPost]
        public ActionResult Followup_add(FormCollection form)
        {

            long inquiery_ID = long.Parse(form["Inquiry_Id"]);

            DateTime nextfollowdate = DateTime.Parse(form["Next_followed_Date"].ToString());
            string longitude = form["log"].ToString();
            string latitude = form["lat"].ToString();
            string loc = form["loc"].ToString();
            string descrip = form["Description"].ToString();
            GeoCoordinateWatcher watcher = new GeoCoordinateWatcher();

            watcher.PositionChanged += (sender, e) =>
            {
                var coordinate = e.Position.Location;
                Console.WriteLine("Lat: {0}, Long: {1}", coordinate.Latitude,
                    coordinate.Longitude);
                // Uncomment to get only one event.
                // watcher.Stop(); 
            };

            // Begin listening for location updates.
            watcher.Start();
            // long assighnto = long.Parse(form["Emp_Id"]);
            //  string statuschk = form["Status"].ToString();
            int Status = 0;
            string Cmd = form["Command"].ToString();
            if (Cmd == "Save")
            {

                FollowUp_Master t = db.FollowUp_Master.Where(x => x.Inquiry_ID == inquiery_ID).FirstOrDefault();
                //if (t != null)
                //{
                //    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Follow up already Exist !" });
                //}
                FollowUp_Master followup = new FollowUp_Master();


                // followup.Company_ID = _LogedUser.Company_ID;
                followup.Inquiry_ID = inquiery_ID;
                // followup.User_ID = assighnto;
                followup.Description = descrip;
                followup.Next_followed_Date = nextfollowdate;
                followup.Status = Status;
                followup.Longitutde = longitude;
                followup.Latitude = latitude;
                followup.Location = loc;
                followup.Added_by = _LogedUser.User_ID;
                followup.Added_on = DateTime.Now;
                db.FollowUp_Master.Add(followup);
                db.SaveChanges();
                TempData["Result"] = "New follow up Added Successfully !";
                return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "New follow Up Added Successfully !", Refresh = "Default" });
            }


            return View();
        }

        public ActionResult AssignFollowUpdate(FormCollection form)
        {


            long inquiery_ID = long.Parse(form["inqAssighn"]);
            long assighnto = long.Parse(form["usernametake"]);
            string longitude = form["logg"].ToString();
            string latitude = form["latt"].ToString();
            string loc = form["locc"].ToString();


            int Status = 0;
            string Cmd = form["Command"].ToString();
            if (Cmd == "Save")
            {
                FollowUp_Master chkassighn = db.FollowUp_Master.Where(x => x.User_ID == assighnto && x.Inquiry_ID == inquiery_ID).FirstOrDefault();
                if (chkassighn == null)
                {
                    FollowUp_Master t = db.FollowUp_Master.Where(x => x.Inquiry_ID == inquiery_ID).FirstOrDefault();
                    //if (t != null)
                    //{
                    //    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Follow up already Exist !" });
                    //}
                    FollowUp_Master followup = new FollowUp_Master();
                    Inquiry_Master inq = db.Inquiry_Master.Where(x => x.Inquiry_Id == inquiery_ID).First();
                    inq.assighnNextID = assighnto;
                    inq.Assign_To = assighnto;
                    db.SaveChanges();

                    followup.Inquiry_ID = inquiery_ID;
                    followup.User_ID = assighnto;

                    followup.Status = Status;
                    followup.Longitutde = longitude;
                    followup.Latitude = latitude;
                    followup.Location = loc;
                    followup.Added_by = _LogedUser.User_ID;
                    followup.Added_on = DateTime.Now;
                    db.FollowUp_Master.Add(followup);
                    db.SaveChanges();
                    TempData["Result"] = " follow up Added Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = " Assighn Added Successfully !", Refresh = "Default" });
                }
                else
                {

                    FollowUp_Master p = db.FollowUp_Master.Where(x => x.Inquiry_ID == inquiery_ID && x.User_ID == assighnto).FirstOrDefault();
                    if (p == null)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Invalid Assighn  !" });
                    }
                    Inquiry_Master inq = db.Inquiry_Master.Where(x => x.Inquiry_Id == inquiery_ID).First();
                    inq.assighnNextID = assighnto;
                    inq.Assign_To = assighnto;
                    db.SaveChanges();

                    p.Added_by = _LogedUser.User_ID;
                    p.Added_on = DateTime.Now;


                    p.User_ID = assighnto;
                    db.SaveChanges();
                    TempData["Result"] = "Assighn Updated Successfully !";
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "Assighn Updated Successfully !", Refresh = "Default" });
                }
            }
            return View();
        }
        public JsonResult getEmployeeToAssighnUSER()
        {

            List<UserMaster> employeInquiery = db.UserMasters.Where(x => x.Company_ID == _LogedUser.Company_ID && x.status == 0).ToList();
            return Json(from obj in employeInquiery.ToList() select new { User_Name = obj.User_Name, User_ID = obj.User_ID }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult getuserfollowuptask(long id, long inquiryid)
        {

            List<FollowUp_Master> usertaskdone = db.FollowUp_Master.Where(x => x.Inquiry_ID == inquiryid && x.Added_by == id && x.User_ID == null).ToList();
            return Json(from obj in usertaskdone select new { Description = obj.Description, Next_followed_Date = obj.Next_followed_Date.ToString("dd MMMM yyyy"), Added_on = obj.Added_on.ToString("dd MMMM yyyy") }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult CustomDocument_List()
        {

            ViewBag.PageTitle = "All CustomDocuments";
            List<Generated_Documents> c = db.Generated_Documents.Where(x => x.Gen_doc_empid == _LogedUser.User_ID).OrderByDescending(x => x.Gen_doc_id).ToList();
            ViewBag.c = c;
            return View();
        }
        public ActionResult PrintEmpDocument(long Gen_doc_id)
        {
            ViewBag.PageTitle = "Print Documents";

            Generated_Documents gendocs = db.Generated_Documents.Where(x => x.Gen_doc_id == Gen_doc_id && x.UserMaster.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
            ViewBag.gendocs = gendocs;
            return View();
        }
        [HttpGet]
        public ActionResult SalarySlip(int month = 99, int year = 99)
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


            ViewBag.Project_Salaries = db.ProjectSalaries.Where(x => x.UserMaster.Project_SalaryGroupID == _LogedUser.Project_SalaryGroupID && x.SalaryMonth.Month == month && x.SalaryMonth.Year == year && x.User_ID==_LogedUser.User_ID).ToList();

            //ViewBag.Project_Salaries = db.ProjectSalaries.ToList();
            ViewBag.month = month;
            ViewBag.year = year;
            ViewBag.Project_SalaryGroupID = _LogedUser.Project_SalaryGroupID;
            return View();
        }
        [HttpPost]
        public ActionResult DownloadSalarySample(FormCollection form)
        {
            int month = int.Parse(form["month"].ToString());
            int year = int.Parse(form["year"].ToString());
           
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
                return RedirectToAction("SalarySlip", new { month = month, year = year, Project_SalaryGroupID = _LogedUser.Project_SalaryGroupID });
            }
        
            else
            {
                
                return RedirectToAction("SalarySlip", new { month = month, year = year, Project_SalaryGroupID = _LogedUser.Project_SalaryGroupID });
            }
        }

    }
}