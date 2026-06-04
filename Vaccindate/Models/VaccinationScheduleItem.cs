using System.Collections.Generic;

namespace Vaccindate.Models
{
    public class VaccinationItem
    {
        public string? Id { get; set; }
        public string? Country { get; set; }
        public string? VaccineName { get; set; }
        public string? Disease { get; set; }
        public string? Category { get; set; }
        public string? ScheduleType { get; set; }

        public int? AgeInDays { get; set; }
        public string? AgeLabel { get; set; }

        public int? MinimumAgeInYears { get; set; }
        public int? MaximumAgeInYears { get; set; }
        public int? RecurrenceYears { get; set; }

        public bool? IsSeasonal { get; set; }
        public bool? IsRequiredInPL { get; set; }

        public List<string>? TargetProfiles { get; set; }
        public string? RecommendedTiming { get; set; }

        public string? Description { get; set; }
    }
}