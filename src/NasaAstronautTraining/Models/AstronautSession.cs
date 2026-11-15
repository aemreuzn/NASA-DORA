namespace NasaAstronautTraining.Models;

/// <summary>
/// Represents an authenticated astronaut trainee session.
/// </summary>
public record AstronautSession(string AstronautName, int UnlockedStage = 1);
