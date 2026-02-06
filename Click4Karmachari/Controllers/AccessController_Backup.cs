using ClickKarmachari;
using ClickKarmachari.Models;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
namespace ClickKarmachari.Controllers
{
    [SessionAdminAuthorizeAttribute]
    public class AccessController : Controller
    {
        private acco_101_kjattenEntities db = new acco_101_kjattenEntities();
        CommonClasses _comm = new CommonClasses();

        public class CaptchaResponse
        {
            [JsonProperty("Success")]
            public bool Success { get; set; }

            [JsonProperty("error-codes")]
            public bool ErrorMessage { get; set; }
        }
        public ActionResult Sessionresest(int id)
        {
            UserMaster _user = db.UserMasters.Where(x => x.User_ID == id).FirstOrDefault();

            Session.Timeout = Session.Timeout + 10;
            Session.Add("LOG_STATUS", "TRUE");
            Session.Add("LOGIN_USER", _user);
            Session.Add("LOGIN_NAME", _user.User_Name.ToString());
            Session.Add("LOGIN_ID", _user.User_ID.ToString());
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public ActionResult Dashboard()
        {

            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
            DateTime dt = DateTime.Now.Date;

            int dept = db.Department_Master.Where(x => x.Company_ID == _LogedUser.Company_ID).Count();
            ViewBag.dept = dept;

            //List<Department_Master> dept = db.Department_Master.Where(x => x.Company_ID == _LogedUser.Company_ID).ToList();
            //ViewBag.dept = dept.Count();

            List<User_Punch> _todayspunch = db.User_Punch.Where(x => x.Time.Day == dt.Day && x.UserMaster.Company_ID == _LogedUser.Company_ID && x.Time.Month == dt.Month && x.Time.Year == dt.Year && x.PunchType_Id == 1 && x.UserMaster.status == 0).ToList();
            List<User_Punch> punch = db.User_Punch.Where(x => x.Time.Day == dt.Day && x.UserMaster.Company_ID == _LogedUser.Company_ID && x.Time.Month == dt.Month && x.Time.Year == dt.Year && x.PunchType_Id == 2).ToList();
            ViewBag._todayspunch = _todayspunch;
            ViewBag.punch = punch;
            List<Leave_Master> _pendingLeave = db.Leave_Master.Where(x => x.Status == 0 & x.UserMaster.Company_ID == _LogedUser.Company_ID).Take(9).ToList();
            ViewBag._pendingLeave = _pendingLeave;
            List<Voucher_Master> _pendingVoucher = db.Voucher_Master.Where(x => x.Status != 6 & x.UserMaster.Company_ID == _LogedUser.Company_ID).ToList();
            List<Request_Master> _pendingRequest = db.Request_Master.Where(x => x.Status != 6 & x.UserMaster.Company_ID == _LogedUser.Company_ID).ToList();
            List<Project_Master> Projects = db.Project_Master.Where(x => x.Company_ID == _LogedUser.Company_ID).ToList();
            List<UserMaster> Emp = db.UserMasters.Where(x => x.Company_ID == _LogedUser.Company_ID & x.User_type != _LogedUser.User_type && (x.status == 0 || x.status == 1)).ToList();
            List<UserMaster> AllEmp = db.UserMasters.Where(x => x.Company_ID == _LogedUser.Company_ID & x.User_type != _LogedUser.User_type && (x.status == 0)).OrderByDescending(x => x.User_ID).ToList();
            List<UserMaster> Trainee = db.UserMasters.Where(x => x.Company_ID == _LogedUser.Company_ID & x.User_type == 5 && x.status == 0).OrderByDescending(x => x.User_ID).ToList();
            List<UserMaster> rEmp = db.UserMasters.Where(x => x.Company_ID == _LogedUser.Company_ID & x.User_type != _LogedUser.User_type & x.User_type != 5 & x.status == 0).OrderByDescending(x => x.User_ID).ToList();
            ViewBag.Emp = Emp.Count();
            ViewBag.AllEmp = AllEmp;
            ViewBag.Trainee = Trainee;
            ViewBag.rEmp = rEmp;
            ViewBag.Projects = Projects.Count();
            List<Project_Task_Master> Runproject = db.Project_Task_Master.Where(x => x.Status != 6 && x.UserMaster.Company_ID == _LogedUser.Company_ID).ToList();
            List<Project_Task_Master> NewProject = db.Project_Task_Master.Where(x => (x.Status == 0 || x.Status == 1) && x.UserMaster.Company_ID == _LogedUser.Company_ID).ToList();
            ViewBag.Runproject = Runproject.Count();
            ViewBag.NewProject = NewProject.Count();
            ViewBag._pendingRequest = _pendingRequest.Count();
            ViewBag._pendingVoucher = _pendingVoucher.Count();
            /////Assset Count
            List<Asset_Movement> movment = db.Asset_Movement.Where(x => x.From_id == _LogedUser.User_ID || x.To_id == _LogedUser.User_ID).ToList();
            List<Asset_Master> assts = db.Asset_Master.Where(x => x.Company_ID == _LogedUser.Company_ID).ToList();
            List<AssetListIO> _myAssets = new List<AssetListIO>();
            foreach (Asset_Master _my in assts)
            {
                movment = db.Asset_Movement.Where(y => y.Asset_id == _my.Asset_id).OrderByDescending(y => y.Movement_id).ToList();
                if (movment.Count != 0)
                {
                    //if (movment.FirstOrDefault().To_id == _LogedUser.User_ID)
                    //{
                    _myAssets.Add(new AssetListIO(_my));
                    //}
                }
            }
            ViewBag.Asset = assts.Count;
            //event and Notice Board

            List<Event_Master> Allevent = db.Event_Master.Where(x => x.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.Date).ToList();
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

            List<NoticeBoard_Master> Allnotice = db.NoticeBoard_Master.Where(x => x.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.Date).ToList();
            ViewBag.Allnotice = Allnotice;
            //calculate current week date

            DayOfWeek currentDay = DateTime.Now.DayOfWeek;
            int daysTillCurrentDay = currentDay - DayOfWeek.Monday;
            DateTime currentWeekStartDate = DateTime.Now.AddDays(-daysTillCurrentDay);
            List<DateTime> wekdate = new List<DateTime>();
            //previus week date
            DateTime lastWeekStartDate = DateTime.Now.AddDays(-daysTillCurrentDay - 7);
            DateTime tuesdaydata = lastWeekStartDate.AddDays(1);
            DateTime wednesdaydata = tuesdaydata.AddDays(1);
            DateTime thursdaydata = wednesdaydata.AddDays(1);
            DateTime fridaydata = thursdaydata.AddDays(1);
            DateTime saturdaydata = fridaydata.AddDays(1);
            List<UserMaster> allemployeget = db.UserMasters.Where(x => x.Company_ID == _LogedUser.Company_ID & x.status == 0).ToList();

            // var indexOflastWeekStartDate = calendar.FindIndex(p => p.CalendarDate == lastWeekStartDate.Date);
            // var lasttWeekDates = calendar.GetRange(indexOflastWeekStartDate, 7).Select(p => new { p.CalendarDate }).ToList();

            // DateTime lastweek=currentWeekStartDate.AddDays()
            int punchmonthdate = DateTime.Now.Month;
            int punchyear = DateTime.Now.Year;
            List<User_Punch> upunch = db.User_Punch.Where(x => x.PunchType_Id == 1 & x.Time.Month.Equals(punchmonthdate) & x.Time.Year == punchyear).ToList();
            // List<User_Punch> testactive = db.User_Punch.Where(x => x.PunchType_Id == 1 & x.Time.Month.Equals(punchmonthdate) & x.Time.Year == punchyear).ToList();
            //present week data
            List<User_Punch> allpunch = db.User_Punch.Where(x => x.PunchType_Id == 1 & x.Time.Day == lastWeekStartDate.Day & x.Time.Month == lastWeekStartDate.Month & x.Time.Year == punchyear).ToList();
            List<User_Punch> tuesdaypunch = db.User_Punch.Where(x => x.PunchType_Id == 1 & x.Time.Day == tuesdaydata.Day & x.Time.Month == tuesdaydata.Month & x.Time.Year == punchyear).ToList();
            List<User_Punch> wedpunch = db.User_Punch.Where(x => x.PunchType_Id == 1 & x.Time.Day == wednesdaydata.Day & x.Time.Month == wednesdaydata.Month & x.Time.Year == punchyear).ToList();
            List<User_Punch> thursdaypunch = db.User_Punch.Where(x => x.PunchType_Id == 1 & x.Time.Day == thursdaydata.Day & x.Time.Month == thursdaydata.Month & x.Time.Year == punchyear).ToList();
            List<User_Punch> fridaypunch = db.User_Punch.Where(x => x.PunchType_Id == 1 & x.Time.Day == fridaydata.Day & x.Time.Month == fridaydata.Month & x.Time.Year == punchyear).ToList();
            List<User_Punch> saturdaypunch = db.User_Punch.Where(x => x.PunchType_Id == 1 & x.Time.Day == saturdaydata.Day & x.Time.Month == saturdaydata.Month & x.Time.Year == punchyear).ToList();

            ViewBag.mondaypresent = allpunch.Count();
            ViewBag.tuesdaypresent = tuesdaypunch.Count();
            ViewBag.wedpresent = wedpunch.Count();
            ViewBag.thurpresent = thursdaypunch.Count();
            ViewBag.fridaypresent = fridaypunch.Count();
            ViewBag.satPresent = saturdaypunch.Count();
            //absent
            ViewBag.monabsent = allemployeget.Count() - allpunch.Count();
            ViewBag.tueabsent = allemployeget.Count() - tuesdaypunch.Count();
            ViewBag.wedabsent = allemployeget.Count() - wedpunch.Count();
            ViewBag.thurabsent = allemployeget.Count() - thursdaypunch.Count();
            ViewBag.friabsent = allemployeget.Count() - fridaypunch.Count();
            ViewBag.satabsent = allemployeget.Count() - saturdaypunch.Count();


            ViewBag.AttendedDays = upunch.Count();

            //event and Notice Board

            ////try to  fill Chart
            DateTime baseDate = DateTime.Today;
            var thisWeekStart = baseDate.AddDays(-(int)baseDate.DayOfWeek);
            var thisWeekEnd = thisWeekStart.AddDays(7).AddSeconds(-1);

            int Sunday = 0, Monday = 0, Tuesday = 0, Wednesday = 0, Thursday = 0, Friday = 0, Saturday = 0;
            List<Attendance_Master> attendances = db.Attendance_Master.Where(x =>  /*x.Punch_Date.Month == dt.Month &&*/ /*x.Punch_Date.Year == dt.Year &&*/ x.UserMaster.Company_ID == _LogedUser.Company_ID).ToList();
            foreach (Attendance_Master a in attendances)
            {
                if (a.Attendance == Convert.ToDecimal(0.5) || a.Attendance == Convert.ToDecimal(1.0))
                {
                    if (thisWeekEnd >= a.Punch_Date && thisWeekStart <= a.Punch_Date)
                    {
                        if (a.Punch_Date.DayOfWeek == DayOfWeek.Sunday)
                        {
                            Sunday++;
                        }
                        if (a.Punch_Date.DayOfWeek == DayOfWeek.Monday)
                        {
                            Monday++;
                        }
                        if (a.Punch_Date.DayOfWeek == DayOfWeek.Tuesday)
                        {
                            Tuesday++;
                        }
                        if (a.Punch_Date.DayOfWeek == DayOfWeek.Wednesday)
                        {
                            Wednesday++;
                        }
                        if (a.Punch_Date.DayOfWeek == DayOfWeek.Thursday)
                        {
                            Thursday++;
                        }
                        if (a.Punch_Date.DayOfWeek == DayOfWeek.Friday)
                        {
                            Friday++;
                        }
                        if (a.Punch_Date.DayOfWeek == DayOfWeek.Saturday)
                        {
                            Saturday++;
                        }
                    }

                }
            }

            int jan = 0, feb = 0, mar = 0, april = 0, may = 0, jun = 0, july = 0, aug = 0, sep = 0, oct = 0, nov = 0, dec = 0;
            List<Project_Task_Master> pm = db.Project_Task_Master.Where(x => x.UserMaster.Company_ID == _LogedUser.Company_ID && x.UserMaster.status == 0 & x.Status != 6).ToList();
            foreach (Project_Task_Master pmst in pm)
            {
                DateTime dtd = Convert.ToDateTime(pmst.Actual_Start_Date);
                int month = dtd.Month;
                if (1 == month)
                {
                    jan++;
                }
                if (2 == month)
                {
                    feb++;
                }
                if (3 == month)
                {
                    mar++;
                }
                if (4 == month)
                {
                    april++;
                }
                if (5 == month)
                {
                    may++;
                }
                if (6 == month)
                {
                    jun++;
                }
                if (7 == month)
                {
                    july++;
                }
                if (8 == month)
                {
                    aug++;
                }
                if (9 == month)
                {
                    sep++;
                }
                if (10 == month)
                {
                    oct++;
                }
                if (11 == month)
                {
                    nov++;
                }
                if (12 == month)
                {
                    dec++;
                }
            }
            ViewBag.Sunday = Sunday;
            ViewBag.Monday = Monday;
            ViewBag.Tuesday = Tuesday;
            ViewBag.Wednesday = Wednesday;
            ViewBag.Thursday = Thursday;
            ViewBag.Friday = Friday;
            ViewBag.Saturday = Saturday;
            ViewBag.jan = jan;
            ViewBag.feb = feb;
            ViewBag.mar = mar;
            ViewBag.april = april;
            ViewBag.may = may;
            ViewBag.jun = jun;
            ViewBag.july = july;
            ViewBag.aug = aug;
            ViewBag.sep = sep;
            ViewBag.oct = oct;
            ViewBag.nov = nov;
            ViewBag.dec = dec;
            //Get Project
            List<Project_Task_Master> cproject = db.Project_Task_Master.Where(x => x.Status == 4 || x.Status == 5 && x.Approved_Date.Value.Month == dt.Month && x.Approved_Date.Value.Year == dt.Year && x.UserMaster.Company_ID == _LogedUser.Company_ID).ToList();
            List<Project_Task_Master> reqproject = db.Project_Task_Master.Where(x => x.Status == 0 && x.Approved_Date.Value.Month == dt.Month && x.Approved_Date.Value.Year == dt.Year && x.UserMaster.Company_ID == _LogedUser.Company_ID).ToList();

            List<Project_Task_Master> comproject = db.Project_Task_Master.Where(x => x.Status == 9 && x.Approved_Date.Value.Month == dt.Month && x.Approved_Date.Value.Year == dt.Year && x.UserMaster.Company_ID == _LogedUser.Company_ID).ToList();
            ViewBag.comproject = comproject.Count();
            ViewBag.cproject = cproject.Count();
            ViewBag.reqproject = reqproject.Count();
            return View();
        }
        public ActionResult PrensentEmployees()
        {
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
            List<City_Master> City = db.City_Master.ToList();
            List<Religion_Master> religion = db.Religion_Master.ToList();
            ViewBag.City = City;
            ViewBag.religion = religion;
            return View();
        }
        [HttpPost]
        public ActionResult User_Profile(FormCollection form, HttpPostedFileBase file)
        {
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
            var _newuser = db.UserMasters.Find(_LogedUser.User_ID);
            string User_Name = form["User_Name"].ToString();
            long User_Mobile = long.Parse(form["User_Mobile"].ToString());
            long Religion_Id = long.Parse(form["Religion_Id"].ToString());
            string PanCard_No = form["PanCard_No"].ToString();
            string User_Email = form["User_Email"].ToString();
            long User_City = long.Parse(form["City"].ToString());
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
            _newuser.City_id = User_City;
            _newuser.Religion_Id = Religion_Id;
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
            Company_Master company = db.Company_Master.Where(x => x.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
            ViewBag.company = company;
            List<Company_Doc_Master> cmp = db.Company_Doc_Master.Where(x => x.Company_ID == _LogedUser.Company_ID & x.Status != 6).ToList();
            ViewBag.cmp = cmp;
            return View();
        }
        [HttpPost]
        public ActionResult Company_Profile(FormCollection form, HttpPostedFileBase file)
        {
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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

                var allowedExtensions = new[] { ".Jpg", ".JPG", ".png", ".jpg", ".jpeg" };
                var fileName = Path.GetFileName(file.FileName);
                var ext = Path.GetExtension(file.FileName);
                if (!allowedExtensions.Contains(ext))
                {
                    TempData["failure"] = "Sorry Invalid Files.";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Sorry Invalid Files.", Refresh = "Default" });
                }
                string name = Path.GetFileNameWithoutExtension(fileName);
                string myfile = name + "_" + DateTime.Now.Second.ToString() + ext;
                var path = Path.Combine(Server.MapPath("~/Uploads/User_Image"), myfile);
                file.SaveAs(path);
                cmp.Logo_URL = myfile;

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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
                        Doc.Doc_Type = myfile;
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
                        var allowedExtensions = new[] { ".Jpg", ".JPG", ".png", ".jpg", ".jpeg", ".pdf", ".PDF" };
                        var fileName = Path.GetFileName(file.FileName);
                        var ext = Path.GetExtension(file.FileName);
                        if (!allowedExtensions.Contains(ext))
                        {
                            TempData["failure"] = "Sorry, Invalid File ";
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Sorry, Invalid File " });
                        }
                        string name = Path.GetFileNameWithoutExtension(fileName);
                        string myfile = name + "_" + DateTime.Now.ToString("ddMMyyyhhmmss") + ext;
                        var path = Path.Combine(Server.MapPath("~/Uploads/User_Document"), myfile);
                        file.SaveAs(path);
                        Doc.Doc_Type = myfile;
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
            List<City_Master> City = db.City_Master.ToList();
            List<Religion_Master> religion = db.Religion_Master.ToList();
            ViewBag.City = City;
            ViewBag.religion = religion;
            return View();
        }
        [HttpPost]
        public ActionResult Change_Password(FormCollection form)
        {
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "New Password Not Match Confirm Password" });
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
        [HttpPost]
        public ActionResult Change_Emp_Pass(FormCollection form)
        {
            long Emp_Id = long.Parse(form["Emp_Id"].ToString());
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
        public ActionResult Employee_All(int Sts = 0)
        {
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
            ViewBag.PageTitle = "All Employees";
            ViewBag.PageDescription = "All Employees Available in the Company";
            List<UserType> U_Type = db.UserTypes.ToList();
            ViewBag.U_Type = U_Type;
            List<UserMaster> user = new List<UserMaster>();
            user = db.UserMasters.Where(x => x.User_type != _LogedUser.User_type && x.Company_ID == _LogedUser.Company_ID && x.status != 6).ToList();

            if (Sts == 99)
            {

            }
            else
            {
                user = user.Where(x => x.status == Sts).ToList();
            }
            ViewBag.user = user;

            if ((TempData["Result"] != null)) { if (TempData["Result"].ToString() != "") { ViewBag.Result = TempData["Result"]; TempData["Result"] = ""; } }
            if ((TempData["failure"] != null)) { if (TempData["failure"].ToString() != "") { ViewBag.failure = TempData["failure"]; TempData["failure"] = ""; } }
            ViewBag.Sts = Sts;
            return View();
        }


        public ActionResult Employee_Update(long User_id)
        {
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
            UserMaster _user = db.UserMasters.Where(x => x.User_ID == User_id).FirstOrDefault();

            List<UserMaster> Reporting_Person = db.UserMasters.Where(x => x.Company_ID == _LogedUser.Company_ID && x.status == 0).ToList();
            List<UserType> U_Type = db.UserTypes.ToList();
            List<Designation_Master> desg = db.Designation_Master.Where(x => x.Company_ID == _LogedUser.Company_ID).ToList();
            List<Department_Master> dept = db.Department_Master.Where(x => x.Company_ID == _LogedUser.Company_ID).ToList();
            List<City_Master> City = db.City_Master.OrderByDescending(x => x.Cityid).ToList();
            List<Religion_Master> religion = db.Religion_Master.OrderByDescending(x => x.Religion_Id).ToList();
            ViewBag.U_Type = U_Type;
            ViewBag.City = City;
            ViewBag._user = _user;
            ViewBag.Desg = desg;
            ViewBag.Dept = dept;
            ViewBag.religion = religion;
            ViewBag.user_detail = Reporting_Person;
            DateTime date = DateTime.Now;
            int Year = date.Year;
            ViewBag.Year = Year;
            List<Leave_Master> leaves = db.Leave_Master.Where(x => x.Status != 6 & x.Leave_Crdr == 2 & x.User_Id == User_id & x.UserMaster.Company_ID == _LogedUser.Company_ID & x.To_Date.Year == Year).OrderBy(x => x.Status).ThenByDescending(x => x.From_Date).ToList();
            ViewBag.leaves = leaves.Count;
            int Month = date.Month;
            ViewBag.Month = Month;
            List<Gift_Master> gift = db.Gift_Master.Where(x => x.User_Id == User_id & x.UserMaster.Company_ID == _LogedUser.Company_ID & x.Date.Year == Year).ToList();
            ViewBag.gift = gift.Count();
            //ViewBag.mm = date.ToString("MM-yyyy");
            //DayOfWeek Weekday = date.DayOfWeek;
            //int totalSunDays = GetSundays(Year, Month, Weekday);
            //ViewBag.TotalSundays = totalSunDays;

            DateTime today = DateTime.Today;
            DateTime endOfMonth = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
            //get only last day of month
            int day = endOfMonth.Day;

            DateTime now = DateTime.Now;
            int totalSunDays;
            totalSunDays = 0;
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

            //.....Count Total Holidays
            List<Holiday_Master> _holiday = db.Holiday_Master.Where(x => x.Company_Id == _LogedUser.Company_ID && x.Holiday_Date.Value.Month == date.Month && x.Holiday_Date.Value.Year == date.Year).ToList();
            int totalHolidays = _holiday.Count();
            int OffDays = totalSunDays + totalHolidays;
            ViewBag.TotalHoliday = totalHolidays;

            //.....Count Total Days
            int totalDays = getdays(Year, Month);


            //.....Count Total Present Days
            int PresentDays = totalDays - OffDays;
            ViewBag.WorkingDays = PresentDays;
            List<Attendance_Master> _atdn = db.Attendance_Master.Where(x => x.Punch_Date.Month == date.Month && x.Punch_Date.Year == date.Year).ToList();
            List<UserMaster> um = db.UserMasters.Where(x => x.User_type != _LogedUser.User_type && x.Company_ID == _LogedUser.Company_ID && x.status == 0).ToList();
            List<Leave_Master> leave = db.Leave_Master.Where(x => x.ReqDate.Month == date.Month && x.ReqDate.Year == date.Year && x.UserMaster.Company_ID == _LogedUser.Company_ID).ToList();
            ViewBag.user = um;
            ViewBag.attend = _atdn;
            ViewBag.leavelist = leave;
            ViewBag.totalDays = totalDays;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Employee_Add(FormCollection form, HttpPostedFileBase file)
        {
            try
            {
                UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
                    _newuser.status = 0;
                    db.UserMasters.Add(_newuser);
                    db.SaveChanges();

                    ///Email Send.....................//
                    string sub = "You are succesfully Registered in " + _LogedUser.Company_Master.Company_Name;
                    string message = "Please Login and Join click4karmachari";
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
                    long? Religion_Id = null; if (form["Religion_Id"] != "") { Religion_Id = long.Parse(form["Religion_Id"].ToString()); }
                    long? Department = null; if (form["Department"] != "") { Department = long.Parse(form["Department"].ToString()); }
                    long? Designation = null; if (form["Designation"] != "") { Designation = long.Parse(form["Designation"].ToString()); }
                    long? Cityid = null; if (form["Cityid"] != "") { Cityid = long.Parse(form["Cityid"].ToString()); }
                    long? RefMobile = null; if (form["RefMobile"] != "") { RefMobile = long.Parse(form["RefMobile"].ToString()); }
                    long? Adharcard_No = null; if (form["Adharcard_No"] != "") { Adharcard_No = long.Parse(form["Adharcard_No"].ToString()); }
                    long Mobile = long.Parse(form["Mobile"].ToString());
                    string User_Name = form["User_Name"].ToString();
                    string PanCard_No = form["PanCard_No"].ToString();
                    string Company_Email = form["Company_Email"].ToString();
                    string Blood_group = form["Blood_group"].ToString();
                    string Posted_At = form["Posted_At"].ToString();
                    string Team_name = form["Team_name"].ToString();
                    string Bank_Acc_No = form["Bank_Acc_No"].ToString();
                    string IFSC_No = form["IFSC_No"].ToString();
                    string Bank_Name = form["Bank_Name"].ToString();
                    long Reporting_Person = long.Parse(form["Reporting_Person"].ToString());
                    string EC_No = form["EC_No"].ToString();
                    string UAN_No = form["UAN_No"].ToString();
                    string ESIC_No = form["ESIC_No"].ToString();
                    string PF_No = form["PF_No"].ToString();
                    string User_Email = form["User_Email"].ToString();
                    int status = int.Parse(form["status"].ToString());
                    DateTime DOB = DateTime.Parse(form["DOB"].ToString());
                    DateTime Date_of_join = DateTime.Parse(form["Date_of_join"].ToString());

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
                    _updateuser.City_id = Cityid;
                    _updateuser.PanCard_No = PanCard_No;
                    _updateuser.User_Name = User_Name;
                    _updateuser.User_type = User_Type;
                    _updateuser.City_id = Cityid;
                    _updateuser.User_Mobile = Mobile;
                    _updateuser.Religion_Id = Religion_Id;
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
                    _updateuser.EC_No = EC_No;
                    _updateuser.UAN_No = UAN_No;
                    _updateuser.ESIC_No = ESIC_No;
                    _updateuser.PF_No = PF_No;
                    _updateuser.status = status;
                    db.SaveChanges();
                    return this.Json(new { EnableError = true, ErrorTitle = "S", ToastMsgSuc = "User Updated Successfully !", Refresh = "Employee_Update?User_id=" + _updateuser.User_ID });
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
            long Add_ID = long.Parse(form["Add_Address_Addid"].ToString());
            string Cmd = form["Command"].ToString();
            if (Cmd == "Save")
            {
                if (Add_ID == 0)
                {
                    long uid = long.Parse(form["Add_Address_Userid"].ToString());
                    long cityid = long.Parse(form["Add_Address_City"].ToString());
                    string address = form["Add_Address_address"].ToString();
                    Address_Master newrAdd = new Address_Master();
                    newrAdd.User_ID = uid;
                    newrAdd.Add_Address = address;
                    newrAdd.Cityid = cityid;
                    newrAdd.Status = 1;
                    db.Address_Master.Add(newrAdd);
                    db.SaveChanges();
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
                    long cityid = long.Parse(form["Add_Address_City"].ToString());
                    string address = form["Add_Address_address"].ToString();
                    newAdd.Add_Address = address;
                    newAdd.Cityid = cityid;
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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

            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
            long Doc_Id = long.Parse(form["Add_Document_Doc_Id"].ToString());
            long User_Id = long.Parse(form["Add_Document_User_ID"].ToString());
            string Doc_Name = form["Add_Document_Doc_Name"].ToString();
            string Cmd = form["Command"].ToString();
            if (Cmd == "Save")
            {
                if (Doc_Id == 0)
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
            ViewBag.PageTitle = "All AssetType";
            List<Asset_type> asset = db.Asset_type.Where(x => x.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.Type_id).ToList();
            ViewBag.asset = asset;
            return View();
        }
        [HttpPost]
        public ActionResult AssetType_Add(FormCollection form)
        {

            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
                if (_delt.UserMasters.Count > 0)
                {
                    TempData["failure"] = "Employee Type Has User Please Delete First...!";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Employee Type Has User Please Delete First...!" });
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
            if (_delt.UserMasters.Count > 0)
            {
                TempData["failure"] = "Employee Type Has User Please Delete First...!";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Employee Type Has User Please Delete First...!" });
            }
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
                if (_delt.UserMasters.Count > 0)
                {
                    TempData["failure"] = "City Has User Please Delete First...!";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "City Has User Please Delete First...!", Refresh = "Default" });
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
            if (_delt.UserMasters.Count > 0)
            {
                TempData["failure"] = "City Has User Please Delete First...!";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "City Has User Please Delete First...!", Refresh = "Default" });
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
                if (_delt.UserMasters.Count > 0)
                {
                    TempData["failure"] = "Religion Has User Please Delete First...!";
                    return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Religion Has User Please Delete First...!" });
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
            if (_delt.UserMasters.Count > 0)
            {
                TempData["failure"] = "Religion Has User Please Delete First...!";
                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Religion Has User Please Delete First...!" });
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
            ViewBag.PageTitle = "All CustomDocuments";
            List<Custom_Document> c = db.Custom_Document.Where(x => x.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.Cust_Id).ToList();
            ViewBag.c = c;
            return View();
        }
        public ActionResult CustomDocument_Add(long Cust_Id)
        {
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
            ViewBag.PageTitle = "Add CustomDocument";
            Custom_Document c = db.Custom_Document.Where(x => x.Cust_Id == Cust_Id && x.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
            ViewBag.c = c;
            return View();
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult CustomDocument_Add(FormCollection form)
        {
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
            ViewBag.PageTitle = "All Department";
            List<Department_Master> dept = db.Department_Master.Where(x => x.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.Dept_ID).ToList();
            ViewBag.dept = dept;
            return View();
        }
        [HttpPost]
        public ActionResult Department_Add(FormCollection form)
        {
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
            ViewBag.PageTitle = "All Designation";
            List<Designation_Master> desg = db.Designation_Master.Where(x => x.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.Desg_Id).ToList();
            ViewBag.desg = desg;
            return View();
        }
        [HttpPost]
        public ActionResult Designation_Add(FormCollection form)
        {
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
        public ActionResult TaskType_List()
        {
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
            ViewBag.PageTitle = "All TaskType";
            List<TaskType> task = db.TaskTypes.Where(x => x.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.TaskType_Id).ToList();
            ViewBag.task = task;
            return View();
        }
        [HttpPost]
        public ActionResult TaskType_Add(FormCollection form)
        {
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
            ViewBag.PageTitle = "All Holiday";
            List<Holiday_Master> holiday = db.Holiday_Master.Where(x => x.Company_Id == _LogedUser.Company_ID).ToList();
            ViewBag.holiday = holiday;

            return View();
        }
        [HttpPost]
        public ActionResult Holiday_Add(FormCollection form)
        {
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
            ViewBag.PageTitle = "All Shift";
            List<Shift_Master> Shift = db.Shift_Master.Where(x => x.Company_ID == _LogedUser.Company_ID).ToList();
            ViewBag.Shift = Shift;

            return View();
        }
        [HttpPost]
        public ActionResult Shift_Add(FormCollection form)
        {
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
            List<UserMaster> empnew = db.UserMasters.Where(x => x.User_type != _LogedUser.User_type && x.Company_ID == _LogedUser.Company_ID && x.status == 0).ToList();
            //foreach (UserMaster user in emp)
            //{
            //    bool _found = false;
            //    foreach (Project_Assign_Master _usr in proj.Project_Assign_Master)
            //    {
            //        if (user.User_ID == _usr.UserMaster.User_ID)
            //        {
            //            _found = true;
            //        }
            //    }
            //    if (_found == false)
            //    {
            //        _notincdlued.Add(user);
            //    }
            //}
            var det = from u in db.Project_Master
                      join a in db.Project_Assign_Master on u.Project_Id equals a.Project_Id
                      join d in db.UserMasters on a.Emp_Id equals d.User_ID
                      where (u.Project_Id == id) && (a.Emp_Id == d.User_ID)
                      select new Projectasigndata { projectdetails = u, proasighndetails = a, userdetails = d };
            ViewBag.jointbldata = det.ToList();
            //return Json(new result=Listing select new ,JsonRequestBehavior.AllowGet);
            var data = db.Project_Master.ToList();
            return Json(from obj in det.ToList() select new { Project_Name = obj.projectdetails.Project_Name, User_name = obj.userdetails.User_Name, Project_Id = obj.projectdetails.Project_Id, Project_Assign_ID = obj.proasighndetails.Project_Assign_Id }, JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public JsonResult dropdownEmploeName(long id)
        {
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];

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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
                return Json(new { success = true, Project_Id = project_Id, Project_Name = project_Name, Cnt = cnt, Req_Id = req_Id, Req_Amt = req_Amt, Cntreq = cntreq });
            }
            else
            {
                return Json(new { success = false });
            }
        }
        public ActionResult AdvanceReq_List(int status = 101)
        {
            ViewBag.PageTitle = "All Advance Request";
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
            if (status == 101)
            {
                List<Request_Master> req = db.Request_Master.Where(x => x.Status != 6 && x.UserMaster.Company_ID == _LogedUser.Company_ID).OrderBy(x => x.Status).ThenByDescending(x => x.Req_Id).ToList();
                List<Project_Assign_Master> projects = db.Project_Assign_Master.Where(x => x.Project_Master.Company_ID == _LogedUser.Company_ID).ToList();
                List<UserMaster> user = db.UserMasters.Where(x => x.User_type != _LogedUser.User_type && x.Company_ID == _LogedUser.Company_ID && x.status == 0).ToList();
                ViewBag.Project = projects;
                ViewBag.user = user;
                ViewBag.req = req;
            }
            if (status == 0)
            {
                List<Request_Master> req = db.Request_Master.Where(x => x.Status == 0 && x.UserMaster.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.Req_Id).ToList();
                List<Project_Assign_Master> projects = db.Project_Assign_Master.Where(x => x.Project_Master.Company_ID == _LogedUser.Company_ID).ToList();
                List<UserMaster> user = db.UserMasters.Where(x => x.User_type != _LogedUser.User_type && x.Company_ID == _LogedUser.Company_ID).ToList();
                ViewBag.Project = projects;
                ViewBag.user = user;
                ViewBag.req = req;
            }
            return View();
        }
        [HttpPost]
        public ActionResult AdvanceReq_Add(FormCollection form)
        {
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
            long Req_Id = long.Parse(form["Req_Id"].ToString());
            long User_Id = long.Parse(form["User_Id"].ToString());
            string Description = form["Description"].ToString();
            decimal Requested_Amount = decimal.Parse(form["Requested_Amount"].ToString());
            decimal Granted_Amount = decimal.Parse(form["Granted_Amount"].ToString());
            string Place = form["Place"].ToString();
            string Parent_Type = form["Parent_Type"].ToString();
            DateTime S_Date = DateTime.Parse(form["S_Date"].ToString());
            DateTime E_Date = DateTime.Parse(form["E_Date"].ToString());
            string Remark = form["Remark"].ToString();
            string Remark_For_Admin = form["Remark_For_Admin"].ToString();
            string Cmd = form["Command"].ToString();
            if (Cmd == "Save")
            {
                if (Req_Id == 0)
                {
                    if (S_Date > E_Date)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Please Add Proper Date.", Refresh = "Default" });
                    }
                    Request_Master Request = new Request_Master();
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
                    Request.Place = Place;
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
                    if (S_Date > E_Date)
                    {
                        return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Please Add Proper Date.", Refresh = "Default" });
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
                    Request.Place = Place;
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
            List<Request_Master> req = db.Request_Master.Where(x => x.Status == 1 && x.UserMaster.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.ReqDate).ToList();
            List<Project_Assign_Master> projects = db.Project_Assign_Master.Where(x => x.Project_Master.Company_ID == _LogedUser.Company_ID).ToList();
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
                        var allowedExtensions = new[] { ".Jpg", ".JPG", ".png", ".jpg", ".jpeg", ".pdf", ".docs" };
                        var fileName = Path.GetFileName(file.FileName);
                        var ext = Path.GetExtension(file.FileName);
                        if (!allowedExtensions.Contains(ext))
                        {
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Sorry Invalid Files.", Refresh = "VoucherReq_List" });
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];

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
        [HttpPost]
        public ActionResult Leave_Emp_Add(FormCollection form)
        {
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
            List<UserMaster> user = db.UserMasters.Where(x => x.User_type != _LogedUser.User_type && x.Company_ID == _LogedUser.Company_ID && x.status == 0).ToList();
            ViewBag.user = user;
            return View();
        }
        [HttpPost]
        public ActionResult Emp_Leave(FormCollection form)
        {
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];


            if (form["datepicker"] != null)
            {
                //.....Count Total Sundays
                DateTime date = DateTime.Parse(form["datepicker"].ToString());
                int Year = date.Year;
                ViewBag.Year = Year;

                int Month = date.Month;
                ViewBag.Month = Month;
                ViewBag.mm = date.ToString("MM-yyyy");
                //DayOfWeek Weekday = date.DayOfWeek;
                //int totalSunDays = GetSundays(Year, Month, Weekday);
                //ViewBag.TotalSundays = totalSunDays;

                DateTime today = DateTime.Today;
                DateTime endOfMonth = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
                //get only last day of month
                int day = endOfMonth.Day;

                DateTime now = DateTime.Now;
                int totalSunDays;
                totalSunDays = 0;
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




                //.....Count Total Holidays
                List<Holiday_Master> _holiday = db.Holiday_Master.Where(x => x.Company_Id == _LogedUser.Company_ID && x.Holiday_Date.Value.Month == date.Month && x.Holiday_Date.Value.Year == date.Year).ToList();
                int totalHolidays = _holiday.Count();
                int OffDays = totalSunDays + totalHolidays;
                ViewBag.TotalHoliday = totalHolidays;

                //.....Count Total Days
                int totalDays = getdays(Year, Month);


                //.....Count Total Present Days
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
        public ActionResult Attendance_history(long User_Id, int Year, int Month)
        {
            ViewBag.PageTitle = "Attendance History";
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
            List<UserMaster> user = db.UserMasters.Where(x => x.User_type != _LogedUser.User_type && x.Company_ID == _LogedUser.Company_ID && x.status == 0).ToList();
            ViewBag.user = user;
            List<Imp_Doc_Master> impdoc = db.Imp_Doc_Master.Where(x => x.Company_ID == _LogedUser.Company_ID && x.UserMaster.status == 0).ToList();
            ViewBag.impdoc = impdoc;
            return View();
        }
        [HttpPost]
        public ActionResult Imp_Document_Add(FormCollection form, HttpPostedFileBase file)
        {
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
                        var allowedExtensions = new[] { ".Jpg", ".JPG", ".png", ".jpg", ".jpeg", ".pdf" };
                        var fileName = Path.GetFileName(file.FileName);
                        var ext = Path.GetExtension(file.FileName);
                        if (!allowedExtensions.Contains(ext))
                        {
                            return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Sorry Invalid Files.!" });
                        }
                        string name = Path.GetFileNameWithoutExtension(fileName);
                        string myfile = name + "_" + DateTime.Now.Second.ToString() + ext;
                        var path = Path.Combine(Server.MapPath("~/Uploads/User_Document"), myfile);
                        file.SaveAs(path);
                        Doc.Doc_Path = path;
                        Doc.File_Name = myfile;
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
                            var allowedExtensions = new[] { ".Jpg", ".JPG", ".png", ".jpg", ".jpeg", ".pdf" };
                            var fileName = Path.GetFileName(file.FileName);
                            var ext = Path.GetExtension(file.FileName);
                            if (!allowedExtensions.Contains(ext))
                            {
                                return this.Json(new { EnableError = true, ErrorTitle = "F", ToastMsgFail = "Sorry Invalid Files.!" });
                            }
                            string name = Path.GetFileNameWithoutExtension(fileName);
                            string myfile = name + "_" + DateTime.Now.Second.ToString() + ext;
                            var path = Path.Combine(Server.MapPath("~/Uploads/User_Document"), myfile);
                            file.SaveAs(path);
                            Doc.Doc_Path = path;
                            Doc.File_Name = myfile;
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
        public ActionResult GenerateDoc(FormCollection form)
        {
            ViewBag.PageTitle = "Generate Document";
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
            UserMaster user = db.UserMasters.Where(x => x.User_ID == empid && x.Company_ID == _LogedUser.Company_ID && x.status == 0).FirstOrDefault();
            Custom_Document document = db.Custom_Document.Where(x => x.Cust_Id == docid && x.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
            ViewBag.user = user;
            ViewBag.document = document;
            return View();
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult GenerateDocbyemp(FormCollection form)
        {
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
            List<Generated_Documents> gendocs = db.Generated_Documents.Where(x => x.UserMaster.Company_ID == _LogedUser.Company_ID && x.UserMaster.status == 0).ToList();
            ViewBag.gendocs = gendocs;
            return View();
        }
        public ActionResult PrintDocument(long Gen_doc_id)
        {
            ViewBag.PageTitle = "Print Documents";
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
            Generated_Documents gendocs = db.Generated_Documents.Where(x => x.Gen_doc_id == Gen_doc_id && x.UserMaster.Company_ID == _LogedUser.Company_ID).FirstOrDefault();
            ViewBag.gendocs = gendocs;
            return View();
        }
        public ActionResult Employee_Task_weelky(string Taskdate = "", long user_id = 0)
        {
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
            ViewBag.PageTitle = "All Events";
            List<Event_Master> eve = db.Event_Master.Where(x => x.Status != 6 && x.UserMaster.Company_Master.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.Event_Id).ToList();
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
            ViewBag.PageTitle = "All NoticeBoardList";
            List<NoticeBoard_Master> n = db.NoticeBoard_Master.Where(x => x.Company_Master.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.Notice_Id).ToList();
            ViewBag.notice = n;
            return View();
        }
        [HttpPost]
        public ActionResult NoticeBoard_Add(FormCollection form)
        {
            UserMaster _LogedUser = (UserMaster)Session["LOGIN_USER"];
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


    }

}