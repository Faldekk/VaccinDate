using System;

namespace Vaccindate.Models
{
    public class VaccinationResult
    {
        public string VaccineName { get; set; } = "";
        public string Disease { get; set; } = "";
        public string Category { get; set; } = "";
        public DateTime PlannedDate { get; set; }
        public int DaysDifference { get; set; }
        public string Description { get; set; } = "";
        public string ScheduleType { get; set; } = "";
        public string AgeLabel { get; set; } = "";
        public string RecommendedTiming { get; set; } = "";

        public bool IsSeasonal { get; set; }
        public bool IsRequiredInPL { get; set; }
    }
}