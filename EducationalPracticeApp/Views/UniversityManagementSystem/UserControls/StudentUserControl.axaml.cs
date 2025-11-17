using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;

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
}
