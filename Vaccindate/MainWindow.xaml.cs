
using System;
using System.Linq;
using System.Text;
using System.Windows;
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
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            BirthDatePicker.SelectedDate = null;
            PastResultsTextBox.Text = "Wybierz datę urodzenia i kliknij „Pokaż szczepienia”.";
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

            try
            {
                var summary = _calculator.GetVaccinationSummary(birthDate);

                StringBuilder pastSb = new StringBuilder();

                if (summary.AlreadyDueVaccinations.Count == 0)
                {
                    pastSb.AppendLine("Brak szczepień, które powinny już być wykonane według harmonogramu.");
                }
                else
                {
                    int counter = 1;

                    foreach (var vaccination in summary.AlreadyDueVaccinations)
                    {
                        int daysAgo = Math.Abs(vaccination.DaysDifference);

                        pastSb.AppendLine($"{counter}. {vaccination.VaccineName}");
                        pastSb.AppendLine($"Choroba: {vaccination.Disease}");
                        pastSb.AppendLine($"Kategoria: {vaccination.Category}");
                        pastSb.AppendLine($"Wiek / termin: {vaccination.AgeLabel}");
                        pastSb.AppendLine($"Planowany termin: {vaccination.PlannedDate:dd.MM.yyyy}");
                        pastSb.AppendLine($"Termin minął: {daysAgo} dni temu");
                        pastSb.AppendLine($"Opis: {vaccination.Description}");
                        pastSb.AppendLine();

                        counter++;
                    }
                }

                pastSb.AppendLine("Uwaga: lista oznacza szczepienia, które powinny być wykonane według harmonogramu.");
                pastSb.AppendLine("Aplikacja nie potwierdza faktycznego wykonania szczepienia.");

                StringBuilder futureSb = new StringBuilder();

                if (summary.FutureVaccinations.Count == 0)
                {
                    futureSb.AppendLine("Brak kolejnych szczepień w harmonogramie.");
                }
                else
                {
                    int counter = 1;

                    foreach (var vaccination in summary.FutureVaccinations)
                    {
                        futureSb.AppendLine($"{counter}. {vaccination.VaccineName}");
                        futureSb.AppendLine($"Choroba: {vaccination.Disease}");
                        futureSb.AppendLine($"Kategoria: {vaccination.Category}");
                        futureSb.AppendLine($"Wiek / termin: {vaccination.AgeLabel}");
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

                futureSb.AppendLine("Uwaga: przyszłe i cykliczne szczepienia zależą od indywidualnych czynników medycznych.");

                PastResultsTextBox.Text = pastSb.ToString();
                FutureResultsTextBox.Text = futureSb.ToString();

                var nextVaccination = summary.FutureVaccinations.FirstOrDefault();

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
    }
}