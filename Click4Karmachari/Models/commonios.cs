using DocumentFormat.OpenXml.Office.PowerPoint.Y2022.M08.Main;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography.Pkcs;

namespace ClickKarmachari.Models
{
    public class InputModel_VerifyMobile
    {
        public long Mobile { get; set; }
    }

    public class InputModel_VerifyOTP
    {
        public long Mobile { get; set; }
        public string Device_Address { get; set; }
        public long OTP { get; set; }

    }
    public partial class Address_MasterIO
    {
        public Address_MasterIO(Address_Master _obj)
        {
            Add_id = _obj.Add_ID;

            if (_obj.Add_Address != null) { Address = _obj.Add_Address; }
            else { Address = ""; }
            City = _obj.CityName.ToString();
            if (_obj.Status != null) { Status = _obj.Status.ToString(); }
            else { Status =""; }
        }
        public long  Add_id { get; set; }
        public string Address { get; set; }

        public string City { get; set; }

        public string Status { get; set; }

        //public long? Status { get; set; }
    }
    public partial class Relative_masterIO
    {
        public Relative_masterIO(Relation_Master _obj)
        {
            Relation_ID = _obj.Relation_Id;
            Relation_Name = _obj.Relation_Name;
            Person_Name = _obj.Person_Name;
            Mobile_No = _obj.Mobile_No;
            Status = _obj.Status;
        }

        public long Relation_ID { get;  set; }
        public string Relation_Name { get;  set; }
        public string Person_Name { get;  set; }
        public long? Mobile_No { get;  set; }
        public long? Status { get;  set; }
    }

    public partial class Education_masterIO
    {
        public Education_masterIO(Education_Master _obj)
        {
            Edu_ID = _obj.Edu_Id;
            Edu_Name = _obj.Edu_Name;
            University = _obj.university;
            Pass_Out = _obj.Pass_Out;
            Status = _obj.Status;
        }

        public long Edu_ID { get;  set; }
        public string Edu_Name { get;  set; }
        public string University { get;  set; }

        public string Pass_Out { get;  set; }
        public long? Status { get;  set; }
    }
    public partial class Experience_masterIO
    {
        public Experience_masterIO(Experience_Master _obj)
        {
            Exp_Id = _obj.Exp_Id;
            Last_Company = _obj.Last_Company;
            Last_Year = _obj.Last_Year;
            Working_years = _obj.working_years;        
            Status = _obj.Status;
        }

        public long Exp_Id { get; set; }
        public string Last_Company { get; set; }
        public string Last_Year { get; set; }
        public string Working_years { get; set; }
        public long? Status { get; set; }
    }
    //=========================================================================Get City============================================================================//

    public partial class City_MasterIO
    {
        public City_MasterIO(City_Master _obj)
        {
            Cityid = _obj.Cityid;
            CityName = _obj.CityName;
            Status = _obj.Status;
        }
        public long Cityid { get; set; }
        public string CityName { get; set; }
        public Nullable<int> Status { get; set; }
    }

    public partial class Religion_MasterIO
    {
        public Religion_MasterIO(Religion_Master _obj)
        {
            Religion_Id = _obj.Religion_Id;
            Religion_Name = _obj.Religion_Name;
           
        }

        public long Religion_Id { get;  set; }
        public string Religion_Name { get;  set; }
    }

    public partial class UserType_MasterIO
    {
        public UserType_MasterIO(UserType _obj)
        {
            Usertype_id = _obj.Usertype_id;
            Usertype_name = _obj.Usertype_name;
       
        }

        public long Usertype_id { get;  set; }
        public string Usertype_name { get;  set; }
    }
    //=========================================================================User Details============================================================================//

