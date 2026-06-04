using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Vaccindate.Services
{
    public class VaccinationCalculator
    {
        private readonly List<VaccinationItem> _schedule;

        public VaccinationCalculator()
        {
            _schedule = LoadScheduleFromJson();
        }

        public VaccinationSummary GetVaccinationSummary(DateTime birthDate)
        {
            int ageInDays = (DateTime.Today - birthDate.Date).Days;

            var allResults = _schedule
                .Select(item => CreateResult(birthDate, item))
                .Where(result => result != null)
                .Cast<VaccinationResult>()
                .OrderBy(result => result.PlannedDate)
                .ToList();

            var alreadyDue = allResults
                .Where(result => result.PlannedDate.Date < DateTime.Today)
                .ToList();

            var future = allResults
                .Where(result => result.PlannedDate.Date >= DateTime.Today)
                .ToList();

            return new VaccinationSummary
            {
                AlreadyDueVaccinations = alreadyDue,
                FutureVaccinations = future
            };
        }

        private List<VaccinationItem> LoadScheduleFromJson()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "vaccines.json");

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    "Nie znaleziono pliku vaccination_schedule.json. Upewnij się, że plik jest dodany do projektu i ma ustawione Copy to Output Directory.",
                    filePath);
            }

            string json = File.ReadAllText(filePath);

            var root = JsonSerializer.Deserialize<VaccinationScheduleRoot>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (root == null || root.Items == null)
            {
                return new List<VaccinationItem>();
            }

            return root.Items;
        }

        private VaccinationResult? CreateResult(DateTime birthDate, VaccinationItem item)
        {
            DateTime? plannedDate = CalculatePlannedDate(birthDate, item);

            if (plannedDate == null)
                return null;

            return new VaccinationResult
            {
                VaccineName = item.VaccineName ?? "Nieznana szczepionka",
                Disease = item.Disease ?? "",
                Category = item.Category ?? "",
                PlannedDate = plannedDate.Value,
                DaysDifference = (plannedDate.Value.Date - DateTime.Today).Days,
                Description = item.Description ?? "",
                ScheduleType = item.ScheduleType ?? "",
                AgeLabel = item.AgeLabel ?? ""
            };
        }

        private DateTime? CalculatePlannedDate(DateTime birthDate, VaccinationItem item)
        {
            DateTime today = DateTime.Today;

            if (item.ScheduleType == "age_based" && item.AgeInDays.HasValue)
            {
                return birthDate.Date.AddDays(item.AgeInDays.Value);
            }

            if (item.ScheduleType == "age_based" && item.MinimumAgeInYears.HasValue)
            {
                return birthDate.Date.AddYears(item.MinimumAgeInYears.Value);
            }

            if (item.ScheduleType == "age_range" && item.MinimumAgeInYears.HasValue)
            {
                DateTime startDate = birthDate.Date.AddYears(item.MinimumAgeInYears.Value);
                DateTime? endDate = item.MaximumAgeInYears.HasValue
                    ? birthDate.Date.AddYears(item.MaximumAgeInYears.Value)
                    : null;

                if (endDate.HasValue && today > endDate.Value)
                    return null;

                if (today <= startDate)
                    return startDate;

                return today;
            }

            if (item.ScheduleType == "recurring"
                && item.MinimumAgeInYears.HasValue
                && item.RecurrenceYears.HasValue
                && item.RecurrenceYears.Value > 0)
            {
                DateTime firstDate = birthDate.Date.AddYears(item.MinimumAgeInYears.Value);

                if (today <= firstDate)
                    return firstDate;

                DateTime nextDate = firstDate;

                while (nextDate < today)
                {
                    nextDate = nextDate.AddYears(item.RecurrenceYears.Value);
                }

                return nextDate;
            }

            return null;
        }
    }

    public class VaccinationScheduleRoot
    {
        public string? SchemaVersion { get; set; }
        public string? SourceProfile { get; set; }
        public string? MedicalDisclaimer { get; set; }
        public List<VaccinationItem>? Items { get; set; }
    }

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

        public string? Description { get; set; }
    }

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
    }

    public class VaccinationSummary
    {
        public List<VaccinationResult> AlreadyDueVaccinations { get; set; } = new();
        public List<VaccinationResult> FutureVaccinations { get; set; } = new();
    }
}