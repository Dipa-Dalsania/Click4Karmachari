using ClickKarmachari.Controllers;
using DocumentFormat.OpenXml.Wordprocessing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Routing;
using System.Web.Mvc;
using System.Web.Routing;


namespace ClickKarmachari.Models
{
   
    public class CommonClasses
    {
        private Prod_Satyamgroup_V1Entities db = new Prod_Satyamgroup_V1Entities();
        public string factor2Key = "86e82263-e23d-11ef-8b17-0200cd936042";

        public String Send_2FACTOR_SMS(string cusname, string mobileno, string content)
        {
            content = content.Replace("_", " ");

            try
            {
                //string uri = "https://2factor.in/API/V1/" + factor2Key + "/SMS/" + mobileno + "/" + content + "/LOTUSOTPV1";

                string uri = "https://2factor.in/API/V1/faff3d9e-8eb7-11ea-9fa5-0200cd936042/SMS/" + mobileno + "/" + content + "/MBH_OTP_NEW2_IMPLICIT";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method = "GET";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader responseStream = new StreamReader(response.GetResponseStream());
                string Body = responseStream.ReadToEnd();
                return Body;
            }
            catch
            {
                return "";
            }
        }
       
      

        public readonly string EncryptionKey = "YourStrongEncryptionKey123"; // Ideally, get from config