    public partial class UserMasterIO
    {
        public UserMasterIO(UserMaster _obj)
        {
           

            DOB = _obj.DOB.ToString("dd MMMM yyyy");
            DOJ = _obj.Date_of_join.ToString("dd MMMM yyyy");
            if (_obj.Religion_name == null)
            {
                Religion_Name = "";

            }
            else
            {
                Religion_Name = _obj.Religion_name.ToString();
            }
            if (_obj.RefMobile != null) { RefMobile = Convert.ToInt64(_obj.RefMobile).ToString(); }
            else { RefMobile = ""; }

            if (_obj.Designation_Master != null) { Designation = _obj.Designation_Master.Designation; } else { Designation = ""; }
            if (_obj.Department_Master != null) { Department = _obj.Department_Master.Department; } else { Department = ""; }


            //img_name = _obj.image_name;

            if (Tokens != null) { Tokens = Tokens; }
            else { Tokens = ""; }


            if (_obj.image_name != null) { img_name = _obj.image_name; }
            else { img_name = ""; }

            if (_obj.Company_Master.Company_Name != null) { Company_Name = _obj.Company_Master.Company_Name; }
            else { Company_Name = ""; }

            Name = _obj.User_Name;
            Reporting_Person = _obj.Reporting_Person;
            if (_obj.Reporting_Person == null)
            {
                Reporting_Person_Name = "";
            }
            else
            {
                Reporting_Person_Name = _obj.UserMaster2.User_Name;
            }
            Status = _obj.status;
            UserType = _obj.User_type;
            Mobile = _obj.User_Mobile;
            Email = _obj.User_Email;
            User_UID = _obj.User_UID;
            PanCard_No = _obj.PanCard_No;
            //RefMobile = _obj.RefMobile;
            CityName = _obj.City_Name;
            Adharcard_No = _obj.Adharcard_No;
            BloodGroup = _obj.Blood_group;
            Posted_At = _obj.Posted_At;
            Team_name = _obj.Team_name;
            Company_Email = _obj.Company_Email;
            Bank_Name = _obj.Bank_Name;
            Bank_Acc_No = _obj.Bank_Acc_No;
            PF_No = _obj.PF_No;
            EC_No = _obj.EC_No;
            IFSC_No = _obj.IFSC_No;
            UAN_No = _obj.UAN_No;
            ESIC_No = _obj.ESIC_No;
            Total_PL = _obj.Total_PL;
            Total_CL = _obj.Total_CL;
            Total_EL = _obj.Total_EL;
            Total_Leave = _obj.Total_Leave;
           
            if (_obj.Profile_Lock == true)
            {
                Profile_Lock = _obj.Profile_Lock;
            }
            else if (_obj.Profile_Lock == false)
            {
                Profile_Lock = _obj.Profile_Lock;
            }
            else
            {
                
            }

            if (_obj.Address_Master != null)
            {          
                AddressList = new List<Address_MasterIO>();
                foreach (Address_Master _a in _obj.Address_Master)
                {                
                    AddressList.Add(new Address_MasterIO(_a));                    
                }
               
            }
           

            if (_obj.Project_Assign_Master != null)
            {
                Projectlist = new List<Project_masterIO>();
                foreach (Project_Assign_Master _a in _obj.Project_Assign_Master)
                {
                    Projectlist.Add(new Project_masterIO(_a.Project_Master));
                }
            }
            if (_obj.Relation_Master != null)
            {
                RelationList = new List<Relative_masterIO>();
                foreach (Relation_Master _a in _obj.Relation_Master)
                {
                    RelationList.Add(new Relative_masterIO(_a));
                }
            }
            if (_obj.Education_Master != null)
            {
                EduList = new List<Education_masterIO>();
                foreach (Education_Master _a in _obj.Education_Master)
                {
                    EduList.Add(new Education_masterIO(_a));
                }
            }
            if (_obj.Experience_Master != null)
            {
                ExpList = new List<Experience_masterIO>();
                foreach (Experience_Master _a in _obj.Experience_Master)
                {
                    ExpList.Add(new Experience_masterIO(_a));
                }
            }
            if (_obj.Generated_Documents != null)
            {
                Certificatelist = new List<GenerateDocumentsIO>();
                foreach (Generated_Documents _a in _obj.Generated_Documents)
                {
                    Certificatelist.Add(new GenerateDocumentsIO(_a));
                }
            }
        }

