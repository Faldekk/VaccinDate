using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Vaccindate.Models;
using Vaccindate.Services;


namespace Vaccindate
{
    public partial class MainWindow : Window
    {
        private readonly VaccinationCalculator _calculator = new();

        public MainWindow()
        {
            InitializeComponent();
        }
        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Vaccindate\n\n" +
                "Aplikacja WPF do orientacyjnego sprawdzania harmonogramu szczepień na podstawie daty urodzenia.\n\n" +
                "Funkcje:\n" +
                "- pokazuje szczepienia, które osoba powinna już mieć,\n" +
                "- pokazuje przyszłe szczepienia,\n" +
                "- pozwala filtrować wyniki,\n" +
                "- umożliwia zapisanie przypomnienia.\n\n" +
                "Uwaga: aplikacja ma charakter edukacyjny i nie zastępuje konsultacji medycznej.",
                "About Vaccindate",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            BirthDatePicker.SelectedDate = null;

            PastResultsTextBox.Text =
                "Tutaj pojawią się szczepienia do kontroli.";

            FutureResultsTextBox.Text =
                "Tutaj pojawią się przyszłe szczepienia.";
        }
        private void CheckVaccinationButton_Click(object sender, RoutedEventArgs e)
        {
            if (BirthDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Wybierz datę urodzenia.");
                return;
            }

            DateTime birthDate = BirthDatePicker.SelectedDate.Value.Date;

            if (birthDate > DateTime.Today)
            {
                MessageBox.Show("Data urodzenia nie może być z przyszłości.");
                return;
            }

            int age = DateTime.Today.Year - birthDate.Year;

            if (birthDate.Date > DateTime.Today.AddYears(-age))
                age--;

         
            
            try
            {
                var summary = _calculator.GetVaccinationSummary(birthDate);
                var pastVaccinations = ApplyFilter(summary.AlreadyDueVaccinations);
                var futureVaccinations = ApplyFilter(summary.FutureVaccinations);

                StringBuilder pastSb = new StringBuilder();

                pastSb.AppendLine($"Wiek użytkownika: {age} lat");
                pastSb.AppendLine();
                pastSb.AppendLine("SZCZEPIENIA DO KONTROLI / KTÓRE POWINNY BYĆ UWZGLĘDNIONE:");
                pastSb.AppendLine();

                if (pastVaccinations.Count == 0)
                {
                    pastSb.AppendLine("Brak szczepień do pokazania w tej sekcji.");
                }
                else
                {
                    int counter = 1;

                    foreach (var vaccination in pastVaccinations)
                    {
                        int daysAgo = Math.Abs(vaccination.DaysDifference);

                        pastSb.AppendLine($"{counter}. {vaccination.VaccineName}");
                        pastSb.AppendLine($"Choroba: {vaccination.Disease}");

                        pastSb.AppendLine($"Wiek / rytm: {vaccination.AgeLabel}");
                        //pastSb.AppendLine($"Zalecany timing: {vaccination.RecommendedTiming}");
                        pastSb.AppendLine($"Najbliższa / planowana data: {vaccination.PlannedDate:dd.MM.yyyy}");

                        if (vaccination.DaysDifference < 0)
                            pastSb.AppendLine($"Termin minął: {daysAgo} dni temu");
                        else if (vaccination.DaysDifference == 0)
                            pastSb.AppendLine("Termin: dzisiaj");
                        else
                            pastSb.AppendLine($"Pozostało dni: {vaccination.DaysDifference}");

                        pastSb.AppendLine($"Opis: {vaccination.Description}");
                        pastSb.AppendLine();

                        counter++;
                    }
                }

                pastSb.AppendLine("Uwaga: aplikacja nie wie, czy szczepienie faktycznie wykonano.");

                StringBuilder futureSb = new StringBuilder();

                futureSb.AppendLine($"Wiek użytkownika: {age} lat");
                futureSb.AppendLine();
                futureSb.AppendLine("PRZYSZŁE SZCZEPIENIA:");
                futureSb.AppendLine();

                if (summary.FutureVaccinations.Count == 0)
                {
                    futureSb.AppendLine("Brak kolejnych szczepień w harmonogramie.");
                }
                else
                {
                    int counter = 1;

                    foreach (var vaccination in futureVaccinations)
                    {
                        futureSb.AppendLine($"{counter}. {vaccination.VaccineName}");
                        futureSb.AppendLine($"Choroba: {vaccination.Disease}");

                        futureSb.AppendLine($"Wiek / rytm: {vaccination.AgeLabel}");
                        //futureSb.AppendLine($"Zalecany timing: {vaccination.RecommendedTiming}");
                        futureSb.AppendLine($"Planowana data: {vaccination.PlannedDate:dd.MM.yyyy}");

                        if (vaccination.DaysDifference == 0)
                            futureSb.AppendLine("Termin: dzisiaj");
                        else
                            futureSb.AppendLine($"Pozostało dni: {vaccination.DaysDifference}");

                        futureSb.AppendLine($"Opis: {vaccination.Description}");
                        futureSb.AppendLine();

                        counter++;
                    }
                }

                futureSb.AppendLine("Uwaga: przyszłe szczepienia zależą od historii szczepień i kwalifikacji lekarskiej.");

                PastResultsTextBox.Text = pastSb.ToString();
                FutureResultsTextBox.Text = futureSb.ToString();

                var nextVaccination = futureVaccinations.FirstOrDefault();
                if (nextVaccination != null)
                {
                    ReminderWindow reminderWindow = new ReminderWindow(
                        nextVaccination.VaccineName,
                        nextVaccination.PlannedDate);

                    reminderWindow.Owner = this;
                    reminderWindow.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Błąd odczytu harmonogramu",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    private List<VaccinationResult> ApplyFilter(List<VaccinationResult> vaccinations)
        {
            string selectedFilter = "Wszystkie";

            if (VaccineFilterComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                selectedFilter = selectedItem.Content.ToString() ?? "Wszystkie";
            }

            return selectedFilter switch
            {
                "Dzieci i młodzież" => vaccinations
                    .Where(v =>
                        v.Category.Contains("dzieci", StringComparison.OrdinalIgnoreCase) ||
                        v.Category.Contains("młodzież", StringComparison.OrdinalIgnoreCase))
                    .ToList(),

                "Dorośli" => vaccinations
                    .Where(v =>
                        v.Category.Contains("dorośli", StringComparison.OrdinalIgnoreCase) ||
                        v.Category.Contains("healthyAdult", StringComparison.OrdinalIgnoreCase) ||
                        v.Category.Contains("diabetes", StringComparison.OrdinalIgnoreCase) ||
                        v.Category.Contains("oncology", StringComparison.OrdinalIgnoreCase))
                    .ToList(),

                "Cykliczne" => vaccinations
                    .Where(v => v.ScheduleType == "recurring")
                    .ToList(),

                "Sezonowe" => vaccinations
                    .Where(v => v.IsSeasonal)
                    .ToList(),

                "Obowiązkowe PL" => vaccinations
                    .Where(v => v.IsRequiredInPL)
                    .ToList(),

                _ => vaccinations
            };
        }
    } 
}