////////////////////////////////Accessssssssss
// Add Leave
//window.addEventListener('load', (event) => {
//    alert('The page has fully loaded');
//    var classdata = document.getElementById("tab11");
//    classdata.classList.add("active");
//    document.getElementById("tab5").style.display = "none";
//    document.getElementById("tab66").style.display = "none";
//    document.getElementById("tab7").style.display = "none";
//    document.getElementById("tab12").style.display = "none";
//});

function Add_Leave() {
    $('#Model_Leave_Add').modal('show');
    $('#btnAdd').show();
    return false;
}


// Address Details
function Add_Address_Emp(Add_Id,User_ID,Address,City) {
    
    $('#Add_Address_Addid').val(Add_Id);
    $('#Add_Address_Userid').val(User_ID);
    $('#Add_Address_address').val(Address);
    $('#Add_Address_City').val(City);
    $('#Model_AddAddress').modal('show');
    if (Add_Id == 0) {
        $('#approvebtn').hide();
        $('#rejectbtn').hide();
        $('#deletebtn').hide();
    } else {
        $('#approvebtn').show();
        $('#rejectbtn').show();
        $('#deletebtn').show();
    }
    return false;
}
//Relation Details
function Add_Relation_Emp(Relation_Id, User_ID, Relation_Name, Person_Name, Mobile_No) {
    $('#Add_Relation_Relation_Id').val(Relation_Id);
    $('#Add_Relation_User_ID').val(User_ID);
    $('#Add_Relation_Relation_Name').val(Relation_Name);
    $('#Add_Relation_Person_Name').val(Person_Name);
    $('#Add_Relation_Mobile_No').val(Mobile_No);
    $('#Model_AddRelation').modal('show');
    if (Relation_Id == 0) {
        $('#approvebtn1').hide();
        $('#rejectbtn1').hide();
        $('#deletebtn1').hide();
    } else {
        $('#approvebtn1').show();
        $('#rejectbtn1').show();
        $('#deletebtn1').show();
    }
    return false;
}
//Education Details
function Add_Education_Emp(Edu_Id,User_ID,Edu_Name,university,Pass_Out) {    
    $('#Add_Education_Edu_Id').val(Edu_Id);
    $('#Add_Education_User_ID').val(User_ID);
    $('#Add_Education_Edu_Name').val(Edu_Name);
    $('#Add_Education_university').val(university);
    $('#Add_Education_Pass_Out').val(Pass_Out);
    $('#Model_AddEducation').modal('show');
    if (Edu_Id == 0) {
        $('#approvebtn2').hide();
        $('#rejectbtn2').hide();
        $('#deletebtn2').hide();
    } else {
        $('#approvebtn2').show();
        $('#rejectbtn2').show();
        $('#deletebtn2').show();
    }
    return false;
}

//Experience Details
function Add_Experience_Emp(Exp_Id, User_ID, Last_Company, working_years, Last_Year) {
    $('#Add_Experience_Edu_Id').val(Exp_Id);
    $('#Add_Experience_User_ID').val(User_ID);
    $('#Add_Experience_Last_Company').val(Last_Company);
    $('#Add_Experience_working_years').val(working_years);
    $('#Add_Experience_Last_Year').val(Last_Year);
    $('#Model_AddExperience').modal('show');
    if (Exp_Id == 0) {
        $('#approvebtn3').hide();
        $('#rejectbtn3').hide();
        $('#deletebtn3').hide();
    } else {
        $('#approvebtn3').show();
        $('#rejectbtn3').show();
        $('#deletebtn3').show();
    }
    return false;
}

//Document Details
function Add_Document_Emp(Doc_Id, User_ID, Doc_Type, Doc_Name) {
    $('#Add_Document_Doc_Id').val(Doc_Id);
    $('#Add_Document_User_ID').val(User_ID);
    $('#Add_Document_Doc_Type').val(Doc_Type);
    $('#Add_Document_Doc_Name').val(Doc_Name);    
    $('#Model_AddDocument').modal('show');
    if (Doc_Id == 0) {
        $('#approvebtn4').hide();
        $('#rejectbtn4').hide();
        $('#deletebtn4').hide();
    } else {
        $('#approvebtn4').show();
        $('#rejectbtn4').show();
        $('#deletebtn4').show();
    }
    return false;
}

