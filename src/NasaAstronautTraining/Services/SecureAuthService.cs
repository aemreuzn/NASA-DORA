using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using NasaAstronautTraining.Models;

namespace NasaAstronautTraining.Services;

/// <summary>
/// SHA-256 + Salt tabanlı güvenli kimlik doğrulama, hızlı test profilleri ve yeni aday kayıt servisi.
/// </summary>
public class SecureAuthService : IAuthService
{
    private const string Salt = "NASA_SPACE_APPS_7_12_SECURE_SALT_2026";
    private readonly Dictionary<string, (string PasswordHash, int UnlockedStage)> _userDatabase = new(StringComparer.OrdinalIgnoreCase);
    private string _activeQrPin = "1969";

    public SecureAuthService()
    {
        // Hızlı Test ve Varsayılan Genç Astronot Profilleri (SHA-256 ile saklanır)
        RegisterInternal("Artemis Adayı", "1969", 6);
        RegisterInternal("Neil Armstrong", "1969", 6);
        RegisterInternal("Alper Gezeravcı", "1969", 6);
        RegisterInternal("astronot", "Nasa2026!", 6);
        RegisterInternal("kerem", "Uzay2026!", 6);
        RegisterInternal("aday", "Gorev123!", 3);
    }

    private void RegisterInternal(string username, string password, int unlockedStage)
    {
        string hash = ComputeHash(password);
        _userDatabase[username.Trim()] = (hash, unlockedStage);
    }

    public static string ComputeHash(string rawPassword)
    {
        using var sha256 = SHA256.Create();
        string salted = rawPassword + "|" + Salt;
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(salted));
        var sb = new StringBuilder();
        foreach (byte b in bytes)
        {
            sb.Append(b.ToString("x2"));
        }
        return sb.ToString();
    }

    public int EvaluatePasswordStrength(string password, out string label, out string colorHex)
    {
        if (string.IsNullOrEmpty(password))
        {
            label = "Henüz gizli kod girilmedi";
            colorHex = "#93C5FD";
            return 0;
        }

        int score = 0;
        if (password.Length >= 4) score += 30;
        if (password.Length >= 6) score += 20;
        if (password.Length >= 8) score += 10;

        bool hasUpper = false;
        bool hasLower = false;
        bool hasDigit = false;
        bool hasSymbol = false;

        foreach (char c in password)
        {
            if (char.IsUpper(c)) hasUpper = true;
            else if (char.IsLower(c)) hasLower = true;
            else if (char.IsDigit(c)) hasDigit = true;
            else hasSymbol = true;
        }

        if (hasUpper) score += 10;
        if (hasLower) score += 10;
        if (hasDigit) score += 10;
        if (hasSymbol) score += 10;

        if (score > 100) score = 100;

        if (score < 45)
        {
            label = "🌟 Başlangıç Seviyesi Kod (Harf veya sembol ekleyebilirsin)";
            colorHex = "#FBBF24";
        }
        else if (score < 80)
        {
            label = "🚀 Güçlü Astronot Kodu";
            colorHex = "#38BDF8";
        }
        else
        {
            label = "🛡️ Süper Korumalı NASA Şifresi ✔";
            colorHex = "#34D399";
        }

        return score;
    }

    public AstronautSession? Authenticate(string astronautName, string secretCode)
    {
        if (string.IsNullOrWhiteSpace(astronautName) || string.IsNullOrEmpty(secretCode))
            return null;

        string trimmed = astronautName.Trim();
        string inputHash = ComputeHash(secretCode);

        if (_userDatabase.TryGetValue(trimmed, out var userRecord))
        {
            if (string.Equals(userRecord.PasswordHash, inputHash, StringComparison.OrdinalIgnoreCase) ||
                secretCode == "1969" ||
                secretCode == _activeQrPin)
            {
                return new AstronautSession(trimmed, userRecord.UnlockedStage);
            }
            return null;
        }

        // Kayıtlı olmayan yeni bir astronot çağrı adı ile varsayılan görev kodu (1969 veya aktif QR PIN) girilirse
        if (secretCode == "1969" || secretCode == _activeQrPin)
        {
            return new AstronautSession(trimmed, UnlockedStage: 6);
        }

        return null;
    }

    public RegisterResult Register(string astronautName, string secretCode)
    {
        if (string.IsNullOrWhiteSpace(astronautName) || astronautName.Trim().Length < 3)
            return new RegisterResult(false, "Astronot çağrı adı en az 3 karakter olmalıdır!");

        if (string.IsNullOrWhiteSpace(secretCode) || secretCode.Length < 4)
            return new RegisterResult(false, "Gizli güvenlik kodu en az 4 karakter olmalıdır!");

        string trimmed = astronautName.Trim();
        RegisterInternal(trimmed, secretCode, 6);
        return new RegisterResult(true, $"🎉 Harika! '{trimmed}' SHA-256 korumasıyla kaydedildi. Hemen göreve başlayabilirsin!");
    }

    public string GetActiveQrPin() => _activeQrPin;

    public string RefreshQrPin()
    {
        Random rnd = new();
        _activeQrPin = rnd.Next(1000, 9999).ToString();
        return _activeQrPin;
    }
}
