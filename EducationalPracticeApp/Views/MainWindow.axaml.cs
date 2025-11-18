using System;
using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;


namespace EducationalPracticeApp.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        private async void LoginButton(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                string login = LoginTB?.Text?.Trim() ?? "";
                string password = PasswordTextBox?.Text?.Trim() ?? "";

                if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
                {
                    ErrorTextBlock.Text = "Неверный логин или пароль";
                    return;
                }

                var staffMember = await App.DbContext.Staff
                    .FirstOrDefaultAsync(s => s.Login == login);

                if (staffMember == null)
                {
                    ErrorTextBlock.Text = "Пользователь не найден";
                    return;
                }

                if (staffMember.Password != password)
                {
                    ErrorTextBlock.Text = "Неправильный пароль";
                    return;
                }


                string position = staffMember.Position?.Trim() ?? "";
                Window nextWindow;

                switch (position.ToLower())
                {
                    case "зав. кафедрой":
                        nextWindow = new DepartmentHeadWindow();
                        break;
                    case "преподаватель":
                        nextWindow = new TeacherWindow();
                        break;
                    case "инженер":
                        nextWindow = new EngineerWindow();
                        break;
                    default:
                        ErrorTextBlock.Text = "такой должности нету";
                        return;
                }

                nextWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                ErrorTextBlock.Text = $"Произошла ошибка при входе: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Login error: {ex}");
            }

        }

        private void LoginGuest(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {

            var parent = this.VisualRoot as Window;
            if (parent != null)
            {
                var universityWindow = new UniversityManagmentWindow();
                universityWindow.Show();
                parent.Close();
            }
        }
    }
}