//AssetType Details
function Add_AssetType(Type_id, Name) {
    $('#Add_AssetType_Type_id').val(Type_id);
    $('#Add_AssetType_Name').val(Name);
    $('#Model_AddAssetType').modal('show');
    if (Type_id == 0) {    
        $('#deletebtn').hide();
    } else {      
        $('#deletebtn').show();
    }
    return false;
}


//EmployeeType Details
function Add_EmpType(Usertype_id, Usertype_name) { 
    $('#Add_Emp_Type_id').val(Usertype_id);    
    $('#Add_EmpType_Name').val(Usertype_name);
    $('#Model_AddEmpType').modal('show');
    if (Usertype_id == 0) {
        $('#deletebtn').hide();
    } else {
        $('#deletebtn').show();
    }
    return false;
}



//Important  Document
function Add__Imp_Document() { 
    $('#Model_Add_Imp_Doc').modal('show');   
    return false;
}
//City Details
function Add_City(Cityid, CityName) {
    $('#Add_Cityid').val(Cityid);
    $('#Add_CityName').val(CityName);
    $('#Model_Add_City').modal('show');
    if (Cityid == 0) {
        $('#deletebtn').hide();
    } else {
        $('#deletebtn').show();
    }
    return false;
}
//Religion Details
function Add_Religion(Religion_Id, Religion_Name) {
    $('#Add_Religion_Id').val(Religion_Id);
    $('#Add_Religion_Name').val(Religion_Name);
    $('#Model_Add_Religion').modal('show');
    if (Religion_Id == 0) {
        $('#deletebtn').hide();
    } else {
        $('#deletebtn').show();
    }
    return false;
}


//Festival Details
function Add_Festival(Fest_Id, Fest_Name, Religion_Id) {
    $('#Add_Fest_Id').val(Fest_Id);
    $('#Add_Fest_Name').val(Fest_Name);
    $('#Add_Religion_Id').val(Religion_Id);
    $('#Model_Add_Festival').modal('show');
    if (Fest_Id == 0) {
        $('#deletebtn').hide();
    } else {
        $('#deletebtn').show();
    }
    return false;
}

//CustomDoc Details
function Add_CustomDoc(Cust_Id, Doc_Name, Doc_Disc) {
    $('#Add_Cust_Id').val(Cust_Id);
    $('#Add_Doc_Name').val(Doc_Name);
    $('#Add_Doc_Disc').val(Doc_Disc);
    $('#Model_Add_CustomDoc').modal('show');
    if (Cust_Id == 0) {
        $('#deletebtn').hide();
    } else {
        $('#deletebtn').show();
    }
    return false;
}

//Department Details
function Add_Department(Dept_ID, Department) {     
    $('#Add_Dept_ID').val(Dept_ID);
    $('#Add_Department').val(Department);   
    $('#Model_Add_Department').modal('show');
    if (Dept_ID == 0) {
        $('#deletebtn').hide();
    } else {
        $('#deletebtn').show();
    }
    return false;
}
//Designation Details
function Add_Designation(Desg_Id,Designation) {
    $('#Add_Desg_Id').val(Desg_Id);
    $('#Add_Designation').val(Designation);
    $('#Model_Add_Designation').modal('show');
    if (Desg_Id == 0) {
        $('#deletebtn').hide();
    } else {
        $('#deletebtn').show();
    }
    return false;
}


//Head Details
function Add_Head(HeadId, HeadName) {
    $('#model_header').html('Add Head');
    $('#HeadId').val(HeadId);
    $('#HeadName').val(HeadName);
    $('#Model_Add_Head').modal('show');
    if (HeadId == 0) {
        $('#deletebtn').hide();
    } else {
        $('#deletebtn').show();
    }
   
    return false;
}


//Gift Details
function Add_Gift(Gift_Id, Gift_Name, User_Id, Festival_Id, str, Gift_Desc) {
    $('#Gift_Id').val(Gift_Id);
    $('#Gift_Name').val(Gift_Name);
    $('#User_Id').val(User_Id);
    $('#Date').val(str);
    $('#Festival_Id').val(Festival_Id);
    $('#Gift_Desc').val(Gift_Desc);    
    $('#Model_Add_Gift').modal('show');
    if (Gift_Id == 0) {
        $('#deletebtn').hide();
    } else {
        $('#deletebtn').show();
    }
    return false;
}

