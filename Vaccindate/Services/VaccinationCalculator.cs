using System;
using System.Collections.Generic;
using System.Linq;

namespace Vaccindate.Services
{
    public class VaccinationCalculator
    {
        private readonly List<VaccinationItem> _schedule = new()
        {
            new VaccinationItem
            {
                VaccineName = "WZW typu B + gruźlica",
                AgeInDays = 0,
                Description = "Szczepienie po urodzeniu."
            },
            new VaccinationItem
            {
                VaccineName = "Rotawirusy + DTP + Hib + pneumokoki + polio + WZW B",
                AgeInDays = 42,
                Description = "Około 6. tygodnia życia."
            },
            new VaccinationItem
            {
                VaccineName = "Rotawirusy + DTP + Hib + pneumokoki + polio",
                AgeInDays = 91,
                Description = "Około 13.–14. tygodnia życia."
            },
            new VaccinationItem
            {
                VaccineName = "Rotawirusy + DTP + Hib + polio",
                AgeInDays = 154,
                Description = "Około 22.–24. tygodnia życia."
            },
            new VaccinationItem
            {
                VaccineName = "MMR + pneumokoki",
                AgeInDays = 365,
                Description = "Około 12.–13. miesiąca życia."
            },
            new VaccinationItem
            {
                VaccineName = "DTP + Hib + polio",
                AgeInDays = 540,
                Description = "Około 16.–18. miesiąca życia."
            },
            new VaccinationItem
            {
                VaccineName = "DTP + polio",
                AgeInDays = 2190,
                Description = "Około 6. roku życia."
            },
            new VaccinationItem
            {
                VaccineName = "HPV",
                AgeInDays = 4380,
                Description = "Około 12. roku życia."
            },
            new VaccinationItem
            {
                VaccineName = "Td/Tdap — tężec, błonica, krztusiec",
                AgeInDays = 6935,
                Description = "Około 19. roku życia."
            },
            new VaccinationItem
            {
                VaccineName = "Grypa",
                AgeInDays = 18 * 365,
                Description = "Zalecane corocznie u dorosłych. W tej uproszczonej wersji pokazane od 18. roku życia."
            },
            new VaccinationItem
            {
                VaccineName = "COVID-19",
                AgeInDays = 18 * 365,
                Description = "Według aktualnych zaleceń sezonowych i indywidualnego ryzyka."
            },
            new VaccinationItem
            {
                VaccineName = "Półpasiec",
                AgeInDays = 50 * 365,
                Description = "Szczepienie zalecane u dorosłych około 50+."
            },
            new VaccinationItem
            {
                VaccineName = "Pneumokoki",
                AgeInDays = 65 * 365,
                Description = "Szczepienie zalecane u osób starszych."
            },
            new VaccinationItem
            {
                VaccineName = "RSV",
                AgeInDays = 75 * 365,
                Description = "Szczepienie zalecane u starszych dorosłych."
            }
           
        };

        public VaccinationSummary GetVaccinationSummary(DateTime birthDate)
        {
            int ageInDays = (DateTime.Today - birthDate.Date).Days;

            var alreadyDue = _schedule
                .Where(v => v.AgeInDays < ageInDays)
                .OrderBy(v => v.AgeInDays)
                .Select(v => CreateResult(birthDate, v))
                .ToList();

            var future = _schedule
                .Where(v => v.AgeInDays >= ageInDays)
                .OrderBy(v => v.AgeInDays)
                .Select(v => CreateResult(birthDate, v))
                .ToList();

            return new VaccinationSummary
            {
                AlreadyDueVaccinations = alreadyDue,
                FutureVaccinations = future
            };
        }

        private VaccinationResult CreateResult(DateTime birthDate, VaccinationItem item)
        {
            DateTime plannedDate = birthDate.Date.AddDays(item.AgeInDays);

            return new VaccinationResult
            {
                VaccineName = item.VaccineName,
                PlannedDate = plannedDate,
                DaysDifference = (plannedDate.Date - DateTime.Today).Days,
                Description = item.Description
            };
        }
    }

    public class VaccinationItem
    {
        public string VaccineName { get; set; } = "";
        public int AgeInDays { get; set; }
        int repeat_in_days { get; set; }
        public string Description { get; set; } = "";
    }

    public class VaccinationResult
    {
        public string VaccineName { get; set; } = "";
        public DateTime PlannedDate { get; set; }
        public int DaysDifference { get; set; }
        public string Description { get; set; } = "";
    }

    public class VaccinationSummary
    {
        public List<VaccinationResult> AlreadyDueVaccinations { get; set; } = new();
        public List<VaccinationResult> FutureVaccinations { get; set; } = new();
    }
}