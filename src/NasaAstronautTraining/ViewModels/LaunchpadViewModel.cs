using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Media;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NasaAstronautTraining.Models;

namespace NasaAstronautTraining.ViewModels;

/// <summary>Kısa "Biliyor muydun?" uzay bilgisi (TR/EN) ve vektör ikonu.</summary>
public sealed record SpaceFact(string Tr, string En, Geometry Icon);

/// <summary>Bilgi kartı altındaki sayfalama noktası.</summary>
public partial class FactDot : ObservableObject
{
    [ObservableProperty]
    private bool _isActive;
}

/// <summary>Biniş kartındaki görev rolü karosu.</summary>
public partial class RoleOption : ObservableObject
{
    private readonly string _titleTr;
    private readonly string _titleEn;

    public RoleOption(string key, string titleTr, string titleEn, Geometry icon)
    {
        Key = key;
        _titleTr = titleTr;
        _titleEn = titleEn;
        Icon = icon;
        _title = titleTr;
    }

    public string Key { get; }
    public Geometry Icon { get; }

    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private bool _isSelected;

    public void ApplyLanguage(bool english) => Title = english ? _titleEn : _titleTr;
}

/// <summary>
/// Girişten sonra açılan "Launchpad" (Karşılama &amp; Biniş) ekranı.
/// Hoş geldin bloğu, "Biliyor muydun?" bilgi kartları, canlı telemetri şeridi ve biniş kartı.
/// </summary>
public partial class LaunchpadViewModel : ObservableObject
{
    private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");

    private readonly Action<AstronautSession, string> _onStartMission;
    private readonly DispatcherTimer _timer;
    private readonly Random _random = new();
    private int _secondsSinceFactChange;
    private int _factIndex;

    public AstronautSession Session { get; }
    public ObservableCollection<SpaceFact> Facts { get; }
    public ObservableCollection<FactDot> FactDots { get; }
    public ObservableCollection<RoleOption> Roles { get; }

    /// <summary>Giriş yapan hesabın adı – "Hoş geldin!" altında gösterilir.</summary>
    public string AccountName => Session.AstronautName;

    [ObservableProperty]
    private string _callSign;

    [ObservableProperty]
    private SpaceFact _currentFact;

    [ObservableProperty]
    private bool _isEnglish;

    [ObservableProperty]
    private string _issSpeed = "27.600";

    [ObservableProperty]
    private string _missionClock = "00:00:00";

    [ObservableProperty]
    private string _callSignError = string.Empty;

    public LaunchpadViewModel(AstronautSession session, Action<AstronautSession, string> onStartMission)
    {
        Session = session;
        _onStartMission = onStartMission;
        _callSign = session.AstronautName;

        Facts = new ObservableCollection<SpaceFact>
        {
            new("Astronotlar uzayda günde 16 kez gün doğumu görür!",
                "Astronauts see 16 sunrises every day in space!",
                Icon("M3,18 H21 M6.5,18 A5.5,5.5 0 0 1 17.5,18 M12,5 V8.5 M4.9,10.4 L7.1,12.4 M19.1,10.4 L16.9,12.4 M7,21.5 H17")),
            new("Uzayda omurga uzar; astronotların boyu 5 cm'ye kadar artabilir!",
                "Without gravity, astronauts can grow up to 5 cm taller!",
                Icon("M12,3 V21 M8,7 L12,3 L16,7 M8,17 L12,21 L16,17 M4,12 H7 M17,12 H20")),
            new("Mars'taki Olympus Dağı, Everest'ten yaklaşık 3 kat yüksektir!",
                "Olympus Mons on Mars is about 3 times taller than Everest!",
                Icon("M2,20 L9,8 L13,14 L16,10 L22,20 Z M7.3,11 L9,12.6 L10.7,11")),
            new("Ay'da rüzgâr yok; ayak izleri milyonlarca yıl kalabilir!",
                "There is no wind on the Moon, so footprints can last for millions of years!",
                Icon("M20,14.5 A8.5,8.5 0 1 1 9.5,4 A6.5,6.5 0 0 0 20,14.5 Z")),
            new("Astronotlar uzay yürüyüşüne dev bir havuzda hazırlanır!",
                "Astronauts train for spacewalks in a giant swimming pool!",
                Icon("M2,8 C5,6 7,10 10,8 C13,6 15,10 18,8 C20,7 21,8 22,8 M2,13 C5,11 7,15 10,13 C13,11 15,15 18,13 C20,12 21,13 22,13 M2,18 C5,16 7,20 10,18 C13,16 15,20 18,18 C20,17 21,18 22,18")),
        };

        FactDots = new ObservableCollection<FactDot>(Facts.Select(_ => new FactDot()));

        Roles = new ObservableCollection<RoleOption>
        {
            new("pilot", "PİLOT", "PILOT",
                Icon("M12,2.5 C15.5,5.5 16.5,10 15,15 H9 C7.5,10 8.5,5.5 12,2.5 Z M9,15 L6,18.5 H9.5 M15,15 L18,18.5 H14.5 M10.5,18.5 L12,21.5 L13.5,18.5 M13.6,9 A1.6,1.6 0 1 1 10.4,9 A1.6,1.6 0 1 1 13.6,9 Z")),
            new("engineer", "MÜHENDİS", "ENGINEER",
                Icon("M3,21 H13 M5,21 V18 H11 V21 M8,18 L10,12.5 M10,12.5 L16,8.5 M16,8.5 L19.5,6.5 M16,8.5 L19.5,11 M11.4,12.5 A1.4,1.4 0 1 1 8.6,12.5 A1.4,1.4 0 1 1 11.4,12.5 Z")),
            new("explorer", "KAŞİF", "EXPLORER",
                Icon("M3.5,13.5 L16,7 L18.2,11.2 L5.7,17.7 Z M16,7 L19.5,5.2 L21.7,9.4 L18.2,11.2 M11,14.5 L8,21 M12,14.5 L15,21 M2.5,13.9 L4.6,18")),
        };
        Roles[0].IsSelected = true;

        _currentFact = Facts[0];
        FactDots[0].IsActive = true;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += OnTick;
        UpdateTelemetry();
        _timer.Start();
    }

