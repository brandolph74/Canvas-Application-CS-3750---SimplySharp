using System;
using System.ComponentModel.DataAnnotations;

namespace SimplySharp.Areas.Identity.Pages.Account
{
    public class MaximumAgeAttribute : ValidationAttribute
    {
        public int MaximumAge { get; }
        public MaximumAgeAttribute(int minAge)
        {
            MaximumAge = minAge;
            ErrorMessage = "{0} error: must be someone at most {1} years of age (current oldest record is 122)";
        }

        public override bool IsValid(object value)
        {
            DateTime date;
            if ((value != null && DateTime.TryParse(value.ToString(), out date)))
            {
                return date.AddYears(MaximumAge) > DateTime.Now;
            }

            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format(ErrorMessageString, name, MaximumAge);
        }
    }
}