        public string Tokens { get; set; }
        public string Name { get; set; }
        public long Mobile { get; set; }
        public string Email { get; set; }
        public string DOB { get; set; }
        public int? Status { get; }
        public string RefMobile { get; set; }
        public string BloodGroup { get; set; }
        public string img_name { get; set; }
        public string Posted_At { get; set; }
        public string Religion_Name { get; set; }
        public string CityName { get; set; }
        public long? Adharcard_No { get; set; }
        public string PanCard_No { get; set; }
        public string User_UID { get; set; }

        public string Company_Name { get; set; }
        public string DOJ { get; set; }
        public string Designation { get; set; }
        public string Department { get; set; }
        public long? Reporting_Person { get; set; }
        public string Reporting_Person_Name { get; set; }          
        public long? UserType { get;  set; }
        public string Team_name { get; set; }
        public string Company_Email { get;  set; }
        public string Bank_Name { get; set; }
        public string Bank_Acc_No { get; set; }
        public string PF_No { get; set; }
        public string EC_No { get; set; }
        public decimal? Total_PL { get; set; }
        public decimal? Total_CL { get; set; }
        public decimal? Total_EL { get; set; }
        public decimal? Total_Leave { get; set; }
        public string IFSC_No { get;  set; }
        public string UAN_No { get;  set; }
        public string ESIC_No { get;  set; }

        public bool? Profile_Lock { get; set; }
        public List<Address_MasterIO> AddressList { get; set; }
        public List<Relative_masterIO> RelationList { get; set; }
        public List<Education_masterIO> EduList { get; set; }
        public List<Experience_masterIO> ExpList { get; set; }
        public List<Project_masterIO> Projectlist { get; set; }

        public List<GenerateDocumentsIO> Certificatelist { get; set; }
    }
    //=========================================================================Get Projects============================================================================//

    public partial class Project_masterIO
        {
            public Project_masterIO(Project_Master _obj)
            {
                ProjectName = _obj.Project_Name;
                ProjectID = _obj.Project_Id;
            }
            public string ProjectName { get;  set; }
            public long ProjectID { get;  set; }
        }
     //=========================================================================Get Task Types============================================================================//

    public partial class TaskTypeIO
        {
            public TaskTypeIO(TaskType _obj)
            {
                TaskTypeID = _obj.TaskType_Id;
                TaskTypeName = _obj.TaskType_Name;             
            }

            public long TaskTypeID { get;  set; }
            public string TaskTypeName { get;  set; }
        }

        //=========================================================================Employee Project List============================================================================//

    public partial class Project_Task_MasterIO
    {
        public Project_Task_MasterIO(Project_Task_Master _obj)
        {
            if(_obj.Comment_Master != null)
            {
                Comments = new List<CommentListIO>();
                foreach(Comment_Master _com in _obj.Comment_Master)
                {
                    Comments.Add(new CommentListIO(_com));
                }
            }
            Start_Date = _obj.Actual_Start_Date.Value.ToString("dd MMMM yyyy");
            End_Date = _obj.Revised_End_Date.Value.ToString("dd MMMM yyyy");
            Task_Id = _obj.Task_Id;
            ProjectName = _obj.Project_Master.Project_Name;
            TaskName = _obj.Task_Name;
            ProjetcId = _obj.Project_Id;
            TaskTypeId = _obj.Task_Type;
            TaskTypeName = _obj.TaskType.TaskType_Name;
            //Approval = _obj.Approval;
            Duration = _obj.Duration;
            //Remark = _obj.Remark;
            if (_obj.Remark != null) { Remark = _obj.Remark; }
            else { Remark = ""; }

            Duration_Unit = _obj.Duration_Unit;
            Desc = _obj.Description;
            Status = _obj.Status;
        }
        public List<CommentListIO> Comments { get; set; }
        public long Task_Id { get;  set; }
        public string ProjectName { get; set; }        
        public string TaskName { get; set; }
        public long? ProjetcId { get; set; }
        public long? TaskTypeId { get; set; }
        public string TaskTypeName { get; set; }
        public string Start_Date { get;  set; }
        public string End_Date { get;  set; }
        //public int? Approval { get; set; }
        public long? Duration { get; set; }
        public string Remark { get; set; }
        public string Duration_Unit { get; set; }
        public string Desc { get; set; }
        public int? Status { get; set; }
    }

