using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Text.RegularExpressions;
using System.Globalization;
using MailBee.BounceMail;
using System.Web.Http.Results;

namespace ClickKarmachari.Models
{
    public class SalaryFormulaResolver
    {

        public static bool IsWageFormula(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            // If it's a valid decimal, it's not a formula
            if (decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                return false;

            // Otherwise, assume it's a formula (contains symbols, variables, etc.)
            return true;
        }

        public static decimal ResolveFormula(string formula, ProjectSalary _salary, int noofdayotph = 0)
        {
            // Define the mapping of field names to their values from the _salary object
            var fieldValues = new Dictionary<string, decimal>
            {
                { "BASIC", (decimal?)_salary.BASIC ?? 0 },
                { "HRA", (decimal?)_salary.HRA ?? 0 },
                { "DA", (decimal?)_salary.DA ?? 0 },
                { "TA", (decimal?)_salary.TA ?? 0 },
                { "BONUS", (decimal?)_salary.BONUS ?? 0 },
                { "LEAVE", (decimal?)_salary.LEAVE ?? 0 },
                { "OTRate", (decimal?)_salary.OTRate ?? 0 },
                { "PH", (decimal?)_salary.PH ?? 0 },
                { "SPECIALALLOWANCE", (decimal?)_salary.SPECIALALLOWANCE ?? 0 },
                { "OTHERALLOWANCE", (decimal?)_salary.OTHERALLOWANCE ?? 0 },
                { "PF", (decimal?)_salary.PF ?? 0 },
                { "PT", (decimal?)_salary.PT ?? 0 },
                { "ESIC", (decimal?)_salary.ESIC ?? 0 },
                { "CANTEEN", (decimal?)_salary.CANTEEN ?? 0 },
                { "HRA_DEDUCT", (decimal?)_salary.HRA_DEDUCT ?? 0 },
                { "TRAVEL", (decimal?)_salary.TRAVEL ?? 0 },
                { "ADVANCE", (decimal?)_salary.ADVANCE ?? 0 },
                { "Uniform", (decimal?)_salary.Uniform ?? 0 },
                { "Welfare", (decimal?)_salary.Welfare ?? 0 },
                { "Other", (decimal?)_salary.Other ?? 0 },
                { "NoOfDays", (decimal?)_salary.NoOfDays ?? 0 },
                { "NoOfPH", (decimal?)_salary.NoOfPH ?? 0 },
                { "NoOfOTShift", (decimal?)_salary.NoOfOTShift ?? 0 },
                { "NoOfOTHors", (decimal?)_salary.NoOfOTHors ?? 0 },
                { "ShiftHours", (decimal?)_salary.ShiftHours ?? 0 },
                { "Wages", (decimal?)_salary.MinWagesPerDay ?? 0 },
                { "GrossIncome", (decimal?)_salary.GROSS_INCOME ?? 0 },
                { "GrossDeduction", (decimal?)_salary.GROSS_DEDUCTION ?? 0 }
            };

            if (!IsWageFormula(formula))
            {
                double _temp = 0;
                if (noofdayotph == 0)
                {
                    _temp = double.Parse(formula) * _salary.NoOfDays;
                }
                else if (noofdayotph == 1)
                {
                    _temp = double.Parse(formula);
                }
                else if (noofdayotph == 2)
                {
                    _temp = double.Parse(formula);
                }
                return Convert.ToDecimal(_temp);
            }

            // Replace the field names in the formula with their corresponding values from _salary
            foreach (var field in fieldValues)
            {
                // Create a regex pattern that matches whole words
                var pattern = $@"\b{Regex.Escape(field.Key)}\b";

                // Replace only when the exact word is found, respecting boundaries like spaces, punctuation, or digits
                formula = Regex.Replace(formula, pattern, field.Value.ToString(), RegexOptions.IgnoreCase);
            }


            // Use DataTable.Compute to evaluate the formula
            try
            {
                var result = new DataTable().Compute(formula, string.Empty);
                return Convert.ToDecimal(result);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error while evaluating the formula: " + ex.Message);
            }
        }

    }
}
