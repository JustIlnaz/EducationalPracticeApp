using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using EducationalPracticeApp.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace EducationalPracticeApp;

public partial class ExamUserControl : UserControl
{
    private bool _isDepartmentHead = false;
    private bool _isTeacher = false;
    private bool _isEngineer = false;
    private Exam? _selectedExam = null;

    public ExamUserControl()
    {
        InitializeComponent();
        this.Loaded += ExamUserControl_Loaded;
    }

    private void ExamUserControl_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        CheckUserRole();
        LoadDataAsync();
    }

    private void CheckUserRole()
    {
        if (App.CurrentUser == null) return;

        var position = App.CurrentUser.Position?.Trim() ?? "";
        var positionLower = position.ToLower();
        _isDepartmentHead = positionLower == "зав. кафедрой";
        _isTeacher = positionLower == "преподаватель";
        _isEngineer = positionLower == "инженер";

        if (AssignExamButton != null)
        {
            AssignExamButton.IsVisible = _isDepartmentHead || _isEngineer;
        }
        if (EditExamButton != null)
        {
            EditExamButton.IsVisible = _isDepartmentHead || _isEngineer;
        }
        if (DeleteExamButton != null)
        {
            DeleteExamButton.IsVisible = _isDepartmentHead || _isEngineer;
        }
        if (SetGradeButton != null)
        {
            SetGradeButton.IsVisible = _isTeacher;
        }
    }

    public async void LoadDataAsync()
    {
        if (DataExam is null) return;

        var query = App.DbContext.Exams
            .Include(e => e.Course)
            .AsQueryable();

        if (App.CurrentUser != null)
        {
            if (_isDepartmentHead && !_isEngineer)
        {
            query = query.Where(e => e.Course.DeptCode == App.CurrentUser.DeptCode);
            }
            else if (_isTeacher)
            {
                query = query.Where(e => e.StaffId == App.CurrentUser.StaffId);
            }
        }

        var allExam = await query.ToListAsync();
        DataExam.ItemsSource = allExam;
    }

    public void LoadData()
    {
        LoadDataAsync();
    }

    private void FullNameFilterTextBox_TextChanged(object? sender, Avalonia.Controls.TextChangedEventArgs e)
    {
        ApplyFilter();
    }

    private async void ApplyFilter()
    {
        if (DataExam is null)
        {
            return;
        }

        var query = App.DbContext.Exams
            .Include(e => e.Course)
            .AsQueryable();

        if (App.CurrentUser != null)
        {
            if (_isDepartmentHead && !_isEngineer)
        {
            query = query.Where(e => e.Course.DeptCode == App.CurrentUser.DeptCode);
            }
            else if (_isTeacher)
            {
                query = query.Where(e => e.StaffId == App.CurrentUser.StaffId);
            }
        }

        var allExams = await query.ToListAsync();
        var filter = FullNameFilterTextBox.Text ?? string.Empty;
        var filteredQuery = allExams.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filteredQuery = filteredQuery.Where(s => s.RegNum.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase));
        }

        DataExam.ItemsSource = filteredQuery.ToList();
    }

    private async void AssignExamButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (App.CurrentUser == null) return;

        var coursesQuery = App.DbContext.Courses.AsQueryable();
        if (_isDepartmentHead && !_isEngineer)
        {
            coursesQuery = coursesQuery.Where(c => c.DeptCode == App.CurrentUser.DeptCode);
        }
        var departmentCourses = await coursesQuery.ToListAsync();

        if (!departmentCourses.Any())
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
                Text = "На кафедре нет дисциплин",
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

        var studentsQuery = App.DbContext.Students
            .Include(s => s.ProgramCodeNavigation)
            .AsQueryable();
        if (_isDepartmentHead && !_isEngineer)
        {
            studentsQuery = studentsQuery.Where(s => s.ProgramCodeNavigation.DeptCode == App.CurrentUser.DeptCode);
        }
        var departmentStudents = await studentsQuery.ToListAsync();

        if (!departmentStudents.Any())
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
                Text = "На кафедре нет студентов",
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

        var staffQuery = App.DbContext.Staff
            .Where(s => s.Lecturer != null)
            .AsQueryable();
        if (_isDepartmentHead && !_isEngineer)
        {
            staffQuery = staffQuery.Where(s => s.DeptCode == App.CurrentUser.DeptCode);
        }
        var departmentStaff = await staffQuery.ToListAsync();

        if (!departmentStaff.Any())
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
                Text = "На кафедре нет преподавателей",
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
            Title = "Назначить экзамен",
            Width = 600,
            Height = 850,
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
        var scrollViewer = new ScrollViewer { VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto };
        var panel = new StackPanel { Spacing = 10 };

        var titleLabel = new TextBlock
        {
            Text = "Назначить экзамен",
            FontSize = 24,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#A3C9D9")),
            Margin = new Thickness(0, 0, 0, 20)
        };

        var courseLabel = new TextBlock { Text = "Дисциплина:", Foreground = Avalonia.Media.Brushes.White, FontSize = 16, Margin = new Thickness(0, 0, 0, 5) };
        var courseComboBox = new ComboBox
        {
            ItemsSource = departmentCourses.Select(c => $"{c.CourseId} - {c.Title}").ToList(),
            Margin = new Thickness(0, 5, 0, 10),
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#346C73")),
            Foreground = Avalonia.Media.Brushes.White,
            BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#6A9BA6")),
            CornerRadius = new CornerRadius(15),
            Padding = new Thickness(15, 10),
            Height = 45,
            FontSize = 14
        };
        if (departmentCourses.Any())
        {
            courseComboBox.SelectedIndex = 0;
        }

        var studentLabel = new TextBlock { Text = "Студент:", Foreground = Avalonia.Media.Brushes.White, FontSize = 16, Margin = new Thickness(0, 0, 0, 5) };
        var studentComboBox = new ComboBox
        {
            ItemsSource = departmentStudents.Select(s => $"{s.RegNum} - {s.FullName}").ToList(),
            Margin = new Thickness(0, 5, 0, 10),
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#346C73")),
            Foreground = Avalonia.Media.Brushes.White,
            BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#6A9BA6")),
            CornerRadius = new CornerRadius(15),
            Padding = new Thickness(15, 10),
            Height = 45,
            FontSize = 14
        };
        if (departmentStudents.Any())
        {
            studentComboBox.SelectedIndex = 0;
        }

        var staffLabel = new TextBlock { Text = "Преподаватель:", Foreground = Avalonia.Media.Brushes.White, FontSize = 16, Margin = new Thickness(0, 0, 0, 5) };
        var staffComboBox = new ComboBox
        {
            ItemsSource = departmentStaff.Select(s => $"{s.StaffId} - {s.FullName}").ToList(),
            Margin = new Thickness(0, 5, 0, 10),
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#346C73")),
            Foreground = Avalonia.Media.Brushes.White,
            BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#6A9BA6")),
            CornerRadius = new CornerRadius(15),
            Padding = new Thickness(15, 10),
            Height = 45,
            FontSize = 14
        };
        if (departmentStaff.Any())
        {
            staffComboBox.SelectedIndex = 0;
        }

        var dateLabel = new TextBlock { Text = "Дата экзамена:", Foreground = Avalonia.Media.Brushes.White, FontSize = 16, Margin = new Thickness(0, 0, 0, 5) };
        var datePicker = new DatePicker
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

        var classroomLabel = new TextBlock { Text = "Аудитория:", Foreground = Avalonia.Media.Brushes.White, FontSize = 16, Margin = new Thickness(0, 0, 0, 5) };
        var classroomTextBox = new TextBox
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
            Content = "Назначить",
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
            if (courseComboBox.SelectedIndex < 0 || studentComboBox.SelectedIndex < 0 || 
                staffComboBox.SelectedIndex < 0 || datePicker.SelectedDate == null || 
                string.IsNullOrWhiteSpace(classroomTextBox.Text))
            {
                return;
            }

            try
            {
                var courseIndex = courseComboBox.SelectedIndex;
                var studentIndex = studentComboBox.SelectedIndex;
                var staffIndex = staffComboBox.SelectedIndex;

                if (courseIndex < 0 || courseIndex >= departmentCourses.Count ||
                    studentIndex < 0 || studentIndex >= departmentStudents.Count ||
                    staffIndex < 0 || staffIndex >= departmentStaff.Count) return;

                var selectedCourse = departmentCourses[courseIndex];
                var selectedStudent = departmentStudents[studentIndex];
                var selectedStaff = departmentStaff[staffIndex];

                var examDate = DateOnly.FromDateTime(datePicker.SelectedDate.Value.Date);
                
                var today = DateOnly.FromDateTime(DateTime.Today);
                if (examDate <       today)
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
                        Text = "Экзамен не может быть назначен позже текущего дня",
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

                var existingExam = await App.DbContext.Exams
                    .FirstOrDefaultAsync(ex => ex.ExamDate == examDate && 
                                               ex.CourseId == selectedCourse.CourseId && 
                                               ex.RegNum == selectedStudent.RegNum);

                if (existingExam != null)
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
                        Text = "Экзамен с такими параметрами уже существует",
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

                var newExam = new Exam
                {
                    ExamDate = examDate,
                    CourseId = selectedCourse.CourseId,
                    RegNum = selectedStudent.RegNum,
                    StaffId = selectedStaff.StaffId,
                    Classroom = classroomTextBox.Text.Trim()
                };

                App.DbContext.Exams.Add(newExam);
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
                    Text = $"Ошибка при назначении экзамена: {ex.Message}",
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
            }
        };

        cancelButton.Click += (s, args) => dialog.Close();

        buttonPanel.Children.Add(okButton);
        buttonPanel.Children.Add(cancelButton);

        panel.Children.Add(titleLabel);
        panel.Children.Add(courseLabel);
        panel.Children.Add(courseComboBox);
        panel.Children.Add(studentLabel);
        panel.Children.Add(studentComboBox);
        panel.Children.Add(staffLabel);
        panel.Children.Add(staffComboBox);
        panel.Children.Add(dateLabel);
        panel.Children.Add(datePicker);
        panel.Children.Add(classroomLabel);
        panel.Children.Add(classroomTextBox);
        panel.Children.Add(buttonPanel);

        scrollViewer.Content = panel;
        border.Child = scrollViewer;
        mainGrid.Children.Add(border);
        dialog.Content = mainGrid;
        await dialog.ShowDialog(this.VisualRoot as Window);
    }

    private void DataExam_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _selectedExam = DataExam.SelectedItem as Exam;
    }

    private async void EditExamButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_selectedExam == null)
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
                Text = "Выберите экзамен для изменения",
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

        if (App.CurrentUser == null) return;

        // Загружаем связанные данные для экзамена
        await App.DbContext.Entry(_selectedExam)
            .Reference(e => e.Course)
            .LoadAsync();
        await App.DbContext.Entry(_selectedExam)
            .Reference(e => e.RegNumNavigation)
            .LoadAsync();
        await App.DbContext.Entry(_selectedExam)
            .Reference(e => e.Staff)
            .LoadAsync();

        var coursesQuery = App.DbContext.Courses.AsQueryable();
        if (_isDepartmentHead && !_isEngineer)
        {
            coursesQuery = coursesQuery.Where(c => c.DeptCode == App.CurrentUser.DeptCode);
        }
        var departmentCourses = await coursesQuery.ToListAsync();

        var studentsQuery = App.DbContext.Students
            .Include(s => s.ProgramCodeNavigation)
            .AsQueryable();
        if (_isDepartmentHead && !_isEngineer)
        {
            studentsQuery = studentsQuery.Where(s => s.ProgramCodeNavigation.DeptCode == App.CurrentUser.DeptCode);
        }
        var departmentStudents = await studentsQuery.ToListAsync();

        var staffQuery = App.DbContext.Staff
            .Where(s => s.Lecturer != null)
            .AsQueryable();
        if (_isDepartmentHead && !_isEngineer)
        {
            staffQuery = staffQuery.Where(s => s.DeptCode == App.CurrentUser.DeptCode);
        }
        var departmentStaff = await staffQuery.ToListAsync();

        var dialog = new Window
        {
            Title = "Изменить экзамен",
            Width = 600,
            Height = 850,
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
        var scrollViewer = new ScrollViewer { VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto };
        var panel = new StackPanel { Spacing = 10 };

        var titleLabel = new TextBlock
        {
            Text = "Изменить экзамен",
            FontSize = 24,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#A3C9D9")),
            Margin = new Thickness(0, 0, 0, 20)
        };

        var courseLabel = new TextBlock { Text = "Дисциплина:", Foreground = Avalonia.Media.Brushes.White, FontSize = 16, Margin = new Thickness(0, 0, 0, 5) };
        var courseComboBox = new ComboBox
        {
            ItemsSource = departmentCourses.Select(c => $"{c.CourseId} - {c.Title}").ToList(),
            Margin = new Thickness(0, 5, 0, 10),
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#346C73")),
            Foreground = Avalonia.Media.Brushes.White,
            BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#6A9BA6")),
            CornerRadius = new CornerRadius(15),
            Padding = new Thickness(15, 10),
            Height = 45,
            FontSize = 14
        };
        var courseIndex = departmentCourses.FindIndex(c => c.CourseId == _selectedExam.CourseId);
        if (courseIndex >= 0)
        {
            courseComboBox.SelectedIndex = courseIndex;
        }

        var studentLabel = new TextBlock { Text = "Студент:", Foreground = Avalonia.Media.Brushes.White, FontSize = 16, Margin = new Thickness(0, 0, 0, 5) };
        var studentComboBox = new ComboBox
        {
            ItemsSource = departmentStudents.Select(s => $"{s.RegNum} - {s.FullName}").ToList(),
            Margin = new Thickness(0, 5, 0, 10),
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#346C73")),
            Foreground = Avalonia.Media.Brushes.White,
            BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#6A9BA6")),
            CornerRadius = new CornerRadius(15),
            Padding = new Thickness(15, 10),
            Height = 45,
            FontSize = 14
        };
        var studentIndex = departmentStudents.FindIndex(s => s.RegNum == _selectedExam.RegNum);
        if (studentIndex >= 0)
        {
            studentComboBox.SelectedIndex = studentIndex;
        }

        var staffLabel = new TextBlock { Text = "Преподаватель:", Foreground = Avalonia.Media.Brushes.White, FontSize = 16, Margin = new Thickness(0, 0, 0, 5) };
        var staffComboBox = new ComboBox
        {
            ItemsSource = departmentStaff.Select(s => $"{s.StaffId} - {s.FullName}").ToList(),
            Margin = new Thickness(0, 5, 0, 10),
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#346C73")),
            Foreground = Avalonia.Media.Brushes.White,
            BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#6A9BA6")),
            CornerRadius = new CornerRadius(15),
            Padding = new Thickness(15, 10),
            Height = 45,
            FontSize = 14
        };
        var staffIndex = departmentStaff.FindIndex(s => s.StaffId == _selectedExam.StaffId);
        if (staffIndex >= 0)
        {
            staffComboBox.SelectedIndex = staffIndex;
        }

        var dateLabel = new TextBlock { Text = "Дата экзамена:", Foreground = Avalonia.Media.Brushes.White, FontSize = 16, Margin = new Thickness(0, 0, 0, 5) };
        var datePicker = new DatePicker
        {
            SelectedDate = _selectedExam.ExamDate.ToDateTime(TimeOnly.MinValue),
            Margin = new Thickness(0, 5, 0, 10),
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#346C73")),
            Foreground = Avalonia.Media.Brushes.White,
            BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#6A9BA6")),
            CornerRadius = new CornerRadius(15),
            Padding = new Thickness(15, 10),
            Height = 45,
            FontSize = 14
        };

        var classroomLabel = new TextBlock { Text = "Аудитория:", Foreground = Avalonia.Media.Brushes.White, FontSize = 16, Margin = new Thickness(0, 0, 0, 5) };
        var classroomTextBox = new TextBox
        {
            Text = _selectedExam.Classroom,
            Margin = new Thickness(0, 5, 0, 10),
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#346C73")),
            Foreground = Avalonia.Media.Brushes.White,
            BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#6A9BA6")),
            CornerRadius = new CornerRadius(15),
            Padding = new Thickness(15, 10),
            Height = 45,
            FontSize = 14
        };

        var gradeLabel = new TextBlock { Text = "Оценка (необязательно):", Foreground = Avalonia.Media.Brushes.White, FontSize = 16, Margin = new Thickness(0, 0, 0, 5) };
        var gradeTextBox = new TextBox
        {
            Text = _selectedExam.Grade?.ToString() ?? "",
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
            if (courseComboBox.SelectedIndex < 0 || studentComboBox.SelectedIndex < 0 || 
                staffComboBox.SelectedIndex < 0 || datePicker.SelectedDate == null || 
                string.IsNullOrWhiteSpace(classroomTextBox.Text))
            {
                return;
            }

            try
            {
                var courseIndex = courseComboBox.SelectedIndex;
                var studentIndex = studentComboBox.SelectedIndex;
                var staffIndex = staffComboBox.SelectedIndex;

                if (courseIndex < 0 || courseIndex >= departmentCourses.Count ||
                    studentIndex < 0 || studentIndex >= departmentStudents.Count ||
                    staffIndex < 0 || staffIndex >= departmentStaff.Count) return;

                var selectedCourse = departmentCourses[courseIndex];
                var selectedStudent = departmentStudents[studentIndex];
                var selectedStaff = departmentStaff[staffIndex];

                var examDate = DateOnly.FromDateTime(datePicker.SelectedDate.Value.Date);
                
                // Валидация: экзамен не может быть назначен позже текущего дня
                var today = DateOnly.FromDateTime(DateTime.Today);
                if (examDate > today)
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
                        Text = "Экзамен не может быть назначен позже текущего дня",
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

                // Проверяем, не существует ли уже экзамен с такими параметрами (кроме текущего)
                var existingExam = await App.DbContext.Exams
                    .FirstOrDefaultAsync(ex => ex.ExamDate == examDate && 
                                               ex.CourseId == selectedCourse.CourseId && 
                                               ex.RegNum == selectedStudent.RegNum &&
                                               !(ex.ExamDate == _selectedExam.ExamDate && 
                                                 ex.CourseId == _selectedExam.CourseId && 
                                                 ex.RegNum == _selectedExam.RegNum));

                if (existingExam != null)
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
                        Text = "Экзамен с такими параметрами уже существует",
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

                // Если изменился составной ключ, нужно удалить старую запись и создать новую
                if (_selectedExam.ExamDate != examDate || 
                    _selectedExam.CourseId != selectedCourse.CourseId || 
                    _selectedExam.RegNum != selectedStudent.RegNum)
                {
                    App.DbContext.Exams.Remove(_selectedExam);
                    await App.DbContext.SaveChangesAsync();

                    var newExam = new Exam
                    {
                        ExamDate = examDate,
                        CourseId = selectedCourse.CourseId,
                        RegNum = selectedStudent.RegNum,
                        StaffId = selectedStaff.StaffId,
                        Classroom = classroomTextBox.Text.Trim(),
                        Grade = int.TryParse(gradeTextBox.Text, out var grade) ? grade : null
                    };

                    App.DbContext.Exams.Add(newExam);
                }
                else
                {
                    // Обновляем существующую запись
                    _selectedExam.StaffId = selectedStaff.StaffId;
                    _selectedExam.Classroom = classroomTextBox.Text.Trim();
                    _selectedExam.Grade = int.TryParse(gradeTextBox.Text, out var grade) ? grade : null;
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
                    Text = $"Ошибка при изменении экзамена: {ex.Message}",
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
            }
        };

        cancelButton.Click += (s, args) => dialog.Close();

        buttonPanel.Children.Add(okButton);
        buttonPanel.Children.Add(cancelButton);

        panel.Children.Add(titleLabel);
        panel.Children.Add(courseLabel);
        panel.Children.Add(courseComboBox);
        panel.Children.Add(studentLabel);
        panel.Children.Add(studentComboBox);
        panel.Children.Add(staffLabel);
        panel.Children.Add(staffComboBox);
        panel.Children.Add(dateLabel);
        panel.Children.Add(datePicker);
        panel.Children.Add(classroomLabel);
        panel.Children.Add(classroomTextBox);
        panel.Children.Add(gradeLabel);
        panel.Children.Add(gradeTextBox);
        panel.Children.Add(buttonPanel);

        scrollViewer.Content = panel;
        border.Child = scrollViewer;
        mainGrid.Children.Add(border);
        dialog.Content = mainGrid;
        await dialog.ShowDialog(this.VisualRoot as Window);
    }

    private async void DeleteExamButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_selectedExam == null)
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
                Text = "Выберите экзамен для удаления",
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
            App.DbContext.Exams.Remove(_selectedExam);
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

    private async void SetGradeButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_selectedExam == null)
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
                Text = "Выберите экзамен для выставления оценки",
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

        if (App.CurrentUser == null) return;

        // Проверяем, что экзамен принадлежит текущему преподавателю
        if (_selectedExam.StaffId != App.CurrentUser.StaffId)
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
                Text = "Вы можете ставить оценку только за свои экзамены",
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
            await errorDialog.ShowDialog(this.VisualRoot as Window);
            return;
        }

        // Загружаем связанные данные для отображения
        await App.DbContext.Entry(_selectedExam)
            .Reference(e => e.Course)
            .LoadAsync();
        await App.DbContext.Entry(_selectedExam)
            .Reference(e => e.RegNumNavigation)
            .LoadAsync();

        var dialog = new Window
        {
            Title = "Поставить оценку",
            Width = 500,
            Height = 400,
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
        var panel = new StackPanel { Spacing = 15 };

        var titleLabel = new TextBlock
        {
            Text = "Поставить оценку за экзамен",
            FontSize = 24,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#A3C9D9")),
            Margin = new Thickness(0, 0, 0, 20),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };

        var examInfoLabel = new TextBlock
        {
            Text = $"Дисциплина: {_selectedExam.Course?.Title ?? "Неизвестно"}\n" +
                   $"Студент: {_selectedExam.RegNumNavigation?.FullName ?? "Неизвестно"}\n" +
                   $"Дата: {_selectedExam.ExamDate:dd.MM.yyyy}",
            Foreground = Avalonia.Media.Brushes.White,
            FontSize = 14,
            Margin = new Thickness(0, 0, 0, 10),
            TextWrapping = Avalonia.Media.TextWrapping.Wrap
        };

        var gradeLabel = new TextBlock 
        { 
            Text = "Оценка:", 
            Foreground = Avalonia.Media.Brushes.White, 
            FontSize = 16, 
            Margin = new Thickness(0, 10, 0, 5) 
        };
        var gradeTextBox = new TextBox
        {
            Text = _selectedExam.Grade?.ToString() ?? "",
            Margin = new Thickness(0, 5, 0, 10),
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#346C73")),
            Foreground = Avalonia.Media.Brushes.White,
            BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#6A9BA6")),
            CornerRadius = new CornerRadius(15),
            Padding = new Thickness(15, 10),
            Height = 45,
            FontSize = 14,
            Watermark = "Введите оценку (2-5)"
        };

        var buttonPanel = new StackPanel 
        { 
            Orientation = Avalonia.Layout.Orientation.Horizontal, 
            Spacing = 15, 
            Margin = new Thickness(0, 25, 0, 0), 
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center 
        };
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
            try
            {
                var gradeText = gradeTextBox.Text?.Trim() ?? "";
                
                if (string.IsNullOrWhiteSpace(gradeText))
                {
                    // Если оценка пустая, устанавливаем null
                    _selectedExam.Grade = null;
                }
                else
                {   
                    if (!int.TryParse(gradeText, out var grade) || grade < 2 || grade > 5)
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
                            Text = "Оценка должна быть числом от 2 до 5",
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
                        return;
                    }

                    _selectedExam.Grade = grade;
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
                    Text = $"Ошибка при сохранении оценки: {ex.Message}",
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
            }
        };

        cancelButton.Click += (s, args) => dialog.Close();

        buttonPanel.Children.Add(okButton);
        buttonPanel.Children.Add(cancelButton);

        panel.Children.Add(titleLabel);
        panel.Children.Add(examInfoLabel);
        panel.Children.Add(gradeLabel);
        panel.Children.Add(gradeTextBox);
        panel.Children.Add(buttonPanel);

        border.Child = panel;
        mainGrid.Children.Add(border);
        dialog.Content = mainGrid;
        await dialog.ShowDialog(this.VisualRoot as Window);
    }
}
   

