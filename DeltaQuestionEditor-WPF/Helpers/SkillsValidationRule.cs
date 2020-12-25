using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DeltaQuestionEditor_WPF.Helpers
{
    class SkillsValidationRule : ValidationRule
    {
        public static ValidationResult ValidateSkills(object value, CultureInfo cultureInfo)
        {
            string val = value as string;
            if (val.IsNullOrWhiteSpace())
                return ValidationResult.ValidResult;
            val = val.Trim().ToLower();
            if (Regex.IsMatch(val.Replace(" ", ""), @"^(?:\d+\.){2,4}\d+(?:,(?:\d+\.){2,4}\d+)*$"))
                return ValidationResult.ValidResult;
            return new ValidationResult(false, "Invalid skill codes");
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return ValidateSkills(value, cultureInfo);
        }
    }
}
