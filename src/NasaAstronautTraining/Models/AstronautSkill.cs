using CommunityToolkit.Mvvm.ComponentModel;

namespace NasaAstronautTraining.Models;

public partial class AstronautSkill : ObservableObject
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ShortCode { get; init; } = string.Empty;
    public string IconText { get; init; } = string.Empty;
    public string EffectDesc { get; init; } = string.Empty;
    public string StageRequired { get; init; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BadgeBg))]
    [NotifyPropertyChangedFor(nameof(BadgeBorder))]
    [NotifyPropertyChangedFor(nameof(BadgeFg))]
    [NotifyPropertyChangedFor(nameof(BadgeOpacity))]
    [NotifyPropertyChangedFor(nameof(StatusLabel))]
    private bool _isUnlocked;

    public string BadgeBg => IsUnlocked ? "#1E3A8A" : "#141E33";
    public string BadgeBorder => IsUnlocked ? "#38BDF8" : "#2A3A54";
    public string BadgeFg => IsUnlocked ? "#FDE047" : "#94A3B8";
    public double BadgeOpacity => IsUnlocked ? 1.0 : 0.85;
    public string StatusLabel => IsUnlocked ? "✓ Aktif" : "Kilitli";
}
