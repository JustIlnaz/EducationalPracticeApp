using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace EducationalPracticeApp;

public partial class ExamUserControl : UserControl
{
    public ExamUserControl()
    {
        InitializeComponent();
        App.DbContext.Students.ToList();
        LoadData();
       
    }
    public void LoadData()
    {
        var allExam = App.DbContext.Exams.ToList();
        DataExam.ItemsSource = allExam;
        
    }
}