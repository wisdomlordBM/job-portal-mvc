using System.Globalization;
using Jobportalwebsite.Models;

namespace Jobportalwebsite.Helper
{
    public static class SalaryFormatter
    {
        public static string Format(decimal? salary, string? currencySymbol, SalaryPeriod? salaryPeriod)
        {
            if (salaryPeriod == SalaryPeriod.Negotiable)
            {
                return "Negotiable";
            }

            if (!salary.HasValue)
            {
                return "Not specified";
            }

            var suffix = salaryPeriod switch
            {
                SalaryPeriod.PerHour => "/hour",
                SalaryPeriod.PerDay => "/day",
                SalaryPeriod.PerWeek => "/week",
                SalaryPeriod.PerMonth => "/month",
                SalaryPeriod.PerYear => "/year",
                SalaryPeriod.Contract => "/contract",
                _ => string.Empty
            };

            return string.IsNullOrWhiteSpace(currencySymbol)
                ? $"{salary.Value.ToString("N0", CultureInfo.InvariantCulture)}{suffix}"
                : $"{currencySymbol}{salary.Value.ToString("N0", CultureInfo.InvariantCulture)}{suffix}";
        }
    }
}
