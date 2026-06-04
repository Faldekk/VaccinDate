using System;

namespace Vaccindate.Models
{
    public class VaccinationResult
    {
        public string VaccineName { get; set; }
        public DateTime PlannedDate { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public VaccinationResult()
        {
            VaccineName = string.Empty;  PlannedDate = new DateTime(); Description = string.Empty; Category = string.Empty;
        }

        public int DaysLeft
        {
            get
            {
                return (PlannedDate.Date - DateTime.Today).Days;
            }
        }
    }
}