    public partial class CommentListIO
    {
        public CommentListIO(Comment_Master _obj)
        {
            Comment_id = _obj.Comnt_ID;
            Comments = _obj.Comments;
            TaskID = _obj.Task_Id;
            EmpID = _obj.Emp_Id;
            CommentDate = _obj.Commented_Date.ToString("dd MMMM yyyy");
            AddByName = _obj.UserMaster.User_Name;

            if (_obj.UserMaster.image_name != null) { AddImage = _obj.UserMaster.image_name; }
            else { AddImage = ""; }

            //AddImage = _obj.UserMaster.image_name;

            User_Type = _obj.UserMaster.User_type;
            CommentType = _obj.Comment_Type;
        }

        public long Comment_id { get;  set; }
        public string Comments { get;  set; }
        public long? TaskID { get;  set; }
        public long? EmpID { get;  set; }
        public string CommentDate { get;  set; }
        public string AddByName { get;  set; }
        public string AddImage { get;  set; }
        public long? User_Type { get;  set; }
        public long? CommentType { get; set; }
    }

    public partial class LeaveListIO
    {
        public LeaveListIO(Leave_Master _obj)
        {
            LeaveDays = _obj.leave_days;
            Leave_Type = _obj.Leave_Type;
            From_Date = _obj.From_Date.ToString("dd MMMM yyyy");
            To_Date = _obj.To_Date.ToString("dd MMMM yyyy");
            ReqDate = _obj.ReqDate.ToString("dd MMMM yyyy");
            Reason = _obj.Reason;
            Approval = _obj.Status;
        }

        public string ReqDate { get;  set; }
        public decimal? LeaveDays { get;  set; }
        public string Leave_Type { get;  set; }
        public string From_Date { get;  set; }
        public string To_Date { get;  set; }
        public  string Reason { get;  set; }
        public int? Approval { get; }
    }

    public partial class AdvanceReqListIO
    {
        public AdvanceReqListIO(Request_Master _obj)
        {
            if (_obj.Project_Id != null)
            {
                projectName = _obj.Project_Master.Project_Name; 
                //projectName = _obj.Project_Master.Project_Name;
            }
            else
            {
                projectName = "";
            }
            ReqDate = _obj.ReqDate.ToString("dd MMMM yyyy");
            ExpenseAmount = _obj.Requested_Amount;
            GrantAmount = _obj.Granted_Amount;

            if (_obj.Remark != null) { Remark = _obj.Remark; }
            else { Remark = ""; }

            //Remark = _obj.Remark_For_Admin;
            Approval = new CommonClasses()._Status_ByNumber(_obj.Status);
        }

        public string ReqDate { get; set; }
        public string projectName { get; set; }
        public decimal? ExpenseAmount { get; set; }
        public decimal? GrantAmount { get; set; }
        public string Remark { get;  set; }
        public string Approval { get; set; }
    }
   
    public partial class PunchListIO
    {
        public PunchListIO(User_Punch _obj)
        {
            Punch_Type_ID = _obj.PunchType_Id;
            Date = _obj.Time.ToString("dd-MM-yyyy"); ;
        }

        public PunchListIO(UserMaster logedUser)
        {
        }

        public long Punch_Type_ID { get;  set; }
        public string Date { get; set; }
    }
    //=========================================================================Employee Voucher List============================================================================//

