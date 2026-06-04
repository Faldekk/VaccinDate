using System;
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

            var summary = _calculator.GetVaccinationSummary(birthDate);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SZCZEPIENIA, KTÓRE OSOBA POWINNA JUŻ MIEĆ:");
            sb.AppendLine();

            if (summary.AlreadyDueVaccinations.Count == 0)
            {
                sb.AppendLine("Brak szczepień, które powinny już być wykonane według uproszczonego harmonogramu.");
            }
            else
            {
                int counter = 1;

                foreach (var vaccination in summary.AlreadyDueVaccinations)
                {
                    int daysAgo = Math.Abs(vaccination.DaysDifference);

                    sb.AppendLine($"{counter}. {vaccination.VaccineName}");
                    sb.AppendLine($"Planowany termin: {vaccination.PlannedDate:dd.MM.yyyy}");
                    sb.AppendLine($"Termin minął: {daysAgo} dni temu");
                    sb.AppendLine($"Opis: {vaccination.Description}");
                    sb.AppendLine();

                    counter++;
                }
            }

            sb.AppendLine();
            sb.AppendLine("======================================");
            sb.AppendLine();

            sb.AppendLine("PRZYSZŁE SZCZEPIENIA:");
            sb.AppendLine();

            if (summary.FutureVaccinations.Count == 0)
            {
                sb.AppendLine("Brak kolejnych szczepień w uproszczonym harmonogramie.");
            }
            else
            {
                int counter = 1;

                foreach (var vaccination in summary.FutureVaccinations)
                {
                    sb.AppendLine($"{counter}. {vaccination.VaccineName}");
                    sb.AppendLine($"Planowana data: {vaccination.PlannedDate:dd.MM.yyyy}");
                    sb.AppendLine($"Pozostało dni: {vaccination.DaysDifference}");
                    sb.AppendLine($"Opis: {vaccination.Description}");
                    sb.AppendLine();

                    counter++;
                }
            }

            sb.AppendLine();
            sb.AppendLine("Uwaga: wynik jest uproszczony i nie potwierdza, że szczepienie faktycznie zostało wykonane.");
            sb.AppendLine("Do realnej oceny potrzebna jest historia szczepień i konsultacja medyczna.");

            ResultsTextBox.Text = sb.ToString();
        }
    }
}