//Gift Details
function Add_Holiday(Holiday_Id, Holiday_Name, Holiday_Date) {
    $('#Holiday_Id').val(Holiday_Id);
    $('#Holiday_Name').val(Holiday_Name);
    $('#Holiday_Date').val(Holiday_Date);   
    $('#Model_Add_Holiday').modal('show');
    if (Holiday_Id == 0) {
        $('#deletebtn').hide();
    } else {
        $('#deletebtn').show();
    }
    return false;
}

//Gift Details
function Add_Shift(Shift_Id, Shift_Name, Shift_From, Shift_To) {

    $('#Shift_Id').val(Shift_Id);
    $('#Shift_Name').val(Shift_Name);
    $('#Shift_From').val(Shift_From);
    $('#Shift_To').val(Shift_To);
    $('#Model_Add_Shift').modal('show');
    if (Shift_Id == 0) {
        $('#deletebtn').hide();
    } else {
        $('#deletebtn').show();
    }
    return false;
}



//Designation Details
function Add_Asset(Asset_id, Asset_Name, Asset_Type, Purchase_Date, Service_Duration, Detail, Serial_Number, Status) {
    $('#Asset_id').val(Asset_id);
    $('#Asset_Name').val(Asset_Name);
    $('#Asset_Type').val(Asset_Type);
    $('#Purchase_Date').val(Purchase_Date);   
    $('#Service_Duration').val(Service_Duration);
    $('#Detail').val(Detail);  
    $('#Serial_Number').val(Serial_Number);  
    $('#Status').val(Status);  
    $('#Model_Add_Asset').modal('show');
    if (Asset_id == 0) {
        $('#deletebtn').hide();
    } else {
        $('#deletebtn').show();
    }
    return false;
}

/////taskType Details
function Add_TaskType(TaskType_Id, TaskType_Name) {
    $('#TaskType_Id').val(TaskType_Id);
    $('#TaskType_Name').val(TaskType_Name);
    $('#Model_Add_TaskType').modal('show');
    if (TaskType_Id == 0) {
        $('#deletebtn').hide();
    } else {
        $('#deletebtn').show();
    }
    return false;
}
//Project Details
function Add_Project(Project_Id, Project_Name) {
    $('#Project_Id').val(Project_Id);
    $('#Project_Name').val(Project_Name);
    $('#Model_Add_Project').modal('show');
    if (Project_Id == 0) {
        $('#deletebtn').hide();
    } else {
        $('#deletebtn').show();
    }
    return false;
}
//EmployeeAdd Details
function Add_Project_Emp(Project_Id, Project_Assign_Id, Emp_Id, User_Name) {   
    $('#Project_Id').val(Project_Id); 
    $('#Assign_Id').val(Project_Assign_Id); 
    $('#Emp_Id').val(Emp_Id);    
    $('#Emp_Name').val(User_Name);
    $('#Model_Emp_Project').modal('show');   
    if (Project_Assign_Id == 0) {
        $('#demo').show();
        $('#demo1').hide();
        $('#deletebtn').hide();
        $('#savebtn').show();
    } else {
        $('#demo').hide();
        $('#demo1').show();
        $('#deletebtn').show();
        $('#savebtn').hide();
    }
    return false;
}

//EmployeeAdd Details
function Add_Asset_Movement(Asset_id) {
    $('#Asset_id').val(Asset_id);
    $('#Model_Asset_Movement').modal('show');   
    return false;
}
//EmployeeAdd Details
function Add_Project_Task(Task_Id,Task_Name,Emp_Id, Project_Id, Task_Type, Duration_Unit, Duration, Actual_Start_Date, Revised_End_Date, Description) {
    $('#Task_Id').val(Task_Id);
    $('#Task_Name').val(Task_Name);
    $('#Emp_Id').val(Emp_Id);
    $('#Project_Id').val(Project_Id);
  
    $('#Task_Type').val(Task_Type);
    $('#Duration_Unit').val(Duration_Unit);
    $('#Duration').val(Duration);
    $('#Actual_Start_Date').val(Actual_Start_Date);
    $('#Revised_End_Date').val(Revised_End_Date);
    $('#Description').val(Description);
    $('#Model_Add_Task').modal('show');
    if (Task_Id == 0) {
        $('#approvebtn').hide();
        $('#rejectbtn').hide();
        $('#deletebtn').hide();
    } else {
        $('#approvebtn').show();
        $('#rejectbtn').show();
        $('#deletebtn').show();
    }
    return false;
}