    public partial class VoucherListIO
    {
        public VoucherListIO(Voucher_Master _obj)
        {
            VoucherID = _obj.Voucher_Id;
            VoucherDate = _obj.Voucher_Date.ToString("dd MMMM yyyy");
            TotalAmount = _obj.Total_Amount;
            //PayableAmount = _obj.Payable_Amount;
            Status = new CommonClasses()._Status_ByNumber(_obj.Status);
            Remark = _obj.Remark;
            //Remark_by_Managment = _obj.Remark_For_Administrator;
            Molbile = _obj.Mobile;
            Petrol = _obj.Petrol;
            Travelling = _obj.Travelling;
            Conveyance = _obj.Conveyance;
            Parent_Type = _obj.Parent_Type;
            Description = _obj.Description;
            Palce = _obj.Place;
          

            if (_obj.Project_Id != null) { Project_Id = Convert.ToInt64(_obj.Project_Id).ToString(); }
            else { Project_Id = ""; }

            if (_obj.Req_Id != null) { Req_ID = Convert.ToInt64(_obj.Req_Id).ToString(); }
            else { Req_ID = ""; }

            if (_obj.Advance_Amount != null) { AdvanceAmt = Convert.ToInt64(_obj.Advance_Amount).ToString(); }
            else { AdvanceAmt = ""; }

            if (_obj.Payable_Amount != null) { PayableAmount = Convert.ToInt64(_obj.Payable_Amount).ToString(); }
            else { PayableAmount = ""; }

            if (_obj.Remark_For_Administrator != null) { Remark_by_Managment =_obj.Remark_For_Administrator; }
            else { Remark_by_Managment = ""; }

            //Project_Id = _obj.Project_Id;
            //Req_ID = _obj.Req_Id;
            //AdvanceAmt = _obj.Advance_Amount;
        }

        public long? VoucherID { get;  set; }
        public string VoucherDate { get;  set; }
        public decimal? TotalAmount { get;  set; }
        public string PayableAmount { get;  set; }
        public string Status { get;  set; }
        public string Remark { get; set; }
        public string Remark_by_Managment { get; set; }
        public decimal? Molbile { get; set; }
        public decimal? Petrol { get; set; }
        public decimal? Travelling { get; set; }
        public decimal? Conveyance { get; set; }
        public string Parent_Type { get; set; }
        public string Description { get; set; }
        public string Palce { get; set; }
        public string Project_Id { get; set; }
        public string Req_ID { get; set; }
        public string AdvanceAmt { get; set; }
    }

    //=========================================================================Employee Statement============================================================================//

    public partial class EmployeeStatementListIO
    {
        public EmployeeStatementListIO(Transaction_Master _obj)
        {
            TransactionDate = _obj.Trans_Date.ToString("dd MMMM yyyy"); ;
            Amount = _obj.Amount;
            TransactionType = _obj.Type_Name;
            VoucherNo = _obj.Ref_Id;
        }

        public string TransactionDate { get;  set; }
        public decimal? Amount { get;  set; }
        public string TransactionType { get;  set; }
        public object VoucherNo { get;  set; }
    }

    //=========================================================================Employee Assets============================================================================//

    public partial class AssetMovementListIO
    {
        public AssetMovementListIO(Asset_Movement _obj)
        {
            AssetName = _obj.Asset_Master.Asset_Name;
            FromUser = _obj.UserMaster.User_Name;
            ToUser = _obj.UserMaster1.User_Name;
            MovementDate = _obj.Movement_Date.ToString("dd MMMM yyyy");
            Detail = _obj.Detail;
        }
        public string AssetName { get;  set; }
        public string FromUser { get;  set; }
        public string ToUser { get;  set; }
        public string MovementDate { get;  set; }
        public string Detail { get;  set; }
    }

    public partial class AssetListIO
    {
        public AssetListIO(Asset_Master _obj)
        {
            AssetName = _obj.Asset_Name;         
        }
        public string AssetName { get; set; }    
    }

    //========================================================================= Employee Document============================================================================//

    public partial class DocumentListIO
    {
        public DocumentListIO(Document_Master _obj)
        {
            DocName = _obj.Doc_Name;
            Doc_Type = _obj.Doc_Type;
         
        }