    // ---------- Dil destekli metinler ----------
    public string RankText => IsEnglish ? "ASTRONAUT CANDIDATE" : "ASTRONOT ADAYI";
    public string WelcomeText => IsEnglish ? "Welcome!" : "Hoş geldin!";
    public string SubWelcomeText => IsEnglish ? "Ready to join the NASA Artemis & DORA training mission." : "NASA Artemis ve DORA uzay eğitim görevine katılmaya hazırsın.";
    public string AppSubtitle => IsEnglish ? "Young Astronaut Academy" : "Genç Astronot Akademisi";
    public string FactBadge => IsEnglish ? "DID YOU KNOW?" : "BİLİYOR MUYDUN?";
    public string FactText => IsEnglish ? CurrentFact.En : CurrentFact.Tr;
    public string BoardingTitle => IsEnglish ? "BOARDING PASS" : "BİNİŞ KARTI";
    public string CallSignLabel => IsEnglish ? "YOUR CALL SIGN" : "ÇAĞRI ADIN";
    public string RoleLabel => IsEnglish ? "CHOOSE YOUR ROLE" : "GÖREV ROLÜNÜ SEÇ";
    public string StartText => IsEnglish ? "START MISSION" : "GÖREVE BAŞLA";
    public string SpeedLabel => IsEnglish ? "ISS ORBIT SPEED" : "ISS YÖRÜNGE HIZI";
    public string SpeedSub => IsEnglish ? "~7.66 km/s orbit" : "Saniyede ~7.66 km";
    public string OrbitLabel => IsEnglish ? "ORBIT ALTITUDE" : "YÖRÜNGE İRTİFASI";
    public string OrbitSub => IsEnglish ? "Low Earth Orbit (LEO)" : "Alçak Yörünge (LEO)";
    public string ClockLabel => IsEnglish ? "TELEMETRY & UTC" : "TELEMETRİ & UTC SAATİ";
    public string StatusText => IsEnglish ? "STATUS: READY" : "DURUM: GÖREVE HAZIR";

    partial void OnIsEnglishChanged(bool value)
    {
        foreach (var role in Roles) role.ApplyLanguage(value);
        CallSignError = string.Empty;
        OnPropertyChanged(string.Empty); // tüm metinleri yenile
    }

    partial void OnCurrentFactChanged(SpaceFact value) => OnPropertyChanged(nameof(FactText));

    partial void OnCallSignChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value)) CallSignError = string.Empty;
    }

    // ---------- Komutlar ----------
    [RelayCommand]
    private void SetLanguage(string? code) => IsEnglish = code == "EN";

    [RelayCommand]
    private void NextFact() => ShowFact(_factIndex + 1);

    [RelayCommand]
    private void PreviousFact() => ShowFact(_factIndex - 1);

    [RelayCommand]
    private void SelectRole(RoleOption? role)
    {
        if (role is null) return;
        foreach (var r in Roles) r.IsSelected = ReferenceEquals(r, role);
    }

    [RelayCommand]
    private void StartMission()
    {
        if (string.IsNullOrWhiteSpace(CallSign))
        {
            CallSignError = IsEnglish ? "Please enter a call sign." : "Lütfen bir çağrı adı yaz.";
            return;
        }

        Stop();
        var role = Roles.FirstOrDefault(r => r.IsSelected)?.Key ?? "pilot";
        _onStartMission(Session, role);
    }

    public void Stop() => _timer.Stop();

    // ---------- Yardımcılar ----------
    private void ShowFact(int index)
    {
        _factIndex = (index + Facts.Count) % Facts.Count;
        for (int i = 0; i < FactDots.Count; i++) FactDots[i].IsActive = i == _factIndex;
        CurrentFact = Facts[_factIndex];
        _secondsSinceFactChange = 0;
    }

    private void OnTick(object? sender, EventArgs e)
    {
        UpdateTelemetry();
        if (++_secondsSinceFactChange >= 8) ShowFact(_factIndex + 1);
    }

    private void UpdateTelemetry()
    {
        IssSpeed = (27_590 + _random.Next(0, 25)).ToString("N0", Tr);
        MissionClock = DateTime.UtcNow.ToString("HH:mm:ss", Tr);
    }

    private static Geometry Icon(string data)
    {
        var g = Geometry.Parse(data);
        g.Freeze();
        return g;
    }
}