//Employee Advance Req Details
function Add_AdvanceReq(Req_Id, Place, User_Id, Parent_Type, Project_Id, Remark, Remark_For_Admin, Description, Granted_Amount, Requested_Amount, S_Date, E_Date, Project_Name) {  
  
    $('#Req_Id').val(Req_Id);
    $('#Place').val(Place);
    $('#User_Id').val(User_Id);
    $('#Parent_Type').val(Parent_Type);
  
    $('#Project_Id').val(Project_Id);    
    $('#Remark').val(Remark);   
    $('#Remark_For_Admin').val(Remark_For_Admin);
    $('#Description').val(Description);
    $('#Granted_Amount').val(Granted_Amount);
    $('#Requested_Amount').val(Requested_Amount);
    $('#S_Date').val(S_Date);
    $('#E_Date').val(E_Date);
    $('#Project_Name').val(Project_Name);
    $('#Model_Add_AdvanceReq').modal('show');
    if (Req_Id == 0) {
        $('#approvebtn').hide();
        $('#rejectbtn').hide();
        $('#deletebtn').hide();
    } else {
        getProject()
        getparent()
        $('#approvebtn').show();
        $('#rejectbtn').show();
        $('#deletebtn').show();
    }
    return false;
}

//Employee Voucher Req Details
function Add_VoucherReq(Voucher_Id, User_Id, Project_Id, Req_Id, Parent_Type, Place, Description, Remark, Remark_For_Administrator, Petrol, Mobile, Travelling, Conveyance, Total_Amount, Advance_Amount, Deduction_Amount, Payable_Amount, Trans_Type) {
    $('#Voucher_Id').val(Voucher_Id);
    $('#User_Id').val(User_Id);
    $('#Project_Id').val(Project_Id);
    $('#Req_Id').val(Req_Id);
    $('#Parent_Type').val(Parent_Type);
    $('#Place').val(Place);
    $('#Description').val(Description);
    $('#Remark').val(Remark);
    $('#Remark_For_Administrator').val(Remark_For_Administrator);
    $('#Petrol').val(Petrol);
    $('#Mobile').val(Mobile);
    $('#Travelling').val(Travelling);
    $('#Conveyance').val(Conveyance);
    $('#Total_Amount').val(Total_Amount);
    $('#Advance_Amount').val(Advance_Amount);
    $('#Deduction_Amount').val(Deduction_Amount);
    $('#Payable_Amount').val(Payable_Amount);
    $('#Trans_Type').val(Trans_Type);
    $('#Model_Add_VoucherReq').modal('show');
    if (Voucher_Id == 0) {
        $('#deletebtn').hide();
    } else {
        $('#deletebtn').show();
    }
    return false;
}

//Employee Advance Req Details
function Add_Emp_Leave(leave_Id, User_Id, leave_days, From_Date, To_Date, Leave_Type, Reason, Remark, F_Date, T_Date) {
    $('#leave_Id').val(leave_Id);
    $('#User_Id').val(User_Id);
    $('#leave_days').val(leave_days);
    $('#From_Date').val(From_Date);
    $('#To_Date').val(To_Date);
    $('#Leave_Type').val(Leave_Type);
    $('#Reason').val(Reason);
    $('#Remark').val(Remark);   
    $('#F_Date_Ch').val(F_Date);   
    $('#T_Date_Ch').val(T_Date);   
    $('#Model_Add_Leave_M').modal('show');
    if (leave_Id == 0) {
        $('#approvebtn').hide();
        $('#deletebtn').hide();
        $('#rejectbtn').hide();
    } else {
        getHalfLeave();
        $('#approvebtn').show();
        $('#deletebtn').show();
        $('#rejectbtn').show();
    }
    return false;
}


