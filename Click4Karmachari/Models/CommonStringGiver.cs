using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClickKarmachari.Models
{
    public class MethodInfo11
    {
        public Func<List<string>> Method { get; set; }
        public int[] Columns { get; set; }
        public bool AllowBlank { get; set; }
    }
    public class CommonStringGiver
    {
        public List<string> GetAllColumns()
        {
            List<string> Colors = new List<string>();
            Colors.Add("Sr.no");
            Colors.Add("Department");
            Colors.Add("Designation");

            Colors.Add("SkilledType");
            Colors.Add("ReportingPerson");
            Colors.Add("Salary Group");

            Colors.Add("EmployeeID");
            Colors.Add("EmployeeName");
            Colors.Add("Mobile");
            Colors.Add("City");

            Colors.Add("Adharcard_No");
            Colors.Add("PanCard_No");
            Colors.Add("Email");
            Colors.Add("Bank_Name");
            Colors.Add("BankAcNo.");
            Colors.Add("IFSC_No");

            Colors.Add("UAN_No");
            Colors.Add("ESIC_No");
            Colors.Add("Religion");
            Colors.Add("DateofBirth");

            Colors.Add("DateofJoin");
            Colors.Add("Blood_group");
            Colors.Add("BASIC");

            Colors.Add("HRA");
            Colors.Add("DA");
            Colors.Add("TA");

            Colors.Add("BONUS");
            Colors.Add("LEAVE");
            Colors.Add("OTRate");

            Colors.Add("PH");

            Colors.Add("SPECIALALLOWANCE");
            Colors.Add("OTHERALLOWANCE");
            Colors.Add("PF");
            Colors.Add("PT");
            Colors.Add("ESIC");

            Colors.Add("CANTEEN");
            Colors.Add("HRA_DEDUCT");
            Colors.Add("TRAVEL");
            Colors.Add("ADVANCE");
            Colors.Add("Uniform");
            Colors.Add("Welfare");
            Colors.Add("Other");

            return Colors;
        }

       
      
    }
}