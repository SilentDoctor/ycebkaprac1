using ProductionCaptchaSystem.Services;
using ProductionCaptchaSystem.Views;
using System.Windows;

namespace ProductionCaptchaSystem;

public partial class MainWindow : Window
{
    public MainWindow(AuthenticationService authService)
    {
        InitializeComponent();
        MainFrame.Navigate(new AuthenticationView(authService));
    }
}
