////////////////////////////////Accessssssssss
// Add Leave
let tab = null;
let tabaddress = null;


function OnAjaxSuccess(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
        //$("#Model_Add_Notice").modal('hide');
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
        addProductNotice(data.ToastMsgFail, '', '', 'failure');
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}

function Add_Leave() {
    $('#Model_Leave_Add').modal('show');
    $('#btnAdd').show();
    return false;
}


// Address Details
function Add_Address_Emp(Add_Id, User_ID, Address, City) {
    
    $('#Add_Address_Addid').val(Add_Id);
    $('#Add_Address_Userid').val(User_ID);
    document.getElementById("Add_Address_address").innerHTML = Address;
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
function Add_Education_Emp(Edu_Id, User_ID, Edu_Name, university, Pass_Out,Field) {
    $('#Add_Education_Edu_Id').val(Edu_Id);
    $('#Add_Education_User_ID').val(User_ID);
    $('#Add_Education_Edu_Name').val(Edu_Name);
    $('#Add_Education_university').val(university);
    $('#Add_Education_Pass_Out').val(Pass_Out);
    $('#Field').val(Field);
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
    $('#Add_Document_Doc_Name').val(Doc_Name).trigger('change.select2');
    //$('#Add_Document_Doc_Name').val(Doc_Name);
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


//Commerical  Document
function Add_Commerical_Document(Document_Id, Document_HeadId, Document_Head2Id, DMonth, DName) {
    
    $('#Document_Id').val(Document_Id);
    $('#HeadId').val(Document_HeadId).trigger('change.select2');
    getCompanyDocHeads2(Document_Head2Id);
    $('#datepicker').val(DMonth);
    $('#DName').val(DName);
    $('#file').hide();


    $('#Model_Add_Imp_Doc').modal('show');
    
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
    $('#Add_Religion_Id').val(Religion_Id).trigger('change.select2');
    //$('#Add_Religion_Id').val(Religion_Id);
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
function Add_Designation(Desg_Id, Designation) {
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
    
    $('#model_header').html('Update Head');
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

function Add_Head2(HeadId, HeadName) {

    $('#model_header').html('Update Head2');
    $('#Head2Id').val(HeadId);
    $('#HeadName2').val(HeadName);
    $('#Model_Add_Head').modal('show');
    if (HeadId == 0) {
        $('#deletebtn').hide();
    } else {
        $('#deletebtn').show();
    }

    return false;
}

function Add_ProjectHeadMaster(Project_MasterID, Project_Name, Project_Address) {
    
   
    $('#Project_MasterID').val(Project_MasterID);
    $('#Project_Name').val(Project_Name);
    $('#Project_Address').val(Project_Address);
    $('#Model_Add_ProjectMaster').modal('show');
    if (Project_MasterID == 0) {
        $('#deletebtn').hide();
    } else {
        $('#model_header').html('Update Project Head Master');
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
    document.getElementById("Gift_Desc").innerHTML = Gift_Desc;
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
    $('#Asset_Type').val(Asset_Type).trigger('change.select2');
   // $('#Asset_Type').val(Asset_Type);
    $('#Purchase_Date').val(Purchase_Date);
    $('#Service_Duration').val(Service_Duration);
    document.getElementById("Detail").innerHTML = Detail;
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
function Add_Project_Task(Task_Id, Task_Name, Emp_Id, Project_Id, Task_Type, Duration_Unit, Duration, Actual_Start_Date, Revised_End_Date, Description) {
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
function Add_AdvanceReq(Req_Id, Place, User_Id, Parent_Type, Project_Id, Remark, Remark_For_Admin, Description, Granted_Amount, Requested_Amount, S_Date, E_Date, Project_Name, Status) {
   
    $('#Req_Id').val(Req_Id);
    $('#Place').val(Place);
    $('#User_Id').val(User_Id).trigger('change');
  /*  $('#User_Id').val(User_Id);*/
    $('#Parent_Type').val(Parent_Type).trigger('change');
    $('#Project_Id').val(Project_Id).trigger('change');
    
    /*$('#Project_Id_Js').val(Project_Id);*/
    document.getElementById("Remark").innerHTML = Remark;
    document.getElementById("Remark_For_Admin").innerHTML = Remark_For_Admin;
    document.getElementById("Description").innerHTML = Description;
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
    }
    else {
        getProject();
        getparent();
        $('#approvebtn').show();
        $('#rejectbtn').show();
        $('#deletebtn').show();
        if (Status == 1) {
            $('#savechange').hide();
            $('#approvebtn').hide();
            $('#rejectbtn').hide();
            $('#deletebtn').hide();
        }
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
    document.getElementById("Reason").innerHTML = Reason;
    document.getElementById("Remark").innerHTML = Remark;
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
    document.getElementById("Suggestion_For_Company").innerHTML = Suggestion_For_Company;
    document.getElementById("Remark_For_Resignation").innerHTML = Remark_For_Resignation;
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



//////////////////////////////////Employee  Master JS


//Reporting Person Can Approve Leave Details
function Apr_Rep_Leave(leave_Id, User_Id, leave_days, From_Date, To_Date, Leave_Type, Reason, Remark, F_Date, T_Date) {
    $('#leave_Id').val(leave_Id);
    $('#User_Id').val(User_Id);
    $('#leave_days').val(leave_days);
    $('#From_Date').val(From_Date);
    $('#To_Date').val(To_Date);
    $('#Leave_Type').val(Leave_Type);
    document.getElementById("Remark").innerHTML = Remark;
    document.getElementById("Reason").innerHTML = Reason;
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
    document.getElementById("Suggestion_For_Company").innerHTML = Suggestion_For_Company;
    document.getElementById("Remark_For_Resignation").innerHTML = Remark_For_Resignation;
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
function Emp_Leave_Add(leave_Id, leave_days, From_Date, To_Date, Leave_Type, Reason, Remark, F_Date, T_Date, Status) {

    $('#leave_Id').val(leave_Id);
    $('#leave_days').val(leave_days);
    $('#From_Date').val(From_Date);
    $('#To_Date').val(To_Date);
    $('#Leave_Type').val(Leave_Type);
    document.getElementById("Remark").innerHTML = Remark;
    document.getElementById("Reason").innerHTML = Reason;
    $('#F_Date_Ch').val(F_Date);
    $('#T_Date_Ch').val(T_Date);
    $('#Model_Add_Leave_M').modal('show');
    if (leave_Id == 0) {
        $('#deletebtn').hide();
    }
    else {
        if (Status == 0) {
            getEmpHalfLeave();
            $('#deletebtn').show();
        } else {
            $('#deletebtn').hide();

        }
    }
    return false;
}

// Address Details
function Add_Emp_Address(Add_Id, Address, City) {
    $('#Address_Addid').val(Add_Id);
    document.getElementById("Address_address").innerHTML = Address;
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
function Add_Emp_Education(Edu_Id, Edu_Name, university, Pass_Out,Field) {
    $('#Education_Edu_Id').val(Edu_Id);
    $('#Education_Edu_Name').val(Edu_Name);
    $('#Education_university').val(university);
    $('#Education_Pass_Out').val(Pass_Out);
    $('#Field').val(Field);
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
    $('#Parent_Type').val(Parent_Type).trigger('change');
    $('#Project_Id').val(Project_Id);
    document.getElementById("Remark").innerHTML = Remark;
    document.getElementById("Remark_For_Admin").innerHTML = Remark_For_Admin;
    document.getElementById("Description").innerHTML = Description;
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
function Add_Event(Event_Id, Title, Date, Description, User_id) {
   
    $('#Add_Event_ID').val(Event_Id);
    $('#Title').val(Title);
    $('#Date').val(Date);
    $('#Description').val(Description);
    $('#User_id').val(User_id).trigger('change.select2');
   // $('#User_id').val(User_id);
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


function OnUpdateUserAt(data) {
 
    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
        $("#Model_AddAssetType").modal('hide');
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
        //alert(data.ToastMsgSuc);
    }
    if (data.ErrorTitle == "F") {
        addProductNotice(data.ToastMsgFail, '', '', 'failure');
        //alert(data.ToastMsgFail);
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
        //location.reload();

    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }


}
function OnUpdateEmployeeType(data) {
   
    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
        $("#Model_AddEmpType").modal('hide');
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
        //alert(data.ToastMsgSuc);
    }
    if (data.ErrorTitle == "F") {
        addProductNotice(data.ToastMsgFail, '', '', 'failure');
        //alert(data.ToastMsgFail);
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
        //location.reload();

    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}

function OnUpdateCity(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
        $("#Model_Add_City").modal('hide');
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
        addProductNotice(data.ToastMsgFail, '', '', 'failure');
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);

    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}

function OnUpdateReligion(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
        $("#Model_Add_Religion").modal('hide');
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
        addProductNotice(data.ToastMsgFail, '', '', 'failure');
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}

function OnUpdateFestival(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
        $("#Model_Add_Festival").modal('hide');
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
        addProductNotice(data.ToastMsgFail, '', '', 'failure');
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}

function OnUpdateCustomDocument(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
  
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
        addProductNotice(data.ToastMsgFail, '', '', 'failure');

    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}

function OnUpdateHoliday(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
        $("#Model_Add_Holiday").modal('hide');
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
        addProductNotice(data.ToastMsgFail, '', '', 'failure');
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}


function OnUpdateAdminGift(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
        $("#Model_Add_Gift").modal('hide');
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
        addProductNotice(data.ToastMsgFail, '', '', 'failure');
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}

function OnUpdateAdminTaskType(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
        $("#Model_Add_TaskType").modal('hide');
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
        addProductNotice(data.ToastMsgFail, '', '', 'failure');
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}
function OnUpdateDesignation(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
        $("#Model_Add_Designation").modal('hide');
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
        addProductNotice(data.ToastMsgFail, '', '', 'failure');
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}

function OnUpdateHead(data) {
    
    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
        $("#Model_Add_Head").modal('hide');
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
        addProductNotice(data.ToastMsgFail, '', '', 'failure');
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}
function OnUpdateProjectMaster(data) {
    
    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
        $("#Model_Add_ProjectMaster").modal('hide');
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
        addProductNotice(data.ToastMsgFail, '', '', 'failure');
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}
function OnUpdateHead2(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
        $("#Model_Add_Head").modal('hide');
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
        addProductNotice(data.ToastMsgFail, '', '', 'failure');
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}
function OnUpdateDepartment(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
        $("#Model_Add_Department").modal('hide');
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
        addProductNotice(data.ToastMsgFail, '', '', 'failure');
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}
function OnUpdateShift(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
        $("#Model_Add_Shift").modal('hide');
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
        addProductNotice(data.ToastMsgFail, '', '', 'failure');
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}

//function OnUpdateImp_Document_Add(data) {

//    if (data.ErrorMsg != null) {
//        alert(data.ErrorMsg);
//    }
//    if (data.ErrorTitle == "S") {
//        $("#Model_Add_Imp_Doc").modal('hide');
//        addProductNotice(data.ToastMsgSuc, '', '', 'success');
//    }
//    if (data.ErrorTitle == "F") {
//        addProductNotice(data.ToastMsgFail, '', '', 'failure');
//    }
//    if (data.Refresh == "Default") {
//        setTimeout(function () {
//            location.reload();
//        }, 2000);
//    }
//    else if (data.Refresh != null) {
//        window.location.href = data.Refresh;
//    }
//}
function OnUpdateImp_Document_Add(data) {
  
    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
        document.getElementById("msgdig").style.display = "block";
        document.getElementById("failuremsg").innerText = data.ErrorMsg;
    }
    if (data.ErrorTitle == "S") {

        var chkmodal = $('#Model_Add_Imp_Doc').hasClass('show');
        if (chkmodal == true) {
            $("#Model_Add_Imp_Doc").modal('hide');

        }
        else {

        }
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
      
        var chkmodal = $('#Model_Add_Imp_Doc').hasClass('show');

        if (chkmodal == true) {
            document.getElementById("msgdig").style.display = "block";
            document.getElementById("failuremsg").innerText = data.ToastMsgFail;

        }
        else {
            addProductNotice(data.ToastMsgFail, '', '', 'failure');
        }
        stopMultipleSubmitImpDoc();

    }

    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}

function OnUpdateCommercial_Add(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
        document.getElementById("msgdig").style.display = "block";
        document.getElementById("failuremsg").innerText = data.ErrorMsg;
    }
    if (data.ErrorTitle == "S") {

        var chkmodal = $('#Model_Add_Imp_Doc').hasClass('show');
        if (chkmodal == true) {
            $("#Model_Add_Imp_Doc").modal('hide');

        }
        else {

        }
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {

        var chkmodal = $('#Model_Add_Imp_Doc').hasClass('show');

        if (chkmodal == true) {
            document.getElementById("msgdig").style.display = "block";
            document.getElementById("failuremsg").innerText = data.ToastMsgFail;

        }
        else {
            addProductNotice(data.ToastMsgFail, '', '', 'failure');
        }
        stopMultipleSubmitImpDoc();

    }

    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}
function OnUpdateAdminAsset(data) {

   
    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
        document.getElementById("msgdig").style.display = "block";
        document.getElementById("failuremsg").innerText = data.ErrorMsg;
    }
    if (data.ErrorTitle == "S") {

        var chkmodal = $('#Model_Add_Asset').hasClass('show');
        if (chkmodal == true) {
            $("#Model_Add_Asset").modal('hide');

        }
        else {

        }
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {

        var chkmodal = $('#Model_Add_Asset').hasClass('show');

        if (chkmodal == true) {
            document.getElementById("msgdig").style.display = "block";
            document.getElementById("failuremsg").innerText = data.ToastMsgFail;

        }
        else {
            addProductNotice(data.ToastMsgFail, '', '', 'failure');
        }
        stopMultipleSubmitAsset();

    }

    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}
function OnUpdateAdminAssetMovment(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
        $("#Model_Asset_Movement").modal('hide');
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
        addProductNotice(data.ToastMsgFail, '', '', 'failure');
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}
function OnUpdateAdminProject(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
        $("#Model_Add_Project").modal('hide');
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
        addProductNotice(data.ToastMsgFail, '', '', 'failure');
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}
function OnUpdateAdminProjectE(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
        $("#Model_Emp_Project").modal('hide');
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
        addProductNotice(data.ToastMsgFail, '', '', 'failure');
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}
function OnUpdateAdminEvent(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
        $("#Model_Add_Event").modal('hide');
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
        addProductNotice(data.ToastMsgFail, '', '', 'failure');
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}
function OnUpdateAdminNotice(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
        $("#Model_Add_Notice").modal('hide');
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
        addProductNotice(data.ToastMsgFail, '', '', 'failure');
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}
//function OnUpdateAdminRes(data) {

//    if (data.ErrorMsg != null) {
//        alert(data.ErrorMsg);
//    }
//    if (data.ErrorTitle == "S") {
//        $("#Model_Add_Resignation_M").modal('hide');
     
//        addProductNotice(data.ToastMsgSuc, '', '', 'success');
//    }
//    if (data.ErrorTitle == "F") {
   
//        addProductNotice(data.ToastMsgFail, '', '', 'failure');
//        stopMultipleSubmitReg();
      
//    }
//    if (data.Refresh == "Default") {
//        setTimeout(function () {
//            location.reload();
//        }, 2000);
//    }
//    else if (data.Refresh != null) {
//        window.location.href = data.Refresh;
//    }
//}
function OnUpdateAdminRes(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
        document.getElementById("msgdig").style.display = "block";
        document.getElementById("failuremsg").innerText = data.ErrorMsg;
    }
    if (data.ErrorTitle == "S") {

        var chkmodal = $('#Model_Add_Resignation_M').hasClass('show');
        if (chkmodal == true) {
            $("#Model_Add_Resignation_M").modal('hide');

        }
        else {

        }
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
        var chkmodal = $('#Model_Add_Resignation_M').hasClass('show');

        if (chkmodal == true) {
            document.getElementById("msgdig").style.display = "block";
            document.getElementById("failuremsg").innerText = data.ToastMsgFail;

        }
        else {
            addProductNotice(data.ToastMsgFail, '', '', 'failure');
        }
        stopMultipleSubmitReg();
       
    }

    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}

function OnAdminUserUpdate(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
        sessionStorage.setItem("Employedetailtab", "active");
        sessionStorage.removeItem("docTabtrack");
        sessionStorage.removeItem("addTabtrack");
        //$("#Model_Add_Notice").modal('hide');
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
        addProductNotice(data.ToastMsgFail, '', '', 'failure');
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}
function OnUploadUser(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
      
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
        addProductNotice(data.ToastMsgFail, '', '', 'failure');
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        debugger
        window.location.href = data.Refresh;
    }
}

function OnUpdateUserDelete(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
        //$("#Model_Add_Notice").modal('hide');
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
        addProductNotice(data.ToastMsgFail, '', '', 'failure');
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}
//function OnUpdateAdminEmp(data) {

//    if (data.ErrorMsg != null) {
//        alert(data.ErrorMsg);
//    }
//    if (data.ErrorTitle == "S") {
//        $("#Model_AddEmployee").modal('hide');
//        addProductNotice(data.ToastMsgSuc, '', '', 'success');
//    }
//    if (data.ErrorTitle == "F") {
//        addProductNotice(data.ToastMsgFail, '', '', 'failure');
//        stopMultipleSubmitEmp();
//    }
//    if (data.Refresh == "Default") {
//        setTimeout(function () {
//            location.reload();
//        }, 2000);
//    }
//    else if (data.Refresh != null) {
//        window.location.href = data.Refresh;
//    }
//}
function OnUpdateAdminEmp(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
        document.getElementById("msgdig").style.display = "block";
        document.getElementById("failuremsg").innerText = data.ErrorMsg;
    }
    if (data.ErrorTitle == "S") {

        var chkmodal = $('#Model_AddEmployee').hasClass('show');
        if (chkmodal == true) {
            $("#Model_AddEmployee").modal('hide');

        }
        else {

        }
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
        var chkmodal = $('#Model_AddEmployee').hasClass('show');

        if (chkmodal == true) {
            document.getElementById("msgdig").style.display = "block";
            document.getElementById("failuremsg").innerText = data.ToastMsgFail;

        }
        else {
            addProductNotice(data.ToastMsgFail, '', '', 'failure');
        }

        stopMultipleSubmitEmp();
    }

    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}

function OnAdminUpdateDoc(data) {

    //if (data.ErrorMsg != null) {
    //    alert(data.ErrorMsg);
    //}
    //if (data.ErrorTitle == "S") {
    //    $("#Model_AddDocument").modal('hide');
    //    addProductNotice(data.ToastMsgSuc, '', '', 'success');
    //}
    //if (data.ErrorTitle == "F") {
    //    addProductNotice(data.ToastMsgFail, '', '', 'failure');
    //}
    //if (data.Refresh == "Default") {
    //    setTimeout(function () {
    //        location.reload();
    //    }, 2000);
    //}
    //else if (data.Refresh != null) {
    //    window.location.href = data.Refresh;
    //}
    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
        document.getElementById("msgdig").style.display = "block";
        document.getElementById("failuremsg").innerText = data.ErrorMsg;
    }
    if (data.ErrorTitle == "S") {
        sessionStorage.setItem("docTabtrack", "active");
        sessionStorage.removeItem("addTabtrack");
        sessionStorage.removeItem("Employedetailtab");
        var chkmodal = $('#Model_AddDocument').hasClass('show');
        if (chkmodal == true) {
            $("#Model_AddDocument").modal('hide');

        }
        else {

        }
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
        var chkmodal = $('#Model_AddDocument').hasClass('show');

        if (chkmodal == true) {
            document.getElementById("msgdig").style.display = "block";
            document.getElementById("failuremsg").innerText = data.ToastMsgFail;

        }
        else {
            addProductNotice(data.ToastMsgFail, '', '', 'failure');
        }

        stopMultipleSubmitDoc();
    }

    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}
function OnAdminUserUpdateAddress(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
        $("#Model_AddAddress").modal('hide');
        sessionStorage.setItem("addTabtrack", "active");
        sessionStorage.removeItem("docTabtrack");
        sessionStorage.removeItem("Employedetailtab");
        /*  alert(data.ToastMsgSuc);*/
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
       /*   alert(data.ToastMsgFail);*/
        addProductNotice(data.ToastMsgFail, '', '', 'failure');
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}
function OnAdminUserUpdateRel(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
        $("#Model_AddRelation").modal('hide');
          //alert(data.ToastMsgSuc);
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
        //alert(data.ToastMsgFail);
        addProductNotice(data.ToastMsgFail, '', '', 'failure');
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}
function OnAdminUserUpdateEdu(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
        $("#Model_AddEducation").modal('hide');

        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {

        addProductNotice(data.ToastMsgFail, '', '', 'failure');
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}
function OnAdminUserUpdateExp(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
        $("#Model_AddExperience").modal('hide');

        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {

        addProductNotice(data.ToastMsgFail, '', '', 'failure');
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}
function OnAdminUserUpdateLeave(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
        $("#Model_Leave_Add").modal('hide');

        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {

        addProductNotice(data.ToastMsgFail, '', '', 'failure');
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}
function OnUserRight(data) {

    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}

function OnUpdateAdminProTask(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {

        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
        addProductNotice(data.ToastMsgFail, '', '', 'failure');
        stopMultipleSubmitProTask();
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
    
}
function OnUpdateAdminProTaskComment(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
        //$("#Model_Leave_Add").modal('hide');
      /*  alert(data.ToastMsgSuc);*/
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
    /*    alert(data.ToastMsgFail);*/
        addProductNotice(data.ToastMsgFail, '', '', 'failure');
    }
    if (data.Refresh == "Project_Task_Add ? TaskID =" +" ") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}
function OnUpdateAdminEmpPunch(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
        $("#Model_Punch_Update").modal('hide');

        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {

        addProductNotice(data.ToastMsgFail, '', '', 'failure');
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}

function OnUpdateAdminAdvReq(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
        $("#Model_Add_AdvanceReq").modal('hide');

        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
        addProductNotice(data.ToastMsgFail, '', '', 'failure');
        stopMultipleSubmitAdvReq();
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}
function OnUpdateAdminVoureq(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
     
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
        addProductNotice(data.ToastMsgFail, '', '', 'failure');
        stopMultipleSubmitVReq();
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}
function OnUpdateAdminChangePswd(data) {
  
    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
        document.getElementById("msgdig").style.display = "block";
        document.getElementById("failuremsg").innerText = data.ErrorMsg;
    }
    if (data.ErrorTitle == "S") {

        var chkmodal = $('#changepasswordnmodal').hasClass('show');
        if (chkmodal == true) {
            $("#changepasswordnmodal").modal('hide');

        }
        else {

        }
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
        var chkmodal = $('#changepasswordnmodal').hasClass('show');
        if (chkmodal == true) {
            document.getElementById("msgdig").style.display = "block";
            document.getElementById("failuremsg").innerText = data.ToastMsgFail;

        }
        else {
            addProductNotice(data.ToastMsgFail, '', '', 'failure');
           
        }
      
    }
    if (data.Refresh == "Default") {
  
            location.reload();
       
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}
function OnUpdateprofile(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {

        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
      
        addProductNotice(data.ToastMsgFail, '', '', 'failure');
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}
function OnUpdateCompany_Profile(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
        $("#Model_Add_Cmp_Doc").modal('hide');
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
    
        addProductNotice(data.ToastMsgFail, '', '', 'failure');
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}
function OnUpdateAdminLeave(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
     
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {

        addProductNotice(data.ToastMsgFail, '', '', 'failure');
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}
//function OnUpdateAdminLeaveList(data) {

//    if (data.ErrorMsg != null) {
//        alert(data.ErrorMsg);
//    }
//    if (data.ErrorTitle == "S") {
//        $("#Model_Add_Leave_M").modal('hide');
//        addProductNotice(data.ToastMsgSuc, '', '', 'success');
//    }
//    if (data.ErrorTitle == "F") {
//        addProductNotice(data.ToastMsgFail, '', '', 'failure');
//        stopMultipleSubmitLeave();
//    }
//    if (data.Refresh == "Default") {
//        setTimeout(function () {
//            location.reload();
//        }, 2000);
//    }
//    else if (data.Refresh != null) {
//        window.location.href = data.Refresh;
//    }
//}

function OnUpdateAdminLeaveList(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
        document.getElementById("msgdig").style.display = "block";
        document.getElementById("failuremsg").innerText = data.ErrorMsg;
    }
    if (data.ErrorTitle == "S") {

        var chkmodal = $('#Model_Add_Leave_M').hasClass('show');
        if (chkmodal == true) {
            $("#Model_Add_Leave_M").modal('hide');

        }
        else {

        }
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
        var chkmodal = $('#Model_Add_Leave_M').hasClass('show');

        if (chkmodal == true) {
            document.getElementById("msgdig").style.display = "block";
            document.getElementById("failuremsg").innerText = data.ToastMsgFail;

        }
        else {
            addProductNotice(data.ToastMsgFail, '', '', 'failure');
        }

        stopMultipleSubmitLeave();
    }

    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}


//function OnUpdateUserChange_pswd(data) {

//    if (data.ErrorMsg != null) {
//        alert(data.ErrorMsg);
//    }
//    if (data.ErrorTitle == "S") {
//        $("#changepasswordnmodal").modal('hide');
//        addProductNotice(data.ToastMsgSuc, '', '', 'success');
//    }
//    if (data.ErrorTitle == "F") {
//      /*  alert(data.ToastMsgFail);*/
//        addProductNotice(data.ToastMsgFail, '', '', 'failure');
//    }
//    if (data.Refresh == "Default") {
//        setTimeout(function () {
//            location.reload();
//        }, 2000);
//    }
//    else if (data.Refresh != null) {
//        window.location.href = data.Refresh;
//    }
//}
function OnUpdateUserChange_pswd(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
        document.getElementById("msgdig").style.display = "block";
        document.getElementById("failuremsg").innerText = data.ErrorMsg;
    }
    if (data.ErrorTitle == "S") {

        var chkmodal = $('#changepasswordnmodalUser').hasClass('show');
        if (chkmodal == true) {
            $("#changepasswordnmodalUser").modal('hide');

        }
        else {

        }
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
        var chkmodal = $('#changepasswordnmodalUser').hasClass('show');
        if (chkmodal == true) {
            document.getElementById("msgdig").style.display = "block";
            document.getElementById("failuremsg").innerText = data.ToastMsgFail;

        }
        else {
            addProductNotice(data.ToastMsgFail, '', '', 'failure');

        }

    }
    if (data.Refresh == "Default") {

        location.reload();

    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}

function OnUpdateUser_Profile(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {

        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {

        addProductNotice(data.ToastMsgFail, '', '', 'failure');
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}
//function OnUpdateUserRes(data) {

//    if (data.ErrorMsg != null) {
//        alert(data.ErrorMsg);
//    }
//    if (data.ErrorTitle == "S") {
//        $("#Model_Emp_Resignation_M").modal('hide');
//        addProductNotice(data.ToastMsgSuc, '', '', 'success');
//    }
//    if (data.ErrorTitle == "F") {
//        //$("#Model_Emp_Resignation_M").modal('hide');
//        addProductNotice(data.ToastMsgFail, '', '', 'failure');
//        stopMultipleSubmitReg();
//    }
//    if (data.Refresh == "Default") {
//        setTimeout(function () {
//            location.reload();
//        }, 2000);
//    }
//    else if (data.Refresh != null) {
//        window.location.href = data.Refresh;
//    }
//}

function OnUpdateUserRes(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
        document.getElementById("msgdig").style.display = "block";
        document.getElementById("failuremsg").innerText = data.ErrorMsg;
    }
    if (data.ErrorTitle == "S") {

        var chkmodal = $('#Model_Emp_Resignation_M').hasClass('show');
        if (chkmodal == true) {
            $("#Model_Emp_Resignation_M").modal('hide');

        }
        else {

        }
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
        var chkmodal = $('#Model_Emp_Resignation_M').hasClass('show');

        if (chkmodal == true) {
            document.getElementById("msgdig").style.display = "block";
            document.getElementById("failuremsg").innerText = data.ToastMsgFail;

        }
        else {
            addProductNotice(data.ToastMsgFail, '', '', 'failure');
        }

        stopMultipleSubmitReg();
    }

    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}


function OnUpdateUserEAdvReq(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
        $("#Model_Emp_AdvanceReq").modal('hide');
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
        $("#Model_Emp_AdvanceReq").modal('hide');
        addProductNotice(data.ToastMsgFail, '', '', 'failure');
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}
function OnUpdateUserEVoureq(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
        $("#Model_Emp_AdvanceReq").modal('hide');
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
          alert(data.ToastMsgFail);
        addProductNotice(data.ToastMsgFail, '', '', 'failure');
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}
function OnUpdateEUserProTask(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {

        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
        addProductNotice(data.ToastMsgFail, '', '', 'failure');
        stopMultipleSubmitVReq();
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}
function OnUpdateUserEcmnt(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
        $("#EPEdit_Comment_M").modal('hide');
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
        addProductNotice(data.ToastMsgFail, '', '', 'failure');
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}
//function OnUpdateUserELeave(data) {

//    if (data.ErrorMsg != null) {
//        alert(data.ErrorMsg);
//    }
//    if (data.ErrorTitle == "S") {
//        $("#Model_Add_Leave_M").modal('hide');
//        addProductNotice(data.ToastMsgSuc, '', '', 'success');
//    }
//    if (data.ErrorTitle == "F") {
      
//        addProductNotice(data.ToastMsgFail, '', '', 'failure');
//        stopMultipleSubmitLeave();
//    }
//    if (data.Refresh == "Default") {
//        setTimeout(function () {
//            location.reload();
//        }, 2000);
//    }
//    else if (data.Refresh != null) {
//        window.location.href = data.Refresh;
//    }
//}
function OnUpdateUserELeave(data) {
   
    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
        document.getElementById("msgdig").style.display = "block";
        document.getElementById("failuremsg").innerText = data.ErrorMsg;
    }
    if (data.ErrorTitle == "S") {


        var chkmodal = $('#Model_Add_Leave_M').hasClass('show');
        if (chkmodal == true) {
            $("#Model_Add_Leave_M").modal('hide');

        }
        else {

        }
          addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
        var chkmodal = $('#Model_Add_Leave_M').hasClass('show');
     
        if (chkmodal == true) {
            document.getElementById("msgdig").style.display = "block";
            document.getElementById("failuremsg").innerText = data.ToastMsgFail;
          
        }
        else {
            addProductNotice(data.ToastMsgFail, '', '', 'failure');
        }
      
        stopMultipleSubmitLeave();
    }

    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}
function OnUpdateUserELeaveApprovelist(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
        $("#Model_Reporting_Leave_M").modal('hide');
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
        $("#Model_Reporting_Leave_M").modal('hide');
        addProductNotice(data.ToastMsgFail, '', '', 'failure');
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}

function OnUpdateUserEDoc(data) {
   
    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
       
        sessionStorage.setItem("documentTab","active");
        sessionStorage.removeItem("addresstab");
       // alert(tab);
       
      // var takedata= settab(tab);

        $("#Model_EmpDocument").modal('hide');

        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
       
    if (data.ErrorTitle == "F") {
        $("#Model_EmpDocument").modal('hide');
        addProductNotice(data.ToastMsgFail, '', '', 'failure');
    }
      
    
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
       
    }
    else if (data.Refresh != null) {

        window.location.href = data.Refresh;
       
    }
      //  $("#tab11").addClass("active");
        //$("#tab5").css("z-index", "-1");
        //$("#tab66").css("z-index", "-1");
        //$("#tab7").css("z-index", "-1");
        //$("#tab12").css("z-index", "-1");
        //$("#tab11").css("z-index", "0");
        /*   document.getElementById("tab11").style.display = "block";*/
       
        //document.getElementById("tab11").classList.add("active");
        //document.getElementById("tab5").style.display = "none";
        //document.getElementById("tab66").style.display = "none";
        //document.getElementById("tab7").style.display = "none";
        //document.getElementById("tab12").style.display = "none";
        //$("#tab5").hide();
        //$("#tab66").hide();
        //$("#tab7").hide();
        //$("#tab12").hide();
        //$("#tab11").show();
    
}

function OnUpdateUserEAdd(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
        $("#Model_EmpAddress").modal('hide');
        sessionStorage.setItem("addresstab", "active");
        sessionStorage.removeItem('documentTab');
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
        addProductNotice(data.ToastMsgFail, '', '', 'failure');
        tabaddress = "active";
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
        tabaddress = "active";
    }
}

window.addEventListener('load', (event) => {
   
    // alert('The page has fully loaded');
  
   
    var tabdetails = sessionStorage.getItem("documentTab");
    var addresstabdetails = sessionStorage.getItem("addresstab");
    var adminTabDoc = sessionStorage.getItem("docTabtrack");
    // alert(sessionStorage.getItem("docTabtrack"));
    var adminTabAdd = sessionStorage.getItem("addTabtrack");
    var adminTabemploye = sessionStorage.getItem("Employedetailtab");
    var tabuser = null;
    
    
    if ((tabdetails == null) && (addresstabdetails == null)) {
       // alert("user");
        document.getElementById("userdetails").classList.add("active");
        document.getElementById("tab5s").classList.add("active");
       
    }
    
    if (tabdetails == "active") {
        document.getElementById("tab5s").classList.remove("active");
        document.getElementById("tab12").classList.remove("active");
        document.getElementById("documentclass").classList.add("active");
       
      
        var classdata = document.getElementById("tab11");
        classdata.classList.add("active");
    }
  
    if (addresstabdetails == "active") {
        document.getElementById("tab5s").classList.remove("active");
        document.getElementById("tab11").classList.remove("active");
        document.getElementById("otherdetailsclass").classList.add("active");
       
      
        var addtab = document.getElementById("tab12");
        addtab.classList.add("active");
    }

    if((adminTabemploye ==null) && (adminTabDoc ==null) && (adminTabAdd ==null)) {
       
        document.getElementById("tab5").classList.add("active");
        document.getElementById("employeedetails").classList.add("active");
        
    }
    if(adminTabAdd == "active") {
        var adminaddress = document.getElementById("tab12admin");
        adminaddress.classList.add("active");
        document.getElementById("admintabother").classList.add("active");
        document.getElementById("tab5").classList.remove("active");
        document.getElementById("tab11admin").classList.remove("active");

    }
    if(adminTabDoc == "active") {
        document.getElementById("tab11admin").classList.add("active");
        document.getElementById("admintabdocument").classList.add("active");
        document.getElementById("tab5").classList.remove("active");
        document.getElementById("tab12admin").classList.remove("active");
      
    }

    if(adminTabemploye == "active") {
        document.getElementById("employeedetails").classList.add("active");
        document.getElementById("tab5").classList.add("active");
        document.getElementById("tab11admin").classList.remove("active");
        document.getElementById("tab12admin").classList.remove("active");
       
      
    }
   
   

});
function OnUpdateUserERel(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
        $("#Model_EmpRelation").modal('hide');
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
        addProductNotice(data.ToastMsgFail, '', '', 'failure');
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}
function OnAdminUserEUpdateEdu(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
        $("#Model_EmpEducation").modal('hide');
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
        addProductNotice(data.ToastMsgFail, '', '', 'failure');
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}
function OnAdminUserEUpdateExp(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
        $("#Model_EmpExperience").modal('hide');
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
        addProductNotice(data.ToastMsgFail, '', '', 'failure');
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}
function OnUpdateUserE(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
     
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
        addProductNotice(data.ToastMsgFail, '', '', 'failure');
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}




function OnAfterRegistersuccessR(data) {

    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {
    
        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {
       
        addProductNotice(data.ToastMsgFail, '', '', 'failure');
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }
}



function OnUpdateUser(data) {
    if (data.ErrorMsg != null) {
        alert(data.ErrorMsg);
    }
    if (data.ErrorTitle == "S") {

        addProductNotice(data.ToastMsgSuc, '', '', 'success');
    }
    if (data.ErrorTitle == "F") {

        addProductNotice(data.ToastMsgFail, '', '', 'failure');
    }
    if (data.Refresh == "Default") {
        setTimeout(function () {
            location.reload();
        }, 2000);
    }
    else if (data.Refresh != null) {
        window.location.href = data.Refresh;
    }

}



function addProductNotice(title, thumb, text, type) {
   /* alert("call..1");*/
    if (type == 'success') {
      /*  alert("call..2" );*/
        $.notify({
            icon: 'fa fa-check',
            /* title: 'Success!',*/
            title: '',
            message: title
        }, {
            element: 'body',
            position: null,
            type: "success",
            allow_dismiss: true,
            newest_on_top: false,
            showProgressbar: false,
            placement: {
                from: "top",
                align: "right"
            },
            offset: 5,
            spacing: 20,
            z_index: 1031,
            delay: 5000,
            animate: {
                enter: 'animated fadeInDown',
                exit: 'animated fadeOutUp'
            },
            icon_type: 'class',
            template: '<div data-notify="container" class="col-xs-11 col-sm-3 alert alert-{0}" role="alert">' +
                '<button type="button" aria-hidden="true" class="close" data-notify="dismiss" style="color:#FFFAFA">×</button>' +
                '<span data-notify="icon"></span> ' +
                '<span data-notify="title">{1}</span> ' +
                '<span data-notify="message">{2}</span>' +
                '<div class="progress" data-notify="progressbar">' +
                '<div class="progress-bar progress-bar-{0}" role="progressbar" aria-valuenow="0" aria-valuemin="0" aria-valuemax="100" style="width: 0%;"></div>' +
                '</div>' +
                '<a href="{3}" target="{4}" data-notify="url"></a>' +
                '</div>'
        });

    } else {

        $.notify({
            icon: 'fa fa-exclamation-circle',
            title: '',
            message: title
        }, {
            element: 'body',
            position: null,
            type: "info",
            allow_dismiss: true,
            newest_on_top: false,
            showProgressbar: false,
            placement: {
                from: "top",
                align: "right"

            },
            offset: 5,
            spacing: 20,
            z_index: 1031,
            delay: 5000,
            animate: {
                enter: 'animated fadeInDown',
                exit: 'animated fadeOutUp'
            },
            icon_type: 'class',
            template: '<div data-notify="container" class="col-xs-11 col-sm-3 alert alert-danger" role="alert" >' +
                '<button type="button" aria-hidden="true" class="close" data-notify="dismiss" style="color:#FFFAFA">×</button>' +
                '<span data-notify="icon"></span> ' +
                '<span data-notify="title">{1}</span> ' +
                '<span data-notify="message">{2}</span>' +
                '<div class="progress" data-notify="progressbar">' +
                '<div class="progress-bar progress-bar-{0}" role="progressbar" aria-valuenow="0" aria-valuemin="0" aria-valuemax="100" style="width: 0%;background-color:#721c24;"></div>' +
                '</div>' +
                '<a href="{3}" target="{4}" data-notify="url"></a>' +
                '</div>'
        });
    }


}

