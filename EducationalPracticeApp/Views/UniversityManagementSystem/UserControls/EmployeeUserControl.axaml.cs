using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using EducationalPracticeApp.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace EducationalPracticeApp;

public partial class EmployeeUserControl : UserControl
{
    private bool _isDepartmentHead = false;
    private bool _isEngineer = false;
    private Staff? _selectedEmployee = null;

    public EmployeeUserControl()
    {
        InitializeComponent();
        CheckUserRole();
        LoadData();
    }

    private void CheckUserRole()
    {
        if (App.CurrentUser == null) return;

        var position = App.CurrentUser.Position?.Trim() ?? "";
        _isDepartmentHead = position.ToLower() == "зав. кафедрой";
        _isEngineer = position.ToLower() == "инженер";


        if (AddEmployeeButton != null)
        {
            AddEmployeeButton.IsVisible = _isDepartmentHead || _isEngineer;
        }
        if (EditEmployeeButton != null)
        {
            EditEmployeeButton.IsVisible = _isEngineer;
        }
        if (DeleteEmployeeButton != null)
        {
            DeleteEmployeeButton.IsVisible = _isEngineer;
        }
    }

    private void DataEmployee_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _selectedEmployee = DataEmployee.SelectedItem as Staff;
    }

    public void LoadData()
    {
        if (DataEmployee is null) return;

        var query = App.DbContext.Staff.AsQueryable();


        if (_isDepartmentHead && App.CurrentUser != null)
        {
            query = query.Where(s => s.DeptCode == App.CurrentUser.DeptCode);
        }


        if (_isEngineer)
        {
            var engineerIds = App.DbContext.Engineers.Select(e => e.StaffId).ToList();
            query = query.Where(s => !engineerIds.Contains(s.StaffId));
        }

        var allEmployee = query.ToList();
        DataEmployee.ItemsSource = allEmployee;
    }

    private void FullNameFilterTextBox_TextChanged(object? sender, Avalonia.Controls.TextChangedEventArgs e)
    {
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        if (DataEmployee is null)
        {
            return;
        }

        var query = App.DbContext.Staff.AsQueryable();


        if (_isDepartmentHead && App.CurrentUser != null)
        {
            query = query.Where(s => s.DeptCode == App.CurrentUser.DeptCode);
        }

        if (_isEngineer)
        {
            var engineerIds = App.DbContext.Engineers.Select(e => e.StaffId).ToList();
            query = query.Where(s => !engineerIds.Contains(s.StaffId));
        }

        var allEmployees = query.ToList();
        var filter = FullNameFilterTextBox.Text ?? string.Empty;
        var filteredQuery = allEmployees.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filteredQuery = filteredQuery.Where(s => s.FullName.Contains(filter, StringComparison.OrdinalIgnoreCase));
        }

        DataEmployee.ItemsSource = filteredQuery.ToList();
    }

    private async void AddEmployeeButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (App.CurrentUser == null) return;

        var departments = await App.DbContext.Departments.ToListAsync();
        var deptCode = App.CurrentUser.DeptCode;

        var dialog = new Window
        {
            Title = "Зачислить сотрудника на кафедру",
            Width = 600,
            Height = _isEngineer ? 650 : 550,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        var mainGrid = new Grid { Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#012326")) };
        var border = new Border
        {
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#103B40")),
            CornerRadius = new CornerRadius(25),
            Margin = new Thickness(20),
            Padding = new Thickness(20)
        };
        var panel = new StackPanel { Spacing = 10 };

        var titleLabel = new TextBlock
        {
            Text = "Зачислить сотрудника на кафедру",
            FontSize = 24,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#A3C9D9")),
            Margin = new Thickness(0, 0, 0, 20)
        };

        var nameLabel = new TextBlock { Text = "ФИО сотрудника:", Foreground = Avalonia.Media.Brushes.White, FontSize = 16, Margin = new Thickness(0, 0, 0, 5) };
        var nameTextBox = new TextBox
        {
            Margin = new Thickness(0, 5, 0, 10),
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#346C73")),
            Foreground = Avalonia.Media.Brushes.White,
            BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#6A9BA6")),
            CornerRadius = new CornerRadius(15),
            Padding = new Thickness(15, 10),
            Height = 45,
            FontSize = 14
        };

        var positionLabel = new TextBlock { Text = "Должность:", Foreground = Avalonia.Media.Brushes.White, FontSize = 16, Margin = new Thickness(0, 0, 0, 5) };
        var positionTextBox = new TextBox
        {
            Margin = new Thickness(0, 5, 0, 10),
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#346C73")),
            Foreground = Avalonia.Media.Brushes.White,
            BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#6A9BA6")),
            CornerRadius = new CornerRadius(15),
            Padding = new Thickness(15, 10),
            Height = 45,
            FontSize = 14
        };

        var salaryLabel = new TextBlock { Text = "Зарплата:", Foreground = Avalonia.Media.Brushes.White, FontSize = 16, Margin = new Thickness(0, 0, 0, 5) };
        var salaryTextBox = new TextBox
        {
            Margin = new Thickness(0, 5, 0, 10),
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#346C73")),
            Foreground = Avalonia.Media.Brushes.White,
            BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#6A9BA6")),
            CornerRadius = new CornerRadius(15),
            Padding = new Thickness(15, 10),
            Height = 45,
            FontSize = 14
        };

        ComboBox? deptComboBox = null;
        if (_isEngineer)
        {
            var deptLabel = new TextBlock { Text = "Кафедра:", Foreground = Avalonia.Media.Brushes.White, FontSize = 16, Margin = new Thickness(0, 0, 0, 5) };
            deptComboBox = new ComboBox
            {
                ItemsSource = departments.Select(d => $"{d.Code} - {d.Name}").ToList(),
                Margin = new Thickness(0, 5, 0, 10),
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#346C73")),
                Foreground = Avalonia.Media.Brushes.White,
                BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#6A9BA6")),
                CornerRadius = new CornerRadius(15),
                Padding = new Thickness(15, 10),
                Height = 45,
                FontSize = 14
            };
            var deptIndex = departments.FindIndex(d => d.Code == deptCode);
            if (deptIndex >= 0)
            {
                deptComboBox.SelectedIndex = deptIndex;
            }
            panel.Children.Add(deptLabel);
            panel.Children.Add(deptComboBox);
        }

        var buttonPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 15, Margin = new Thickness(0, 25, 0, 0), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };
        var okButton = new Button
        {
            Content = "Добавить",
            Width = 150,
            Height = 45,
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#2A5E65")),
            Foreground = Avalonia.Media.Brushes.White,
            BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#6A9BA6")),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(15),
            Padding = new Thickness(20, 10),
            FontSize = 16,
            Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand)
        };
        var cancelButton = new Button
        {
            Content = "Отмена",
            Width = 150,
            Height = 45,
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#2A5E65")),
            Foreground = Avalonia.Media.Brushes.White,
            BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#6A9BA6")),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(15),
            Padding = new Thickness(20, 10),
            FontSize = 16,
            Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand)
        };

        okButton.Click += async (s, args) =>
        {
            if (string.IsNullOrWhiteSpace(nameTextBox.Text) || string.IsNullOrWhiteSpace(positionTextBox.Text))
            {
                return;
            }

            if (_isEngineer && (deptComboBox == null || deptComboBox.SelectedIndex < 0))
            {
                return;
            }

            try
            {
                var selectedDeptCode = _isEngineer 
                    ? departments[deptComboBox!.SelectedIndex].Code  
                    : App.CurrentUser.DeptCode;

                var maxId = await App.DbContext.Staff.MaxAsync(s => (int?)s.StaffId) ?? 0;
                var newStaffId = maxId + 1;

                var newStaff = new Staff
                {
                    StaffId = newStaffId,
                    FullName = nameTextBox.Text.Trim(),
                    Position = positionTextBox.Text.Trim(),
                    DeptCode = selectedDeptCode,
                    Salary = decimal.TryParse(salaryTextBox.Text, out var salary) ? salary : null
                };

                App.DbContext.Staff.Add(newStaff);
                await App.DbContext.SaveChangesAsync();

                LoadData();
                dialog.Close();
            }
            catch (Exception ex)
            {
                var errorGrid = new Grid { Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#012326")) };
                var errorBorder = new Border
                {
                    Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#103B40")),
                    CornerRadius = new CornerRadius(25),
                    Margin = new Thickness(20),
                    Padding = new Thickness(20)
                };
                var errorText = new TextBlock
                {
                    Text = $"Ошибка при добавлении: {ex.Message}",
                    Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#A3C9D9")),
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap
                };
                errorBorder.Child = errorText;
                errorGrid.Children.Add(errorBorder);
                var errorDialog = new Window
                {
                    Title = "Ошибка",
                    Content = errorGrid,
                    Width = 350,
                    Height = 150,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                await errorDialog.ShowDialog(dialog);
            }
        };

        cancelButton.Click += (s, args) => dialog.Close();

        buttonPanel.Children.Add(okButton);
        buttonPanel.Children.Add(cancelButton);

        panel.Children.Add(titleLabel);
        panel.Children.Add(nameLabel);
        panel.Children.Add(nameTextBox);
        panel.Children.Add(positionLabel);
        panel.Children.Add(positionTextBox);
        panel.Children.Add(salaryLabel);
        panel.Children.Add(salaryTextBox);
        panel.Children.Add(buttonPanel);

        border.Child = panel;
        mainGrid.Children.Add(border);
        dialog.Content = mainGrid;
        await dialog.ShowDialog(this.VisualRoot as Window);
    }

    private async void EditEmployeeButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_selectedEmployee == null)
        {
            var errorGrid = new Grid { Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#012326")) };
            var errorBorder = new Border
            {
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#103B40")),
                CornerRadius = new CornerRadius(25),
                Margin = new Thickness(20),
                Padding = new Thickness(20)
            };
            var errorText = new TextBlock
            {
                Text = "Выберите сотрудника для изменения",
                Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#A3C9D9")),
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            };
            errorBorder.Child = errorText;
            errorGrid.Children.Add(errorBorder);
            var errorDialog = new Window
            {
                Title = "Ошибка",
                Content = errorGrid,
                Width = 350,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            await errorDialog.ShowDialog(this.VisualRoot as Window);
            return;
        }

        var departments = await App.DbContext.Departments.ToListAsync();

        var dialog = new Window
        {
            Title = "Изменить сотрудника",
            Width = 600,
            Height = 650,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        var mainGrid = new Grid { Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#012326")) };
        var border = new Border
        {
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#103B40")),
            CornerRadius = new CornerRadius(25),
            Margin = new Thickness(20),
            Padding = new Thickness(20)
        };
        var panel = new StackPanel { Spacing = 10 };

        var titleLabel = new TextBlock
        {
            Text = "Изменить сотрудника",
            FontSize = 24,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#A3C9D9")),
            Margin = new Thickness(0, 0, 0, 20)
        };

        var nameLabel = new TextBlock { Text = "ФИО сотрудника:", Foreground = Avalonia.Media.Brushes.White, FontSize = 16, Margin = new Thickness(0, 0, 0, 5) };
        var nameTextBox = new TextBox
        {
            Text = _selectedEmployee.FullName,
            Margin = new Thickness(0, 5, 0, 10),
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#346C73")),
            Foreground = Avalonia.Media.Brushes.White,
            BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#6A9BA6")),
            CornerRadius = new CornerRadius(15),
            Padding = new Thickness(15, 10),
            Height = 45,
            FontSize = 14
        };

        var positionLabel = new TextBlock { Text = "Должность:", Foreground = Avalonia.Media.Brushes.White, FontSize = 16, Margin = new Thickness(0, 0, 0, 5) };
        var positionTextBox = new TextBox
        {
            Text = _selectedEmployee.Position,
            Margin = new Thickness(0, 5, 0, 10),
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#346C73")),
            Foreground = Avalonia.Media.Brushes.White,
            BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#6A9BA6")),
            CornerRadius = new CornerRadius(15),
            Padding = new Thickness(15, 10),
            Height = 45,
            FontSize = 14
        };

        var salaryLabel = new TextBlock { Text = "Зарплата:", Foreground = Avalonia.Media.Brushes.White, FontSize = 16, Margin = new Thickness(0, 0, 0, 5) };
        var salaryTextBox = new TextBox
        {
            Text = _selectedEmployee.Salary?.ToString() ?? "",
            Margin = new Thickness(0, 5, 0, 10),
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#346C73")),
            Foreground = Avalonia.Media.Brushes.White,
            BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#6A9BA6")),
            CornerRadius = new CornerRadius(15),
            Padding = new Thickness(15, 10),
            Height = 45,
            FontSize = 14
        };

        var deptLabel = new TextBlock { Text = "Кафедра:", Foreground = Avalonia.Media.Brushes.White, FontSize = 16, Margin = new Thickness(0, 0, 0, 5) };
        var deptComboBox = new ComboBox
        {
            ItemsSource = departments.Select(d => $"{d.Code} - {d.Name}").ToList(),
            Margin = new Thickness(0, 5, 0, 10),
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#346C73")),
            Foreground = Avalonia.Media.Brushes.White,
            BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#6A9BA6")),
            CornerRadius = new CornerRadius(15),
            Padding = new Thickness(15, 10),
            Height = 45,
            FontSize = 14
        };
        var deptIndex = departments.FindIndex(d => d.Code == _selectedEmployee.DeptCode);
        if (deptIndex >= 0)
        {
            deptComboBox.SelectedIndex = deptIndex;
        }

        var buttonPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 15, Margin = new Thickness(0, 25, 0, 0), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };
        var okButton = new Button
        {
            Content = "Сохранить",
            Width = 150,
            Height = 45,
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#2A5E65")),
            Foreground = Avalonia.Media.Brushes.White,
            BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#6A9BA6")),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(15),
            Padding = new Thickness(20, 10),
            FontSize = 16,
            Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand)
        };
        var cancelButton = new Button
        {
            Content = "Отмена",
            Width = 150,
            Height = 45,
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#2A5E65")),
            Foreground = Avalonia.Media.Brushes.White,
            BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#6A9BA6")),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(15),
            Padding = new Thickness(20, 10),
            FontSize = 16,
            Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand)
        };

        okButton.Click += async (s, args) =>
        {
            if (string.IsNullOrWhiteSpace(nameTextBox.Text) || string.IsNullOrWhiteSpace(positionTextBox.Text) || deptComboBox.SelectedIndex < 0)
            {
                return;
            }

            try
            {
                _selectedEmployee.FullName = nameTextBox.Text.Trim();
                _selectedEmployee.Position = positionTextBox.Text.Trim();
                _selectedEmployee.DeptCode = departments[deptComboBox.SelectedIndex].Code;
                _selectedEmployee.Salary = decimal.TryParse(salaryTextBox.Text, out var salary) ? salary : null;

                await App.DbContext.SaveChangesAsync();

                LoadData();
                dialog.Close();
            }
            catch (Exception ex)
            {
                var errorGrid = new Grid { Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#012326")) };
                var errorBorder = new Border
                {
                    Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#103B40")),
                    CornerRadius = new CornerRadius(25),
                    Margin = new Thickness(20),
                    Padding = new Thickness(20)
                };
                var errorText = new TextBlock
                {
                    Text = $"Ошибка при изменении: {ex.Message}",
                    Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#A3C9D9")),
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap
                };
                errorBorder.Child = errorText;
                errorGrid.Children.Add(errorBorder);
                var errorDialog = new Window
                {
                    Title = "Ошибка",
                    Content = errorGrid,
                    Width = 350,
                    Height = 150,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                await errorDialog.ShowDialog(dialog);
            }
        };

        cancelButton.Click += (s, args) => dialog.Close();

        buttonPanel.Children.Add(okButton);
        buttonPanel.Children.Add(cancelButton);

        panel.Children.Add(titleLabel);
        panel.Children.Add(nameLabel);
        panel.Children.Add(nameTextBox);
        panel.Children.Add(positionLabel);
        panel.Children.Add(positionTextBox);
        panel.Children.Add(salaryLabel);
        panel.Children.Add(salaryTextBox);
        panel.Children.Add(deptLabel);
        panel.Children.Add(deptComboBox);
        panel.Children.Add(buttonPanel);

        border.Child = panel;
        mainGrid.Children.Add(border);
        dialog.Content = mainGrid;
        await dialog.ShowDialog(this.VisualRoot as Window);
    }

    private async void DeleteEmployeeButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_selectedEmployee == null)
        {
            var errorGrid = new Grid { Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#012326")) };
            var errorBorder = new Border
            {
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#103B40")),
                CornerRadius = new CornerRadius(25),
                Margin = new Thickness(20),
                Padding = new Thickness(20)
            };
            var errorText = new TextBlock
            {
                Text = "Выберите сотрудника для удаления",
                Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#A3C9D9")),
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            };
            errorBorder.Child = errorText;
            errorGrid.Children.Add(errorBorder);
            var errorDialog = new Window
            {
                Title = "Ошибка",
                Content = errorGrid,
                Width = 350,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            await errorDialog.ShowDialog(this.VisualRoot as Window);
            return;
        }

        try
        {
            var exams = await App.DbContext.Exams
                .Where(ex => ex.StaffId == _selectedEmployee.StaffId)
                .ToListAsync();
            App.DbContext.Exams.RemoveRange(exams);

            App.DbContext.Staff.Remove(_selectedEmployee);
            await App.DbContext.SaveChangesAsync();

            LoadData();
        }
        catch (Exception ex)
        {
            var errorGrid = new Grid { Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#012326")) };
            var errorBorder = new Border
            {
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#103B40")),
                CornerRadius = new CornerRadius(25),
                Margin = new Thickness(20),
                Padding = new Thickness(20)
            };
            var errorText = new TextBlock
            {
                Text = $"Ошибка при удалении: {ex.Message}",
                Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#A3C9D9")),
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            };
            errorBorder.Child = errorText;
            errorGrid.Children.Add(errorBorder);
            var errorDialog = new Window
            {
                Title = "Ошибка",
                Content = errorGrid,
                Width = 350,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            await errorDialog.ShowDialog(this.VisualRoot as Window);
        }
    }
}