//Add Resignation
function Add_Resignation(Resignation_Id, Emp_Id, Department, Designation, Team_Name, Reporting_Person, Reason_For_Exit, Suggestion_For_Company, Remark_For_Resignation, HR_Authority, Reliving_Date, Date_Of_Join, Date_Of_Resignation) {
    $('#Resignation_Id').val(Resignation_Id);
    $('#Emp_Id').val(Emp_Id);
    $('#Department').val(Department);
    $('#Designation').val(Designation);
    $('#Team_Name').val(Team_Name);
    $('#Reporting_Manager').val(Reporting_Person);
    $('#Reason_For_Exit').val(Reason_For_Exit);
    $('#Suggestion_For_Company').val(Suggestion_For_Company);
    $('#Remark_For_Resignation').val(Remark_For_Resignation);
    $('#HR_Authority').val(HR_Authority);
    $('#Reliving_Date').val(Reliving_Date);
    $('#Date_Of_Join').val(Date_Of_Join);
    $('#Date_Of_Resignation').val(Date_Of_Resignation);
    $('#Model_Add_Resignation_M').modal('show');
    if (Resignation_Id == 0) {
        $('#deletebtn').hide();
    } else {
        $('#deletebtn').show();
    }
    return false;
}
function Punch_Update(Punch_Id, Time) {    
    $('#Punch_Id').val(Punch_Id);      
    $('#Punchdate').val(Time);
    $('#Model_Punch_Update').modal('show');    
    return false;
}
function Add_Cmp_Doc(Cmp_Doc_Id, Doc_Name) { 
    $('#Doc_Id').val(Cmp_Doc_Id);      
    $('#Doc_Name').val(Doc_Name);    
    $('#Model_Add_Cmp_Doc').modal('show');
    if (Cmp_Doc_Id == 0) {        
        $('#deletebtn').hide();
    } else {
        $('#deletebtn').show();
    }
   
    return false;
}

function Change_Emp_Pass(User_ID, User_Password) {
    $('#Emp_Id').val(User_ID);    
    $('#Old_Password').val(User_Password);    
    $('#Model_Change_Pass').modal('show');
    return false;
}



///////////Addd User

var OnAddUser = function (result) {
    alert(result.ErrorMsg);
    var user_id = result.user_id;
    window.location.href = '/Access/User_update?User_id=' + user_id;

}
    

var OnUpdateUser = function (result) {
    if (result.ErrorMsg != null) {
        alert(result.ErrorMsg);
    }
    if (result.ToastMsgSuc != null) {
        toastr.success(result.ToastMsgSuc);
    }

    if (result.ToastMsgFail != null) {
        toastr.error(result.ToastMsgFail);
    }
    if (result.Refresh == "Default") {
        location.reload();
    } else if (result.Refresh != null) {
        window.location.href = result.Refresh;
    }
}



var OnAjaxcallbackPage = function (result) {   
   
    if (result.ToastMsgSuc != null) {     
        $('#mobileotp11').val(result.Mobile);
        toastr.success(result.ToastMsgSuc);
    }

    else if (result.ToastMsgFail != null) {  
        toastr.error(result.ToastMsgFail);
    }
    if (result.Refresh == "Default") {
        location.reload();
    } else if (result.Refresh != null) {
        window.location.href = result.Refresh;
    }  
}

var OnAjaxcallbackOTPPage = function (result) {
    if (result.EnableError) {
        alert(result.ErrorMsg);
    }
    else {
        alert(result.SuccessMsg);
        window.location.reload();
    }
}









//////////////////////////////////Employee  Master JS


//Reporting Person Can Approve Leave Details
function Apr_Rep_Leave(leave_Id, User_Id, leave_days, From_Date, To_Date, Leave_Type, Reason, Remark, F_Date, T_Date) {
    $('#leave_Id').val(leave_Id);
    $('#User_Id').val(User_Id);
    $('#leave_days').val(leave_days);
    $('#From_Date').val(From_Date);
    $('#To_Date').val(To_Date);
    $('#Leave_Type').val(Leave_Type);
    $('#Reason').val(Reason);
    $('#Remark').val(Remark);
    $('#F_Date_Ch').val(F_Date);
    $('#T_Date_Ch').val(T_Date);
    $('#Model_Reporting_Leave_M').modal('show');
    if (leave_Id == 0) {
        $('#approvebtn').hide();
        $('#deletebtn').hide();
        $('#rejectbtn').hide();
    } else {
        getHalfLeave();
        $('#approvebtn').show();
        $('#deletebtn').show();
        $('#rejectbtn').show();
    }
    return false;
}

