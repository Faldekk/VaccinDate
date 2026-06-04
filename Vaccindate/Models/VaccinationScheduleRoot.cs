using System.Collections.Generic;

namespace Vaccindate.Models
{
    public class VaccinationScheduleRoot
    {
        public string? SchemaVersion { get; set; }
        public string? SourceProfile { get; set; }
        public string? Title { get; set; }
        public string? SourceDocument { get; set; }
        public string? Language { get; set; }
        public string? MedicalDisclaimer { get; set; }

        public List<VaccinationItem> Items { get; set; } = new();
        public List<VaccinationItem> AdultScheduleItems { get; set; } = new();
    }
}