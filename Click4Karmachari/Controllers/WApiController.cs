using ClickKarmachari.Models;
using DocumentFormat.OpenXml.Office.PowerPoint.Y2022.M08.Main;
using DocumentFormat.OpenXml.Spreadsheet;
using Grpc.Core;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;


namespace ClickKarmachari.Controllers

{
    [CookieAdminAuthorizeAttribute]
    public class WApiController : ApiController
    {
        private readonly CommonFacitilities _commonFacilities;

        private Prod_Satyamgroup_V1Entities db = new Prod_Satyamgroup_V1Entities();

        public WApiController()
        {
            _commonFacilities = new CommonFacitilities();
        }



        UserMaster LoggedDistributorEmployee = null;
        public string IPAddress { get; private set; }

        private async Task<Master_Token> Check_Token(string Tokens, string User_UID, string Device_Address)
        {
            Master_Token _token = await db.Master_Token.Where(x => x.Token == Tokens.ToString()).FirstOrDefaultAsync();
            if (_token == null)
            {
                return null;
            }
            else
            {
                if (_token.Expire < DateTime.Now || _token.Device_Address != Device_Address || _token.UserMaster.User_UID.ToString() != User_UID.ToString())
                {
                    return null;
                }
                if (_token.UserMaster.User_type == 1)
                {
                    return null;
                }
                Thread t = new Thread(new ThreadStart(() => new CommonClasses().Update_tokenExTime(_token.access_id)));
                t.Start();
                return _token;
            }
        }

        [HttpPost]
        public async Task<object> VersionCheck(string testdata = "")
        {
            return new { Major = 1, Minor = 10, Mini = 10 };
        }

        [HttpPost]
        public async Task<object> Login(IO_LoginUser getUser)
        {

            var data = new object();
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState
                                 where item.Value.Errors.Any()
                                 select item.Value.Errors[0].ErrorMessage).ToList();
                data = new { rescode = 2, resmsg = "Please Validate Your Input!", errors = errorList };
                return data;
            }

            long usrMobile = getUser.Mobile;
            string password = getUser.pwd;

            UserMaster _newuser = new UserMaster();

            UserMaster _usr = await db.UserMasters.Where(x => x.User_Mobile == usrMobile && x.User_Password == password).FirstOrDefaultAsync();