        public string DocName { get;  set; }
        public string Doc_Type { get;  set; }
    }

    public partial class GenerateDocumentsIO
    {
        public GenerateDocumentsIO(Generated_Documents _obj)
        {
            Gen_doc_id = _obj.Gen_doc_id;
            Gen_doc_no = _obj.Gen_doc_no;
            Gen_doc_text = _obj.Gen_doc_text;
            Gen_doc_Title = _obj.Gen_doc_Title;
           
        }
        public long Gen_doc_id { get; set; }
        public string Gen_doc_no { get; set; }
        public string Gen_doc_text { get; set; }
        public string Gen_doc_Title { get; set; }
       

    }
    //========================================================================= Employee Salry Slip============================================================================//

    public partial class SalarySlipListIO
    {
        public SalarySlipListIO(Generated_Documents _obj)
        {
            Gen_doc_Title = _obj.Gen_doc_Title;
            //File = _obj.File_Name;

        }

        public string Gen_doc_Title { get; set; }
        //public string File { get; set; }
    }
    public partial class GetTaskListIO
    {
        public GetTaskListIO(Employee_Task_Master _obj)
        {
            Employee_Task_id = _obj.Employee_Task_id;
            User_id = (long)_obj.User_id;
            Task_Detail = _obj.Task_Detail; 
            Task_Date = Convert.ToDateTime(_obj.Task_Date).ToString("dd MMMM yyyy");
        }

        public long Employee_Task_id { get; set; }
        public long User_id { get; set; }
        public string Task_Detail { get; set; }
        public string Task_Date { get; set; }
    

    }
    public partial class GetNoticeBoardListIO
    {
        public GetNoticeBoardListIO(NoticeBoard_Master _obj)
        {
            Notice_Id = _obj.Notice_Id;
            Title = _obj.Title;
            Description=_obj.Description;
            Date = Convert.ToDateTime(_obj.Date).ToString("dd MMMM yyyy");
            added_on = Convert.ToDateTime(_obj.added_on).ToString("dd MMMM yyyy");
     
        }

        public long Notice_Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Date { get; set; }
        public string added_on { get; set; }
    

    }
    public partial class GetEventListIO
    {
        public GetEventListIO(Event_Master _obj)
        {
            Event_Id = _obj.Event_Id;
            Title = _obj.Title;
            Description = _obj.Description;
            Date = Convert.ToDateTime(_obj.Date).ToString("dd MMMM yyyy");
            added_on = Convert.ToDateTime(_obj.added_on).ToString("dd MMMM yyyy");
            employeename = _obj.UserMaster.User_Name;
        }

        public long Event_Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Date { get; set; }
        public string added_on { get; set; }
        public string employeename { get; set; }
    }
    public partial class ResignationListIO
    {
        public ResignationListIO(Resignation_Master _obj)
        {
            Resignation_Id = _obj.Resignation_Id;
            Emp_Id = (long)_obj.Emp_Id;
            Designation = (long)_obj.Designation;
            Date_Of_Resignation = Convert.ToDateTime(_obj.Date_Of_Resignation).ToString("dd MMMM yyyy");
            Team_Name = _obj.Team_Name;
            Department = (long)_obj.Department;
            Reporting_Person = (long)_obj.Reporting_Person;
            Reliving_Date = Convert.ToDateTime(_obj.Reliving_Date).ToString("dd MMMM yyyy");
            Reason_For_Exit = _obj.Reason_For_Exit;
            Remark_For_Resignation = _obj.Remark_For_Resignation;
            Suggestion_For_Company = _obj.Suggestion_For_Company;
            Status = (int)_obj.Status;
        }

        public long Resignation_Id { get; set; }

        public long Emp_Id { get; set; }

        public long Designation { get; set; }

        public string Date_Of_Resignation { get; set; }

        public string Team_Name { get; set; }

        public long Department { get; set; }
        public long Reporting_Person { get; set; }

