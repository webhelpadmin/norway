using System;

namespace WebApp.Models
{
    /// <summary>
    /// Model for parsed, validated & sorted data from CSV
    /// </summary>
    public class PercentModel : IComparable<PercentModel>
    {
        public int? Id { get; set; }
        public DateTime BeginDate { get; set; }
        public double IRR_ACB { get; set; }
        public double? Percent { get; set; }

        public int CompareTo(PercentModel other)
        {
            if (other == null) return 1;
            if (BeginDate > other.BeginDate) return 1;
            if (BeginDate < other.BeginDate) return -1;
            return 0;
        }
    }
}