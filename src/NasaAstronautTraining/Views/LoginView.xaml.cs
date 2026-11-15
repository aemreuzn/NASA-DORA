using System.Windows;
using System.Windows.Controls;
using NasaAstronautTraining.ViewModels;

namespace NasaAstronautTraining.Views;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
    }

    private void LanguageButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm)
        {
            vm.ToggleLanguageCommand.Execute(null);
        }
    }

    private void QuickUser_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string userTag } && DataContext is LoginViewModel vm)
        {
            var parts = userTag.Split(':');
            string username = parts[0];
            string password = parts.Length > 1 ? parts[1] : "1969";

            vm.SetQuickProfile(username, password);
            SecretCodeBox.Password = password;
            vm.UpdatePasswordStrength(password);
        }
    }

    private void SecretCodeBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm)
        {
            vm.UpdatePasswordStrength(SecretCodeBox.Password);
        }
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm)
        {
            vm.LoginCommand.Execute(SecretCodeBox.Password);
        }
    }

    private void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm)
        {
            vm.RegisterCommand.Execute(SecretCodeBox.Password);
        }
    }

    private void ToggleFullscreen_Click(object sender, RoutedEventArgs e)
    {
        Window? parentWindow = Window.GetWindow(this);
        if (parentWindow is null) return;

        if (parentWindow.WindowState == WindowState.Maximized && parentWindow.WindowStyle == WindowStyle.None)
        {
            parentWindow.WindowStyle = WindowStyle.SingleBorderWindow;
            parentWindow.WindowState = WindowState.Normal;
        }
        else
        {
            parentWindow.WindowStyle = WindowStyle.None;
            parentWindow.WindowState = WindowState.Maximized;
        }
    }
}
