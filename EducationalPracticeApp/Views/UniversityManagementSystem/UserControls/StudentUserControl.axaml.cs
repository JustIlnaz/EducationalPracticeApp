using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using EducationalPracticeApp.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EducationalPracticeApp;

public partial class StudentUserControl : UserControl
{
    private bool _isDepartmentHead = false;
    private bool _isEngineer = false;
    private Student? _selectedStudent = null;

    public StudentUserControl()
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

        if (AddStudentButton != null)
        {
            AddStudentButton.IsVisible = _isDepartmentHead || _isEngineer;
        }
        if (EditStudentButton != null)
        {
            EditStudentButton.IsVisible = _isEngineer;
        }
        if (DeleteStudentButton != null)
        {
            DeleteStudentButton.IsVisible = _isDepartmentHead || _isEngineer;
        }
    }

    public void LoadData()
    {
        if (DataStudent is null) return;

        var query = App.DbContext.Students
            .Include(s => s.ProgramCodeNavigation)
            .AsQueryable();

        if (_isDepartmentHead && App.CurrentUser != null && !_isEngineer)
        {
            query = query.Where(s => s.ProgramCodeNavigation.DeptCode == App.CurrentUser.DeptCode);
        }

        var allStudents = query.ToList();
        DataStudent.ItemsSource = allStudents;
    }

    private void SurnameFilterTextBox_TextChanged(object? sender, Avalonia.Controls.TextChangedEventArgs e)
    {
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        if (DataStudent is null)
        {
            return;
        }

        var query = App.DbContext.Students
            .Include(s => s.ProgramCodeNavigation)
            .AsQueryable();

        if (_isDepartmentHead && App.CurrentUser != null && !_isEngineer)
        {
            query = query.Where(s => s.ProgramCodeNavigation.DeptCode == App.CurrentUser.DeptCode);
        }

        var allStudents = query.ToList();
        var filter = SurnameFilterTextBox.Text ?? string.Empty;
        var filteredQuery = allStudents.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filteredQuery = filteredQuery.Where(s => s.FullName.Contains(filter, StringComparison.OrdinalIgnoreCase));
        }

        DataStudent.ItemsSource = filteredQuery.ToList();
    }

    private void DataStudent_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _selectedStudent = DataStudent.SelectedItem as Student;
    }

    private async void AddStudentButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (App.CurrentUser == null) return;

        var query = App.DbContext.Programs.AsQueryable();
        if (_isDepartmentHead)
        {
            query = query.Where(p => p.DeptCode == App.CurrentUser.DeptCode);
        }
        var departmentPrograms = await query.ToListAsync();

        if (!departmentPrograms.Any())
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
                Text = "На кафедре нет программ обучения",
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

        var dialog = new Window
        {
            Title = "Добавить студента",
            Width = 600,
            Height = 550,
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
            Text = "Добавить студента",
            FontSize = 24,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#A3C9D9")),
            Margin = new Thickness(0, 0, 0, 20)
        };

        var nameLabel = new TextBlock { Text = "ФИО студента:", Foreground = Avalonia.Media.Brushes.White, FontSize = 16, Margin = new Thickness(0, 0, 0, 5) };
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

        var programLabel = new TextBlock { Text = "Программа обучения:", Foreground = Avalonia.Media.Brushes.White, FontSize = 16, Margin = new Thickness(0, 0, 0, 5) };
        var programComboBox = new ComboBox
        {
            ItemsSource = departmentPrograms.Select(p => $"{p.Code} - {p.Title}").ToList(),
            Margin = new Thickness(0, 5, 0, 10),
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#346C73")),
            Foreground = Avalonia.Media.Brushes.White,
            BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#6A9BA6")),
            CornerRadius = new CornerRadius(15),
            Padding = new Thickness(15, 10),
            Height = 45,
            FontSize = 14
        };
        if (departmentPrograms.Any())
        {
            programComboBox.SelectedIndex = 0;
        }

        var regNumLabel = new TextBlock { Text = "Регистрационный номер:", Foreground = Avalonia.Media.Brushes.White, FontSize = 16, Margin = new Thickness(0, 0, 0, 5) };
        var regNumTextBox = new TextBox
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
            if (string.IsNullOrWhiteSpace(nameTextBox.Text) || programComboBox.SelectedItem == null || string.IsNullOrWhiteSpace(regNumTextBox.Text))
            {
                return;
            }

            try
            {
                if (!int.TryParse(regNumTextBox.Text, out var regNum))
                {
                    return;
                }

                var existingStudent = await App.DbContext.Students.FindAsync(regNum);
                if (existingStudent != null)
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
                        Text = "Студент с таким регистрационным номером уже существует",
                        Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#A3C9D9")),
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap
                    };
                    errorBorder.Child = errorText;
                    errorGrid.Children.Add(errorBorder);
                    var errorDialog = new Window
                    {
                        Title = "Ошибка",
                        Content = errorGrid,
                        Width = 400,
                        Height = 150,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                    await errorDialog.ShowDialog(dialog);
                    return;
                }

                var selectedIndex = programComboBox.SelectedIndex;
                if (selectedIndex < 0 || selectedIndex >= departmentPrograms.Count) return;

                var selectedProgram = departmentPrograms[selectedIndex];
                var newStudent = new Student
                {
                    RegNum = regNum,
                    FullName = nameTextBox.Text.Trim(),
                    ProgramCode = selectedProgram.Code
                };

                App.DbContext.Students.Add(newStudent);
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
        panel.Children.Add(programLabel);
        panel.Children.Add(programComboBox);
        panel.Children.Add(regNumLabel);
        panel.Children.Add(regNumTextBox);
        panel.Children.Add(buttonPanel);

        border.Child = panel;
        mainGrid.Children.Add(border);
        dialog.Content = mainGrid;
        await dialog.ShowDialog(this.VisualRoot as Window);
    }

    private async void DeleteStudentButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_selectedStudent == null)
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
                Text = "Выберите студента для удаления",
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
                .Where(ex => ex.RegNum == _selectedStudent.RegNum)
                .ToListAsync();
            App.DbContext.Exams.RemoveRange(exams);

            App.DbContext.Students.Remove(_selectedStudent);
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

    private async void EditStudentButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_selectedStudent == null)
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
                Text = "Выберите студента для изменения",
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

        var programs = await App.DbContext.Programs.ToListAsync();

        var dialog = new Window
        {
            Title = "Изменить студента",
            Width = 600,
            Height = 550,
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
            Text = "Изменить студента",
            FontSize = 24,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#A3C9D9")),
            Margin = new Thickness(0, 0, 0, 20)
        };

        var nameLabel = new TextBlock { Text = "ФИО студента:", Foreground = Avalonia.Media.Brushes.White, FontSize = 16, Margin = new Thickness(0, 0, 0, 5) };
        var nameTextBox = new TextBox
        {
            Text = _selectedStudent.FullName,
            Margin = new Thickness(0, 5, 0, 10),
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#346C73")),
            Foreground = Avalonia.Media.Brushes.White,
            BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#6A9BA6")),
            CornerRadius = new CornerRadius(15),
            Padding = new Thickness(15, 10),
            Height = 45,
            FontSize = 14
        };

        var programLabel = new TextBlock { Text = "Программа обучения:", Foreground = Avalonia.Media.Brushes.White, FontSize = 16, Margin = new Thickness(0, 0, 0, 5) };
        var programComboBox = new ComboBox
        {
            ItemsSource = programs.Select(p => $"{p.Code} - {p.Title}").ToList(),
            Margin = new Thickness(0, 5, 0, 10),
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#346C73")),
            Foreground = Avalonia.Media.Brushes.White,
            BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#6A9BA6")),
            CornerRadius = new CornerRadius(15),
            Padding = new Thickness(15, 10),
            Height = 45,
            FontSize = 14
        };
        var programIndex = programs.FindIndex(p => p.Code == _selectedStudent.ProgramCode);
        if (programIndex >= 0)
        {
            programComboBox.SelectedIndex = programIndex;
        }

        var regNumLabel = new TextBlock { Text = "Регистрационный номер (неизменяем):", Foreground = Avalonia.Media.Brushes.White, FontSize = 16, Margin = new Thickness(0, 0, 0, 5) };
        var regNumTextBox = new TextBox
        {
            Text = _selectedStudent.RegNum.ToString(),
            IsReadOnly = true,
            Margin = new Thickness(0, 5, 0, 10),
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#346C73")),
            Foreground = Avalonia.Media.Brushes.White,
            BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#6A9BA6")),
            CornerRadius = new CornerRadius(15),
            Padding = new Thickness(15, 10),
            Height = 45,
            FontSize = 14
        };

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
            if (string.IsNullOrWhiteSpace(nameTextBox.Text) || programComboBox.SelectedItem == null)
            {
                return;
            }

            try
            {
                var selectedIndex = programComboBox.SelectedIndex;
                if (selectedIndex < 0 || selectedIndex >= programs.Count) return;

                var selectedProgram = programs[selectedIndex];
                _selectedStudent.FullName = nameTextBox.Text.Trim();
                _selectedStudent.ProgramCode = selectedProgram.Code;

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
        panel.Children.Add(programLabel);
        panel.Children.Add(programComboBox);
        panel.Children.Add(regNumLabel);
        panel.Children.Add(regNumTextBox);
        panel.Children.Add(buttonPanel);

        border.Child = panel;
        mainGrid.Children.Add(border);
        dialog.Content = mainGrid;
        await dialog.ShowDialog(this.VisualRoot as Window);
    }
}