//Add Resignation
function Emp_Resignation(Resignation_Id, Department, Designation, Team_Name, Reporting_Person, Reason_For_Exit, Suggestion_For_Company, Remark_For_Resignation, HR_Authority, Reliving_Date, Date_Of_Join, Date_Of_Resignation) {
    $('#Resignation_Id').val(Resignation_Id);
    $('#Department').val(Department);
    $('#Designation').val(Designation);
    $('#Team_Name').val(Team_Name);
    $('#Reporting_Manager').val(Reporting_Person);
    $('#Reason_For_Exit').val(Reason_For_Exit);
    $('#Suggestion_For_Company').val(Suggestion_For_Company);
    $('#Remark_For_Resignation').val(Remark_For_Resignation);
    $('#HR_Authority').val(HR_Authority);
    $('#Reliving_Date').val(Reliving_Date);
    $('#Date_Of_Join').val(Date_Of_Join);
    $('#Date_Of_Resignation').val(Date_Of_Resignation);
    $('#Model_Emp_Resignation_M').modal('show');
    if (Resignation_Id == 0) {        
        getEmp_Detail();
        $('#deletebtn').hide();
    } else {
        getEmp_Detail();
        $('#deletebtn').show();
    }
    return false;
}

//Employee Emp Req Details
function Emp_Leave_Add(leave_Id, leave_days, From_Date, To_Date, Leave_Type, Reason, Remark, F_Date, T_Date,Status) {
    $('#leave_Id').val(leave_Id);
    $('#leave_days').val(leave_days);
    $('#From_Date').val(From_Date);
    $('#To_Date').val(To_Date);
    $('#Leave_Type').val(Leave_Type);
    $('#Reason').val(Reason);
    $('#Remark').val(Remark);
    $('#F_Date_Ch').val(F_Date);
    $('#T_Date_Ch').val(T_Date);
    $('#Model_Add_Leave_M').modal('show');
    if (leave_Id == 0)
    {
        $('#deletebtn').hide();
    }
    else
    {
        if (Status == 0)
        {
            getEmpHalfLeave();
            $('#deletebtn').show();
        } else {
            $('#deletebtn').hide();

        }
    }    
    return false;
}

// Address Details
function Add_Emp_Address(Add_Id,Address, City) {
    $('#Address_Addid').val(Add_Id);
    $('#Address_address').val(Address);
    $('#Address_City').val(City);
    $('#Model_EmpAddress').modal('show');
    if (Add_Id == 0) { 
        $('#deletebtn').hide();
    } else {      
        $('#deletebtn').show();
    }
    return false;
}
//Relation Details
function Add_Emp_Relation(Relation_Id, Relation_Name, Person_Name, Mobile_No) {
    $('#Relation_Relation_Id').val(Relation_Id);
    $('#Relation_Relation_Name').val(Relation_Name);
    $('#Relation_Person_Name').val(Person_Name);
    $('#Relation_Mobile_No').val(Mobile_No);
    $('#Model_EmpRelation').modal('show');
    if (Relation_Id == 0) {    
        $('#deletebtn1').hide();
    } else {   
        $('#deletebtn1').show();
    }
    return false;
}
//Education Details
function Add_Emp_Education(Edu_Id, Edu_Name, university, Pass_Out) {
    $('#Education_Edu_Id').val(Edu_Id);
    $('#Education_Edu_Name').val(Edu_Name);
    $('#Education_university').val(university);
    $('#Education_Pass_Out').val(Pass_Out);
    $('#Model_EmpEducation').modal('show');
    if (Edu_Id == 0) {      
        $('#deletebtn2').hide();
    } else {  
        $('#deletebtn2').show();
    }
    return false;
}

//Experience Details
function Add_Emp_Experience(Exp_Id, Last_Company, working_years, Last_Year) {
    $('#Experience_Edu_Id').val(Exp_Id);
    $('#Experience_Last_Company').val(Last_Company);
    $('#Experience_working_years').val(working_years);
    $('#Experience_Last_Year').val(Last_Year);
    $('#Model_EmpExperience').modal('show');
    if (Exp_Id == 0) {
        $('#deletebtn3').hide();
    } else {
        $('#deletebtn3').show();
    }
    return false;
}

//Document Details
function Add_Emp_Document(Doc_Id, Doc_Type, Doc_Name) {
    $('#Document_Doc_Id').val(Doc_Id);
    $('#Document_Doc_Type').val(Doc_Type);
    $('#Document_Doc_Name').val(Doc_Name);
    $('#Model_EmpDocument').modal('show');
    if (Doc_Id == 0) { 
        $('#deletebtn4').hide();
    } else {
        $('#deletebtn4').show();
    }
    return false;
}



