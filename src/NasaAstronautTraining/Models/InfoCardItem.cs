using CommunityToolkit.Mvvm.ComponentModel;

namespace NasaAstronautTraining.Models;

public partial class InfoCardItem : ObservableObject
{
    public int Index { get; init; } // 1..5
    public string NumberBadge => Index.ToString("00");
    public string StageTitle { get; init; } = string.Empty;
    public string Subtitle { get; init; } = string.Empty;
    public string EmojiIcon { get; init; } = string.Empty;
    public string VectorPathData { get; init; } = string.Empty;
    public string MainTitle { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string DidYouKnow { get; init; } = string.Empty;
    public string SourceText { get; init; } = string.Empty;
    public bool IsEarthCard { get; init; }

    [ObservableProperty]
    private bool _isRead;

    [ObservableProperty]
    private bool _isSelected;
}
