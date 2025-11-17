using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Linq;

namespace EducationalPracticeApp;

public partial class EmployeeUserControl : UserControl
{
    public EmployeeUserControl()
    {
        InitializeComponent();
        LoadData();
    }
    public void LoadData()
    {
        var allEmployee = App.DbContext.Staff.ToList();
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
        var allStudents = App.DbContext.Staff.ToList();

        var filter = FullNameFilterTextBox.Text ?? string.Empty;
        var query = allStudents.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            query = query.Where(s => s.FullName.Contains(filter, StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            query = allStudents;
        }

        DataEmployee.ItemsSource = query.ToList();
    }
}