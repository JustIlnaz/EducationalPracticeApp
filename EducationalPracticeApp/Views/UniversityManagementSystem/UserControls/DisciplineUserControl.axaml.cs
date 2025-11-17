using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

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
        var allDiscipline= App.DbContext.Courses.ToList();
        DataDiscipline.ItemsSource = allDiscipline;
    }
}