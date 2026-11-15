using CommunityToolkit.Mvvm.ComponentModel;

namespace NasaAstronautTraining.Models;

public partial class StageMedalItem : ObservableObject
{
    public int StageId { get; init; }
    public string StageNumberBadge => StageId.ToString("00");
    public string StageTitle { get; init; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MedalEmoji))]
    [NotifyPropertyChangedFor(nameof(MedalName))]
    [NotifyPropertyChangedFor(nameof(MedalColor))]
    [NotifyPropertyChangedFor(nameof(MedalBadgeText))]
    [NotifyPropertyChangedFor(nameof(MedalBadgeBg))]
    [NotifyPropertyChangedFor(nameof(MedalBadgeBorder))]
    [NotifyPropertyChangedFor(nameof(MedalBadgeFg))]
    [NotifyPropertyChangedFor(nameof(HasMedal))]
    private MadalyaTuru _medal = MadalyaTuru.Yok;

    public string MedalEmoji => Medal.ToEmoji();
    public string MedalName => Medal.ToName();
    public string MedalColor => Medal.ToHexColor();

    public bool HasMedal => Medal != MadalyaTuru.Yok;

    public string MedalBadgeText => Medal switch
    {
        MadalyaTuru.Altin => "ALTIN MADALYA",
        MadalyaTuru.Gumus => "GÜMÜŞ MADALYA",
        MadalyaTuru.Bronz => "BRONZ MADALYA",
        _ => "BEKLİYOR"
    };

    public string MedalBadgeBg => Medal switch
    {
        MadalyaTuru.Altin => "#FEF3C7",
        MadalyaTuru.Gumus => "#F1F5F9",
        MadalyaTuru.Bronz => "#FFEDD5",
        _ => "#F8FAFC"
    };

    public string MedalBadgeBorder => Medal switch
    {
        MadalyaTuru.Altin => "#F59E0B",
        MadalyaTuru.Gumus => "#94A3B8",
        MadalyaTuru.Bronz => "#EA580C",
        _ => "#CBD5E1"
    };

    public string MedalBadgeFg => Medal switch
    {
        MadalyaTuru.Altin => "#B45309",
        MadalyaTuru.Gumus => "#334155",
        MadalyaTuru.Bronz => "#9A3412",
        _ => "#64748B"
    };

    public StageMedalItem(int stageId, string stageTitle, MadalyaTuru medal = MadalyaTuru.Yok)
    {
        StageId = stageId;
        StageTitle = stageTitle;
        _medal = medal;
    }
}
