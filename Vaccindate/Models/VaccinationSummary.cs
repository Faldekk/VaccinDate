using System.Collections.Generic;

namespace Vaccindate.Models
{
    public class VaccinationSummary
    {
        public List<VaccinationResult> AlreadyDueVaccinations { get; set; } = new();
        public List<VaccinationResult> FutureVaccinations { get; set; } = new();
    }
}