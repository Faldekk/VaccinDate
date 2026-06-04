using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;

namespace Vaccindate
{
    public partial class ReminderWindow : Window
    {
        private readonly string _vaccinationName;
        private readonly DateTime _vaccinationDate;

        public ReminderWindow(string vaccinationName, DateTime vaccinationDate)
        {
            InitializeComponent();

            _vaccinationName = vaccinationName;
            _vaccinationDate = vaccinationDate;

            VaccinationInfoTextBlock.Text =
                $"Najbliższe szczepienie: {_vaccinationName}\n" +
                $"Proponowana data: {_vaccinationDate:dd.MM.yyyy}";

            ReminderDatePicker.SelectedDate = _vaccinationDate;
            ReminderDatePicker.DisplayDate = _vaccinationDate;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Podaj imię użytkownika.");
                return;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Podaj adres e-mail.");
                return;
            }

            if (!IsValidEmail(email))
            {
                MessageBox.Show("Podaj poprawny adres e-mail.");
                return;
            }

            if (ReminderDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Wybierz datę przypomnienia.");
                return;
            }

            DateTime reminderDate = ReminderDatePicker.SelectedDate.Value.Date;

            ReminderRequest request = new ReminderRequest
            {
                UserName = name,
                Email = email,
                VaccinationName = _vaccinationName,
                VaccinationDate = _vaccinationDate,
                ReminderDate = reminderDate,
                CreatedAt = DateTime.Now
            };

            SaveReminder(request);

            MessageBox.Show(
                "Przypomnienie zostało zapisane.",
                "Zapisano",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(
                email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase);
        }

        private void SaveReminder(ReminderRequest request)
        {
            string filePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "reminders.json");

            List<ReminderRequest> reminders;

            if (File.Exists(filePath))
            {
                string existingJson = File.ReadAllText(filePath);

                reminders = JsonSerializer.Deserialize<List<ReminderRequest>>(
                    existingJson,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<ReminderRequest>();
            }
            else
            {
                reminders = new List<ReminderRequest>();
            }

            reminders.Add(request);

            string json = JsonSerializer.Serialize(
                reminders,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            File.WriteAllText(filePath, json);
        }
    }

    public class ReminderRequest
    {
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public string VaccinationName { get; set; } = "";
        public DateTime VaccinationDate { get; set; }
        public DateTime ReminderDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}