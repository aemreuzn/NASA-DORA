using CommunityToolkit.Mvvm.ComponentModel;

namespace NasaAstronautTraining.Models;

public partial class CelebrationMedalSlot : ObservableObject
{
    public int StageId { get; init; }
    public string StageNumberText => $"AŞAMA 0{StageId}";

    [ObservableProperty]
    private string _stageTitle = string.Empty;

    [ObservableProperty]
    private string _emoji = "★";

    [ObservableProperty]
    private string _badgeColor = "#B45309";

    [ObservableProperty]
    private string _badgeBg = "#FEF3C7";

    [ObservableProperty]
    private string _badgeBorder = "#F59E0B";

    [ObservableProperty]
    private string _medalLabel = "Altın Madalya";

    [ObservableProperty]
    private bool _isVisible;

    [ObservableProperty]
    private double _opacity;

    public CelebrationMedalSlot(int stageId, string stageTitle = "")
    {
        StageId = stageId;
        _stageTitle = stageTitle;
    }
}
