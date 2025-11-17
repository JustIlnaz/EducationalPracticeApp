using System;
using Avalonia.Controls;


namespace EducationalPracticeApp.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        private async void Button_Click_1(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                string login = LoginTB?.Text?.Trim() ?? "";
                string password = PasswordTextBox?.Text?.Trim() ?? "";


                if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
                {
                    ErrorTextBlock.Text = "Логин и пароль не могут быть пустыми";
                    return;
                }
            }

            //    var userLogin = await App.DbContext.Logins
            //        .Include(x => x.User)
            //        .ThenInclude(u => u.Role)
            //        .FirstOrDefaultAsync(x => x.Login1.Trim().ToLower() == login.ToLower());

            //    if (userLogin == null || userLogin.User == null)
            //    {
            //        ErrorTextBlock.Text = "Пользователь не найден";
            //        return;
            //    }


            //    if (string.IsNullOrEmpty(userLogin.Password) || userLogin.Password.Trim() != password)
            //    {
            //        ErrorTextBlock.Text = "Неверный пароль";
            //        return;
            //    }


            //    if (userLogin.User.Role == null)
            //    {
            //        ErrorTextBlock.Text = "Ошибка доступа: роль не назначена";
            //        return;
            //    }

            //    // Store login and user data in ContextData
            //    ContextData.selectedLogin1InMainWindow = userLogin;
            //    ContextData.CurrentUser = userLogin.User;
            //    ContextData.selectedRoleInMainWindow = userLogin.User.Role;

            //    if (userLogin.User.Role.NameRole?.Trim() == "Покупатель")
            //    {
            //        var basket = await App.DbContext.Baskets
            //            .FirstOrDefaultAsync(b => b.UsersId == userLogin.User.IdUsers);

            //        if (basket == null)
            //        {
            //            basket = new Basket { UsersId = userLogin.User.IdUsers };
            //            App.DbContext.Baskets.Add(basket);
            //            await App.DbContext.SaveChangesAsync();
            //        }
            //        ContextData.selectedBasketInMainWindow = basket;
            //    }

            //    string roleName = userLogin.User.Role.NameRole?.Trim() ?? "";
            //    Window nextWindow;

            //    switch (roleName.ToLower())
            //    {
            //        case "администратор":
            //            nextWindow = new AdminWindow();
            //            break;
            //        case "покупатель":
            //            nextWindow = new CustomerWindow();
            //            break;
            //        case "менеджер":
            //            nextWindow = new ManagerWindow();
            //            break;
            //        default:
            //            ErrorTextBlock.Text = "Неизвестная роль пользователя";
            //            return;
            //    }

            //    nextWindow.Show();
            //    this.Close();
            //}
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