using NasaAstronautTraining.Models;

namespace NasaAstronautTraining.Services;

public record RegisterResult(bool Success, string Message);

/// <summary>
/// Contract for astronaut authentication using Astronaut Name and Secret Code.
/// </summary>
public interface IAuthService
{
    AstronautSession? Authenticate(string astronautName, string secretCode);
    RegisterResult Register(string astronautName, string secretCode);
    int EvaluatePasswordStrength(string password, out string label, out string colorHex);
    string GetActiveQrPin();
    string RefreshQrPin();
}
