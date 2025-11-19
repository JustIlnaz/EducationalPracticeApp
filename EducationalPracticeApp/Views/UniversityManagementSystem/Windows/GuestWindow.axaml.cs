using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using EducationalPracticeApp.Views;

namespace EducationalPracticeApp;

public partial class GuestWindow : Window
{
    public GuestWindow()
    {
        InitializeComponent();
    }
    private void Exit(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var parent = this.VisualRoot as Window;
        if (parent != null)
        {
            var mainwindow = new MainWindow();
            mainwindow.Show();
            parent.Close();
        }
    }
}