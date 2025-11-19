using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using EducationalPracticeApp.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace EducationalPracticeApp;

public partial class DisciplineUserControl : UserControl
{
    private bool _isDepartmentHead = false;
    private bool _isEngineer = false;
    private Course? _selectedCourse = null;

    public DisciplineUserControl()
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

        if (AddDisciplineButton != null)
        {
            AddDisciplineButton.IsVisible = _isEngineer;
        }
        if (EditDisciplineButton != null)
        {
            EditDisciplineButton.IsVisible = _isDepartmentHead || _isEngineer;
        }
        if (DeleteDisciplineButton != null)
        {
            DeleteDisciplineButton.IsVisible = _isEngineer;
        }
    }

    public void LoadData()
    {
        if (DataDiscipline is null) return;

        var query = App.DbContext.Courses.AsQueryable();

        if (_isDepartmentHead && App.CurrentUser != null && !_isEngineer)
        {
            query = query.Where(c => c.DeptCode == App.CurrentUser.DeptCode);
        }

        var allDiscipline = query.ToList();
        DataDiscipline.ItemsSource = allDiscipline;
    }

    private void TitleNameFilterTextBox_TextChanged(object? sender, Avalonia.Controls.TextChangedEventArgs e)
    {
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        if (DataDiscipline is null)
        {
            return;
        }

        var query = App.DbContext.Courses.AsQueryable();

        if (_isDepartmentHead && App.CurrentUser != null && !_isEngineer)
        {
            query = query.Where(c => c.DeptCode == App.CurrentUser.DeptCode);
        }

        var allDiscipline = query.ToList();
        var filter = TitleNameFilterTextBox.Text ?? string.Empty;
        var filteredQuery = allDiscipline.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filteredQuery = filteredQuery.Where(s => s.Title.Contains(filter, StringComparison.OrdinalIgnoreCase));
        }

        DataDiscipline.ItemsSource = filteredQuery.ToList();
    }

    private void DataDiscipline_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _selectedCourse = DataDiscipline.SelectedItem as Course;
    }

    private async void EditDisciplineButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_selectedCourse == null)
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
                Text = "Выберите дисциплину для изменения",
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
            Title = "Изменить дисциплину",
            Width = 600,
            Height = _isEngineer ? 550 : 450,
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

        var titleHeader = new TextBlock
        {
            Text = "Изменить дисциплину",
            FontSize = 24,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#A3C9D9")),
            Margin = new Thickness(0, 0, 0, 20)
        };

        var titleLabel = new TextBlock { Text = "Название дисциплины:", Foreground = Avalonia.Media.Brushes.White, FontSize = 16, Margin = new Thickness(0, 0, 0, 5) };
        var titleTextBox = new TextBox
        {
            Text = _selectedCourse.Title,
            Margin = new Thickness(0, 5, 0, 10),
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#346C73")),
            Foreground = Avalonia.Media.Brushes.White,
            BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#6A9BA6")),
            CornerRadius = new CornerRadius(15),
            Padding = new Thickness(15, 10),
            Height = 45,
            FontSize = 14
        };

        var workloadLabel = new TextBlock { Text = "Объем (часы):", Foreground = Avalonia.Media.Brushes.White, FontSize = 16, Margin = new Thickness(0, 0, 0, 5) };
        var workloadTextBox = new TextBox
        {
            Text = _selectedCourse.Workload.ToString(),
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
            var deptIndex = departments.FindIndex(d => d.Code == _selectedCourse.DeptCode);
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
            if (string.IsNullOrWhiteSpace(titleTextBox.Text) || string.IsNullOrWhiteSpace(workloadTextBox.Text))
            {
                return;
            }

            if (_isEngineer && (deptComboBox == null || deptComboBox.SelectedIndex < 0))
            {
                return;
            }

            try
            {
                if (!int.TryParse(workloadTextBox.Text, out var workload))
                {
                    return;
                }

                _selectedCourse.Title = titleTextBox.Text.Trim();
                _selectedCourse.Workload = workload;
                if (_isEngineer)
                {
                    _selectedCourse.DeptCode = departments[deptComboBox!.SelectedIndex].Code;
                }

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

        panel.Children.Add(titleHeader);
        panel.Children.Add(titleLabel);
        panel.Children.Add(titleTextBox);
        panel.Children.Add(workloadLabel);
        panel.Children.Add(workloadTextBox);
        panel.Children.Add(buttonPanel);

        border.Child = panel;
        mainGrid.Children.Add(border);
        dialog.Content = mainGrid;
        await dialog.ShowDialog(this.VisualRoot as Window);
    }

    private async void AddDisciplineButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var departments = await App.DbContext.Departments.ToListAsync();

        var dialog = new Window
        {
            Title = "Добавить дисциплину",
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

        var titleHeader = new TextBlock
        {
            Text = "Добавить дисциплину",
            FontSize = 24,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#A3C9D9")),
            Margin = new Thickness(0, 0, 0, 20)
        };

        var titleLabel = new TextBlock { Text = "Название дисциплины:", Foreground = Avalonia.Media.Brushes.White, FontSize = 16, Margin = new Thickness(0, 0, 0, 5) };
        var titleTextBox = new TextBox
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

        var workloadLabel = new TextBlock { Text = "Объем (часы):", Foreground = Avalonia.Media.Brushes.White, FontSize = 16, Margin = new Thickness(0, 0, 0, 5) };
        var workloadTextBox = new TextBox
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
        if (departments.Any())
        {
            deptComboBox.SelectedIndex = 0;
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
            if (string.IsNullOrWhiteSpace(titleTextBox.Text) || string.IsNullOrWhiteSpace(workloadTextBox.Text) || deptComboBox.SelectedIndex < 0)
            {
                return;
            }

            try
            {
                if (!int.TryParse(workloadTextBox.Text, out var workload))
                {
                    return;
                }

                var maxId = await App.DbContext.Courses.MaxAsync(c => (int?)c.CourseId) ?? 0;
                var newCourseId = maxId + 1;

                var newCourse = new Course
                {
                    CourseId = newCourseId,
                    Title = titleTextBox.Text.Trim(),
                    Workload = workload,
                    DeptCode = departments[deptComboBox.SelectedIndex].Code
                };

                App.DbContext.Courses.Add(newCourse);
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

        panel.Children.Add(titleHeader);
        panel.Children.Add(titleLabel);
        panel.Children.Add(titleTextBox);
        panel.Children.Add(workloadLabel);
        panel.Children.Add(workloadTextBox);
        panel.Children.Add(deptLabel);
        panel.Children.Add(deptComboBox);
        panel.Children.Add(buttonPanel);

        border.Child = panel;
        mainGrid.Children.Add(border);
        dialog.Content = mainGrid;
        await dialog.ShowDialog(this.VisualRoot as Window);
    }

    private async void DeleteDisciplineButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_selectedCourse == null)
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
                Text = "Выберите дисциплину для удаления",
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
            // Удаляем связанные экзамены
            var exams = await App.DbContext.Exams
                .Where(ex => ex.CourseId == _selectedCourse.CourseId)
                .ToListAsync();
            App.DbContext.Exams.RemoveRange(exams);

            App.DbContext.Courses.Remove(_selectedCourse);
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