            if (_usr == null)
            {
                data = new { rescode = 2, resmsg = "Please Try again UserMobile and Password is wrong..!" };
                return data;
            }
            else if (_usr.User_type == 1)
            {
                data = new { rescode = 2, resmsg = "Only Employee Can Login" };
                return data;
            }
            else if (_usr.status == 1 || _usr.status == 6)
            {
                data = new { rescode = 2, resmsg = "Sorry,You Are Deactive/Deleted Right Now!" };
                return data;
            }
            else
            {
                UserMasterIO _user = new UserMasterIO(_usr);
                Master_Token _token = new Master_Token();
                _token = await db.Master_Token.Where(s => s.User_ID == _usr.User_ID && s.Device_Address == getUser.Device_Address && s.Expire > DateTime.Now).FirstOrDefaultAsync();
                if (_token == null)
                {
                    _token = new Master_Token();
                    _token.User_ID = _usr.User_ID;
                    _token.Token = Guid.NewGuid().ToString().Replace("-", "");
                    _token.CreatedOn = DateTime.Now;
                    _token.Expire = DateTime.Now.AddYears(1);
                    _token.Device_Address = getUser.Device_Address.ToString();
                    _token.Private_key = new Random().Next(0, 10).ToString();
                    _token.Public_key = Guid.NewGuid().ToString().Replace("-", "");
                    _token.FCM_DeviceID = getUser.FCM_Key;
                    db.Master_Token.Add(_token);
                }
                else
                {
                    _token.Expire = DateTime.Now.AddYears(1);
                }
                await db.SaveChangesAsync();
                _user.Tokens = _token.Token;
                data = new { rescode = 0, resmsg = "Login successfully", UserDetail = _user };
                return data;
            }
        }

        public int SendOTP_User(long mobileNumber)
        {
            try
            {
                // 1. Generate a 6-digit random OTP (range 100000-999999).
                //    Using "D6" ensures a 6-digit string (e.g., leading zeros if needed).
                string otp = new Random().Next(100000, 999999).ToString("D6");
                // 2. Check if the user already exists
                var user = db.UserMasters.FirstOrDefault(x => x.User_Mobile == mobileNumber);
                if (user == null)
                {
                    return 1;
                }
                else
                {
                    // 3. If user exists, update OTP and expiration
                    user.last_otp = int.Parse(otp.ToString());
                    user.last_otp_expire = DateTime.Now.AddMinutes(5);
                    db.SaveChanges();
                    // 3b. Send the OTP via SMS in a separate thread
                    var t = new Thread(() => new CommonClasses().Send_2FACTOR_SMS(user.User_Name.ToString(), user.User_Mobile.ToString(), otp));
                    t.Start();
                }
                // 4. Return 0 on success
                return 0;
            }
            catch (Exception ex)
            {
                // Optionally log 'ex' if needed
                // Return 1 to indicate an error occurred
                return 1;
            }
        }

        [HttpPost]
        public async Task<object> VerifyMobile(InputModel_VerifyMobile request)
        {
            var data = new object();
            try
            {
                int sendOtpResult = await System.Threading.Tasks.Task.Run(() => SendOTP_User(request.Mobile));

                if(sendOtpResult==0)
                {
                    data = new { rescode = 0, resmsg = "Verification Code sent!" };
                   
                    
                }
                else
                {
                    data = new { rescode = 1, resmsg = "OTP Sending Failed, please try again." };
                    
                }
                return data;
                //var response = new
                //    {
                //        IsSuccess = sendOtpResult == 0,
                //        ErrorTitle = sendOtpResult == 0 ? "Success" : "Failure",
                //        ErrorMsg = sendOtpResult == 0 ? "Verification Code sent!" : "OTP Sending Failed, please try again."
                //    };
                //return new { rescode = 1, resmsg = response };
                // return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in VerifyMobile: {ex.Message}");
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { IsSuccess = false, ErrorTitle = "Failure", ErrorMsg = "Technical issue, please try again!" });
            }
        }

        
        [HttpPost]
        public async Task<object> VerifyOTP(InputModel_VerifyOTP request)
        {
            var data = new object();
            try
            {
                // 1. Find the user by mobile number
                var user = db.UserMasters.FirstOrDefault(x => x.User_Mobile == request.Mobile);
                if (user == null)
                {
                    return new { rescode = 1, resmsg = "Employee not found!" };
                }
                // 2. Check OTP (including master OTP "339009")
                if (user.last_otp != request.OTP && request.OTP.ToString() != "339009")
                {
                    return new { rescode = 1, resmsg = "Invalid OTP!" };
                }
                // 3. Check if OTP has expired
                if (user.last_otp_expire < DateTime.Now)
                {
                    return new { rescode = 1, resmsg = "OTP expired!" };
                }
                // 4. Check user status (0 = active, otherwise inactive)
                if (user.User_type ==1)
                {
                    return new { rescode = 1, resmsg = "Only Employee Can Login" }; 
                }
                if (user.status == 1 && user.status==6)
                {
                    return new { rescode = 1, resmsg = "Sorry,You Are Deactive/Deleted Right Now!" }; 
                }
                Master_Token _token = db.Master_Token.Where(x => x.User_ID == user.User_ID && x.Device_Address == request.Device_Address).FirstOrDefault();
                if (_token == null)
                {
                    List<Master_Token> _tokens = db.Master_Token.Where(x => x.Device_Address == request.Device_Address).ToList();
                    if (_tokens.Count > 0)
                    {
                        db.Master_Token.RemoveRange(_tokens);
                    }
                    _token = new Master_Token();
                    _token.User_ID = user.User_ID;
                    _token.Token = Guid.NewGuid().ToString().Replace("-", "");
                    _token.CreatedOn = DateTime.Now;
                    _token.Expire = DateTime.Now.AddYears(1);
                    _token.Device_Address = request.Device_Address;
                    _token.Private_key = "";
                    _token.Public_key = "";
                    db.Master_Token.Add(_token);
                }
                else
                {
                    _token.Expire = DateTime.Now.AddYears(1);
                }
                db.SaveChanges();
                UserMasterIO _user = new UserMasterIO(user);
                _user.Tokens = _token.Token;
                data = new { rescode = 0, resmsg = "Login Successful.", user = _user };
                return data;
            }
            catch (Exception ex)
            {
                return new { rescode = 1, resmsg = "Technical issue, please try again!" };
                
            }
        }


        [HttpPost]
        public async Task<object> Update_Profile(IO_Profile _UserTokenLive)
        {
            

            var data = new object();
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState
                                 where item.Value.Errors.Any()
                                 select item.Value.Errors[0].ErrorMessage).ToList();
                data = new { rescode = 2, resmsg = "Please Validate Your Input!", errors = errorList };
                return data;
            }
            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null)
            {
                return new { rescode = 1, resmsg = "UnAthorised Session !" };
            }



            UserMaster _user = db.UserMasters.Where(x => x.User_ID == _token.User_ID).FirstOrDefault();
            if (_user != null)
            {
                if(_user.Profile_Lock== true)
                {
                    data = new { rescode = 1, resmsg = "Profile is lock so will not update your profile..!" };
                }
                else
                {
                    if (_UserTokenLive.RefMobile == 0) { _UserTokenLive.RefMobile = null; }
                    if (_UserTokenLive.Adharcard_No == 0) { _UserTokenLive.Adharcard_No = null; }
                    _user.RefMobile = _UserTokenLive.RefMobile;
                    _user.Adharcard_No = _UserTokenLive.Adharcard_No;
                    _user.PanCard_No = _UserTokenLive.PanCard_No;
                    _user.User_Name = _UserTokenLive.User_Name;
                    _user.User_Email = _UserTokenLive.User_Email;
                    //_user.City_id = _UserTokenLive.User_City;
                    //_user.Religion_Id = _UserTokenLive.Religion_Id;
                    _user.City_Name = _UserTokenLive.User_City;
                    _user.Blood_group = _UserTokenLive.BloodGroup;
                    db.SaveChanges();
                    UserMasterIO _userio = new UserMasterIO(_user);
                    _userio.Tokens = _token.Token;
                    _userio.Company_Name = _user.Company_Master.Company_Name;
                    data = new { rescode = 0, resmsg = "Profile Update successfully !", user = _userio };
                }
               
            }
            else
            {
                data = new { rescode = 1, resmsg = "Invalid input so not updated profile..!" };
            }
            return data;
        }

        [HttpPost]
        public async Task<object> GetProfile(IO_getUser _UserTokenLive)
        {
            var data = new object();
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState
                                 where item.Value.Errors.Any()
                                 select item.Value.Errors[0].ErrorMessage).ToList();
                data = new { rescode = 2, resmsg = "Please Validate Your Input!", errors = errorList };
                return data;
            }
            UserMaster _LogedUser = new UserMaster();
            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null)
            {
                return new { rescode = 1, resmsg = "UnAthorised Session !" };
            }
            _LogedUser = _token.UserMaster;
            UserMasterIO _User = new UserMasterIO(_LogedUser);
            _User.Tokens = _token.Token;
            _User.Company_Name = _LogedUser.Company_Master.Company_Name;
            return new { rescode = 0, resmsg = "get data!", User = _User };
        }
        [HttpPost]
        public async Task<object> GetDashboard(IO_getUser _UserTokenLive)
        {
            var data = new object();
            UserMaster _LogedUser = new UserMaster();
            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null) { return new { rescode = 1, resmsg = "UnAthorised Session !" }; }
            _LogedUser = _token.UserMaster;
          
            if (_LogedUser != null)
            {
                List<User_Punch> panch_List = db.User_Punch.Where(x => x.User_Id == _LogedUser.User_ID).ToList();
                if (panch_List.Count == 0)
                {

                }
                else
                {
                    var max = db.User_Punch.Where(x => x.User_Id == _LogedUser.User_ID).OrderByDescending(p => p.Punch_Id).FirstOrDefault().Punch_Id;
                    User_Punch panch_detail = db.User_Punch.Where(x => x.Punch_Id == max).FirstOrDefault();
                    if (panch_detail != null)
                    {
                        var punch_detail = panch_detail;

                    }
                    else
                    {

                    }
                }
             
                
            }

            List<Leave_Master> pendingLeave = db.Leave_Master.Where(x => x.leave_Id == _LogedUser.Total_Leave && x.Status == 0).ToList();
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

            long Pending_leave = 0; int leaves = 0; int Asset = 0; int task_N = 0; int task_R = 0; int task_S = 0; int project_Tasks = 0; int vouchers = 0;

            Asset = _myAssets.Count;
            task_N = project_Task.Where(x => x.Status == 0).ToList().Count;
            task_R = project_Task.Where(x => x.Status == 7).ToList().Count;
            task_S = project_Task.Where(x => x.Status == 9).ToList().Count;
            project_Tasks = project_Task.Count;
            vouchers = voucher.Count;
            leaves = leave.Count;
            Pending_leave = pendingLeave.Count;
            var day = DateTime.Today;
            var monthh = day.AddMonths(5);
            List<UserMaster> user = db.UserMasters.Where(x => x.Company_ID == _LogedUser.Company_ID & x.status == 0).ToList().OrderBy(x => x.DOB).ToList();
            List<Event_Master> Event = db.Event_Master.Where(x => x.Company_ID == _LogedUser.Company_ID && (x.Date > DateTime.Now || x.Date == DateTime.Now)).OrderByDescending(x => x.Date).ToList();
            List<GetEventListIO> _Event_list = new List<GetEventListIO>();

            foreach (Event_Master _obj in Event)
            {
                _Event_list.Add(new GetEventListIO(_obj));
            }
            List<UserMaster> _userlisttodisplay = new List<UserMaster>();
            for (int i = day.Year; i <= day.Year; i++)
            {

                foreach (UserMaster _user in user.Where(x => x.DOB.Month >= day.Month && x.DOB.Month < monthh.Month))
                {
                    _userlisttodisplay.Add(_user);
                }
            }
            List<NoticeBoard_Master> notices = db.NoticeBoard_Master.Where(x => x.Company_ID == _LogedUser.Company_ID && (x.Date > DateTime.Now || x.Date==DateTime.Now)).OrderByDescending(x => x.Date).Take(5).ToList();
            List<GetNoticeBoardListIO> _Notice_list = new List<GetNoticeBoardListIO>();
            foreach (NoticeBoard_Master _obj in notices)
            {
                _Notice_list.Add(new GetNoticeBoardListIO(_obj));
            }
            List<PunchListIO> _Punch_list = new List<PunchListIO>();
            DateTime searchDate = DateTime.Today;
            List<User_Punch> panch_Listdata = db.User_Punch.Where(x => x.UserMaster.Company_ID == _LogedUser.Company_ID && x.User_Id==_LogedUser.User_ID).ToList();
          
            foreach (User_Punch _obj in panch_Listdata)
            {
                _Punch_list.Add(new PunchListIO(_obj));
            }
            long punchtype_id = 0;
            if (panch_Listdata.Count != 0)
            {
                var data1 = panch_Listdata
                  .Where(x => x.Time.Date == DateTime.Today)
                  .ToList();
                if(data1.Count!=0)
                {
                    punchtype_id = data1.Last().PunchType_Id;
                }
                
            }

            return new { rescode = 0, resmsg = "get Dashboard data!", User = new UserMasterIO(_LogedUser), EventList = _Event_list, notices = _Notice_list, PunchList= _Punch_list, Voucher = voucher.Count, Asset = _myAssets.Count, LeaveRequest = leave.Count, Pending_Leave = pendingLeave.Count, Task_Pending = task_N, Task_Running = task_R, Task_Completed = task_S,PunchType_id= punchtype_id };
        }

        [HttpPost]
        public async Task<object> Change_Password(IoChangePassword _UserTokenLive)
        {
            var data = new object();
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState
                                 where item.Value.Errors.Any()
                                 select item.Value.Errors[0].ErrorMessage).ToList();
                data = new { rescode = 2, resmsg = "Please Validate Your Input!", errors = errorList };
                return data;
            }
            UserMaster _LogedUser = new UserMaster();
            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null)
            {
                return new { rescode = 1, resmsg = "UnAthorised Session !" };
            }
            _LogedUser = _token.UserMaster;

            UserMaster _newadd = db.UserMasters.Where(x => x.User_Password == _UserTokenLive.OldPassword && x.User_ID == _LogedUser.User_ID).FirstOrDefault();
            if (_newadd != null && _newadd.status != 6)
            {

                if (_UserTokenLive.NewPassword == _UserTokenLive.ConfirmPassword)
                {
                    _newadd.User_Password = _UserTokenLive.NewPassword;
                    db.SaveChanges();
                    data = new { rescode = 0, resmsg = "Password Changed Successfully !" };
                    return data;
                }

                else
                {
                    data = new { rescode = 2, resmsg = "New password Does not match with Confirm password !" };
                    return data;
                }

            }
            else
            {
                data = new { rescode = 2, resmsg = "Old Password Not Match !" };
                return data;
            }

        }

        [HttpPost]
        public async Task<object> AddtoAddress(IO_AddtoAddress _UserTokenLive)
        {
            var data = new object();
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState
                                 where item.Value.Errors.Any()
                                 select item.Value.Errors[0].ErrorMessage).ToList();
                data = new { rescode = 2, resmsg = "Please Validate Your Input!", errors = errorList };
                return data;
            }
            UserMaster _LogedUser = new UserMaster();
            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null)
            {
                return new { rescode = 1, resmsg = "UnAthorised Session !" };
            }
            _LogedUser = _token.UserMaster;
            Address_Master _newadd = new Address_Master();
            _newadd.User_ID = _LogedUser.User_ID;
            _newadd.Add_Address = _UserTokenLive.Address;
            _newadd.Status = 0;
            _newadd.CityName = _UserTokenLive.City;
            db.Address_Master.Add(_newadd);
            db.SaveChanges();

            Prod_Satyamgroup_V1Entities _db = new Prod_Satyamgroup_V1Entities();
            UserMasterIO _User = new UserMasterIO(_db.UserMasters.Where(x => x.User_ID == _LogedUser.User_ID).FirstOrDefault());
            return new { rescode = 0, resmsg = "New Address added Successfully !", User = _User };
        }

        [HttpPost]
        public async Task<object> AddtoRelation(IO_AddtoRelation _UserTokenLive)
        {
            var data = new object();
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState
                                 where item.Value.Errors.Any()
                                 select item.Value.Errors[0].ErrorMessage).ToList();
                data = new { rescode = 2, resmsg = "Please Validate Your Input!", errors = errorList };
                return data;
            }
            UserMaster _LogedUser = new UserMaster();

            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null)
            {
                return new { rescode = 1, resmsg = "UnAthorised Session !" };
            }
            _LogedUser = _token.UserMaster;
            Relation_Master newrel = new Relation_Master();
            newrel.User_Id = _LogedUser.User_ID;
            newrel.Relation_Name = _UserTokenLive.Relation;
            newrel.Person_Name = _UserTokenLive.Person;
            newrel.Mobile_No = _UserTokenLive.Mobile;
            newrel.Status = 0;
            db.Relation_Master.Add(newrel);
            db.SaveChanges();
            Prod_Satyamgroup_V1Entities _db = new Prod_Satyamgroup_V1Entities();
            UserMasterIO _User = new UserMasterIO(_db.UserMasters.Where(x => x.User_ID == _LogedUser.User_ID).FirstOrDefault());
            return new { rescode = 0, resmsg = "New Relation added Successfully !", User = _User };
        }

        [HttpPost]
        public async Task<object> AddtoEducation(IO_AddtoEducation _UserTokenLive)
        {
            var data = new object();
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState
                                 where item.Value.Errors.Any()
                                 select item.Value.Errors[0].ErrorMessage).ToList();
                data = new { rescode = 2, resmsg = "Please Validate Your Input!", errors = errorList };
                return data;
            }
            UserMaster _LogedUser = new UserMaster();

            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null)
            {
                return new { rescode = 1, resmsg = "UnAthorised Session !" };
            }
            _LogedUser = _token.UserMaster;
            Education_Master Edu = new Education_Master();
            Edu.User_Id = _LogedUser.User_ID;
            Edu.Edu_Name = _UserTokenLive.Education;
            Edu.university = _UserTokenLive.University;
            Edu.Pass_Out = _UserTokenLive.PassOut;
            Edu.Status = 0;
            db.Education_Master.Add(Edu);
            db.SaveChanges();
            Prod_Satyamgroup_V1Entities _db = new Prod_Satyamgroup_V1Entities();
            UserMasterIO _User = new UserMasterIO(_db.UserMasters.Where(x => x.User_ID == _LogedUser.User_ID).FirstOrDefault());
            return new { rescode = 0, resmsg = "New Education added Successfully !", User = _User };
        }

        [HttpPost]
        public async Task<object> AddtoExperience(IO_AddtoExperience _UserTokenLive)
        {
            var data = new object();
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState
                                 where item.Value.Errors.Any()
                                 select item.Value.Errors[0].ErrorMessage).ToList();
                data = new { rescode = 2, resmsg = "Please Validate Your Input!", errors = errorList };
                return data;
            }
            UserMaster _LogedUser = new UserMaster();

            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null)
            {
                return new { rescode = 1, resmsg = "UnAthorised Session !" };
            }
            _LogedUser = _token.UserMaster;
            Experience_Master Exp = new Experience_Master();
            Exp.User_Id = _LogedUser.User_ID;
            Exp.Last_Company = _UserTokenLive.LastCompany;
            Exp.working_years = _UserTokenLive.WorkingYears;
            Exp.Last_Year = _UserTokenLive.LastYear;
            Exp.Status = 2;
            db.Experience_Master.Add(Exp);
            db.SaveChanges();
            Prod_Satyamgroup_V1Entities _db = new Prod_Satyamgroup_V1Entities();
            UserMasterIO _User = new UserMasterIO(_db.UserMasters.Where(x => x.User_ID == _LogedUser.User_ID).FirstOrDefault());
            return new { rescode = 0, resmsg = "New Experience added Successfull !", User = _User };
        }

        [HttpPost]
        public async Task<object> AddtoBankDetails(IO_AddtoBankDetails _UserTokenLive)
        {
            var data = new object();
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState
                                 where item.Value.Errors.Any()
                                 select item.Value.Errors[0].ErrorMessage).ToList();
                data = new { rescode = 2, resmsg = "Please Validate Your Input!", errors = errorList };
                return data;
            }
            UserMaster _LogedUser = new UserMaster();

            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null)
            {
                return new { rescode = 1, resmsg = "UnAthorised Session !" };
            }
            _LogedUser = _token.UserMaster;

            UserMaster _updateuser = db.UserMasters.Find(_LogedUser.User_ID);
            _updateuser.Bank_Name = _UserTokenLive.Bank_Name;
            _updateuser.User_Name = _UserTokenLive.Account_Holder;
            _updateuser.Bank_Acc_No = _UserTokenLive.Bank_Acc_No;
            _updateuser.IFSC_No = _UserTokenLive.IFSC_No;
            _updateuser.UAN_No = _UserTokenLive.UAN_No;
            _updateuser.ESIC_No = _UserTokenLive.ESIC_No;
            db.SaveChanges();

            Prod_Satyamgroup_V1Entities _db = new Prod_Satyamgroup_V1Entities();
            UserMasterIO _User = new UserMasterIO(_db.UserMasters.Where(x => x.User_ID == _LogedUser.User_ID).FirstOrDefault());
            return new { rescode = 0, resmsg = "New BankDetails added Successfully !", User = _User };
        }

        [HttpPost]
        public async Task<object> GetMyAttendance(IO_AttendanceMaster _UserTokenLive)
        {

            var data = new object();
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState
                                 where item.Value.Errors.Any()
                                 select item.Value.Errors[0].ErrorMessage).ToList();
                data = new { rescode = 2, resmsg = "Please Validate Your Input!", errors = errorList };
                return data;
            }
            UserMaster _LogedUser = new UserMaster();
            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null)
            {
                return new { rescode = 1, resmsg = "UnAthorised Session !" };
            }
            _LogedUser = _token.UserMaster;
            List<Attendance_Master> _myattnd = new List<Attendance_Master>();
            if (_UserTokenLive.Year != null && _UserTokenLive.Month != null)
            {
                _myattnd = db.Attendance_Master.Where(x => x.User_Id == _LogedUser.User_ID && x.Punch_Date.Month == _UserTokenLive.Month && x.Punch_Date.Year == _UserTokenLive.Year).ToList();
            }
            else
            {
                _myattnd = db.Attendance_Master.Where(x => x.User_Id == _LogedUser.User_ID).ToList();
            }

            List<Attendance_MasterIO> _myattndsss = new List<Attendance_MasterIO>();
            foreach (Attendance_Master _att in _myattnd)
            {
                _myattndsss.Add(new Attendance_MasterIO(_att));
            }

            if (_myattndsss.Count == 0 || _myattndsss == null)
            {
                return new { rescode = 2, resmsg = "No Attendance Found !" };
            }
            else
            {
                return new { rescode = 0, resmsg = "Attendance Found !", Attendance = _myattndsss };
            }

        }

        //[HttpPost]
        //public async Task<object> GetPunch(IO_getUser _UserTokenLive)
        //{
        //    var data = new object();
        //    if (!ModelState.IsValid)
        //    {
        //        var errorList = (from item in ModelState
        //                         where item.Value.Errors.Any()
        //                         select item.Value.Errors[0].ErrorMessage).ToList();
        //        data = new { rescode = 2, resmsg = "Please Validate Your Input!", errors = errorList };
        //        return data;
        //    }
        //    UserMaster _LogedUser = new UserMaster();
        //    Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
        //    if (_token == null)
        //    {
        //        return new { rescode = 1, resmsg = "UnAthorised Session !" };
        //    }
        //    _LogedUser = _token.UserMaster;
        //    List<User_Punch> Punch = new List<User_Punch>();
        //    User_Punch lastpunch = db.User_Punch.Where(x => x.User_Id == _LogedUser.User_ID
        //                     && x.Time.Day == DateTime.Today.Day && x.Time.Month == DateTime.Today.Month
        //                     && x.Time.Year == DateTime.Today.Year).OrderByDescending(x => x.Punch_Id).FirstOrDefault();
        //    if (lastpunch == null)
        //    {
        //        return new { rescode = 2, resmsg = "No Punch for Today !" };
        //    }
        //    else
        //    {
        //        return new { rescode = 0, resmsg = "get data !", Get_LastPunch = new UserPunchIO(lastpunch) };
        //    }
        //}
        
        [HttpPost]
        public async Task<object> GetPunch(IO_getPunch _UserTokenLive)
        {
            var data = new object();
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState
                                 where item.Value.Errors.Any()
                                 select item.Value.Errors[0].ErrorMessage).ToList();
                data = new { rescode = 2, resmsg = "Please Validate Your Input!", errors = errorList };
                return data;
            }
            UserMaster _LogedUser = new UserMaster();
            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null)
            {
                return new { rescode = 1, resmsg = "UnAthorised Session !" };
            }
            _LogedUser = _token.UserMaster;
            List<User_Punch> Punch = new List<User_Punch>();
            DateTime dateTimeactual = DateTime.Now;
            if (_UserTokenLive.dateforpunch != null)
            {
                dateTimeactual = _UserTokenLive.dateforpunch.Value;
            }

            List<User_Punch> Punchlist = db.User_Punch
                .Where(x => x.User_Id == _LogedUser.User_ID
                            && DbFunctions.TruncateTime(x.Time) == dateTimeactual.Date)
                .OrderByDescending(x => x.Punch_Id)
                .ToList();

            List<UserPunchIO> PunchlistIO = new List<UserPunchIO>();
            foreach (User_Punch upunch in Punchlist)
            {
                PunchlistIO.Add(new UserPunchIO(upunch));
            }
            if (PunchlistIO.Count <= 0)
            {
                return new { rescode = 2, resmsg = "No Punch for the day !" };
            }
            else
            {
                return new { rescode = 0, resmsg = "Punch for the day!", Punchlist = PunchlistIO };
            }
        }
       
        
        
        
        [HttpPost]
        public async Task<object> GetCity()
        {
            List<GetCity_Result> CityList = db.GetCity(null, null, null, null, 10000, 1).ToList();
            return new { rescode = 0, resmsg = "Punch for the day!", CityList = CityList };
        }

        [HttpPost]
        public async Task<object> GetProject(IO_getPunch _UserTokenLive)
        {
            var data = new object();
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState
                                 where item.Value.Errors.Any()
                                 select item.Value.Errors[0].ErrorMessage).ToList();
                data = new { rescode = 2, resmsg = "Please Validate Your Input!", errors = errorList };
                return data;
            }
            UserMaster _LogedUser = new UserMaster();
            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null)
            {
                return new { rescode = 1, resmsg = "UnAthorised Session !" };
            }
            _LogedUser = _token.UserMaster;
            List<Project_masterIO> project_Masters = new List<Project_masterIO>();  
            foreach(Project_Master project in db.Project_Master.Where(x=> x.Company_ID == _LogedUser.Company_ID))
            {
                project_Masters.Add(new Project_masterIO(project));
            }

            return new { rescode = 0, resmsg = "Project List for User!", Projects = project_Masters };
        }


        [HttpPost]
        public async Task<object> GetProjectTaskType(IO_getPunch _UserTokenLive)
        {
            var data = new object();
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState
                                 where item.Value.Errors.Any()
                                 select item.Value.Errors[0].ErrorMessage).ToList();
                data = new { rescode = 2, resmsg = "Please Validate Your Input!", errors = errorList };
                return data;
            }
            UserMaster _LogedUser = new UserMaster();
            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null)
            {
                return new { rescode = 1, resmsg = "UnAthorised Session !" };
            }
            _LogedUser = _token.UserMaster;
            List<TaskTypeIO> project_Masters = new List<TaskTypeIO>();
            foreach (TaskType project in db.TaskTypes.Where(x => x.Company_ID == _LogedUser.Company_ID))
            {
                project_Masters.Add(new TaskTypeIO(project));
            }
            return new { rescode = 0, resmsg = "Project Task Type List for User!", TaskType = project_Masters };
        }


        [HttpPost]
        public async Task<object> AddPunch(IO_AddtoPunch _UserTokenLive)
        {
            var data = new object();
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState
                                 where item.Value.Errors.Any()
                                 select item.Value.Errors[0].ErrorMessage).ToList();
                data = new { rescode = 2, resmsg = "Please Validate Your Input!", errors = errorList };
                return data;
            }
            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null) { return new { rescode = 1, resmsg = "UnAthorised Session !" }; }
            UserMaster _LogedUser = _token.UserMaster;
            User_Punch punch = new User_Punch();
            punch.latitude = _UserTokenLive.latitude;
            punch.longitude = _UserTokenLive.longitude;
            punch.Punch_Via = 0;
            punch.Location = _UserTokenLive.Location;
            User_Punch lastpunch = db.User_Punch.Where(x => x.User_Id == _LogedUser.User_ID
                                && x.Time.Day == DateTime.Today.Day && x.Time.Month == DateTime.Today.Month
                                && x.Time.Year == DateTime.Today.Year).OrderByDescending(x => x.Punch_Id).FirstOrDefault();
            CommonClasses _newobj = new CommonClasses();
            //Add Ip
            string ip = _newobj.GetIPAddress(IPAddress);
            punch.IP_Address = ip;

            if (_UserTokenLive.PunchType_Id == 1 && (lastpunch == null || lastpunch.PunchType_Id == 2))
            {
                _newobj._attendancefordate(DateTime.Now, _LogedUser.User_ID);
                punch.PunchType_Id = 1;
                punch.User_Id = _LogedUser.User_ID;
                punch.Time = DateTime.Now;
                db.User_Punch.Add(punch);
                db.SaveChanges();
                return new { rescode = 0, resmsg = "Check IN Successfully !" };
            }
            else if (_UserTokenLive.PunchType_Id == 2 && (lastpunch.PunchType_Id == 1 || lastpunch.PunchType_Id == 4))
            {
                punch.PunchType_Id = 2;
                punch.User_Id = _LogedUser.User_ID;
                punch.Time = DateTime.Now;
                db.User_Punch.Add(punch);
                db.SaveChanges();
                _newobj._attendancefordate(DateTime.Now, _LogedUser.User_ID);
                return new { rescode = 0, resmsg = "Check Out Successfully !" };
            }
            else if (_UserTokenLive.PunchType_Id == 3 && (lastpunch.PunchType_Id == 1))
            {
                punch.PunchType_Id = 3;
                punch.User_Id = _LogedUser.User_ID;
                punch.Time = DateTime.Now;
                db.User_Punch.Add(punch);
                db.SaveChanges();
                return new { rescode = 0, resmsg = "Lunch In Successfully !" };
            }
            else if (_UserTokenLive.PunchType_Id == 4 && (lastpunch.PunchType_Id == 3))
            {
                punch.PunchType_Id = 4;
                punch.User_Id = _LogedUser.User_ID;
                punch.Time = DateTime.Now;
                db.User_Punch.Add(punch);
                db.SaveChanges();
                return new { rescode = 0, resmsg = "Lunch Out Successfully !" };
            }
            else
            {
                return new { rescode = 4, resmsg = "Sorry, You are currently " + _newobj._PunchName_ByNumber(lastpunch.PunchType_Id).ToString() + ". So, You can't " + _newobj._PunchName_ByNumber(_UserTokenLive.PunchType_Id).ToString() + ". !" };
            }
        }

        [HttpPost]
        public async Task<object> GetVisits(IO_getUser _UserTokenLive)
        {
            var data = new object();
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState
                                 where item.Value.Errors.Any()
                                 select item.Value.Errors[0].ErrorMessage).ToList();
                data = new { rescode = 2, resmsg = "Please Validate Your Input!", errors = errorList };
                return data;
            }
            UserMaster _LogedUser = new UserMaster();
            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null)
            {
                return new { rescode = 1, resmsg = "UnAthorised Session !" };
            }
            _LogedUser = _token.UserMaster;
            List<User_Visits> todaysVisits = db.User_Visits.Where(x => x.User_Id == _LogedUser.User_ID
                             && x.Time.Day == DateTime.Today.Day && x.Time.Month == DateTime.Today.Month
                             && x.Time.Year == DateTime.Today.Year).OrderByDescending(x => x.Visit_id).ToList();
            List<UserVisitIO> _visits = new List<UserVisitIO>();
            foreach (User_Visits vis in todaysVisits)
            {
                _visits.Add(new UserVisitIO(vis));
            }

            if (todaysVisits == null)
            {
                return new { rescode = 2, resmsg = "No Visit for Today !" };
            }
            else
            {
                return new { rescode = 0, resmsg = "get data!", visits = _visits };
            }
        }

        [HttpPost]
        public async Task<object> AddVisit()
        {

            var data = new object();
            try
            {
                var httpRequest = HttpContext.Current.Request;
                var Tokens = httpRequest.Form[0].ToString();
                var User_UID = httpRequest.Form[1].ToString();
                var Device_Address = httpRequest.Form[2].ToString();
                var latitude = httpRequest.Form[3].ToString();
                var longitude = httpRequest.Form[4].ToString();
                var Location = httpRequest.Form[5].ToString();
                var LocationName = httpRequest.Form[6].ToString();
                var PunchType_Id = httpRequest.Form[7].ToString();

                UserMaster _LogedUser = new UserMaster();
                Master_Token _token = await Check_Token(Tokens, User_UID, Device_Address);
                if (_token == null)
                {
                    return new { rescode = 1, resmsg = "UnAthorised Session !" };
                }
                _LogedUser = _token.UserMaster;

                if (httpRequest.Files.Count == 0)
                {
                    return new { rescode = 1, resmsg = "No file uploaded!" };
                }

              
                var postedFile = httpRequest.Files[0];

                // Only allow image types
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".JPG" };
                string fileExtension = Path.GetExtension(postedFile.FileName)?.ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    return new { rescode = 1, resmsg = "Only image files allowed!" };
                }

                var fileName = Path.GetFileName(postedFile.FileName);
                string name = Path.GetFileNameWithoutExtension(fileName);
                string myfile = name + "_" + DateTime.Now.ToString("ddMMyyyhhmmss") + fileExtension;
                string filePath = HttpContext.Current.Server.MapPath("~/Uploads/Visit_Image/" + myfile);

                // Ensure the directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                //save file
                postedFile.SaveAs(filePath);

                User_Visits punch = new User_Visits();
                punch.latitude =latitude;
                punch.longitude = longitude;
                punch.Punch_Via = 0;
                punch.Location = Location;
                punch.LocationName = LocationName;
                punch.PunchType_Id = long.Parse(PunchType_Id);
                punch.User_Id = _LogedUser.User_ID;
                punch.Time = DateTime.Now;
                punch.image_path = filePath;
                punch.image_name = myfile;

                ////Add IP Address
                CommonClasses common = new CommonClasses();
                string ip = common.GetIPAddress(IPAddress);
                punch.IP_Address = ip;
                db.User_Visits.Add(punch);
                db.SaveChanges();
                return new { rescode = 0, resmsg = "Visit Added Successfully  !" };

            }
            catch (Exception ex)
            {
                return new { rescode = 1, resmsg = "Something Wrong!" };
            }
        }

        [HttpPost]
        public async Task<object> DeleteProjectTask(IO_DeletetoProjectTask _UserTokenLive)
        {
            var data = new object();
            UserMaster _LogedUser = new UserMaster();
            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null) { return new { rescode = 1, resmsg = "UnAthorised Session !" }; }
            _LogedUser = _token.UserMaster;

            var _del = db.Project_Task_Master.Find(_UserTokenLive.Task_Id);
            if (_del.Status != 3)
            {
                return new { rescode = 1, resmsg = "Task Has Started, Do not Delete task." };
            }
            if (_del.Comment_Master.Count > 0)
            {
                List<Comment_Master> _delCmt = db.Comment_Master.Where(x => x.Task_Id == _UserTokenLive.Task_Id).ToList();
                foreach (Comment_Master cm in _delCmt)
                {
                    db.Comment_Master.Remove(cm);
                }
            }
            db.Project_Task_Master.Remove(_del);
            db.SaveChanges();
            return new { rescode = 0, resmsg = "Employee Task Deleted Successfully." };
        }

        [HttpPost]
        public async Task<object> AddProjectTask(IO_AddtoProjectTask _UserTokenLive)
        {
            var data = new object();
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState
                                 where item.Value.Errors.Any()
                                 select item.Value.Errors[0].ErrorMessage).ToList();
                data = new { rescode = 2, resmsg = "Please Validate Your Input!", errors = errorList };
                return data;
            }
            UserMaster _LogedUser = new UserMaster();
            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null) { return new { rescode = 1, resmsg = "UnAthorised Session !" }; }
            _LogedUser = _token.UserMaster;
            if (_UserTokenLive.Task_Id == 0)
            {
                if (_UserTokenLive.A_Start_Date > _UserTokenLive.R_End_Date)
                {
                    data = new { rescode = 2, resmsg = "Please Add Proper Date ! " };
                    return data;
                }
                Project_Task_Master Task = new Project_Task_Master();
                Task.Task_Name = _UserTokenLive.Task_Name;
                Task.Emp_Id = _LogedUser.User_ID;
                Task.Project_Id = _UserTokenLive.Project_Id;
                Task.Description = _UserTokenLive.Description;
                Task.Actual_Start_Date = _UserTokenLive.A_Start_Date;
                Task.Revised_Start_Date = _UserTokenLive.A_Start_Date;
                Task.Revised_End_Date = _UserTokenLive.R_End_Date;
                Task.Status = 0;
                Task.Task_No = 0;
                Task.Duration = _UserTokenLive.Duration;
                Task.Task_Type = _UserTokenLive.Task_Type;
                Task.Duration_Unit = _UserTokenLive.Duration_Unit;
                db.Project_Task_Master.Add(Task);
                Comment_Master cmt = new Comment_Master();
                cmt.Comment_Type = 2;
                cmt.Comments = "New Task Update !";
                cmt.Commented_Date = DateTime.Now;
                cmt.Emp_Id = _LogedUser.User_ID;
                Task.Comment_Master.Add(cmt);
                db.SaveChanges();
                return new { rescode = 0, resmsg = "Add Project Task Successfully !" };
            }
            else
            {
                Project_Task_Master Task = db.Project_Task_Master.Where(x => x.Task_Id == _UserTokenLive.Task_Id && x.Emp_Id == _LogedUser.User_ID).FirstOrDefault();
                if (Task == null)
                {
                    data = new { rescode = 2, resmsg = "task Invalid !" };
                    return data;

                }
                Task.Task_Name = _UserTokenLive.Task_Name;
                Task.Emp_Id = _LogedUser.User_ID;
                Task.Project_Id = _UserTokenLive.Project_Id;
                Task.Description = _UserTokenLive.Description;
                Task.Actual_Start_Date = _UserTokenLive.A_Start_Date;
                Task.Revised_Start_Date = _UserTokenLive.A_Start_Date;
                Task.Revised_End_Date = _UserTokenLive.R_End_Date;
                Task.Task_Type = _UserTokenLive.Task_Type;
                Task.Duration_Unit = _UserTokenLive.Duration_Unit;
                Task.Duration = _UserTokenLive.Duration;
                Comment_Master cmt = new Comment_Master();
                cmt.Comment_Type = 2;
                cmt.Comments = "New Task Update !";
                cmt.Commented_Date = DateTime.Now;
                cmt.Emp_Id = _LogedUser.User_ID;
                Task.Comment_Master.Add(cmt);
                db.SaveChanges();
                return new { rescode = 0, resmsg = " Project Task Updated Successfully !" };
            }
        }

        [HttpPost]
        public async Task<object> GetProjectTask(IO_ProjectTask_View _UserTokenLive)
        {
            var data = new object();
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState
                                 where item.Value.Errors.Any()
                                 select item.Value.Errors[0].ErrorMessage).ToList();
                data = new { rescode = 2, resmsg = "Please Validate Your Input!", errors = errorList };
                return data;
            }
            UserMaster _LogedUser = new UserMaster();
            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null) { return new { rescode = 1, resmsg = "UnAthorised Session !" }; }
            _LogedUser = _token.UserMaster;

            if (_UserTokenLive.Task_Id == 0)
            {
                List<Project_Task_Master> _project = db.Project_Task_Master.Where(x => x.Emp_Id == _LogedUser.User_ID & x.Status != 6).OrderByDescending(x => x.Task_Id).ToList();
                List<Project_Task_MasterIO> _projectList = new List<Project_Task_MasterIO>();
                foreach (Project_Task_Master _t in _project)
                {
                    _projectList.Add(new Project_Task_MasterIO(_t));
                }
                return new { rescode = 0, resmsg = "get data!", Get_Project_List = _projectList };
            }
            else
            {
                Project_Task_Master _project = db.Project_Task_Master.Where(x => x.Task_Id == _UserTokenLive.Task_Id && x.Emp_Id == _LogedUser.User_ID).FirstOrDefault();
                if (_project == null)
                {
                    return new { rescode = 2, resmsg = "No Task Found ! " };
                }
                return new { rescode = 0, resmsg = "get data!", Get_Project_List = new Project_Task_MasterIO(_project) };
            }
        }

        [HttpPost]
        public async Task<object> Project_task_Status_Update(IO_ProjectTask_Status _UserTokenLive)
        {

            var data = new object();
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState
                                 where item.Value.Errors.Any()
                                 select item.Value.Errors[0].ErrorMessage).ToList();
                data = new { rescode = 2, resmsg = "Please Validate Your Input!", errors = errorList };
                return data;
            }
            UserMaster _LogedUser = new UserMaster();
            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null) { if (_token == null) { return new { rescode = 1, resmsg = "UnAthorised Session !" }; } }
            _LogedUser = _token.UserMaster;
            Project_Task_Master _project = db.Project_Task_Master.Where(x => x.Task_Id == _UserTokenLive.Task_Id && x.Emp_Id == _LogedUser.User_ID).FirstOrDefault();
            if (_UserTokenLive.Status == 1)
            {
                _project.Status = (int?)_UserTokenLive.Status;
                Comment_Master cmt = new Comment_Master();
                cmt.Comment_Type = 2;
                cmt.Comments = "Task Running Started";
                cmt.Commented_Date = DateTime.Now;
                cmt.Emp_Id = _LogedUser.User_ID;
                cmt.Task_Id = _UserTokenLive.Task_Id;
                db.Comment_Master.Add(cmt);
                db.SaveChanges();
                return new { rescode = 0, resmsg = "Task Running Started" };
            }

            if (_UserTokenLive.Status == 2)
            {
                _project.Status = (int?)_UserTokenLive.Status;
                Comment_Master cmt = new Comment_Master();
                cmt.Comment_Type = 2;
                cmt.Comments = "Task InProgress";
                cmt.Commented_Date = DateTime.Now;
                cmt.Emp_Id = _LogedUser.User_ID;
                cmt.Task_Id = _UserTokenLive.Task_Id;
                db.Comment_Master.Add(cmt);
                db.SaveChanges();
                return new { rescode = 0, resmsg = "Task InProgress" };
            }
            
            else if (_UserTokenLive.Status == 3)
            {
                _project.Status = (int?)_UserTokenLive.Status;
                Comment_Master cmt = new Comment_Master();
                cmt.Comment_Type = 2;
                cmt.Comments = "Task has been Stopped";
                cmt.Commented_Date = DateTime.Now;
                cmt.Emp_Id = _LogedUser.User_ID;
                cmt.Task_Id = _UserTokenLive.Task_Id;
                db.Comment_Master.Add(cmt);
                db.SaveChanges();
                return new { rescode = 0, resmsg = "Task has been Stopped" };
            }
            else if (_UserTokenLive.Status == 4)
            {
                Comment_Master cmt = new Comment_Master();
                cmt.Comment_Type = 2;
                cmt.Comments = "Task has been Completed";
                cmt.Commented_Date = DateTime.Now;
                cmt.Emp_Id = _LogedUser.User_ID;
                cmt.Task_Id = _UserTokenLive.Task_Id;
                db.Comment_Master.Add(cmt);
                _project.Status = (int?)_UserTokenLive.Status;
                db.SaveChanges();
                return new { rescode = 0, resmsg = "Task has been Completed" };
            }
            else
            {
                return new { rescode = 2, resmsg = "Invalid Status" };
            }
        }

        [HttpPost]
        public async Task<object> AddComments(IO_AddComments _UserTokenLive)
        {
            var data = new object();
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState
                                 where item.Value.Errors.Any()
                                 select item.Value.Errors[0].ErrorMessage).ToList();
                data = new { rescode = 2, resmsg = "Please Validate Your Input!", errors = errorList };
                return data;
            }
            UserMaster _LogedUser = new UserMaster();
            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null) { if (_token == null) { return new { rescode = 1, resmsg = "UnAthorised Session !" }; } }
            _LogedUser = _token.UserMaster;
            Comment_Master comment_Master = new Comment_Master();
            Comment_Master cm = new Comment_Master();
            cm.Comments = _UserTokenLive.Comments;
            cm.Task_Id = _UserTokenLive.Task_Id;
            cm.Comment_Type = 1;
            cm.Emp_Id = _LogedUser.User_ID;
            cm.Commented_Date = DateTime.Now;
            cm.Add_By = _LogedUser.User_ID;
            cm.Add_On = DateTime.Now;
            db.Comment_Master.Add(cm);
            db.SaveChanges();
            GetCommentListIO _AddComment_list = new GetCommentListIO(cm);
            return new { rescode = 0, resmsg = "Commented Successfully !", AddComment_list = _AddComment_list };

        }

        [HttpPost]
        public async Task<object> EditComment(IO_EditComment _UserTokenLive)
        {
            var data = new object();
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState
                                 where item.Value.Errors.Any()
                                 select item.Value.Errors[0].ErrorMessage).ToList();
                data = new { rescode = 2, resmsg = "Please Validate Your Input!", errors = errorList };
                return data;
            }
            UserMaster _LogedUser = new UserMaster();
            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null) { if (_token == null) { return new { rescode = 1, resmsg = "UnAthorised Session !" }; } }
            _LogedUser = _token.UserMaster;
            Comment_Master _cmt = db.Comment_Master.Where(x => x.Comnt_ID == _UserTokenLive.Comment_id).FirstOrDefault();
            _cmt.Comments = _UserTokenLive.Comments;
            db.SaveChanges();
            GetCommentListIO _AddComment_list = new GetCommentListIO(_cmt);
            return new { rescode = 0, resmsg = "Comment Updated Successfully !", UpdateComment_list = _AddComment_list };
        }

        [HttpPost]
        public async Task<object> DeleteComment(IO_DeletetoComment _UserTokenLive)
        {
            var data = new object();
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState
                                 where item.Value.Errors.Any()
                                 select item.Value.Errors[0].ErrorMessage).ToList();
                data = new { rescode = 2, resmsg = "Please Validate Your Input!", errors = errorList };
                return data;
            }
            UserMaster _LogedUser = new UserMaster();
            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null) { if (_token == null) { return new { rescode = 1, resmsg = "UnAthorised Session !" }; } }
            Comment_Master _del = db.Comment_Master.Find(_UserTokenLive.Comment_id);
            db.Comment_Master.Remove(_del);
            db.SaveChanges();
            return new { rescode = 0, resmsg = "Comment Deleted Successfully !" };
        }

        [HttpPost]
        public async Task<object> GetLeaveList(IO_getUser _UserTokenLive)
        {
            var data = new object();
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState
                                 where item.Value.Errors.Any()
                                 select item.Value.Errors[0].ErrorMessage).ToList();
                data = new { rescode = 2, resmsg = "Please Validate Your Input!", errors = errorList };
                return data;
            }

            UserMaster _LogedUser = new UserMaster();
            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null)
            {

                return new { rescode = 1, resmsg = "UnAthorised Session !" };

            }
            _LogedUser = _token.UserMaster;
            List<LeaveListIO> _leaveList = new List<LeaveListIO>();
            foreach (Leave_Master _l in db.Leave_Master.Where(x => x.User_Id == _LogedUser.User_ID & x.Leave_Crdr == 2 & x.Status != 6).OrderByDescending(x => x.Status).ThenBy(x => x.From_Date).ToList())
            {
                _leaveList.Add(new LeaveListIO(_l));
            }
            return new { rescode = 0, resmsg = "get data!", Get_LeaveList = _leaveList };
        }

        [HttpPost]
        public async Task<object> AddLeave(IO_AddtoLeave _UserTokenLive)
        {
            var data = new object();
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState
                                 where item.Value.Errors.Any()
                                 select item.Value.Errors[0].ErrorMessage).ToList();
                data = new { rescode = 2, resmsg = "Please Validate Your Input!", errors = errorList };
                return data;
            }
            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null) { if (_token == null) { return new { rescode = 1, resmsg = "UnAthorised Session !" }; } }
            UserMaster _LogedUser = _token.UserMaster;
            List<Holiday_Master> holidays = db.Holiday_Master.ToList();
            int days = (_UserTokenLive.To_Date - _UserTokenLive.From_Date).Days + 1;
            List<DateTime> leaveDays = new List<DateTime>();
            List<DateTime> leaveDaysNew = new List<DateTime>();

            if (_UserTokenLive.Leave_Type == "SL")
            {
                if (_UserTokenLive.From_Date < DateTime.Now.Date)
                {
                    return new { rescode = 1, resmsg = "From Date must be after Today" };
                }
                if (_UserTokenLive.To_Date < _UserTokenLive.From_Date)
                {
                    return new { rescode = 1, resmsg = "To Date Can't before From Date" };
                }

                //List<Leave_Master> leaves = db.Leave_Master.Where(x => x.Status != 6 && x.User_Id == _LogedUser.User_ID).ToList();
                //foreach (Leave_Master leave in leaves)
                //{
                //    if (leave.From_Date == _UserTokenLive.From_Date)
                //    {
                //        return new { rescode = 0, resmsg = "Leave request already exist !" };
                //    }
                //}
                foreach (Holiday_Master hm in holidays)
                {
                    for (var day = _UserTokenLive.From_Date; day <= _UserTokenLive.To_Date; day = day.AddDays(1))
                    {
                        if (day.DayOfWeek == DayOfWeek.Sunday || day.Date == hm.Holiday_Date)
                        {
                            return new { rescode = 1, resmsg = "Leave days contains holiday!" };
                        }
                    }
                }
                TimeSpan l = _UserTokenLive.To_Date.Subtract(_UserTokenLive.From_Date);
                double answer = (_UserTokenLive.To_Date - _UserTokenLive.From_Date).TotalDays;
                int Days = Convert.ToInt32(answer);
                decimal _TotalDays = Days;
                if (_UserTokenLive.F_Date == 1)
                {
                    _TotalDays = _TotalDays - decimal.Parse("0.5");
                }
                if (_UserTokenLive.T_Date == 1)
                {
                    _TotalDays = _TotalDays - decimal.Parse("0.5");
                }
                _TotalDays = _TotalDays + 1;
                if (_TotalDays <= 0)
                {
                    return new { rescode = 1, resmsg = "Can't Add 0 Leave" };
                }
                Leave_Master Leave = new Leave_Master();
                Leave.User_Id = _LogedUser.User_ID;
                Leave.From_Date = _UserTokenLive.From_Date;
                Leave.To_Date = _UserTokenLive.To_Date;
                Leave.Leave_Crdr = 2;
                Leave.Add_By = _LogedUser.User_ID;
                Leave.ReqDate = DateTime.Now;
                Leave.Reason = _UserTokenLive.Reason;
                Leave.Leave_Type = _UserTokenLive.Leave_Type;
                Leave.Status = 0;
                Leave.leave_days = _TotalDays;
                if (_UserTokenLive.F_Date == 1)
                {
                    Leave.F_Date = 1;
                }
                else
                {
                    Leave.F_Date = 0;
                }
                if (_UserTokenLive.T_Date == 1)
                {
                    Leave.T_Date = 1;
                }
                else
                {
                    Leave.T_Date = 0;
                }


                if (Leave.Leave_Type == "PL" && Leave.leave_days > _LogedUser.Total_PL)
                {
                    return new { rescode = 1, resmsg = "The Employee Does Not Have PL" };
                }
                else if (Leave.Leave_Type == "CL" && Leave.leave_days > _LogedUser.Total_CL)
                {
                    return new { rescode = 1, resmsg = "The Employee Does Not Have CL" };
                }
                else if (Leave.Leave_Type == "EL" && Leave.leave_days > _LogedUser.Total_EL)
                {
                    return new { rescode = 1, resmsg = "The Employee Does Not Have EL" };
                }
                db.Leave_Master.Add(Leave);
                db.SaveChanges();
                return new { rescode = 0, resmsg = "Leave Added Successfully." };
            }
            if (days == 1)
            {
                if (_UserTokenLive.Leave_Type != "EL")
                {
                    if (_UserTokenLive.From_Date < DateTime.Now.Date.AddDays(1))
                    {
                        return new { rescode = 1, resmsg = "Leave for 1 to 3 days should be planned before 2 days !" };
                    }
                    if (_UserTokenLive.From_Date == DateTime.Now.Date.AddDays(1))
                    {
                        return new { rescode = 1, resmsg = "Leave for 1 to 3 days should be planned before 2 days !" };
                    }
                    if (_UserTokenLive.To_Date < _UserTokenLive.From_Date)
                    {
                        return new { rescode = 1, resmsg = "To Date Can't before From Date  !" };
                    }
                    List<Leave_Master> leaves = db.Leave_Master.Where(x => x.Status != 6 && x.User_Id == _LogedUser.User_ID).ToList();
                    foreach (Leave_Master leave in leaves)
                    {
                        if (leave.From_Date == _UserTokenLive.From_Date)
                        {
                            return new { rescode = 1, resmsg = "Leave request already exist !" };
                        }
                    }
                    foreach (Holiday_Master hm in holidays)
                    {
                        for (var day = _UserTokenLive.From_Date; day <= _UserTokenLive.To_Date; day = day.AddDays(1))
                        {
                            if (day.DayOfWeek == DayOfWeek.Sunday || day.Date == hm.Holiday_Date)
                            {
                                return new { rescode = 1, resmsg = "Leave days contains holiday!" };
                            }
                        }
                    }
                    TimeSpan l = _UserTokenLive.To_Date.Subtract(_UserTokenLive.From_Date);
                    double answer = (_UserTokenLive.To_Date - _UserTokenLive.From_Date).TotalDays;
                    int Days = Convert.ToInt32(answer);
                    decimal _TotalDays = Days;
                    if (_UserTokenLive.F_Date == 1)
                    {
                        _TotalDays = _TotalDays - decimal.Parse("0.5");
                    }
                    if (_UserTokenLive.T_Date == 1)
                    {
                        _TotalDays = _TotalDays - decimal.Parse("0.5");
                    }
                    _TotalDays = _TotalDays + 1;
                    if (_TotalDays <= 0)
                    {
                        return new { rescode = 1, resmsg = "Can't Add 0 Leave" };
                    }
                    Leave_Master Leave = new Leave_Master();
                    Leave.User_Id = _LogedUser.User_ID;
                    Leave.From_Date = _UserTokenLive.From_Date;
                    Leave.To_Date = _UserTokenLive.To_Date;
                    Leave.Leave_Crdr = 2;
                    Leave.Add_By = _LogedUser.User_ID;
                    Leave.ReqDate = DateTime.Now;
                    Leave.Reason = _UserTokenLive.Reason;
                    Leave.Leave_Type = _UserTokenLive.Leave_Type;
                    Leave.Status = 0;
                    Leave.leave_days = _TotalDays;
                    if (_UserTokenLive.F_Date == 1)
                    {
                        Leave.F_Date = 1;
                    }
                    else
                    {
                        Leave.F_Date = 0;
                    }
                    if (_UserTokenLive.T_Date == 1)
                    {
                        Leave.T_Date = 1;
                    }
                    else
                    {
                        Leave.T_Date = 0;
                    }


                    if (Leave.Leave_Type == "PL" && Leave.leave_days > _LogedUser.Total_PL)
                    {
                        return new { rescode = 1, resmsg = "The Employee Does Not Have PL" };
                    }
                    else if (Leave.Leave_Type == "CL" && Leave.leave_days > _LogedUser.Total_CL)
                    {
                        return new { rescode = 1, resmsg = "The Employee Does Not Have CL" };
                    }
                    else if (Leave.Leave_Type == "EL" && Leave.leave_days > _LogedUser.Total_EL)
                    {
                        return new { rescode = 1, resmsg = "The Employee Does Not Have EL" };
                    }
                    db.Leave_Master.Add(Leave);
                    db.SaveChanges();
                    return new { rescode = 0, resmsg = "Leave Added Successfully." };
                }
            }
            else if (days == 2 || days == 3)
            {
                if (_UserTokenLive.From_Date < DateTime.Now.Date.AddDays(1))
                {
                    return new { rescode = 1, resmsg = "Leave for 1 to 3 days should be planned before 2 days !" };
                }
                if (_UserTokenLive.From_Date == DateTime.Now.Date.AddDays(1))
                {
                    return new { rescode = 1, resmsg = "Leave for 1 to 3 days should be planned before 2 days !" };
                }
                if (_UserTokenLive.To_Date < _UserTokenLive.From_Date)
                {
                    return new { rescode = 1, resmsg = "To Date Can't before From Date  !" };

                }
                for (var day = _UserTokenLive.From_Date.Date; day <= _UserTokenLive.To_Date; day = day.AddDays(1))
                {
                    leaveDays.Add(day);
                }
                //foreach (DateTime d in leaveDays)
                //{
                //    List<Leave_Master> leaves = db.Leave_Master.Where(x => x.Status != 6 && x.User_Id == _LogedUser.User_ID).ToList();
                //    foreach (Leave_Master leave in leaves)
                //    {
                //        if (leave != null)
                //        {
                //            DateTime fd = leave.From_Date;
                //            DateTime td = leave.To_Date;
                //            for (var day = fd.Date; day <= td; day = day.AddDays(1))
                //            {
                //                if (day == d)
                //                {
                //                    return new { rescode = 0, resmsg = "Leave request already exist !" };
                //                }
                //                else
                //                {
                //                    leaveDaysNew.Add(day);
                //                }
                //            }

                //        }
                //    }
                //}
                foreach (Holiday_Master hm in holidays)
                {
                    for (var day = _UserTokenLive.From_Date.Date; day <= _UserTokenLive.To_Date; day = day.AddDays(1))
                    {
                        if (day.DayOfWeek == DayOfWeek.Sunday || day.Date == hm.Holiday_Date)
                        {
                            return new { rescode = 1, resmsg = "Leave days contains holiday!" };
                        }
                    }
                }

                if (_UserTokenLive.From_Date < DateTime.Now.Date)
                {
                    return new { rescode = 1, resmsg = "Yesterday Leave Not Consider!" };
                }

                TimeSpan l = _UserTokenLive.To_Date.Subtract(_UserTokenLive.From_Date);
                double answer = (_UserTokenLive.To_Date - _UserTokenLive.From_Date).TotalDays;
                int Days = Convert.ToInt32(answer);
                decimal _TotalDays = Days;
                if (_UserTokenLive.F_Date == 1)
                {
                    _TotalDays = _TotalDays - decimal.Parse("0.5");
                }
                if (_UserTokenLive.T_Date == 1)
                {
                    _TotalDays = _TotalDays - decimal.Parse("0.5");
                }
                _TotalDays = _TotalDays + 1;
                if (_TotalDays <= 0)
                {
                    return new { rescode = 1, resmsg = "Can't Add 0 Leave" };
                }
                Leave_Master Leave = new Leave_Master();
                Leave.User_Id = _LogedUser.User_ID;
                Leave.From_Date = _UserTokenLive.From_Date;
                Leave.To_Date = _UserTokenLive.To_Date;
                Leave.Leave_Crdr = 2;
                Leave.Add_By = _LogedUser.User_ID;
                Leave.ReqDate = DateTime.Now;
                Leave.Reason = _UserTokenLive.Reason;
                Leave.Leave_Type = _UserTokenLive.Leave_Type;
                Leave.Status = 0;
                Leave.leave_days = _TotalDays;
                if (_UserTokenLive.F_Date == 1)
                {
                    Leave.F_Date = 1;
                }
                else
                {
                    Leave.F_Date = 0;
                }
                if (_UserTokenLive.T_Date == 1)
                {
                    Leave.T_Date = 1;
                }
                else
                {
                    Leave.T_Date = 0;
                }


                if (Leave.Leave_Type == "PL" && Leave.leave_days > _LogedUser.Total_PL)
                {
                    return new { rescode = 1, resmsg = "The Employee Does Not Have PL" };
                }
                else if (Leave.Leave_Type == "CL" && Leave.leave_days > _LogedUser.Total_CL)
                {
                    return new { rescode = 1, resmsg = "The Employee Does Not Have CL" };
                }
                else if (Leave.Leave_Type == "EL" && Leave.leave_days > _LogedUser.Total_EL)
                {
                    return new { rescode = 1, resmsg = "The Employee Does Not Have EL" };
                }
                db.Leave_Master.Add(Leave);
                db.SaveChanges();
                return new { rescode = 0, resmsg = "Leave Added Successfully." };

            }
            else if (days == 4 || days == 5 || days == 6)
            {
                if (_UserTokenLive.From_Date < DateTime.Now.Date.AddDays(4))
                {
                    return new { rescode = 1, resmsg = "Leave for 4 to 6 days should be planned before 5 days !" };
                }
                if (_UserTokenLive.From_Date == DateTime.Now.Date.AddDays(4))
                {
                    return new { rescode = 1, resmsg = "Leave for 4 to 6 days should be planned before 5 days !" };
                }
                if (_UserTokenLive.To_Date < _UserTokenLive.From_Date)
                {
                    return new { rescode = 1, resmsg = "To Date Can't before From Date  !" };
                }
                for (var day = _UserTokenLive.From_Date.Date; day <= _UserTokenLive.To_Date; day = day.AddDays(1))
                {
                    leaveDays.Add(day);
                }

                //List<Leave_Master> leaves = db.Leave_Master.Where(x => x.Status != 6 && x.User_Id == _LogedUser.User_ID).ToList();
                //foreach (Leave_Master leave in leaves)
                //{
                //    if (leave.From_Date == _UserTokenLive.From_Date)
                //    {
                //        return new { rescode = 0, resmsg = "Leave request already exist !" };
                //    }
                //}
                foreach (Holiday_Master hm in holidays)
                {
                    for (var day = _UserTokenLive.From_Date; day <= _UserTokenLive.To_Date; day = day.AddDays(1))
                    {
                        if (day.DayOfWeek == DayOfWeek.Sunday || day.Date == hm.Holiday_Date)
                        {
                            return new { rescode = 1, resmsg = "Leave days contains holiday!" };
                        }
                    }
                }
                TimeSpan l = _UserTokenLive.To_Date.Subtract(_UserTokenLive.From_Date);
                double answer = (_UserTokenLive.To_Date - _UserTokenLive.From_Date).TotalDays;
                int Days = Convert.ToInt32(answer);
                decimal _TotalDays = Days;
                if (_UserTokenLive.F_Date == 1)
                {
                    _TotalDays = _TotalDays - decimal.Parse("0.5");
                }
                if (_UserTokenLive.T_Date == 1)
                {
                    _TotalDays = _TotalDays - decimal.Parse("0.5");
                }
                _TotalDays = _TotalDays + 1;
                if (_TotalDays <= 0)
                {
                    return new { rescode = 1, resmsg = "Can't Add 0 Leave" };
                }
                Leave_Master Leave = new Leave_Master();
                Leave.User_Id = _LogedUser.User_ID;
                Leave.From_Date = _UserTokenLive.From_Date;
                Leave.To_Date = _UserTokenLive.To_Date;
                Leave.Leave_Crdr = 2;
                Leave.Add_By = _LogedUser.User_ID;
                Leave.ReqDate = DateTime.Now;
                Leave.Reason = _UserTokenLive.Reason;
                Leave.Leave_Type = _UserTokenLive.Leave_Type;
                Leave.Status = 0;
                Leave.leave_days = _TotalDays;
                if (_UserTokenLive.F_Date == 1)
                {
                    Leave.F_Date = 1;
                }
                else
                {
                    Leave.F_Date = 0;
                }
                if (_UserTokenLive.T_Date == 1)
                {
                    Leave.T_Date = 1;
                }
                else
                {
                    Leave.T_Date = 0;
                }


                if (Leave.Leave_Type == "PL" && Leave.leave_days > _LogedUser.Total_PL)
                {
                    return new { rescode = 1, resmsg = "The Employee Does Not Have PL" };
                }
                else if (Leave.Leave_Type == "CL" && Leave.leave_days > _LogedUser.Total_CL)
                {
                    return new { rescode = 1, resmsg = "The Employee Does Not Have CL" };
                }
                else if (Leave.Leave_Type == "EL" && Leave.leave_days > _LogedUser.Total_EL)
                {
                    return new { rescode = 1, resmsg = "The Employee Does Not Have EL" };
                }
                db.Leave_Master.Add(Leave);
                db.SaveChanges();
                return new { rescode = 0, resmsg = "Leave Added Successfully." };
            }
            else if (days >= 6)
            {
                if (_UserTokenLive.Leave_Type == "PL" && _UserTokenLive.Leave_Type == "CL" && _UserTokenLive.Leave_Type == "SL")
                {
                    if (_UserTokenLive.From_Date < DateTime.Now.Date.AddDays(9))
                    {
                        return new { rescode = 1, resmsg = "Leave for 6 or more days should be planned before 10 days !" };
                    }
                    if (_UserTokenLive.From_Date == DateTime.Now.Date.AddDays(9))
                    {
                        return new { rescode = 1, resmsg = "Leave for 6 or more days should be planned before 10 days !" };
                    }
                    if (_UserTokenLive.To_Date < _UserTokenLive.From_Date)
                    {
                        return new { rescode = 1, resmsg = "To Date Can't before From Date  !" };
                    }
                    for (var day = _UserTokenLive.From_Date.Date; day <= _UserTokenLive.To_Date; day = day.AddDays(1))
                    {
                        leaveDays.Add(day);
                    }

                }
                //List<Leave_Master> leaves = db.Leave_Master.Where(x => x.Status != 6 && x.User_Id == _LogedUser.User_ID).ToList();
                //    foreach (Leave_Master leave in leaves)
                //    {
                //        if (leave.From_Date == _UserTokenLive.From_Date)
                //        {
                //            return new { rescode = 0, resmsg = "Leave request already exist !" };
                //        }
                //    }
                foreach (Holiday_Master hm in holidays)
                {
                    for (var day = _UserTokenLive.From_Date; day <= _UserTokenLive.To_Date; day = day.AddDays(1))
                    {
                        if (day.DayOfWeek == DayOfWeek.Sunday || day.Date == hm.Holiday_Date)
                        {
                            return new { rescode = 1, resmsg = "Leave days contains holiday!" };
                        }
                    }
                }
                TimeSpan l = _UserTokenLive.To_Date.Subtract(_UserTokenLive.From_Date);
                double answer = (_UserTokenLive.To_Date - _UserTokenLive.From_Date).TotalDays;
                int Days = Convert.ToInt32(answer);
                decimal _TotalDays = Days;
                if (_UserTokenLive.F_Date == 1)
                {
                    _TotalDays = _TotalDays - decimal.Parse("0.5");
                }
                if (_UserTokenLive.T_Date == 1)
                {
                    _TotalDays = _TotalDays - decimal.Parse("0.5");
                }
                _TotalDays = _TotalDays + 1;
                if (_TotalDays <= 0)
                {
                    return new { rescode = 1, resmsg = "Can't Add 0 Leave" };
                }
                Leave_Master Leave = new Leave_Master();
                Leave.User_Id = _LogedUser.User_ID;
                Leave.From_Date = _UserTokenLive.From_Date;
                Leave.To_Date = _UserTokenLive.To_Date;
                Leave.Leave_Crdr = 2;
                Leave.Add_By = _LogedUser.User_ID;
                Leave.ReqDate = DateTime.Now;
                Leave.Reason = _UserTokenLive.Reason;
                Leave.Leave_Type = _UserTokenLive.Leave_Type;
                Leave.Status = 0;
                Leave.leave_days = _TotalDays;
                if (_UserTokenLive.F_Date == 1)
                {
                    Leave.F_Date = 1;
                }
                else
                {
                    Leave.F_Date = 0;
                }
                if (_UserTokenLive.T_Date == 1)
                {
                    Leave.T_Date = 1;
                }
                else
                {
                    Leave.T_Date = 0;
                }


                if (Leave.Leave_Type == "PL" && Leave.leave_days > _LogedUser.Total_PL)
                {
                    return new { rescode = 1, resmsg = "The Employee Does Not Have PL" };
                }
                else if (Leave.Leave_Type == "CL" && Leave.leave_days > _LogedUser.Total_CL)
                {
                    return new { rescode = 1, resmsg = "The Employee Does Not Have CL" };
                }
                else if (Leave.Leave_Type == "EL" && Leave.leave_days > _LogedUser.Total_EL)
                {
                    return new { rescode = 1, resmsg = "The Employee Does Not Have EL" };
                }
                db.Leave_Master.Add(Leave);
                db.SaveChanges();
                return new { rescode = 0, resmsg = "Leave Added Successfully." };

            }

            List<LeaveListIO> _AddleaveList = new List<LeaveListIO>();
            foreach (Leave_Master _l in db.Leave_Master.Where(x => x.User_Id == _LogedUser.User_ID & x.Leave_Crdr == 2 & x.Status != 6).OrderByDescending(x => x.Status).ThenBy(x => x.From_Date).ToList())
            {
                _AddleaveList.Add(new LeaveListIO(_l));
            }
            return new { rescode = 0, resmsg = "Leave List", Get_LeaveList = _AddleaveList };


        }

        [HttpPost]
        public async Task<object> GetAdvanceReqList(IO_getUser _UserTokenLive)
        {
            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null) { if (_token == null) { return new { rescode = 1, resmsg = "UnAthorised Session !" }; } }
            UserMaster _LogedUser = _token.UserMaster;
            if (_UserTokenLive.Voucher == null) { _UserTokenLive.Voucher = 0; }
            List<AdvanceReqListIO> _reqList = new List<AdvanceReqListIO>();
            List<Request_Master> Request = new List<Request_Master>();
            if (_UserTokenLive.Voucher == 1)
            {
                Request = db.Request_Master.Where(x => x.User_Id == _LogedUser.User_ID && x.Voucher_Master == null & x.Status != 6).OrderByDescending(x => x.Req_Id).ToList();
            }
            else
            {
                Request = db.Request_Master.Where(x => x.User_Id == _LogedUser.User_ID & x.Status != 6).OrderByDescending(x => x.Req_Id).ToList();
            }
            foreach (Request_Master _A in Request)
            {
                _reqList.Add(new AdvanceReqListIO(_A));
            }
            if (_reqList.Count == 0)
            {
                return new { rescode = 2, resmsg = "No Data" };
            }
            else
            {
                return new { rescode = 0, resmsg = "Your Request ID ", Requests = _reqList };
            }
        }

        [HttpPost]
        public async Task<object> AddAdvanceReq(IO_AddtoAdvanceReq _UserTokenLive)
        {
            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null) { if (_token == null) { return new { rescode = 1, resmsg = "UnAthorised Session !" }; } }
            UserMaster _LogedUser = _token.UserMaster;
           
            if (_UserTokenLive.Requested_Amount <= 0) { return new { rescode = 0, resmsg = "Sorry, Can't Create Request for 0 Amount" }; }

            Request_Master Request = new Request_Master();
            Request.User_Id = _LogedUser.User_ID;
            Request.Parent_Type = _UserTokenLive.Parent_Type;
            if (_UserTokenLive.Parent_Type == "Personal Advance")
            {
                Request.S_Date = DateTime.Now;
                Request.E_Date = DateTime.Now;
                Request.Place = "";
                Request.Remark = "";
            }
            else
            {
                if (_UserTokenLive.S_Date < DateTime.Now.Date) { return new { rescode = 0, resmsg = "Start Date must after Today" }; }
                if (_UserTokenLive.E_Date < _UserTokenLive.S_Date) { return new { rescode = 0, resmsg = "End Date Can't before Start Date" }; }
                Request.S_Date = _UserTokenLive.S_Date;
                Request.E_Date = _UserTokenLive.E_Date;
                if (_UserTokenLive.Project_Id != null)
                {
                    Request.Project_Id = _UserTokenLive.Project_Id;
                }
                Request.Place = _UserTokenLive.Place;
            }
            Request.Requested_Amount = _UserTokenLive.Requested_Amount;
            Request.Remark = _UserTokenLive.Remark;
            Request.Description = _UserTokenLive.Description;
            Request.A_Date = DateTime.Now;
            Request.Granted_Amount = 0;
            Request.Status = 0;
            Request.ReqDate = DateTime.Now; 
            db.Request_Master.Add(Request);
            db.SaveChanges();
            return new { rescode = 0, resmsg = "Request Added Successfully." };
        }

        [HttpPost]
        public async Task<object> GetVoucherList(IO_GetbyStatus _UserTokenLive)
        {
            var data = new object();
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState
                                 where item.Value.Errors.Any()
                                 select item.Value.Errors[0].ErrorMessage).ToList();
                data = new { rescode = 2, resmsg = "Please Validate Your Input!", errors = errorList };
                return data;
            }
            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null) { if (_token == null) { return new { rescode = 1, resmsg = "UnAthorised Session !" }; } }
            UserMaster _LogedUser = _token.UserMaster;

            if (_UserTokenLive.Status == null) { _UserTokenLive.Status = 99; }
            List<Voucher_Master> Voucher = new List<Voucher_Master>();
            if (_UserTokenLive.Status == 99)
            {
                Voucher = db.Voucher_Master.Where(x => x.User_Id == _LogedUser.User_ID & x.Status != 6).OrderByDescending(X => X.Voucher_Id).ToList();
            }
            else
            {
                Voucher = db.Voucher_Master.Where(x => x.User_Id == _LogedUser.User_ID && x.Status == _UserTokenLive.Status).OrderByDescending(X => X.Voucher_Id).ToList();
            }
            List<VoucherListIO> _VoucherList = new List<VoucherListIO>();
            foreach (Voucher_Master _A in Voucher)
            {
                _VoucherList.Add(new VoucherListIO(_A));
            }
            return new { rescode = 0, resmsg = "get data!", Get_VoucherList = _VoucherList };
        }

        [HttpPost]
        public async Task<object> AddVoucherReq(IO_AddtoVoucherReq _UserTokenLive)
        {
            var data = new object();
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState
                                 where item.Value.Errors.Any()
                                 select item.Value.Errors[0].ErrorMessage).ToList();
                data = new { rescode = 2, resmsg = "Please Validate Your Input!", errors = errorList };
                return data;
            }
            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null) { if (_token == null) { return new { rescode = 1, resmsg = "UnAthorised Session !" }; } }
            UserMaster _LogedUser = _token.UserMaster;

            if (_UserTokenLive.Voucher_Id == 0)
            {
                Voucher_Master Voucher = new Voucher_Master();
                Voucher.User_Id = _LogedUser.User_ID;
                Voucher.Status = 0;
                Voucher.Voucher_Date = DateTime.Now;
                Voucher.Payment_Mode = "Online";
                Voucher.Place = _UserTokenLive.Place;
                Voucher.Remark = _UserTokenLive.Remark;
                Voucher.Petrol = _UserTokenLive.Petrol;
                Voucher.Travelling = _UserTokenLive.Travelling;
                Voucher.Mobile = _UserTokenLive.Mobile;
                Voucher.Other = _UserTokenLive.Other;
                Voucher.Conveyance = _UserTokenLive.Conveyance;
                Voucher.Parent_Type = _UserTokenLive.Parent_Type;
                Voucher.Description = _UserTokenLive.Description;
                Voucher.Total_Amount = _UserTokenLive.Petrol + _UserTokenLive.Travelling + _UserTokenLive.Mobile + _UserTokenLive.Conveyance + _UserTokenLive.Other;

                Voucher.Project_Id = _UserTokenLive.Project_Id;
                Voucher.Req_Id = _UserTokenLive.ReqNo;

                db.Voucher_Master.Add(Voucher);
                db.SaveChanges();

                return new { rescode = 0, resmsg = "Voucher Request Added Successfully." };
            }
            else
            {
                Voucher_Master Voucher = db.Voucher_Master.Where(x => x.Voucher_Id == _UserTokenLive.Voucher_Id).FirstOrDefault();
                if (Voucher.Status != 0) { return new { rescode = 0, resmsg = "Voucher Can't Update Once Approve / Reject !" }; }
                Voucher.Project_Id = _UserTokenLive.Project_Id;
                Voucher.Req_Id = _UserTokenLive.ReqNo;

                Voucher.Place = _UserTokenLive.Place;
                Voucher.Remark = _UserTokenLive.Remark;
                Voucher.Petrol = _UserTokenLive.Petrol;
                Voucher.Travelling = _UserTokenLive.Travelling;
                Voucher.Mobile = _UserTokenLive.Mobile;
                Voucher.Other = _UserTokenLive.Other;
                Voucher.Conveyance = _UserTokenLive.Conveyance;
                Voucher.Parent_Type = _UserTokenLive.Parent_Type;
                Voucher.Description = _UserTokenLive.Description;
                Voucher.Total_Amount = _UserTokenLive.Petrol + _UserTokenLive.Travelling + _UserTokenLive.Mobile + _UserTokenLive.Conveyance + _UserTokenLive.Other;

                db.SaveChanges();
                return new { rescode = 0, resmsg = "Voucher Updated Successfully." };
            }

        }

        [HttpPost]
        public async Task<object> GetEmpStatement(IO_getUser _UserTokenLive)
        {
            var data = new object();
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState
                                 where item.Value.Errors.Any()
                                 select item.Value.Errors[0].ErrorMessage).ToList();
                data = new { rescode = 2, resmsg = "Please Validate Your Input!", errors = errorList };
                return data;
            }
            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null) { if (_token == null) { return new { rescode = 1, resmsg = "UnAthorised Session !" }; } }
            UserMaster _LogedUser = _token.UserMaster;
            List<EmployeeStatementListIO> _EmpList = new List<EmployeeStatementListIO>();
            foreach (Transaction_Master _T in db.Transaction_Master.Where(x => x.User_id == _LogedUser.User_ID).OrderByDescending(x => x.Trans_id).ToList())
            {
                _EmpList.Add(new EmployeeStatementListIO(_T));
            }
            return new { rescode = 0, resmsg = "get data!", Transactions = _EmpList };
        }

        [HttpPost]
        public async Task<object> GetMyAssets(IO_getUser _UserTokenLive)
        {
            var data = new object();
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState
                                 where item.Value.Errors.Any()
                                 select item.Value.Errors[0].ErrorMessage).ToList();
                data = new { rescode = 2, resmsg = "Please Validate Your Input!", errors = errorList };
                return data;
            }
            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null) { if (_token == null) { return new { rescode = 1, resmsg = "UnAthorised Session !" }; } }
            UserMaster _LogedUser = _token.UserMaster;

            List<Asset_Movement> Allmovment = db.Asset_Movement.ToList();
            List<Asset_Master> AllAssets = db.Asset_Master.ToList();

            List<Asset_Movement> movment = Allmovment.Where(x => x.From_id == _LogedUser.User_ID || x.To_id == _LogedUser.User_ID).OrderByDescending(x => x.Movement_id).ToList();
            List<AssetMovementListIO> _AssetList = new List<AssetMovementListIO>();
            foreach (Asset_Movement _A in movment)
            {
                _AssetList.Add(new AssetMovementListIO(_A));
            }

            List<AssetListIO> _myAssets = new List<AssetListIO>();
            foreach (Asset_Master _my in AllAssets)
            {
                movment = Allmovment.Where(y => y.Asset_id == _my.Asset_id).OrderByDescending(y => y.Movement_id).ToList();
                if (movment.Count != 0)
                {
                    if (movment.FirstOrDefault().To_id == _LogedUser.User_ID)
                    {
                        _myAssets.Add(new AssetListIO(_my));
                    }
                }
            }
            return new { rescode = 0, resmsg = "get data!", Get_AssetList = _AssetList, Employee_Assets = _myAssets };
        }

        [HttpPost]
        public async Task<object> GetDocuments(IO_getUser _UserTokenLive)
        {
            var data = new object();
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState
                                 where item.Value.Errors.Any()
                                 select item.Value.Errors[0].ErrorMessage).ToList();
                data = new { rescode = 2, resmsg = "Please Validate Your Input!", errors = errorList };
                return data;
            }
            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null) { if (_token == null) { return new { rescode = 1, resmsg = "UnAthorised Session !" }; } }
            UserMaster _LogedUser = _token.UserMaster;

            List<DocumentListIO> _DocList = new List<DocumentListIO>();
            foreach (Document_Master _D in db.Document_Master.Where(x => x.User_Id == _LogedUser.User_ID && x.Status==0).OrderByDescending(x => x.Doc_Id).ToList())
            {
                _DocList.Add(new DocumentListIO(_D));
            }
            return new { rescode = 0, resmsg = "get data!", Get_DocList = _DocList };
        }

        [HttpPost]
        public async Task<object> GetSalarySlips(IO_getUser _UserTokenLive)
        {
            var data = new object();
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState
                                 where item.Value.Errors.Any()
                                 select item.Value.Errors[0].ErrorMessage).ToList();
                data = new { rescode = 2, resmsg = "Please Validate Your Input!", errors = errorList };
                return data;
            }
            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null) { if (_token == null) { return new { rescode = 1, resmsg = "UnAthorised Session !" }; } }
            UserMaster _LogedUser = _token.UserMaster;

            List<SalarySlipListIO> _SalaryslipList = new List<SalarySlipListIO>();
            foreach (Generated_Documents _D in db.Generated_Documents.Where(x => x.Gen_doc_empid == _LogedUser.User_ID /*& x.Type == 2*/).OrderByDescending(x => x.Gen_doc_id).ToList())
            {
                _SalaryslipList.Add(new SalarySlipListIO(_D));
            }
            return new { rescode = 0, resmsg = "get data!", Get_SalaryslipList = _SalaryslipList };
        }

        [HttpPost]
        public async Task<object> NoticeboardList(IO_NoticeboardList _UserTokenLive)
        {
            var data = new object();
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState
                                 where item.Value.Errors.Any()
                                 select item.Value.Errors[0].ErrorMessage).ToList();
                data = new { rescode = 2, resmsg = "Please Validate Your Input!", errors = errorList };
                return data;
            }
            UserMaster _LogedUser = new UserMaster();
            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null)
            {
                return new { rescode = 1, resmsg = "UnAthorised Session !" };
            }
            _LogedUser = _token.UserMaster;
            List<NoticeBoard_Master> notices = db.NoticeBoard_Master.Where(x => x.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.Date).Take(5).ToList();
            List<GetNoticeBoardListIO> _Notice_list = new List<GetNoticeBoardListIO>();

            foreach (NoticeBoard_Master _obj in notices)
            {
                _Notice_list.Add(new GetNoticeBoardListIO(_obj));
            }
            data = new { rescode = 0, resmsg = "Get All NoticeBoardList!", All_Notice_Board_List = _Notice_list };
            return data;
        }

        [HttpPost]
        public async Task<object> EventList(IO_EventList _UserTokenLive)
        {
            var data = new object();
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState
                                 where item.Value.Errors.Any()
                                 select item.Value.Errors[0].ErrorMessage).ToList();
                data = new { rescode = 2, resmsg = "Please Validate Your Input!", errors = errorList };
                return data;
            }
            UserMaster _LogedUser = new UserMaster();
            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null)
            {
                return new { rescode = 1, resmsg = "UnAthorised Session !" };
            }
            _LogedUser = _token.UserMaster;

            var day = DateTime.Today;

            var monthh = day.AddMonths(5);

            List<UserMaster> user = db.UserMasters.Where(x => x.Company_ID == _LogedUser.Company_ID & x.status == 0).ToList().OrderBy(x => x.DOB).ToList();


            List<Event_Master> Event = db.Event_Master.Where(x => x.Company_ID == _LogedUser.Company_ID).Take(5).ToList();
            List<GetEventListIO> _Event_list = new List<GetEventListIO>();

            foreach (Event_Master _obj in Event)
            {
                _Event_list.Add(new GetEventListIO(_obj));
            }
            data = new { rescode = 0, resmsg = "Get All EventList!", All_Event_List = _Event_list };
            return data;
        }

        [HttpPost]
        public async Task<object> ResignationList(IO_GetResignationlist _UserTokenLive)
        {
            var data = new object();
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState
                                 where item.Value.Errors.Any()
                                 select item.Value.Errors[0].ErrorMessage).ToList();
                data = new { rescode = 2, resmsg = "Please Validate Your Input!", errors = errorList };
                return data;
            }
            UserMaster _LogedUser = new UserMaster();
            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null)
            {
                return new { rescode = 1, resmsg = "UnAthorised Session !" };
            }
            _LogedUser = _token.UserMaster;

            List<Resignation_Master> Resignation = db.Resignation_Master.Where(x => x.Status != 6 && x.Emp_Id == _LogedUser.User_ID && x.UserMaster.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.Status).ThenBy(x => x.Date_Of_Resignation).ToList();

            Resignation_Master resg = db.Resignation_Master.Where(x => x.Emp_Id == _LogedUser.User_ID & x.Status == 1).OrderByDescending(x => x.Resignation_Id).FirstOrDefault();
            if (resg != null)
            {
                if (resg.Status == 1)
                {
                    return new { rescode = 1, resmsg = "Your Resignation Is Approved !" };
                }
            }

            List<ResignationListIO> _Resignation_list = new List<ResignationListIO>();

            foreach (Resignation_Master _obj in Resignation)
            {
                _Resignation_list.Add(new ResignationListIO(_obj));
            }
            data = new { rescode = 0, resmsg = "Get All Resignation List!", All_ResignationList = _Resignation_list };
            return data;
        }
        [HttpPost]
        public async Task<object> AddtoResignation(IO_AddtoResignation _UserTokenLive)
        {
            var data = new object();
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState
                                 where item.Value.Errors.Any()
                                 select item.Value.Errors[0].ErrorMessage).ToList();
                data = new { rescode = 2, resmsg = "Please Validate Your Input!", errors = errorList };
                return data;
            }
            UserMaster _LogedUser = new UserMaster();
            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null)
            {
                data = new { rescode = 1, resmsg = "UnAthorised Session !" };
                return data;
            }
            _LogedUser = _token.UserMaster;
            UserMaster um = db.UserMasters.Where(x => x.User_type == 1 && x.status == 0).FirstOrDefault();

            if (_UserTokenLive.Resignation_Id != 0)
            {
                Resignation_Master Resignation = db.Resignation_Master.Where(x => x.Resignation_Id == _UserTokenLive.Resignation_Id).FirstOrDefault();

                if (Resignation.Status != 0)
                {
                    data = new { rescode = 2, resmsg = "Resignation Not Updated ! " };
                    return data;

                }
                if (Resignation == null)
                {
                    data = new { rescode = 2, resmsg = "Invalid Resignation ! " };
                    return data;
                }
                if (DateTime.Now > _UserTokenLive.Reliving_Date)
                {
                    data = new { rescode = 2, resmsg = "Please Add Proper Date. " };
                    return data;
                }
                if (Resignation.Status != 0)
                {
                    data = new { rescode = 2, resmsg = "Resignation Not Updated ! " };
                    return data;
                }
                Resignation.Emp_Id = _LogedUser.User_ID;
                Resignation.Designation = _UserTokenLive.Designation;
                Resignation.Team_Name = _UserTokenLive.Team_Name;
                Resignation.Date_Of_Join = _UserTokenLive.Date_Of_Join;
                Resignation.Department = _UserTokenLive.Department;
                Resignation.Date_Of_Resignation = DateTime.Now;
                Resignation.Reporting_Person = _UserTokenLive.Reporting_Person;
                Resignation.Reliving_Date = _UserTokenLive.Reliving_Date;
                Resignation.Reason_For_Exit = _UserTokenLive.Reason_For_Exit;
                Resignation.Remark_For_Resignation = _UserTokenLive.Remark_For_Resignation;
                Resignation.Suggestion_For_Company = _UserTokenLive.Suggestion_For_Company;
                Resignation.HR_Authority = um.User_ID;
                Resignation.Status = 0;
                db.SaveChanges();
                ResignationListIO UpdateRes = new ResignationListIO(Resignation);
                return new { rescode = 0, resmsg = "Resignation Updated Successfully !", UpdateRes = UpdateRes };
            }
            else
            {
                Resignation_Master _newadd = new Resignation_Master();
                Resignation_Master resg = db.Resignation_Master.Where(x => x.Emp_Id == _LogedUser.User_ID & x.Status == 1).OrderByDescending(x => x.Resignation_Id).FirstOrDefault();
                if (resg != null)
                {
                    if (resg.Status == 1)
                    {
                        data = new { rescode = 2, resmsg = "Your Resignation Is Approved ! " };
                        return data;
                    }
                    else if (resg.Status == 0)
                    {
                        data = new { rescode = 2, resmsg = "Your Resignation Is Pending ! " };
                        return data;
                    }
                }
                if (DateTime.Now > _UserTokenLive.Reliving_Date)
                {
                    data = new { rescode = 2, resmsg = "Please Add Proper Date ! " };
                    return data;
                }
                _newadd.Emp_Id = _UserTokenLive.Emp_Id;
                _newadd.Designation = _UserTokenLive.Designation;
                _newadd.Team_Name = _UserTokenLive.Team_Name;
                _newadd.Date_Of_Join = _UserTokenLive.Date_Of_Join;
                _newadd.Department = _UserTokenLive.Department;
                _newadd.Date_Of_Resignation = DateTime.Now;
                _newadd.Reporting_Person = _UserTokenLive.Reporting_Person;
                _newadd.Reliving_Date = _UserTokenLive.Reliving_Date;
                _newadd.Reason_For_Exit = _UserTokenLive.Reason_For_Exit;
                _newadd.Remark_For_Resignation = _UserTokenLive.Remark_For_Resignation;
                _newadd.Suggestion_For_Company = _UserTokenLive.Suggestion_For_Company;
                _newadd.HR_Authority = um.User_ID;
                _newadd.Status = 0;
                _newadd.Reporting_Person = _UserTokenLive.Reporting_Person;
                db.Resignation_Master.Add(_newadd);
                db.SaveChanges();

                ResignationListIO Res = new ResignationListIO(_newadd);
                return new { rescode = 0, resmsg = "Resignation Added Successfully !", AddResignation = Res };
            }
        }

        [HttpPost]
        public async Task<object> DeleteResignation(IO_DeleteResignationlist _UserTokenLive)
        {
            var data = new object();

            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState
                                 where item.Value.Errors.Any()
                                 select item.Value.Errors[0].ErrorMessage).ToList();
                data = new { rescode = 2, resmsg = "Please Validate Your Input!", errors = errorList };
                return data;
            }

            UserMaster _LogedUser = new UserMaster();
            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null)
            {
                data = new { rescode = 1, resmsg = "UnAthorised Session !" };
                return data;
            }
            _LogedUser = _token.UserMaster;
            Resignation_Master _del = db.Resignation_Master.Find(_UserTokenLive.Resignation_Id);
            db.Resignation_Master.Remove(_del);
            db.SaveChanges();

            return new { rescode = 0, resmsg = "Group Deleted Successfully !" };
        }

        //new
        [HttpPost]
        public async Task<object> Employee_AssetMovement_List(IO_getUser _UserTokenLive)
        {
            var data = new object();
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState
                                 where item.Value.Errors.Any()
                                 select item.Value.Errors[0].ErrorMessage).ToList();
                data = new { rescode = 2, resmsg = "Please Validate Your Input!", errors = errorList };
                return data;
            }
            UserMaster _LogedUser = new UserMaster();
            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null)
            {
                return new { rescode = 1, resmsg = "UnAthorised Session !" };
            }
            _LogedUser = _token.UserMaster;

            List<Asset_Movement> movment = db.Asset_Movement.Where(x => x.UserMaster.Company_ID == _LogedUser.Company_ID).OrderByDescending(x => x.Movement_id).ToList();
           

            List<AssetMovementListIO> _movment_list = new List<AssetMovementListIO>();
            foreach (Asset_Movement _obj in movment)
            {
                _movment_list.Add(new AssetMovementListIO(_obj));
            }

            List<Asset_Master> assts = db.Asset_Master.Where(x => x.Company_ID == _LogedUser.Company_ID).ToList();
            List<string> assname = new List<string>();
            List<Asset_Movement> astm;
           
            if (assts.Count != 0)
            {
                foreach (Asset_Master _newobj in assts)
                {
                    astm = movment.Where(y => y.Asset_id == _newobj.Asset_id).OrderByDescending(y => y.Movement_id).ToList();
                    if (astm.Count != 0)
                    {
                        if (astm.FirstOrDefault().To_id == _LogedUser.User_ID)
                        {
                            assname.Add(_newobj.Asset_Name + "-" + _newobj.Serial_Number);
                        }
                    }
                }
                
            }
            data = new { rescode = 0, resmsg = "Get All Employee AssetMovement List!",Employee_Asset_Movement_List=assname, All_Asset_Movement_List = _movment_list };
            return data;
        }


        [HttpPost]
        public async Task<object> UploadImage()
        {
            var data = new object();
            try
            {
                var httpRequest = HttpContext.Current.Request;
                var Tokens = httpRequest.Form[0].ToString();
                var User_UID = httpRequest.Form[1].ToString();
                var Device_Address = httpRequest.Form[2].ToString();

                UserMaster _LogedUser = new UserMaster();
                Master_Token _token = await Check_Token(Tokens, User_UID, Device_Address);
                if (_token == null)
                {
                    return new { rescode = 1, resmsg = "UnAthorised Session !" };
                }
                _LogedUser = _token.UserMaster;

                if (httpRequest.Files.Count == 0)
                {
                    return new { rescode = 1, resmsg = "No file uploaded!" };
                }
                    

                var postedFile = httpRequest.Files[0];

                // Only allow image types
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" , ".JPG" };
                string fileExtension = Path.GetExtension(postedFile.FileName)?.ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    return new { rescode = 1, resmsg = "Only image files allowed!" };
                }
                
                var fileName = Path.GetFileName(postedFile.FileName);
                string name = Path.GetFileNameWithoutExtension(fileName);
                string myfile = name + "_" + DateTime.Now.ToString("ddMMyyyhhmmss") + fileExtension;
                string filePath = HttpContext.Current.Server.MapPath("~/Uploads/User_Image/" + myfile);

                // Ensure the directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                
                //save file
                postedFile.SaveAs(filePath);
                _LogedUser.image_path = filePath;
                _LogedUser.image_name = myfile;
                db.SaveChanges();
                UserMasterIO _user = new UserMasterIO(_LogedUser);
                data = new { rescode = 0, resmsg = "File uploaded successfully!",User_Details=_user };
                return data;
                
            }
            catch (Exception ex)
            {
                data = new { rescode = 1, resmsg = "No file uploaded!" };
                return data;
                
            }
        }

        [HttpPost]
        public async Task<object> Final_AddPunch()
        {
            var data = new object();
            try
            {
                var httpRequest = HttpContext.Current.Request;
                var Tokens = httpRequest.Form[0].ToString();
                var User_UID = httpRequest.Form[1].ToString();
                var Device_Address = httpRequest.Form[2].ToString();
                var latitude = httpRequest.Form[3].ToString();
                var longitude = httpRequest.Form[4].ToString();
                var Location = httpRequest.Form[5].ToString();
                var PunchType_Id= httpRequest.Form[6].ToString();

                UserMaster _LogedUser = new UserMaster();
                Master_Token _token = await Check_Token(Tokens, User_UID, Device_Address);
                if (_token == null)
                {
                    return new { rescode = 1, resmsg = "UnAthorised Session !" };
                }
                _LogedUser = _token.UserMaster;

                if (httpRequest.Files.Count == 0)
                {
                    return new { rescode = 1, resmsg = "No file uploaded!" };
                }


                var postedFile = httpRequest.Files[0];

                // Only allow image types
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".JPG" };
                string fileExtension = Path.GetExtension(postedFile.FileName)?.ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    return new { rescode = 1, resmsg = "Only image files allowed!" };
                }

                var fileName = Path.GetFileName(postedFile.FileName);
                string name = Path.GetFileNameWithoutExtension(fileName);
                string myfile = name + "_" + DateTime.Now.ToString("ddMMyyyhhmmss") + fileExtension;
                string filePath = HttpContext.Current.Server.MapPath("~/Uploads/Punch_Image/" + myfile);

                // Ensure the directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                //save file
                postedFile.SaveAs(filePath);
               

                User_Punch punch = new User_Punch();
                punch.latitude = latitude;
                punch.longitude = longitude;
                punch.Punch_Via = 0;
                punch.Location = Location;
                User_Punch lastpunch = db.User_Punch.Where(x => x.User_Id == _LogedUser.User_ID
                                    && x.Time.Day == DateTime.Today.Day && x.Time.Month == DateTime.Today.Month
                                    && x.Time.Year == DateTime.Today.Year).OrderByDescending(x => x.Punch_Id).FirstOrDefault();
                CommonClasses _newobj = new CommonClasses();
                //Add Ip
                string ip = _newobj.GetIPAddress(IPAddress);
                punch.IP_Address = ip;

                if (int.Parse(PunchType_Id) == 1 && (lastpunch == null || lastpunch.PunchType_Id == 2))
                {
                    _newobj._attendancefordate(DateTime.Now, _LogedUser.User_ID);
                    punch.PunchType_Id = 1;
                    punch.User_Id = _LogedUser.User_ID;
                    punch.Time = DateTime.Now;
                    punch.image_path = filePath;
                    punch.image_name = myfile;
                    db.SaveChanges();
                    db.User_Punch.Add(punch);
                    db.SaveChanges();
                    return new { rescode = 0, resmsg = "Check IN Successfully !" };
                }

                else if (int.Parse(PunchType_Id) == 2 && (lastpunch.PunchType_Id == 1 || lastpunch.PunchType_Id == 4))
                {
                    punch.PunchType_Id = 2;
                    punch.User_Id = _LogedUser.User_ID;
                    punch.Time = DateTime.Now;
                    punch.image_path = filePath;
                    punch.image_name = myfile;
                    db.User_Punch.Add(punch);
                    db.SaveChanges();
                    _newobj._attendancefordate(DateTime.Now, _LogedUser.User_ID);
                    return new { rescode = 0, resmsg = "Check Out Successfully !" };
                }
                else if (int.Parse(PunchType_Id) == 3 && (lastpunch.PunchType_Id == 1))
                {
                    punch.PunchType_Id = 3;
                    punch.User_Id = _LogedUser.User_ID;
                    punch.Time = DateTime.Now;
                    punch.image_path = filePath;
                    punch.image_name = myfile;
                    db.User_Punch.Add(punch);
                    db.SaveChanges();
                    return new { rescode = 0, resmsg = "Lunch In Successfully !" };
                }
                else if (int.Parse(PunchType_Id) == 4 && (lastpunch.PunchType_Id == 3))
                {
                    punch.PunchType_Id = 4;
                    punch.User_Id = _LogedUser.User_ID;
                    punch.Time = DateTime.Now;
                    punch.image_path = filePath;
                    punch.image_name = myfile;
                    db.User_Punch.Add(punch);
                    db.SaveChanges();
                    return new { rescode = 0, resmsg = "Lunch Out Successfully !" };
                }
                else
                {
                    return new { rescode = 4, resmsg = "Sorry, You are currently " + _newobj._PunchName_ByNumber(lastpunch.PunchType_Id).ToString() + ". So, You can't " + _newobj._PunchName_ByNumber(int.Parse(PunchType_Id)).ToString() + ". !" };
                }

            }
            catch (Exception ex)
            {
                data = new { rescode = 1, resmsg = "No file uploaded!" };
                return data;

            }


           
        }

        [HttpPost]
        public async Task<object> UploadDocument()
        {
            var data = new object();
            try
            {
                var httpRequest = HttpContext.Current.Request;
                var Tokens = httpRequest.Form[0].ToString();
                var User_UID = httpRequest.Form[1].ToString();
                var Device_Address = httpRequest.Form[2].ToString();
                var Doc_Name = httpRequest.Form[3].ToString();

                UserMaster _LogedUser = new UserMaster();
                Master_Token _token = await Check_Token(Tokens, User_UID, Device_Address);
                if (_token == null)
                {
                    return new { rescode = 1, resmsg = "UnAthorised Session !" };
                }
                _LogedUser = _token.UserMaster;

                if (httpRequest.Files.Count == 0)
                {
                    return new { rescode = 1, resmsg = "No file uploaded!" };
                }


                var postedFile = httpRequest.Files[0];

                // Only allow image types
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".JPG",".pdf", ".PDF" };
                string fileExtension = Path.GetExtension(postedFile.FileName)?.ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    return new { rescode = 1, resmsg = "Sorry, Invalid File!" };
                }

                var fileName = Path.GetFileName(postedFile.FileName);
                string name = Path.GetFileNameWithoutExtension(fileName);
                string myfile = name + "_" + DateTime.Now.ToString("ddMMyyyhhmmss") + fileExtension;
                string filePath = HttpContext.Current.Server.MapPath("~/Uploads/User_Document/" + myfile);

                // Ensure the directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                //save file
                postedFile.SaveAs(filePath);

                Document_Master dm = db.Document_Master.Where(x => x.User_Id == _LogedUser.User_ID && x.Doc_Name == Doc_Name).FirstOrDefault();
                if(dm==null)
                {
                    Document_Master d = new Document_Master();
                    d.Company_ID = _LogedUser.Company_ID;
                    d.User_Id = _LogedUser.User_ID;
                    d.Doc_Path = filePath;
                    d.Doc_Type = myfile;
                    d.Doc_Name = Doc_Name;
                    d.Status = 0;
                    db.Document_Master.Add(d);
                    db.SaveChanges();
                }
                else
                {
                    dm.Doc_Path = filePath;
                    dm.Doc_Type = myfile;

                    db.SaveChanges();
                }
                List<DocumentListIO> _DocList = new List<DocumentListIO>();
                foreach (Document_Master _D in db.Document_Master.Where(x => x.User_Id == _LogedUser.User_ID).OrderByDescending(x => x.Doc_Id).ToList())
                {
                    _DocList.Add(new DocumentListIO(_D));
                }
                data = new { rescode = 0, resmsg = "Document uploaded successfully!", Get_DocList = _DocList };
                return data;

            }
            catch (Exception ex)
            {
                data = new { rescode = 1, resmsg = "No file uploaded!" };
                return data;

            }
        }

        [HttpPost]
        public async Task<object> SalarySlip(IO_getsalaryslip _UserTokenLive)
        {
            CommonClasses _newobj = new CommonClasses();
            var data = new object();
            string redirectUrl="";
            string baseUrl= HttpContext.Current?.Request?.Url?.Scheme+"://"+ HttpContext.Current?.Request?.Url?.Authority;
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState
                                 where item.Value.Errors.Any()
                                 select item.Value.Errors[0].ErrorMessage).ToList();
                data = new { rescode = 2, resmsg = "Please Validate Your Input!", errors = errorList };
                return data;
            }
            UserMaster _LogedUser = new UserMaster();
            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null)
            {
                return new { rescode = 1, resmsg = "UnAthorised Session !" };
            }
            _LogedUser = _token.UserMaster;
            try
            {
                List<ProjectSalary> ps = db.ProjectSalaries.Where(x => x.User_ID == _LogedUser.User_ID).ToList();
                if (ps.Count != 0)
                {
                    //    foreach (ProjectSalary p in ps)
                    //    {
                    //        int dm = p.SalaryMonth.Month;
                    //        int dy = p.SalaryMonth.Year;
                    //        if (dm == _UserTokenLive.Month && dy == _UserTokenLive.Year)
                    //        {

                    //            salaryid = p.ProjectSalaryID;
                    //            string chk = "enc_" + _newobj.Encrypt(p.ProjectSalaryID.ToString());
                    //            //salaryid = long.Parse(Encrypt(p.ProjectSalaryID.ToString()));
                    //            redirectUrl = baseUrl + "/Access/Salarydetail?salaryid=" + chk;
                    //            return Redirect(redirectUrl);
                    //        }
                    //    }
                    redirectUrl = baseUrl + "/Home/PrintSalarySlip?Tokens=" + _UserTokenLive.Tokens+ "&User_UID="+ _UserTokenLive.User_UID+ "&Device_Address="+ _UserTokenLive.Device_Address+ "&Month="+ _UserTokenLive.Month+ "&Year="+ _UserTokenLive.Year;
                    return Redirect(redirectUrl);
                }
                else
                {
                    data = new { rescode = 1, resmsg = "Data not found!" };
                    return data;
                }
            }
            catch (Exception ex)
            {
                data = new { rescode = 1, resmsg = "Data not found!" };
                return data;

            }
           
        }

        [HttpPost]
        public async Task<object> PrintDocument(IO_getdocument _UserTokenLive)
        {
            CommonClasses _newobj = new CommonClasses();
            var data = new object();
            string redirectUrl = "";
            string baseUrl = HttpContext.Current?.Request?.Url?.Scheme + "://" + HttpContext.Current?.Request?.Url?.Authority;
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState
                                 where item.Value.Errors.Any()
                                 select item.Value.Errors[0].ErrorMessage).ToList();
                data = new { rescode = 2, resmsg = "Please Validate Your Input!", errors = errorList };
                return data;
            }
            UserMaster _LogedUser = new UserMaster();
            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null)
            {
                return new { rescode = 1, resmsg = "UnAthorised Session !" };
            }
            _LogedUser = _token.UserMaster;
            try
            {
                List<Generated_Documents> ps = db.Generated_Documents.Where(x => x.Gen_doc_id== _UserTokenLive.Gen_doc_id).ToList();
                if (ps.Count != 0)
                {
                    redirectUrl = baseUrl + "/Home/PrintDocument?Tokens=" + _UserTokenLive.Tokens + "&User_UID=" + _UserTokenLive.User_UID + "&Device_Address=" + _UserTokenLive.Device_Address + "&Gen_doc_id=" + _UserTokenLive.Gen_doc_id;
                    return Redirect(redirectUrl);
                }
                else
                {
                    data = new { rescode = 1, resmsg = "Data not found!" };
                    return data;
                }
            }
            catch (Exception ex)
            {
                data = new { rescode = 1, resmsg = "Data not found!" };
                return data;

            }

        }

        [HttpPost]
        public async Task<object> Employee_DailyTask_List(IO_DailyTask _UserTokenLive)
        {
            var data = new object();
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState
                                 where item.Value.Errors.Any()
                                 select item.Value.Errors[0].ErrorMessage).ToList();
                data = new { rescode = 2, resmsg = "Please Validate Your Input!", errors = errorList };
                return data;
            }
            UserMaster _LogedUser = new UserMaster();
            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null)
            {
                return new { rescode = 1, resmsg = "UnAthorised Session !" };
            }
            _LogedUser = _token.UserMaster;
            List<Employee_Task_Master> tasklist = new List<Employee_Task_Master>();
            if ( _UserTokenLive.Taskdate!=null)
            {
                tasklist = db.Employee_Task_Master.Where(x => x.UserMaster.Company_ID == _LogedUser.Company_ID && x.User_id == _LogedUser.User_ID && x.Task_Date==_UserTokenLive.Taskdate).OrderByDescending(x => x.Employee_Task_id).ToList();


            }
            else
            {
                tasklist = db.Employee_Task_Master.Where(x => x.UserMaster.Company_ID == _LogedUser.Company_ID && x.User_id == _LogedUser.User_ID && x.Task_Date==DateTime.Now).OrderByDescending(x => x.Employee_Task_id).ToList();


            }
            List<DailyTask_MasterIO> _dailytask_list = new List<DailyTask_MasterIO>();
            foreach (Employee_Task_Master _obj in tasklist)
            {
                _dailytask_list.Add(new DailyTask_MasterIO(_obj));
            }
            data = new { rescode = 0, resmsg = "Get All Employee Task List!", Employee_Task_List = _dailytask_list };
            return data;
        }


        [HttpPost]
        public async Task<object> Employee_DailyTask_Status_Update(IO_UpdateDailyTaskStatus _UserTokenLive)
        {

            var data = new object();
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState
                                 where item.Value.Errors.Any()
                                 select item.Value.Errors[0].ErrorMessage).ToList();
                data = new { rescode = 2, resmsg = "Please Validate Your Input!", errors = errorList };
                return data;
            }
            UserMaster _LogedUser = new UserMaster();
            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null) { if (_token == null) { return new { rescode = 1, resmsg = "UnAthorised Session !" }; } }
            _LogedUser = _token.UserMaster;
            Employee_Task_Master _emptask = db.Employee_Task_Master.Where(x => x.Employee_Task_id == _UserTokenLive.Employee_Task_id && x.User_id == _LogedUser.User_ID).FirstOrDefault();
            if (_UserTokenLive.status == 1)
            {
                _emptask.Status = (int?)_UserTokenLive.status;
                db.SaveChanges();
                return new { rescode = 0, resmsg = "Task Running Started" };
            }
            else if (_UserTokenLive.status == 2)
            {
                _emptask.Status = (int?)_UserTokenLive.status;
                db.SaveChanges();
                return new { rescode = 0, resmsg = "Task InProgress" };
            }
            else if (_UserTokenLive.status == 3)
            {
                _emptask.Status = (int?)_UserTokenLive.status;
                db.SaveChanges();
                return new { rescode = 0, resmsg = "Task has been Stopped" };
            }
            else if (_UserTokenLive.status == 4)
            {
                _emptask.Status = (int?)_UserTokenLive.status;
                db.SaveChanges();
                return new { rescode = 0, resmsg = "Task has been Completed" };
            }
            else
            {
                return new { rescode = 2, resmsg = "Invalid Status" };
            }
        }


        [HttpPost]
        public async Task<object> AddDailyTask(IO_AddDailyTask _UserTokenLive)
        {
            var data = new object();
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState
                                 where item.Value.Errors.Any()
                                 select item.Value.Errors[0].ErrorMessage).ToList();
                data = new { rescode = 2, resmsg = "Please Validate Your Input!", errors = errorList };
                return data;
            }
            UserMaster _LogedUser = new UserMaster();
            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null)
            {
                data = new { rescode = 1, resmsg = "UnAthorised Session !" };
                return data;
            }
            _LogedUser = _token.UserMaster;
            List<string> taskall = _UserTokenLive.Task_Detail;
            foreach(string s in taskall)
            {
                Employee_Task_Master etask = new Employee_Task_Master();
                etask.User_id = _LogedUser.User_ID;
                etask.Task_Detail = s;
                etask.Task_Date = DateTime.Now;
                etask.Status = 0;
                db.Employee_Task_Master.Add(etask);
                db.SaveChanges();
            }
            
            List<Employee_Task_Master> tasklistall = db.Employee_Task_Master.Where(x => x.User_id == _LogedUser.User_ID && x.Task_Date == DateTime.Today).OrderByDescending(x => x.Employee_Task_id).ToList();

            List<DailyTask_MasterIO> _dailytask_listall = new List<DailyTask_MasterIO>();
            foreach (Employee_Task_Master _obj in tasklistall)
            {
                _dailytask_listall.Add(new DailyTask_MasterIO(_obj));
            }
            
                return new { rescode = 0, resmsg = "Task Added Successfully !", Employee_Task_List = _dailytask_listall };
            
        }

        [HttpPost]
        public async Task<object> DeleteEmployeeTask(IO_getEmployeeTaskdelete _UserTokenLive)
        {
            var data = new object();
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState
                                 where item.Value.Errors.Any()
                                 select item.Value.Errors[0].ErrorMessage).ToList();
                data = new { rescode = 2, resmsg = "Please Validate Your Input!", errors = errorList };
                return data;
            }
            UserMaster _LogedUser = new UserMaster();
            Master_Token _token = await Check_Token(_UserTokenLive.Tokens, _UserTokenLive.User_UID, _UserTokenLive.Device_Address);
            if (_token == null)
            {
                return new { rescode = 1, resmsg = "UnAthorised Session !" };
            }
            _LogedUser = _token.UserMaster;

            Employee_Task_Master _del = db.Employee_Task_Master.Find(_UserTokenLive.Employee_Task_id);
            db.Employee_Task_Master.Remove(_del);
            db.SaveChanges();
            List<Employee_Task_Master> tasklistallt = db.Employee_Task_Master.Where(x => x.User_id == _LogedUser.User_ID && x.Task_Date == DateTime.Today).OrderByDescending(x => x.Employee_Task_id).ToList();

            List<DailyTask_MasterIO> _dailytask_listallt = new List<DailyTask_MasterIO>();
            foreach (Employee_Task_Master _obj in tasklistallt)
            {
                _dailytask_listallt.Add(new DailyTask_MasterIO(_obj));
            }
            return new { rescode = 0, resmsg = "Employee Task Deleted Successfully !", Employee_Task_List = _dailytask_listallt };
        }
    }


}



