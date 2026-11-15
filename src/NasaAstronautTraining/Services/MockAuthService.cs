using NasaAstronautTraining.Models;

namespace NasaAstronautTraining.Services;

/// <summary>
/// Fallback mock authentication service delegating to SecureAuthService for compatibility.
/// </summary>
public class MockAuthService : IAuthService
{
    private readonly SecureAuthService _inner = new();

    public AstronautSession? Authenticate(string astronautName, string secretCode)
        => _inner.Authenticate(astronautName, secretCode);

    public RegisterResult Register(string astronautName, string secretCode)
        => _inner.Register(astronautName, secretCode);

    public int EvaluatePasswordStrength(string password, out string label, out string colorHex)
        => _inner.EvaluatePasswordStrength(password, out label, out colorHex);

    public string GetActiveQrPin() => _inner.GetActiveQrPin();

    public string RefreshQrPin() => _inner.RefreshQrPin();
}
