using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Linq;

namespace EducationalPracticeApp;

public partial class DisciplineUserControl : UserControl
{
    public DisciplineUserControl()
    {
        InitializeComponent();
        LoadData();
    }

    public void LoadData()
    {
        var allDiscipline = App.DbContext.Courses.ToList();
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
        var allDiscipline = App.DbContext.Courses.ToList();

        var filter = TitleNameFilterTextBox.Text ?? string.Empty;
        var query = allDiscipline.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            query = query.Where(s => s.Title.Contains(filter, StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            query = allDiscipline;
        }

        DataDiscipline.ItemsSource = query.ToList();
    }
}