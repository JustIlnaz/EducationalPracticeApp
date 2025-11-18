using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EducationalPracticeApp;

public partial class StudentUserControl : UserControl
{
    public StudentUserControl()
    {
        InitializeComponent();
        LoadData();
    }

    public void LoadData()
    {
        var allStudents = App.DbContext.Students1.ToList();
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
        var allStudents = App.DbContext.Students1.ToList();

        var filter = SurnameFilterTextBox.Text ?? string.Empty;
        var query = allStudents.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            query = query.Where(s => s.FullName.Contains(filter, StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            query = allStudents;
        }

        DataStudent.ItemsSource = query.ToList();
    }

}



