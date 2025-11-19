using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using EducationalPracticeApp.Views;

namespace EducationalPracticeApp;

public partial class EngineerWindow : Window
{
    public EngineerWindow()
    {
        InitializeComponent();
    }
    private void Exit(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        App.CurrentUser = null;

        var parent = this.VisualRoot as Window;
        if (parent != null)
        {
            var mainwindow = new MainWindow();
            mainwindow.Show();
            parent.Close();
        }
    }
}