//Employee Project Task Details
function Add_Emp_Task(Task_Id, Task_Name, Project_Id, Task_Type, Duration_Unit, Duration, Actual_Start_Date, Revised_End_Date, Description, Status) {
    $('#Task_Id').val(Task_Id)
    $('#Task_Name').val(Task_Name);
    $('#Project_Id').val(Project_Id);
    $('#Task_Type').val(Task_Type);
    $('#Duration_Unit').val(Duration_Unit);
    $('#Duration').val(Duration);
    $('#Actual_Start_Date').val(Actual_Start_Date);
    $('#Revised_End_Date').val(Revised_End_Date);
    $('#Description').val(Description);
    $('#Model_Emp_Task').modal('show');
    if (Task_Id == 0) {        
        $('#deletebtn').hide();
    } else {      
        $('#deletebtn').show(); 
        if (Status == 7) {
            $('#stopbtn').show();
            $('#completebtn').show();
        } else {
            $('#stopbtn').hide();
            $('#completebtn').hide();
        }
        if (Status == 1 || Status == 8) {
            $('#Runningbtn').show();
        }
        else {
            $('#Runningbtn').hide();
        }
    }    
    return false;
}



//Employee Advance Req Details
function Emp_AdvanceReq(Req_Id, Place, Parent_Type, Project_Id, Remark, Remark_For_Admin, Description, Granted_Amount, Requested_Amount, S_Date, E_Date, Project_Name) {

    $('#Req_Id').val(Req_Id);
    $('#Place').val(Place);
    $('#Parent_Type').val(Parent_Type);
    $('#Project_Id').val(Project_Id);
    $('#Remark').val = Remark.innerHTML;
    $('#Remark_For_Admin').val = Remark_For_Admin.innerHTML;
    $('#Description').val = Description.innerHTML;
    $('#Granted_Amount').val(Granted_Amount);
    $('#Requested_Amount').val(Requested_Amount);
    $('#S_Date').val(S_Date);
    $('#E_Date').val(E_Date);
    $('#Project_Name').val(Project_Name);
    $('#Model_Emp_AdvanceReq').modal('show');
    if (Req_Id == 0) {        
        $('#deletebtn').hide();
    } else {       
        getparent()      
        $('#deletebtn').show();
    }
    return false;
}


///////////////////////////////////Home Controller
function Forgot_Pass() {
    $('#Model_Forgot_Pass').modal('show');
    $('#btnAdd').show();
    return false;
}
function CheckOTP() {
    $('#Model_CheckOTP').modal('show');
    $('#btnAdd').show();
    return false;
}

function Company_Registration() {
    $('#Model_Registration').modal('show');
    $('#btnAdd').show();
    return false;
}
/////////////////Employee Edit Comment
function EPEditComment(Comnt_ID, Comments, Emp_Id, Task_Id) {
    $('#UComnt_ID').val(Comnt_ID);
    $('#UComments').val(Comments);
    $('#UEmp_Id').val(Emp_Id);
    $('#UTask_Id').val(Task_Id);
    $('#EPEdit_Comment_M').modal('show');
    $('#btnAdd').show();
    return false;
}

/////////////////Admin Edit Comment
function ADEditComment(Comnt_ID, Comments, Emp_Id, Task_Id) {
    $('#UComnt_ID').val(Comnt_ID);
    $('#UComments').val(Comments);
    $('#UEmp_Id').val(Emp_Id);
    $('#UTask_Id').val(Task_Id);
    $('#ADEdit_Comment_M').modal('show');
    $('#btnAdd').show();
    return false;
}
//Event Details
function Add_Event(Event_Id, Title, Date, Description, User_Id) {

    $('#Add_Event_ID').val(Event_Id);
    $('#Title').val(Title);
    $('#Date').val(Date);
    $('#Description').val(Description);
    $('#User_Id').val(User_Id);
    $('#Model_Add_Event').modal('show');
    if (Event_Id == 0) {
        $('#deletebtn').hide();
    } else {
        $('#deletebtn').show();
    }
    return false;
}

//Notice Board Details
function Add_Notice(Notice_Id, Title, Date, Description) {

    $('#Add_Notice_Id').val(Notice_Id);
    $('#Title').val(Title);
    $('#Date').val(Date);
    $('#Description').val(Description);
    $('#Model_Add_Notice').modal('show');
    if (Notice_Id == 0) {
        $('#deletebtn').hide();
    } else {
        $('#deletebtn').show();
    }
    return false;
}