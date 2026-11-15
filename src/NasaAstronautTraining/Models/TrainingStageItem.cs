using CommunityToolkit.Mvvm.ComponentModel;

namespace NasaAstronautTraining.Models;

/// <summary>
/// Represents a single stage tab in the astronaut training sidebar.
/// </summary>
public partial class TrainingStageItem : ObservableObject
{
    public int Id { get; init; }
    public string Title { get; init; }
    public string Subtitle { get; init; }
    public string Description { get; init; }
    public string MissionTask { get; init; }
    public bool IsSimulationStage => Id == 6;
    public string StageIcon => IsSimulationStage ? "🚀" : "📖";
    public string ShortTitle => Title.Contains(':')
        ? Title.Split(':')[1].Trim()
        : (Title.Contains(". ") ? Title[(Title.IndexOf(". ") + 2)..].Trim() : Title);

    [ObservableProperty]
    private bool _isUnlocked;

    [ObservableProperty]
    private bool _isCompleted;

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MedalEmoji))]
    private MadalyaTuru _medal = MadalyaTuru.Yok;

    public string MedalEmoji => Medal.ToEmoji();

    public TrainingStageItem(int id, string title, string subtitle, string description, string missionTask, bool isUnlocked = true, bool isCompleted = false)
    {
        Id = id;
        Title = title;
        Subtitle = subtitle;
        Description = description;
        MissionTask = missionTask;
        _isUnlocked = isUnlocked;
        _isCompleted = isCompleted;
    }
}
