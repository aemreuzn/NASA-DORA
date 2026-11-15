using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NasaAstronautTraining.Models;
using NasaAstronautTraining.Services;

namespace NasaAstronautTraining.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly Action<AstronautSession> _onLoginSuccess;

    [ObservableProperty]
    private string _astronautName = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _selectedLanguage = "TR";

    [ObservableProperty]
    private double _passwordStrengthValue = 0;

    [ObservableProperty]
    private string _passwordStrengthLabel = "Şifre girilmedi";

    [ObservableProperty]
    private string _passwordStrengthColor = "#64748B";

    public LoginViewModel(IAuthService authService, Action<AstronautSession> onLoginSuccess)
    {
        _authService = authService;
        _onLoginSuccess = onLoginSuccess;
    }

    public void UpdatePasswordStrength(string password)
    {
        int score = _authService.EvaluatePasswordStrength(password, out string label, out string color);
        PasswordStrengthValue = score;
        PasswordStrengthLabel = label;
        PasswordStrengthColor = color;
    }

    public void SetQuickProfile(string username, string password)
    {
        AstronautName = username;
        ErrorMessage = string.Empty;
        StatusMessage = $"{username} seçildi.";
    }

    [RelayCommand]
    private void ToggleLanguage()
    {
        SelectedLanguage = SelectedLanguage == "TR" ? "EN" : "TR";
    }

    [RelayCommand]
    private void Register(string? secretCode)
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(AstronautName))
        {
            ErrorMessage = "Kullanıcı adı giriniz.";
            return;
        }

        if (string.IsNullOrWhiteSpace(secretCode))
        {
            ErrorMessage = "Şifre giriniz.";
            return;
        }

        var res = _authService.Register(AstronautName, secretCode);
        if (!res.Success)
        {
            ErrorMessage = res.Message;
            return;
        }

        StatusMessage = "Kayıt başarılı. Giriş yapabilirsiniz.";
    }

    [RelayCommand]
    private void Login(string? secretCode)
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(AstronautName))
        {
            ErrorMessage = "Lütfen kullanıcı adınızı girin.";
            return;
        }

        if (string.IsNullOrWhiteSpace(secretCode))
        {
            ErrorMessage = "Lütfen şifrenizi girin.";
            return;
        }

        var session = _authService.Authenticate(AstronautName, secretCode);
        if (session is null)
        {
            ErrorMessage = "Kullanıcı adı veya şifre hatalı.";
            return;
        }

        _onLoginSuccess(session);
    }
}