        public  string Encrypt(string clearText)
        {
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                var pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] {
                0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64,
                0x76, 0x65, 0x64, 0x65, 0x76
            });

                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);

                using (var ms = new MemoryStream())
                using (var cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(clearBytes, 0, clearBytes.Length);
                    cs.Close();
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public  string Decrypt(string cipherText)
        {
            cipherText = cipherText.Replace(" ", "+"); // In case of URL issues
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                var pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] {
                0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64,
                0x76, 0x65, 0x64, 0x65, 0x76
            });

                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);

                using (var ms = new MemoryStream())
                using (var cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(cipherBytes, 0, cipherBytes.Length);
                    cs.Close();
                    return Encoding.Unicode.GetString(ms.ToArray());
                }
            }
        }

        public string GetIPAddress(string IPAddress)
        {
            IPHostEntry Host = default(IPHostEntry);
            string Hostname = null;
            Hostname = System.Environment.MachineName;
            Host = Dns.GetHostEntry(Hostname);
            foreach (IPAddress IP in Host.AddressList)
            {
                if (IP.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    IPAddress = Convert.ToString(IP);
                }
            }
            return IPAddress;
        }

        public string GetTextStringScript(string Str)
        {
            if (string.IsNullOrEmpty(Str))
                return string.Empty;

            Str = Str.Replace("'", @"\'");
            Str = Str.Replace("\r\n", "&#13;&#10;");
            return Str;

            //Str = Str.Replace("'", @"\'");
            //Str = Str.Replace("\r\n", "&#13;&#10;");
            //return Str;
        }

        public string GetTextStringTable(string Str)
        {
            Str = Str.Replace("\r\n", "<br/>");
            Str = Str.Replace("&#13;&#10;", "<br/>");
            return Str;
        } 
        public String Send_sms(string mobileno, string content)
        {
            content = content.Replace("_", " ");
            //  content = HttpContext.Current.Server.UrlEncode(content);
            string uri = "http://msg.msgclub.net/rest/services/sendSMS/sendGroupSms?AUTH_KEY=60e4adeded8510062bb23324c15f8c0&message=" + content + "&senderId=MBHWEB&routeId=1&mobileNos=" + mobileno + "&smsContentType=english";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "GET";
            request.Accept = @"text/html, application/xhtml+xml, */*";
            request.Referer = @"http://msg.msgclub.net/";
            request.Headers.Add("Accept-Language", "en-GB");
            request.UserAgent = @"Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0)";
            request.Host = @"www.VACCISAFE.com";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader responseStream = new StreamReader(response.GetResponseStream());
            string Body = responseStream.ReadToEnd();
            return Body;
        }
        public bool Email_Verify(string fromAddress, string toAddress, string subject, string body)
        {
            MailMessage mail = new MailMessage(fromAddress, toAddress, subject, body);
            mail.Bcc.Add(new MailAddress("support@mbhweb.com"));
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;
            SmtpClient client = new SmtpClient();
            client.Host = "smtp.gmail.com";
            client.Port = 587;
            client.EnableSsl = true;
            client.Timeout = 10000;
            client.UseDefaultCredentials = true;
            client.Credentials = new NetworkCredential("contact@mshere.com", "spjhbtuagdgenepx");
            try
            {
                //client.Send(mail);
                return true; ;
            }
            catch (Exception)
            {
                return false;
            }
            
        }

        public void Reloadweb() { }

        internal void Update_tokenExTime(long access_id)
        { 
            db.Master_Token.Where(x => x.access_id == access_id).FirstOrDefault().Expire = DateTime.Now.AddYears(1);
            db.SaveChanges();
        }

        public string _PunchName_ByNumber(long statusid)
        {
            string _status = "";
            if (statusid == 1) { _status = "Check IN"; }
            else if (statusid == 2) { _status = "Check Out"; }
            else if (statusid == 3) { _status = "Lunch IN"; }
            else if (statusid == 4) { _status = "Lunch Out"; }
            return _status;
        }

        public string _Status_ByNumber(long? statusid)
        {
            if(statusid == null) { statusid = 0; }
            string _status = "";
            if (statusid == 0) { _status = "Pending"; }
            else if (statusid == 1) { _status = "Approved "; }
            else if (statusid == 2) { _status = "Rejected "; }
            else if (statusid == 3) { _status = "Paid"; }              
            else if (statusid == 6) { _status = "Deleted"; }
            else if (statusid == 7) { _status = "Running"; }
            else if (statusid == 8) { _status = "Stop"; }
            else if (statusid == 9) { _status = "Completed"; }
            return _status;
        }


        public void _attendancefordate(DateTime Dateforattendance, long userid)
        {
            List<User_Punch> lastdateallpunch = db.User_Punch.Where(x => x.User_Id == userid
                                && x.Time.Day == Dateforattendance.Day && x.Time.Month == Dateforattendance.Month
                                && x.Time.Year == Dateforattendance.Year).OrderBy(x => x.Time).ToList();

            long firstpunch = 0;
            double todayshrs = 0;
            foreach (User_Punch _punch in lastdateallpunch.Where(x=> x.PunchType_Id == 1))
            {
                if(firstpunch == 0)
                {
                    firstpunch = lastdateallpunch.Where(x => x.PunchType_Id == 1).FirstOrDefault().Punch_Id;
                }
                if (_punch.Punch_Id >= firstpunch)
                {
                    DateTime? _firstpnch = null;
                    DateTime? _firstcheckout = null;
                    DateTime? _firstlunchin = null;
                    DateTime? _firstlunchout = null;
                    
                    List<User_Punch> lastdateallpunch1 = lastdateallpunch.Where(x => x.Punch_Id >= firstpunch).ToList();
                    if (lastdateallpunch1.Where(x => x.PunchType_Id == 1).FirstOrDefault() != null)
                    {
                        _firstpnch = lastdateallpunch1.Where(x => x.PunchType_Id == 1).FirstOrDefault().Time;
                        firstpunch = lastdateallpunch1.Where(x => x.PunchType_Id == 1).FirstOrDefault().Punch_Id + 1;
                    }

                    if (lastdateallpunch1.Where(x => x.PunchType_Id == 3).FirstOrDefault() != null)
                    {
                        _firstlunchin = lastdateallpunch1.Where(x => x.PunchType_Id == 3).FirstOrDefault().Time;
                        firstpunch = lastdateallpunch1.Where(x => x.PunchType_Id == 3).FirstOrDefault().Punch_Id + 1;
                    }

                    if (lastdateallpunch1.Where(x => x.PunchType_Id == 4).FirstOrDefault() != null)
                    {
                        _firstlunchout = lastdateallpunch1.Where(x => x.PunchType_Id == 4).FirstOrDefault().Time;
                        firstpunch = lastdateallpunch1.Where(x => x.PunchType_Id == 4).FirstOrDefault().Punch_Id + 1;
                    }

                    if (lastdateallpunch1.Where(x => x.PunchType_Id == 2).FirstOrDefault() != null)
                    {
                        _firstcheckout = lastdateallpunch1.Where(x => x.PunchType_Id == 2).FirstOrDefault().Time;
                        firstpunch = lastdateallpunch1.Where(x => x.PunchType_Id == 2).FirstOrDefault().Punch_Id + 1;
                    }

                    if (_firstpnch != null)
                    {
                        double lunchhrs = 0;
                        if (_firstlunchin != null && _firstlunchout != null)
                        {
                            DateTime t1 = DateTime.Parse(_firstlunchin.ToString());
                            DateTime t2 = DateTime.Parse(_firstlunchout.ToString());
                            lunchhrs = (t2 - t1).TotalMinutes;
                        }
                        else if (_firstlunchin != null && _firstlunchout == null && _firstcheckout == null)
                        {
                            _firstlunchout = _firstlunchin;
                            lunchhrs = 0;
                        }
                        else if (_firstlunchin != null && _firstlunchout == null && _firstcheckout != null)
                        {
                            DateTime t1 = DateTime.Parse(_firstlunchin.ToString());
                            DateTime t2 = DateTime.Parse(_firstcheckout.ToString());
                            lunchhrs = (t2 - t1).TotalMinutes;
                        }

                        int flag = 0; //for not calcualte lunch time in total working mins. 

                        if (_firstcheckout == null && _firstlunchin == null)
                        {
                            _firstcheckout = _firstpnch;
                        }
                        else if (_firstcheckout == null && _firstlunchout != null)
                        {
                            _firstcheckout = _firstlunchout;
                        }
                        else if (_firstcheckout == null && _firstlunchin != null)
                        {
                            _firstcheckout = _firstlunchin;
                            flag = 1;
                        }

                        DateTime t3 = DateTime.Parse(_firstpnch.ToString());
                        DateTime t4 = DateTime.Parse(_firstcheckout.ToString());
                        double TotalWorkingHrs = (t4 - t3).TotalMinutes;
                        double TotalAttendingHrs = (TotalWorkingHrs - lunchhrs);
                        if (flag == 1)
                        {
                            TotalAttendingHrs = (TotalWorkingHrs);
                        }
                        else
                        {
                            TotalAttendingHrs = (TotalWorkingHrs - lunchhrs);
                          
                        }
                        todayshrs = todayshrs + TotalAttendingHrs;
                    }
                }
            }

            double attendance = 0;
            if(todayshrs >= 480)
            {
                attendance = 1;
            }else if(todayshrs >= 240)
            {
                attendance = 0.5;
            }
            Attendance_Master _newatt = db.Attendance_Master.Where(x=> x.User_Id == userid
                                && x.Punch_Date.Day == Dateforattendance.Day && x.Punch_Date.Month == Dateforattendance.Month
                                && x.Punch_Date.Year == Dateforattendance.Year).FirstOrDefault(); 
            if(_newatt == null)
            {
                Attendance_Master _newatt11 = new Attendance_Master();
                _newatt11.User_Id = userid;
                _newatt11.Punch_Date = Dateforattendance;
                _newatt11.Attendance = decimal.Parse(attendance.ToString());
                _newatt11.TotalHrs = decimal.Parse(todayshrs.ToString());
                db.Attendance_Master.Add(_newatt11);
                db.SaveChanges();
            }
            else
            {
                _newatt.Attendance = decimal.Parse(attendance.ToString());
                _newatt.TotalHrs = decimal.Parse(todayshrs.ToString());
                db.SaveChanges();
            }
        }
    }
    //============================================================Total Amount of Head ==============================================================================//
    public class IO_Profile
    {
        [Display(Name = "Tokens:")]
        [Required(ErrorMessage = "Tokens required.")]
        public String Tokens { get; set; }

        [Display(Name = "User UID:")]
        [Required(ErrorMessage = "User UID  is required.")]
        public string User_UID { get; set; }

        [Display(Name = "Device_Address:")]
        [Required(ErrorMessage = "Device_Address is required.")]
        public string Device_Address { get; set; }
        public string User_Name { get; set; }

        [RegularExpression(@"[6789][0-9]{9}", ErrorMessage = "Invalid Mobile Number.")]
        public long? RefMobile { get; set; }
        public string BloodGroup { get; set; }        
        public long Religion_Id { get; set; }

        //[RegularExpression(@"^\d{4}\s\d{4}\s\d{4}$", ErrorMessage = "Invalid Adharcard No.")]
        public long? Adharcard_No { get; set; }

        [RegularExpression(@"[A-Z]{5}\d{4}[A-Z]{1}", ErrorMessage = "Invalid PANNo.")]
        public string PanCard_No { get; set; }

        [RegularExpression(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid Email.")]
        public string User_Email { get; set; }
        public string User_City { get; set; }
    }
   
    public class IO_LoginUser  
    {
      
        [Required(ErrorMessage = "Mobile Number is required")]
        [RegularExpression(@"[6789][0-9]{9}", ErrorMessage = "Invalid Mobile Number.")]
        public long Mobile { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string pwd { get; set; }      
        public string Device_Address { get; set; }
        public string FCM_Key { get; set; }
    }
  
    public class IO_getUser
    {
        [Display(Name = "Tokens:")]
        [Required(ErrorMessage = "Tokens required.")]
        public String Tokens { get; set; }

        [Display(Name = "User UID:")]
        [Required(ErrorMessage = "User UID  is required.")]
        public string User_UID { get; set; }

        [Display(Name = "Device Address:")]
        [Required(ErrorMessage = "Device Address is required.")]
        public string Device_Address { get; set; }
        public int? Voucher { get; set; }
    }

    public class IO_getPunch
    {
        [Display(Name = "Tokens:")]
        [Required(ErrorMessage = "Tokens required.")]
        public String Tokens { get; set; }

        [Display(Name = "User UID:")]
        [Required(ErrorMessage = "User UID  is required.")]
        public string User_UID { get; set; }

        [Display(Name = "Device Address:")]
        [Required(ErrorMessage = "Device Address is required.")]
        public string Device_Address { get; set; }

        public DateTime? dateforpunch { get; set; }
    }

    public class IoChangePassword
    {
        [Display(Name = "Tokens:")]
        [Required(ErrorMessage = "Tokens required.")]
        public String Tokens { get; set; }

        [Display(Name = "User UID:")]
        [Required(ErrorMessage = "User UID  is required.")]
        public string User_UID { get; set; }

        [Display(Name = "Device Address:")]
        [Required(ErrorMessage = "Device Address is required.")]
        public string Device_Address { get; set; }
     
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
    public class IO_GetbyStatus
    {
        [Display(Name = "Tokens:")]
        [Required(ErrorMessage = "Tokens required.")]
        public String Tokens { get; set; }

        [Display(Name = "User UID:")]
        [Required(ErrorMessage = "User UID  is required.")]
        public string User_UID { get; set; }

        [Display(Name = "Device Address:")]
        [Required(ErrorMessage = "Device Address is required.")]
        public string Device_Address { get; set; }
        public int? Status { get; set; }
    }

    public class IO_AttendanceMaster
    {
        [Display(Name = "Tokens:")]
        [Required(ErrorMessage = "Tokens required.")]
        public String Tokens { get; set; }

        [Display(Name = "User_UID:")]
        [Required(ErrorMessage = "User_UID  is required.")]
        public string User_UID { get; set; }

        [Display(Name = "Device_Address:")]
        [Required(ErrorMessage = "Device_Address is required.")]
        public string Device_Address { get; set; }
        public long? Year { get; set; }
        public long? Month { get; set; }
    }


    public class IO_AddtoAddress
    {
        [Display(Name = "Tokens:")]
        [Required(ErrorMessage = "Tokens required.")]
        public String Tokens { get; set; }

        [Display(Name = "User_UID:")]
        [Required(ErrorMessage = "User_UID  is required.")]
        public string User_UID { get; set; }

        [Display(Name = "Device_Address:")]
        [Required(ErrorMessage = "Device_Address is required.")]
        public string Device_Address { get; set; }

        [Required(ErrorMessage = "City is required.")]
        public string City { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        public string Address { get; set; }

    }
    public class IO_AddtoRelation
    {
        [Display(Name = "Tokens:")]
        [Required(ErrorMessage = "Tokens required.")]
        public String Tokens { get; set; }

        [Display(Name = "User_UID:")]
        [Required(ErrorMessage = "User_UID  is required.")]
        public string User_UID { get; set; }

        [Display(Name = "Device_Address:")]
        [Required(ErrorMessage = "Device_Address is required.")]
        public string Device_Address { get; set; }
        public string Relation  { get; set; }
        public string Person  { get; set; }
        public long Mobile { get; set; }
    }
    public class IO_AddtoBankDetails
    {
        [Display(Name = "Tokens:")]
        [Required(ErrorMessage = "Tokens required.")]
        public String Tokens { get; set; }

        [Display(Name = "User_UID:")]
        [Required(ErrorMessage = "User_UID  is required.")]
        public string User_UID { get; set; }

        [Display(Name = "Device_Address:")]
        [Required(ErrorMessage = "Device_Address is required.")]
        public string Device_Address { get; set; }
        public string Bank_Name { get; set; }
        public string Account_Holder { get; set; }
        public string Bank_Acc_No { get; set; }
        public string IFSC_No { get; set; }
        public string UAN_No { get; set; }

        public string ESIC_No { get; set; }
    }
    public class IO_AddtoEducation
    {
        [Display(Name = "Tokens:")]
        [Required(ErrorMessage = "Tokens required.")]
        public String Tokens { get; set; }

        [Display(Name = "User_UID:")]
        [Required(ErrorMessage = "User_UID  is required.")]
        public string User_UID { get; set; }

        [Display(Name = "Device_Address:")]
        [Required(ErrorMessage = "Device_Address is required.")]
        public string Device_Address { get; set; }
        public string Education { get; set; }
        public string University { get; set; }
        public string PassOut { get; set; }
    }
    public class IO_AddtoExperience
    {
        [Display(Name = "Tokens:")]
        [Required(ErrorMessage = "Tokens required.")]
        public String Tokens { get; set; }

        [Display(Name = "User_UID:")]
        [Required(ErrorMessage = "User_UID  is required.")]
        public string User_UID { get; set; }

        [Display(Name = "Device_Address:")]
        [Required(ErrorMessage = "Device_Address is required.")]
        public string Device_Address { get; set; }
        public string LastCompany { get; set; }
        public string WorkingYears { get; set; }
        public string LastYear { get; set; }
    }

    public class IO_AddtoPunch
    {
        [Display(Name = "Tokens:")]
        [Required(ErrorMessage = "Tokens required.")]
        public String Tokens { get; set; }

        [Display(Name = "User_UID:")]
        [Required(ErrorMessage = "User_UID  is required.")]
        public string User_UID { get; set; }

        [Display(Name = "Device_Address:")]
        [Required(ErrorMessage = "Device_Address is required.")]
        public string Device_Address { get; set; }

        [Required(ErrorMessage = "Punch Type Id  is required")]
        public long PunchType_Id { get; set; }      
        public string longitude { get; set; }
        public string latitude { get; set; }
        public string Location { get; set; }
    }

    public class IO_AddtoVisit
    {
        [Display(Name = "Tokens:")]
        [Required(ErrorMessage = "Tokens required.")]
        public String Tokens { get; set; }

        [Display(Name = "User_UID:")]
        [Required(ErrorMessage = "User_UID  is required.")]
        public string User_UID { get; set; }

        [Display(Name = "Device_Address:")]
        [Required(ErrorMessage = "Device_Address is required.")]
        public string Device_Address { get; set; }
        public long PunchType_Id { get; set; }
        public string longitude { get; set; }
        public string latitude { get; set; }
        public string Location { get; set; }
        public string LocationName { get; set; }
    }
    public class IO_AddtoProjectTask
    {
        [Display(Name = "Tokens:")]
        [Required(ErrorMessage = "Tokens required.")]
        public String Tokens { get; set; }

        [Display(Name = "User_UID:")]
        [Required(ErrorMessage = "User_UID  is required.")]
        public string User_UID { get; set; }

        [Display(Name = "Device_Address:")]
        [Required(ErrorMessage = "Device_Address is required.")]
        public string Device_Address { get; set; }
        public long Project_Id { get; set; }
        public long Task_Id { get; set; }
        public string Task_Name { get; set; }
        public DateTime A_Start_Date { get; set; }
        public DateTime R_End_Date { get; set; }
        public string Duration_Unit { get; set; }
        public long Duration { get; set; }
        public string Description { get; set; }
        public long Task_Type { get; set; }
    }
    public class IO_AddtoEmployeeTask
    {
        [Display(Name = "Tokens:")]
        [Required(ErrorMessage = "Tokens is required.")]
        public String Tokens { get; set; }

        [Display(Name = "User_UID:")]
        [Required(ErrorMessage = "User_UID is required.")]
        public string User_UID { get; set; }

        [Display(Name = "Device Address:")]
        [Required(ErrorMessage = "Device Address is required.")]
        public string Device_Address { get; set; }
        public long Employee_Task_id { get; set; }
        public long User_id { get; set; }
        public string Task_Date { get; set; }

        [Display(Name = "Task Detail:")]
        [Required(ErrorMessage = "Task Detail is required.")]
        public string Task_Detail { get; set; }
        
    }
    public class IO_getEmployeeTask
    {
        [Display(Name = "Tokens:")]
        [Required(ErrorMessage = "Tokens is required.")]
        public String Tokens { get; set; }

        [Display(Name = "User_UID:")]
        [Required(ErrorMessage = "User_UID is required.")]
        public string User_UID { get; set; }

        [Display(Name = "Device Address:")]
        [Required(ErrorMessage = "Device Address is required.")]
        public string Device_Address { get; set; }
      
       
        [Required(ErrorMessage = "Task Date is required.")]
        public DateTime Task_Date { get; set; }

    }

    public class IO_NoticeboardList
    {
        [Display(Name = "Tokens:")]
        [Required(ErrorMessage = "Tokens is required.")]
        public String Tokens { get; set; }

        [Display(Name = "User_UID:")]
        [Required(ErrorMessage = "User_UID is required.")]
        public string User_UID { get; set; }

        [Display(Name = "Device Address:")]
        [Required(ErrorMessage = "Device Address is required.")]
        public string Device_Address { get; set; }

        public long Notice_Id { get; set; }

        public String Title { get; set; }

        public DateTime Date { get; set; }

        public string Description { get; set; }

    }
    public class IO_EventList
    {
        [Display(Name = "Tokens:")]
        [Required(ErrorMessage = "Tokens is required.")]
        public String Tokens { get; set; }

        [Display(Name = "User_UID:")]
        [Required(ErrorMessage = "User_UID is required.")]
        public string User_UID { get; set; }

        [Display(Name = "Device Address:")]
        [Required(ErrorMessage = "Device Address is required.")]
        public string Device_Address { get; set; }

        public long Event_Id { get; set; }

        public long User_id { get; set; }

        public String Title { get; set; }

        public DateTime Date { get; set; }

        public string Description { get; set; }

    }
    public class IO_GetResignationlist
    {
        [Display(Name = "Tokens:")]
        [Required(ErrorMessage = "Tokens is required.")]
        public String Tokens { get; set; }

        [Display(Name = "User_UID:")]
        [Required(ErrorMessage = "User_UID is required.")]
        public string User_UID { get; set; }

        [Display(Name = "Device Address:")]
        [Required(ErrorMessage = "Device Address is required.")]
        public string Device_Address { get; set; }

        public long Resignation_Id { get; set; }

        public long Emp_Id { get; set; }

        public long Designation { get; set; }

        public DateTime Date_Of_Resignation { get; set; }

        public string Team_Name { get; set; }

        public long Department { get; set; }
        public long Reporting_Person { get; set; }

        public DateTime Reliving_Date { get; set; }

        public string Reason_For_Exit { get; set;}
        public string Remark_For_Resignation { get; set; }
        public string Suggestion_For_Company { get; set; }

        public string Remark_For_Managment { get; set; }

        public int Status { get; set; }



    }
    public class IO_DeleteNoticeBoard
    {
        [Display(Name = "Tokens:")]
        [Required(ErrorMessage = "Tokens required.")]
        public string Tokens { get; set; }

        [Display(Name = "Admin_UId:")]
        [Required(ErrorMessage = "Admin_UId  is required.")]
        public string Admin_UId { get; set; }

        [Display(Name = "Device_Address:")]
        [Required(ErrorMessage = "Device_Address is required.")]
        public string Device_Address { get; set; }

        [Required(ErrorMessage = "Notice Id  is required.")]
        public long Notice_Id { get; set; }
      

    }


    public class IO_getEmployeeTaskdelete
    {
        [Display(Name = "Tokens:")]
        [Required(ErrorMessage = "Tokens is required.")]
        public String Tokens { get; set; }

        [Display(Name = "User_UID:")]
        [Required(ErrorMessage = "User_UID is required.")]
        public string User_UID { get; set; }

        [Display(Name = "Device Address:")]
        [Required(ErrorMessage = "Device Address is required.")]
        public string Device_Address { get; set; }

        [Display(Name = "Employee Task id:")]
        [Required(ErrorMessage = "Employee Task id is required.")]
        public long Employee_Task_id { get; set; }


    }
    public class IO_DeletetoProjectTask
    {
        public String Tokens { get; set; }
        public string User_UID { get; set; }
        public string Device_Address { get; set; }
        public long Task_Id { get; set; }
    }
    public class IO_DeletetoComment
    {
        public String Tokens { get; set; }
        public string User_UID { get; set; }
        public string Device_Address { get; set; }
        public long Comment_id { get; set; }
    }
    public class IO_DeletetoSubComment
    {
        public String Tokens { get; set; }
        public string User_UID { get; set; }
        public string Device_Address { get; set; }
        public long SubComnt_ID { get; set; }
    }
    public class IO_ProjectTask_View
    {
        [Display(Name = "Tokens:")]
        [Required(ErrorMessage = "Tokens is required.")]
        public String Tokens { get; set; }

        [Display(Name = "User UID:")]
        [Required(ErrorMessage = "User UID is required.")]
        public string User_UID { get; set; }

        [Display(Name = "Device Address:")]
        [Required(ErrorMessage = "Device Address is required.")]
        public string Device_Address { get; set; }

        [Required(ErrorMessage = "Task_Id is required.")]
        public long Task_Id { get; set; }        
    }
    public class IO_ProjectTask_Status
    {
        [Display(Name = "Tokens:")]
        [Required(ErrorMessage = "Tokens is required.")]
        public String Tokens { get; set; }

        [Display(Name = "User UID:")]
        [Required(ErrorMessage = "User UID is required.")]
        public string User_UID { get; set; }

        [Display(Name = "Device Address:")]
        [Required(ErrorMessage = "Device Address is required.")]
        public string Device_Address { get; set; }

        [Required(ErrorMessage = "Task_Id is required.")]
        public long Task_Id { get; set; }
        public long Status { get; set; }
    }
    public class IO_AddComments
    {
        [Display(Name = "Tokens:")]
        [Required(ErrorMessage = "Tokens is required.")]
        public String Tokens { get; set; }

        [Display(Name = "User UID:")]
        [Required(ErrorMessage = "User UID is required.")]
        public string User_UID { get; set; }

        [Display(Name = "Device Address:")]
        [Required(ErrorMessage = "Device Address is required.")]
        public string Device_Address { get; set; }

        [Required(ErrorMessage = "Task_Id is required.")]
        public long Task_Id { get; set; }      
        public string Comments { get; set; }
        public long Comment_id { get; set; }
    }
    public class IO_EditComment
    {
        [Display(Name = "Tokens:")]
        [Required(ErrorMessage = "Tokens is required.")]
        public String Tokens { get; set; }

        [Display(Name = "User UID:")]
        [Required(ErrorMessage = "User UID is required.")]
        public string User_UID { get; set; }

        [Display(Name = "Device Address:")]
        [Required(ErrorMessage = "Device Address is required.")]
        public string Device_Address { get; set; }
        public long Comment_id { get; set; }
        public string Comments { get; set; }
    }
    public class IO_EditSubComment
    {
        [Display(Name = "Tokens:")]
        [Required(ErrorMessage = "Tokens is required.")]
        public String Tokens { get; set; }

        [Display(Name = "User UID:")]
        [Required(ErrorMessage = "User UID is required.")]
        public string User_UID { get; set; }

        [Display(Name = "Device Address:")]
        [Required(ErrorMessage = "Device Address is required.")]
        public string Device_Address { get; set; }
        public long Comment_id { get; set; }
        public long TaskID { get; set; }        
        public long SubcommentId { get; set; }
        public string Comments { get; set; }
    }
    public class IO_AddtoLeave
    {
        [Display(Name = "Tokens:")]
        [Required(ErrorMessage = "Tokens is required.")]
        public String Tokens { get; set; }

        [Display(Name = "User_UID:")]
        [Required(ErrorMessage = "User_UID is required.")]
        public string User_UID { get; set; }

        [Display(Name = "Device Address:")]
        [Required(ErrorMessage = "Device Address is required.")]
        public string Device_Address { get; set; }  
        public DateTime From_Date { get; set; }
        public DateTime To_Date { get; set; }
        public int F_Date { get; set; }
        public int T_Date { get; set; }

        [Required(ErrorMessage = "Leave Type is required.")]
        public string Leave_Type { get; set; }

        [Required(ErrorMessage = "Reason is required.")]
        public string Reason { get; set; }      
    }
    public class IO_AddtoAdvanceReq
    {
        [Display(Name = "Tokens:")]
        [Required(ErrorMessage = "Tokens is required.")]
        public String Tokens { get; set; }

        [Display(Name = "User_UID:")]
        [Required(ErrorMessage = "User_UID is required.")]
        public string User_UID { get; set; }

        [Display(Name = "Device Address:")]
        [Required(ErrorMessage = "Device Address is required.")]
        public string Device_Address { get; set; }      
        public string Description { get; set; }
        public decimal Requested_Amount { get; set; }
        public string Place { get; set; }
        public string Remark  { get; set; }
        public string Parent_Type { get; set; }
        public long? Project_Id { get; set; }
        public DateTime S_Date { get; set; }        
        public DateTime E_Date { get; set; }
    }
    public class IO_AddtoVoucherReq
    {
        [Display(Name = "Tokens:")]
        [Required(ErrorMessage = "Tokens is required.")]
        public String Tokens { get; set; }

        [Display(Name = "User_UID:")]
        [Required(ErrorMessage = "User_UID is required.")]
        public string User_UID { get; set; }

        [Display(Name = "Device Address:")]
        [Required(ErrorMessage = "Device Address is required.")]
        public string Device_Address { get; set; }
        public decimal Total_Amount { get; set; }
        public string Description { get; set; }
        public string Place { get; set; }
        public string Remark { get; set; }
        public decimal Petrol { get; set; }
        public decimal Travelling { get; set; }
        public decimal Mobile { get; set; }
        public decimal Other { get; set; }
        public decimal Conveyance { get; set; }
        public string Parent_Type { get; set; }
        public long Approval { get; set; }
        public long Voucher_Id { get; set; }
        public long? Project_Id { get; set; }
        public long? ReqNo { get; set; }
        
    }
    public class IO_AddtoResignation
    {
        [Display(Name = "Tokens:")]
        [Required(ErrorMessage = "Tokens is required.")]
        public String Tokens { get; set; }

        [Display(Name = "User_UID:")]
        [Required(ErrorMessage = "User_UID is required.")]
        public string User_UID { get; set; }

        [Display(Name = "Device Address:")]
        [Required(ErrorMessage = "Device Address is required.")]

        public string Device_Address { get; set; }

        public long Resignation_Id { get; set; }
        public long Emp_Id { get; set; }

        public long Designation { get; set; }

        public DateTime Date_Of_Resignation { get; set; }

        public DateTime Date_Of_Join { get; set; }

        public string Team_Name { get; set; }

        public long Department { get; set; }
        public long Reporting_Person { get; set; }

        public DateTime Reliving_Date { get; set; }

        public string Reason_For_Exit { get; set; }
        public string Remark_For_Resignation { get; set; }
        public string Suggestion_For_Company { get; set; }

        public long HR_Authority { get; set; }
        public int Status { get; set; }
    }
    public class IO_DeleteResignationlist
    {
        [Display(Name = "Tokens:")]
        [Required(ErrorMessage = "Tokens is required.")]
        public String Tokens { get; set; }

        [Display(Name = "User_UID:")]
        [Required(ErrorMessage = "User_UID is required.")]
        public string User_UID { get; set; }

        [Display(Name = "Device Address:")]
        [Required(ErrorMessage = "Device Address is required.")]

        public string Device_Address { get; set; }

        [Required(ErrorMessage = "Resignation Id is required.")]
        public long Resignation_Id { get; set; }
      
    }
    public class TotalAmount
    {
        public long h_id { get; set; }
        public string h_name { get; set; }
        public decimal? Total_Amount { get; set; }
    }
    public class InputModel_updateprofile
    {
        public long Mobile { get; set; }
        public int gender { get; set; }
        public string fname { get; set; }
        public string lname { get; set; }
        public string email { get; set; }
        public string dateofbirth { get; set; }
    }

    public class IO_getsalaryslip
    {
        [Display(Name = "Tokens:")]
        [Required(ErrorMessage = "Tokens required.")]
        public String Tokens { get; set; }

        [Display(Name = "User UID:")]
        [Required(ErrorMessage = "User UID  is required.")]
        public string User_UID { get; set; }

        [Display(Name = "Device Address:")]
        [Required(ErrorMessage = "Device Address is required.")]
        public string Device_Address { get; set; }
        public int? Month{ get; set; }
        public int? Year { get; set; }
    }

    public class IO_getdocument
    {
        [Display(Name = "Tokens:")]
        [Required(ErrorMessage = "Tokens required.")]
        public String Tokens { get; set; }

        [Display(Name = "User UID:")]
        [Required(ErrorMessage = "User UID  is required.")]
        public string User_UID { get; set; }

        [Display(Name = "Device Address:")]
        [Required(ErrorMessage = "Device Address is required.")]
        public string Device_Address { get; set; }
        public long Gen_doc_id { get; set; }
        
    }

    public class IO_DailyTask
    {
        [Display(Name = "Tokens:")]
        [Required(ErrorMessage = "Tokens required.")]
        public String Tokens { get; set; }

        [Display(Name = "User UID:")]
        [Required(ErrorMessage = "User UID  is required.")]
        public string User_UID { get; set; }

        [Display(Name = "Device Address:")]
        [Required(ErrorMessage = "Device Address is required.")]
        public string Device_Address { get; set; }

        public DateTime Taskdate { get; set; }
    }
    public class IO_AddDailyTask
    {
        [Display(Name = "Tokens:")]
        [Required(ErrorMessage = "Tokens is required.")]
        public String Tokens { get; set; }

        [Display(Name = "User_UID:")]
        [Required(ErrorMessage = "User_UID is required.")]
        public string User_UID { get; set; }

        [Display(Name = "Device Address:")]
        [Required(ErrorMessage = "Device Address is required.")]

        public string Device_Address { get; set; }

       public List<string> Task_Detail { get; set; }

    
    }
    public class IO_UpdateDailyTaskStatus
    {
        [Display(Name = "Tokens:")]
        [Required(ErrorMessage = "Tokens is required.")]
        public String Tokens { get; set; }

        [Display(Name = "User_UID:")]
        [Required(ErrorMessage = "User_UID is required.")]
        public string User_UID { get; set; }

        [Display(Name = "Device Address:")]
        [Required(ErrorMessage = "Device Address is required.")]

        public string Device_Address { get; set; }

        public long Employee_Task_id { get; set; }

        public int status { get; set; }
    }
}