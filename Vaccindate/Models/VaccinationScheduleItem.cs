namespace Vaccindate.Models
{
    public class VaccinationScheduleItem
    {
        public string VaccineName { get; set; }
        public int? AgeInDays { get; set; }
        public int? MinimumAgeInYears { get; set; }
        public int? MaximumAgeInYears { get; set; }
        public bool IsRecurring { get; set; }
        public int? RecurrenceYears { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public VaccinationScheduleItem() {
            VaccineName = string.Empty;
            Description = string.Empty;
            Category = string.Empty;
        }
    }
}