using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

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
}