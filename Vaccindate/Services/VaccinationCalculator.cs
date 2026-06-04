using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Vaccindate.Models;

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
            string filePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "vaccines.json");

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    "Nie znaleziono pliku vaccines.json. Dodaj go do projektu i ustaw Copy to Output Directory: Copy if newer.",
                    filePath);
            }

            string json = File.ReadAllText(filePath);

            var root = JsonSerializer.Deserialize<VaccinationScheduleRoot>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (root == null)
                return new List<VaccinationItem>();

            var combined = new List<VaccinationItem>();

            if (root.Items != null)
                combined.AddRange(root.Items);

            if (root.AdultScheduleItems != null)
                combined.AddRange(root.AdultScheduleItems);

            return combined;
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
                Category = item.Category ?? string.Join(", ", item.TargetProfiles ?? new List<string>()),
                PlannedDate = plannedDate.Value,
                DaysDifference = (plannedDate.Value.Date - DateTime.Today).Days,
                Description = item.Description ?? "",
                ScheduleType = item.ScheduleType ?? "",
                AgeLabel = BuildAgeLabel(item),
                RecommendedTiming = item.RecommendedTiming ?? "",
                IsSeasonal = item.IsSeasonal ?? false,
                IsRequiredInPL = item.IsRequiredInPL ?? false
            };
        }

        private DateTime? CalculatePlannedDate(DateTime birthDate, VaccinationItem item)
        {
            DateTime today = DateTime.Today;

            if (item.ScheduleType == "age_based")
            {
                if (item.AgeInDays.HasValue)
                    return birthDate.Date.AddDays(item.AgeInDays.Value);

                if (item.MinimumAgeInYears.HasValue)
                    return birthDate.Date.AddYears(item.MinimumAgeInYears.Value);

                return null;
            }

            if (item.ScheduleType == "age_range")
            {
                if (!item.MinimumAgeInYears.HasValue)
                    return null;

                DateTime startDate = birthDate.Date.AddYears(item.MinimumAgeInYears.Value);

                if (item.MaximumAgeInYears.HasValue)
                {
                    DateTime endDate = birthDate.Date.AddYears(item.MaximumAgeInYears.Value);

                    if (today > endDate)
                        return null;
                }

                return today <= startDate ? startDate : today;
            }

            if (item.ScheduleType == "recurring")
            {
                if (!item.MinimumAgeInYears.HasValue || !item.RecurrenceYears.HasValue)
                    return null;

                DateTime nextDate = birthDate.Date.AddYears(item.MinimumAgeInYears.Value);

                while (nextDate < today)
                {
                    nextDate = nextDate.AddYears(item.RecurrenceYears.Value);
                }

                return nextDate;
            }

            if (item.ScheduleType == "conditional")
            {
                if (item.MinimumAgeInYears.HasValue)
                    return birthDate.Date.AddYears(item.MinimumAgeInYears.Value);

                return today;
            }

            return null;
        }

        private string BuildAgeLabel(VaccinationItem item)
        {
            if (!string.IsNullOrWhiteSpace(item.AgeLabel))
                return item.AgeLabel;

            if (item.ScheduleType == "age_based")
            {
                if (item.AgeInDays.HasValue)
                    return $"{item.AgeInDays.Value} dni od urodzenia";

                if (item.MinimumAgeInYears.HasValue)
                    return $"od {item.MinimumAgeInYears.Value}. r.ż.";
            }

            if (item.ScheduleType == "age_range")
                return $"{item.MinimumAgeInYears}–{item.MaximumAgeInYears} lat";

            if (item.ScheduleType == "recurring")
                return $"od {item.MinimumAgeInYears}. r.ż., co {item.RecurrenceYears} lat";

            if (item.ScheduleType == "conditional")
                return "jeśli są wskazania";

            return "";
        }
    }
}