        public string Reliving_Date { get; set; }

        public string Reason_For_Exit { get; set; }
        public string Remark_For_Resignation { get; set; }
        public string Suggestion_For_Company { get; set; }
        public int Status { get; set; }

    }


    public partial class GetCommentListIO
    {
        public GetCommentListIO(Comment_Master _obj)
        {
            Comnt_ID = _obj.Comnt_ID;
            Task_Id = (long)_obj.Task_Id;
            Emp_Id = (long)_obj.Emp_Id;
            Comments = _obj.Comments;
            Commented_Date = _obj.Commented_Date.ToString("dd MMMM yyyy");
            Comment_Type = (long)_obj.Comment_Type;
        }

        public long Comnt_ID { get; set; }
        public long Task_Id { get; set; }
        public long Emp_Id { get; set; }
        public string Comments { get; set; }
        public String Commented_Date { get; set; }
        public long Comment_Type { get; set; }


    }
    public class IO_AddressDelete
    {
        [Display(Name = "Tokens:")]
        [Required(ErrorMessage = "Tokens required !")]
        public String Tokens { get; set; }

        [Display(Name = "UserID:")]
        [Required(ErrorMessage = "UserID is required.")]
        public string User_UID { get; set; }

        [Display(Name = "Device_Address:")]
        [Required(ErrorMessage = "Device_Address is required.")]
        public string Device_Address { get; set; }

        [Display(Name = "Address ID:")]
        [Required(ErrorMessage = "Address ID required !")]
        public long AddressID { get; set; }

    }
    //========================================================================= Employee Details============================================================================//

    public partial class UserPunchIO
    {
     

        public UserPunchIO(User_Punch _obj)
        {            
            PunchtypeID = _obj.PunchType_Id;

            Date = _obj.Time.ToString("dd MMMM yyyy hh:mm tt");
            Location = _obj.Location;
            image_path = _obj.image_path;
            image_name = _obj.image_name;
        }

      
        public long PunchtypeID { get; set; }
        public string Date { get;  set; }
        public string Location { get;  set; }

        public string image_path { get; set; }

        public string image_name { get; set; }
    }

    public partial class UserVisitIO
    {
        public UserVisitIO(User_Visits _obj)
        {
            PunchTypeId = _obj.PunchType_Id;
            Date = _obj.Time.ToString("dd MMMM yyyy hh:mm tt");
            Location = _obj.Location;
            LocationName = _obj.LocationName;
            image_path = _obj.image_path;
            image_name = _obj.image_name;
        }
        public long PunchTypeId { get; set; }
        public string Date { get; set; }
        public string Location { get; set; }
        public string LocationName { get; set; }

        public string image_path { get; set; }

        public string image_name { get; set; }
    }


    public partial class Attendance_MasterIO
    {
        public Attendance_MasterIO(Attendance_Master _obj)
        {
            Attendance_id = _obj.Attendance_id;
            Attendance = _obj.Attendance;
            TotalHrs = _obj.TotalHrs;
            Punch_Date = _obj.Punch_Date.ToString("dd MMMM yyyy");
            User_Id = _obj.User_Id;
        }
        public long Attendance_id { get; set; }
        public string Punch_Date { get; set; }
        public decimal? Attendance { get; set; }
        public decimal? TotalHrs { get; set; }
        public long User_Id { get; set; }
    }

    //========================================================================= Resignation Details============================================================================//
    public partial class DailyTask_MasterIO
    {
        public DailyTask_MasterIO(Employee_Task_Master _obj)
        {
            Employee_Task_id = _obj.Employee_Task_id;
            Task_Detail = _obj.Task_Detail;
            Task_Date = Convert.ToDateTime(_obj.Task_Date).ToString("dd MMMM yyyy");
            Status = _obj.Status;
        }
        public long Employee_Task_id { get; set; }
        public string Task_Detail { get; set; }
        public string Task_Date { get; set; }
        public int? Status { get; set; }
    }
}
