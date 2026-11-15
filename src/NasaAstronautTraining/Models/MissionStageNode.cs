using CommunityToolkit.Mvvm.ComponentModel;

namespace NasaAstronautTraining.Models;

public enum StageStatus
{
    Completed,
    Active,
    Locked
}

/// <summary>
/// Görev Yörüngesi (Mission Orbit Path) üzerindeki 6 aşama durağını temsil eder.
/// </summary>
public partial class MissionStageNode : ObservableObject
{
    public int Number { get; init; }
    public string NumberBadge => Number.ToString("00");
    public string ShortName { get; init; } = string.Empty;
    public string TitleTr { get; init; } = string.Empty;
    public string TitleEn { get; init; } = string.Empty;
    public string SubtitleTr { get; init; } = string.Empty;
    public string SubtitleEn { get; init; } = string.Empty;
    public string DescriptionTr { get; init; } = string.Empty;
    public string DescriptionEn { get; init; } = string.Empty;
    public string ObjectiveTr { get; init; } = string.Empty;
    public string ObjectiveEn { get; init; } = string.Empty;

    public double X { get; init; }
    public double Y { get; init; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCompleted))]
    [NotifyPropertyChangedFor(nameof(IsActive))]
    [NotifyPropertyChangedFor(nameof(IsLocked))]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    [NotifyPropertyChangedFor(nameof(StatusColorHex))]
    [NotifyPropertyChangedFor(nameof(NodeFillHex))]
    private StageStatus _status;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasMedal))]
    [NotifyPropertyChangedFor(nameof(MedalBadgeText))]
    [NotifyPropertyChangedFor(nameof(MedalShortText))]
    [NotifyPropertyChangedFor(nameof(MedalBgHex))]
    [NotifyPropertyChangedFor(nameof(MedalBorderHex))]
    [NotifyPropertyChangedFor(nameof(MedalFgHex))]
    private MadalyaTuru _medal = MadalyaTuru.Yok;

    [ObservableProperty]
    private bool _isRoundCompleted;

    public double OxygenPercent { get; init; } = 98;
    public double PressureKpa { get; init; } = 101.3;
    public string GameUrl { get; init; } = string.Empty;

    [ObservableProperty]
    private bool _isSelected;

    public bool IsCompleted => Status == StageStatus.Completed;
    public bool IsActive => Status == StageStatus.Active;
    public bool IsLocked => Status == StageStatus.Locked;
    public bool HasMedal => Medal != MadalyaTuru.Yok;

    public string StatusText => Status switch
    {
        StageStatus.Completed => "TAMAMLANDI",
        StageStatus.Active => "AKTİF GÖREV",
        _ => "GÖREVE HAZIR"
    };

    public string StatusColorHex => Status switch
    {
        StageStatus.Completed => "#16A34A",
        StageStatus.Active => "#2563EB",
        _ => "#64748B"
    };

    public string NodeFillHex => Status switch
    {
        StageStatus.Completed => "#16A34A",
        StageStatus.Active => "#2563EB",
        _ => "#475569"
    };

    public string MedalBadgeText => Medal switch
    {
        MadalyaTuru.Altin => "ALTIN MADALYA",
        MadalyaTuru.Gumus => "GÜMÜŞ MADALYA",
        MadalyaTuru.Bronz => "BRONZ MADALYA",
        _ => "HENÜZ MADALYA YOK"
    };

    public string MedalShortText => Medal switch
    {
        MadalyaTuru.Altin => "★ ALTIN",
        MadalyaTuru.Gumus => "★ GÜMÜŞ",
        MadalyaTuru.Bronz => "★ BRONZ",
        _ => string.Empty
    };

    public string MedalBgHex => Medal switch
    {
        MadalyaTuru.Altin => "#FEF3C7",
        MadalyaTuru.Gumus => "#F1F5F9",
        MadalyaTuru.Bronz => "#FFEDD5",
        _ => "#F8FAFC"
    };

    public string MedalBorderHex => Medal switch
    {
        MadalyaTuru.Altin => "#F59E0B",
        MadalyaTuru.Gumus => "#94A3B8",
        MadalyaTuru.Bronz => "#EA580C",
        _ => "#CBD5E1"
    };

    public string MedalFgHex => Medal switch
    {
        MadalyaTuru.Altin => "#B45309",
        MadalyaTuru.Gumus => "#334155",
        MadalyaTuru.Bronz => "#9A3412",
        _ => "#64748B"
    };
}
