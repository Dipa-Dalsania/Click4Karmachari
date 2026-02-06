using ClickKarmachari;
using ClickKarmachari.Models;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class ExcelValidator
{
    private Prod_Satyamgroup_V1Entities db = new Prod_Satyamgroup_V1Entities();
    public (MemoryStream memoryStream, bool errorOccurred) ValidateAndMarkExcel(Stream uploadedFile,long dept, long desgn)
    {
        //trySSS
        //{
        bool erroraccured = false;
        var memoryStream = new MemoryStream(); // Create the MemoryStream outside the using block
        using (var workbook = new XLWorkbook(uploadedFile))
        {
            var resultSheet = workbook.Worksheets.FirstOrDefault(ws => ws.Name == "ResultSheet");
            if (resultSheet == null)
            {
                workbook.Worksheets.Add("ResultSheet");
            }

            var ResultSheet = workbook.Worksheet("ResultSheet");
            int ResultSheetColumns = 1;
            int ResultSheetRows = 1;

            var worksheet = workbook.Worksheet("EmployeeAdd"); // Assuming the worksheet name is "Products"
            var lastRow = worksheet.LastRowUsed().RowNumber();
            if (lastRow < 2)
            {
                ResultSheet.Cell(ResultSheetRows, ResultSheetColumns).Value = $"Add Atleast One Employee to the Excel to Upload Employee.";
                ResultSheetRows++;
                erroraccured = true;
                worksheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.Red;
            }
            else
            {
                Department_Master deptmst = db.Department_Master.Where(x => x.Dept_ID == dept).FirstOrDefault();
                Designation_Master desigmst = db.Designation_Master.Where(x => x.Desg_Id == desgn).FirstOrDefault();
                int ColumnCounter = 1;
                foreach (string columnname in new CommonStringGiver().GetAllColumns())
                {
                    if (worksheet.Cell(1, ColumnCounter).GetValue<string>() != columnname)
                    {
                        ResultSheet.Cell(ResultSheetRows, ResultSheetColumns).Value = $"Invalid header in column 1,{ColumnCounter}. Expected '{columnname}'.";
                        ResultSheetRows++;
                        erroraccured = true;
                        worksheet.Cell(1, ColumnCounter).Style.Fill.BackgroundColor = XLColor.Red;
                    }
                    ColumnCounter++;
                }
              


                for (int row = 2; row <= lastRow; row++)
                {
                    ColumnCounter = 1;
                    foreach (string columnname in new CommonStringGiver().GetAllColumns())
                    {
                        if (ColumnCounter == 1)
                        {
                            var productIDFieldValue = worksheet.Cell(row, ColumnCounter).GetValue<string>().Trim();
                            if (!long.TryParse(productIDFieldValue, out long productId))
                            {
                                var ProductIDField = worksheet.Cell(row, ColumnCounter).GetValue<string>();
                                ResultSheet.Cell(ResultSheetRows, ResultSheetColumns).Value = $"Invalid header in column Sr.No - {row},{ColumnCounter}. Expected long Value, For New Employee Enter 0 in {ColumnCounter} column.";
                                ResultSheetRows++;
                                erroraccured = true;
                                worksheet.Cell(row, ColumnCounter).Style.Fill.BackgroundColor = XLColor.Red;
                            }
                        }
                        else if (ColumnCounter == 2)
                        {
                            var categoryfield = worksheet.Cell(row, ColumnCounter).GetValue<string>();
                            if (categoryfield != deptmst.Department)
                            {
                                ResultSheet.Cell(ResultSheetRows, ResultSheetColumns).Value = $"Invalid header in column department - {row},{ColumnCounter}. Expected '{deptmst.Department}'.";
                                ResultSheetRows++;
                                erroraccured = true;
                                worksheet.Cell(row, ColumnCounter).Style.Fill.BackgroundColor = XLColor.Red;
                            }
                        }
                     
                        else
                        {
                             
                        }
                        ColumnCounter++;
                    }

                  
                }
            }

            if (!erroraccured)
            {
                ResultSheet.Cell(1, 2).Value = $"Success";
                ResultSheetRows++;
            }
            else
            {
                ResultSheet.Cell(1, 2).Value = $"Failed";
                ResultSheetRows++;
            }
            workbook.SaveAs(memoryStream);
        }
        memoryStream.Position = 0; // Reset the stream position to the beginning
        return (memoryStream, erroraccured);
        //}
        //catch
        //{
        //    return (null, true);
        //}
    }

    

}
