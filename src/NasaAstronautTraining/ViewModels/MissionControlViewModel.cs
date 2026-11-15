using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NasaAstronautTraining.Models;
using NasaAstronautTraining.Services;

namespace NasaAstronautTraining.ViewModels;

public partial class MissionControlViewModel : ObservableObject
{
    private readonly Action _onLogout;
    private readonly ScoreService _scoreService = new();
    private readonly NasaDonkiService _donkiService = new();
    private readonly Dictionary<int, List<Senaryo>> _senaryoHavuzu = SenaryoHavuzuData.Olustur();
    private readonly Dictionary<int, int> _sonSecilenSenaryoIndex = new();
    private readonly HashSet<int> _aktifTurTamamlananAsamalar = new();
    private readonly Random _rng = new();

    private static readonly SolidColorBrush GreenBrush = new((Color)ColorConverter.ConvertFromString("#16A34A"));
    private static readonly SolidColorBrush AmberBrush = new((Color)ColorConverter.ConvertFromString("#D97706"));
    private static readonly SolidColorBrush RedBrush = new((Color)ColorConverter.ConvertFromString("#DC2626"));

    public AstronautSession Session { get; }
    public string Role { get; }

    // ==================== 1. ÜST BAR SEKMELERİ ====================
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsKomutaTab))]
    [NotifyPropertyChangedFor(nameof(IsAkademiTab))]
    [NotifyPropertyChangedFor(nameof(IsHangarTab))]
    [NotifyPropertyChangedFor(nameof(IsKunyeTab))]
    [NotifyPropertyChangedFor(nameof(IsAyarlarTab))]
    private string _activeTab = "KOMUTA";

    public bool IsKomutaTab => ActiveTab == "KOMUTA";
    public bool IsAkademiTab => ActiveTab == "AKADEMİ";
    public bool IsHangarTab => ActiveTab == "HANGAR";
    public bool IsKunyeTab => ActiveTab == "KÜNYE";
    public bool IsAyarlarTab => ActiveTab == "AYARLAR";

    [ObservableProperty]
    private bool _isEnglish = false;

    [ObservableProperty]
    private bool _isSoundEnabled = true;

    // ==================== 2. KOMUTA: YÖRÜNGE HARİTASI & ENTEGRE SİMÜLASYON KOKPİTİ ====================
    public ObservableCollection<MissionStageNode> Stages { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNextStage))]
    private MissionStageNode _selectedStage = null!;

    public bool HasNextStage => SelectedStage != null && SelectedStage.Number < 6;

    /// <summary>True olduğunda KOMUTA sekmesi içinde canlı Görev &amp; Kriz Simülasyonu Kokpiti açılır.</summary>
    [ObservableProperty]
    private bool _isStageCockpitActive = false;

    [ObservableProperty]
    private string _completedStagesCountText = "Tamamlanan: 0 / 6";

    [ObservableProperty]
    private string _activeStageSummaryText = "Aktif Hedef: 01 Uzay & Yer Eğitimi";

    [ObservableProperty]
    private int _level = 3;

    [ObservableProperty]
    private int _currentXp = 1450;

    [ObservableProperty]
    private int _nextLevelXp = 2500;

    [ObservableProperty]
    private double _xpProgressPercent = 58.0;

    // Günlük Mini Bilim Testi
    [ObservableProperty]
    private string _quizQuestion = string.Empty;

    [ObservableProperty]
    private string _quizOptionA = string.Empty;

    [ObservableProperty]
    private string _quizOptionB = string.Empty;

    [ObservableProperty]
    private string _quizFeedback = string.Empty;

    [ObservableProperty]
    private bool _quizAnswered = false;

    [ObservableProperty]
    private bool _quizIsSuccess = false;

    // ==================== 3. SİMÜLASYON KOKPİTİ: 5 KAYNAK, KRİZ KARAR & MADALYA KUTLAMA SİSTEMİ ====================
    [ObservableProperty]
    private double _oxygenLevel = 85.0;

    [ObservableProperty]
    private double _energyLevel = 80.0;

    [ObservableProperty]
    private double _moraleLevel = 75.0;

    [ObservableProperty]
    private double _knowledgeLevel = 20.0;

    [ObservableProperty]
    private double _timeLevel = 100.0;

    [ObservableProperty]
    private SolidColorBrush _oxygenStatusBrush = GreenBrush;

    [ObservableProperty]
    private SolidColorBrush _energyStatusBrush = GreenBrush;

    [ObservableProperty]
    private string _currentCrisisTitle = string.Empty;

    [ObservableProperty]
    private string _currentCrisisText = string.Empty;

    [ObservableProperty]
    private string _currentScenarioNumberText = "Olasılık 1 / 4";

    [ObservableProperty]
    private string _decisionAText = string.Empty;

    [ObservableProperty]
    private string _decisionACostText = string.Empty;
    private DecisionOption? _currentOptionA;

    [ObservableProperty]
    private string _decisionBText = string.Empty;

    [ObservableProperty]
    private string _decisionBCostText = string.Empty;
    private DecisionOption? _currentOptionB;

    [ObservableProperty]
    private string _decisionCText = string.Empty;

    [ObservableProperty]
    private string _decisionCCostText = string.Empty;
    private DecisionOption? _currentOptionC;

    /// <summary>Bu aşamadaki kriz sorusuna cevap verildiğinde true olur; aynı soruya 10 kez tıklanmasını engeller.</summary>
    [ObservableProperty]
    private bool _isCurrentStageAnswered;

    [ObservableProperty]
    private bool _isHintVisible;

    [ObservableProperty]
    private string _hintText = string.Empty;

    [ObservableProperty]
    private bool _hasLastBadge;

    [ObservableProperty]
    private string _lastBadgeResult = string.Empty;

    [ObservableProperty]
    private string _lastBadgeMessage = string.Empty;

    [ObservableProperty]
    private string _journalFact = string.Empty;

    [ObservableProperty]
    private string _journalDeltaSummary = string.Empty;

    [ObservableProperty]
    private bool _isFailureDialogOpen;

    [ObservableProperty]
    private string _failureReason = string.Empty;

    [ObservableProperty]
    private string _failureMessage = string.Empty;

    // 6 Aşamalı Madalya Kutlama Ekranı (Tüm 6 aşama bittiğinde açılır, 1'e geri dönmez)
    public ObservableCollection<CelebrationMedalSlot> CelebrationMedalSlots { get; } = new();

    [ObservableProperty]
    private bool _isMedalCelebrationVisible;

    [ObservableProperty]
    private string _medalCelebrationSummary = string.Empty;

    [ObservableProperty]
    private string _leaderboardRankText = string.Empty;

    // Konum & NASA DONKI Canlı Telemetri
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMarsSelected))]
    [NotifyPropertyChangedFor(nameof(IsMoonSelected))]
    private MissionLocation _currentLocation = MissionLocations.Mars;

    public bool IsMarsSelected => CurrentLocation.Id == "Mars";
    public bool IsMoonSelected => CurrentLocation.Id == "Moon";

    [ObservableProperty]
    private string _solarActivityText = "Güneş: Normal";

    [ObservableProperty]
    private string _temperatureText = "-63°C";

    [ObservableProperty]
    private string _donkiSourceText = "Kaynak: NASA DONKI API";

    [ObservableProperty]
    private string _earthMessageText = "Enerji rezervlerinizi ve yaşam destek valflerini dengede tutun.";

    // ==================== 4. AKADEMİ: İLK 5 EĞİTİM MODÜLÜ (BİLGİ KARTLARI) ====================
    public ObservableCollection<InfoCardItem> InfoCards { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCardPrevEnabled))]
    [NotifyPropertyChangedFor(nameof(CardNextButtonText))]
    private InfoCardItem _currentCard = null!;

    [ObservableProperty]
    private int _readCardsCount = 1;

    public bool IsCardPrevEnabled => CurrentCard != null && CurrentCard.Index > 1;
    public string CardNextButtonText => CurrentCard != null && CurrentCard.Index == 5
        ? "Simülasyon Kokpitine Geç"
        : "Sonraki Eğitim Modülü";

    // ==================== 5. HANGAR: 8 NASA SPACE PLACE OYUNU & WEBVIEW2 ====================
    public ObservableCollection<SpaceGameItem> Games { get; }

    [ObservableProperty]
    private SpaceGameItem _selectedGame = null!;

    [ObservableProperty]
    private bool _isPlayingGame = false;

    // ==================== 6. KÜNYE: PROFİL, BECERİLER, MADALYALAR & LİDERLİK TABLOSU ====================
    public Dictionary<int, MadalyaTuru> KazanilanMadalyalar { get; } = new();
    public ObservableCollection<StageMedalItem> StageMedals { get; } = new();
    public ObservableCollection<AstronautSkill> Skills { get; } = new();
    public ObservableCollection<LeaderboardItem> LeaderboardScores { get; } = new();

    [ObservableProperty]
    private int _totalGoldCount;

    [ObservableProperty]
    private int _totalSilverCount;

    [ObservableProperty]
    private int _totalBronzeCount;

    [ObservableProperty]
    private int _currentScore;

    [ObservableProperty]
    private int _roundLevel = 1;

    [ObservableProperty]
    private string _roundTitle = "1. Tur: Aday Astronot";

    [ObservableProperty]
    private string _playerRankText = "#1 / 4 Astronot";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFilterMadalya))]
    [NotifyPropertyChangedFor(nameof(IsFilterBilgi))]
    [NotifyPropertyChangedFor(nameof(IsFilterHiz))]
    private string _leaderboardFilter = "madalya";

    public bool IsFilterMadalya => LeaderboardFilter == "madalya";
    public bool IsFilterBilgi => LeaderboardFilter == "bilgi";
    public bool IsFilterHiz => LeaderboardFilter == "hiz";

    // ==================== DİL DESTEKLİ METİNLER ====================
    public string AstronautFullName => Session.AstronautName;

    public string AstronautRoleTitle => IsEnglish
        ? $"{Session.AstronautName} ({GetRoleDisplayEn(Role)})"
        : $"{Session.AstronautName} ({GetRoleDisplayTr(Role)})";

    public string RoleBadgeText => IsEnglish ? GetRoleDisplayEn(Role).ToUpperInvariant() : GetRoleDisplayTr(Role).ToUpperInvariant();

    public string AppTitle => "ALTAY TUNGA DORA";
    public string AppSubtitle => IsEnglish ? "Young Astronaut Academy" : "Genç Astronot Akademisi";

    public string TabKomuta => IsEnglish ? "COMMAND" : "KOMUTA";
    public string TabAkademi => IsEnglish ? "ACADEMY" : "AKADEMİ";
    public string TabHangar => IsEnglish ? "HANGAR" : "HANGAR";
    public string TabKunye => IsEnglish ? "DOSSIER" : "KÜNYE";
    public string TabAyarlar => IsEnglish ? "SETTINGS" : "AYARLAR";

    public string MapTitle => IsEnglish ? "MISSION ORBIT & STAGES" : "GÖREV YÖRÜNGESİ & AŞAMALAR";
    public string MapSubtitle => IsEnglish ? "Artemis & DORA Exploration Expeditions" : "6 Aşamalı Eğitim, Yörünge Rotası ve Kriz Simülasyonu";

    public string BriefingHeading => IsEnglish ? "MISSION DIRECTIVE" : "SEÇİLİ AŞAMA GÖREV BRİFİNGİ";
    public string EnterMissionButtonText => IsEnglish ? "LAUNCH SIMULATION" : "GÖREVE GİRİŞ YAP";
    public string OpenTrainingButtonText => IsEnglish ? "STUDY TRAINING MODULE" : "EĞİTİMİ AÇ";

    public string ProgressHeading => IsEnglish ? "ASTRONAUT PROGRESS" : "ASTRONOT GELİŞİMİ & MADALYALAR";
    public string LevelText => IsEnglish ? $"Level {Level}: Pilot Cadet" : $"Seviye {Level}: Pilot Kadet";
    public string AchievementsLabel => IsEnglish ? "Earned Medals & Rank" : "Kazanılan Görev Madalyaları";

    public string QuizHeading => IsEnglish ? "DAILY MINI QUIZ" : "GÜNÜN MİNİ TESTİ";

    public MissionControlViewModel(
        AstronautSession session,
        string role,
        Action? onLogout = null)
    {
        Session = session;
        Role = string.IsNullOrWhiteSpace(role) ? "pilot" : role;
        _onLogout = onLogout ?? (() => { });

        // 1. 6 Görev Yörüngesi Aşaması (Eğitim + Kriz Simülasyonu Entegre)
        Stages = new ObservableCollection<MissionStageNode>
        {
            new()
            {
                Number = 1,
                ShortName = "Uzay & Yer Eğitimi",
                TitleTr = "01: Uzay Nedir? & Yer Eğitimi",
                TitleEn = "01: What is Space? & Ground Sim",
                SubtitleTr = "Kármán Hattı, Vakum Ortamı & Biyometrik Refleks",
                SubtitleEn = "Kármán Line, Vacuum Physics & Biometric Reflex",
                DescriptionTr = "Dünya atmosferinden Kármán Hattı'na (100 km) çıkış, uzayın mutlak sessizliği ve astronot adaylarının biyometrik stres/refleks simülasyonu.",
                DescriptionEn = "Transition past the 100 km Kármán Line, extreme thermal gradients, and astronaut candidate biometric stress simulations.",
                ObjectiveTr = "Uzay fiziği eğitimini tamamla ve ASCAN biyometrik kriz simülasyonunda doğru kararı ver.",
                ObjectiveEn = "Complete space physics training and resolve the ASCAN biometric crisis.",
                Status = StageStatus.Active,
                OxygenPercent = 100,
                PressureKpa = 101.3,
                GameUrl = "https://spaceplace.nasa.gov/"
            },
            new()
            {
                Number = 2,
                ShortName = "ASCAN & Mikroçekim",
                TitleTr = "02: Astronot Seçimi & Mikroçekim",
                TitleEn = "02: Astronaut Selection & Microgravity",
                SubtitleTr = "ASCAN Kriterleri, Çelik İrade & NBL Su Altı",
                SubtitleEn = "ASCAN Criteria, Psychological Resilience & NBL",
                DescriptionTr = "Binlerce aday arasından seçilme kriterleri, G-kuvveti dayanıklılığı, su altı ağırlıksızlık (NBL) antrenmanları ve güneş paneli/filtre kriz yönetimi.",
                DescriptionEn = "ASCAN selection trials, Neutral Buoyancy Lab underwater EVA prep, and solar array crisis handling.",
                Status = StageStatus.Locked,
                ObjectiveTr = "Ağırlıksızlık eğitimini incele ve ekipman/yaşam destek krizini yönet.",
                ObjectiveEn = "Complete microgravity prep and resolve the orbital equipment crisis.",
                OxygenPercent = 99,
                PressureKpa = 101.2,
                GameUrl = "https://spaceplace.nasa.gov/"
            },
            new()
            {
                Number = 3,
                ShortName = "ISS Uzay İstasyonu",
                TitleTr = "03: Uluslararası Uzay İstasyonu (ISS)",
                TitleEn = "03: International Space Station (ISS)",
                SubtitleTr = "28.000 km/s Yörünge Evi & T-38 Manevrası",
                SubtitleEn = "28,000 km/h Orbital Lab & T-38 Maneuvers",
                DescriptionTr = "Dünya'nın 400 km üzerinde günde 16 gün doğumu yaşayan dev bilim laboratuvarı ISS'in modülleri, yüksek G manevraları ve iletişim kesintisi protokolleri.",
                DescriptionEn = "Maintain ISS orbital modules at 400 km altitude and execute emergency flight maneuvers.",
                ObjectiveTr = "Oksijen seviyesini %95+ üzerinde tut, kabin basıncını 101.3 kPa değerinde dengele ve krizi çöz.",
                ObjectiveEn = "Keep oxygen above 95% and equalize cabin pressure at 101.3 kPa.",
                Status = StageStatus.Locked,
                OxygenPercent = 98,
                PressureKpa = 101.3,
                GameUrl = "https://iss-sim.spacex.com/"
            },
            new()
            {
                Number = 4,
                ShortName = "ECLSS & Ay Yürüyüşü",
                TitleTr = "04: Kaynak Yönetimi & ECLSS",
                TitleEn = "04: Resource Management & ECLSS",
                SubtitleTr = "Oksijen, Su Geri Dönüşümü & Uzay Yürüyüşü (EVA)",
                SubtitleEn = "Closed-Loop Life Support & EVA Spacewalks",
                DescriptionTr = "Kapalı döngü yaşam destek sistemi (ECLSS), Sabatier reaktörleri, %98 su geri dönüşümü ve uzay yürüyüşü (EVA) sırasında eldiven/regülatör krizleri.",
                DescriptionEn = "Closed-loop ECLSS water/oxygen recycling and emergency EVA spacesuit integrity protocols.",
                ObjectiveTr = "Yaşam destek döngüsünü koru ve EVA uzay yürüyüşü krizini hasarsız atlat.",
                ObjectiveEn = "Preserve closed-loop ECLSS reserves and complete EVA crisis recovery.",
                Status = StageStatus.Locked,
                OxygenPercent = 95,
                PressureKpa = 99.8,
                GameUrl = "https://eyes.nasa.gov/apps/solar-system/#/home"
            },
            new()
            {
                Number = 5,
                ShortName = "Mars & Robotik Kol",
                TitleTr = "05: Görev Hazırlığı & Robotik Kol",
                TitleEn = "05: Mission Prep & Canadarm2 Robotics",
                SubtitleTr = "GO/NO-GO Protokolü & Canadarm2 Operasyonu",
                SubtitleEn = "Flight Director GO/NO-GO & Canadarm2",
                DescriptionTr = "Uçuş kontrolörlerinin GO/NO-GO karar mekanizması, kargo kapsülü kenetlenme sapmaları ve Canadarm2 robotik kol tork/kamera arızalarının çözümü.",
                DescriptionEn = "Flight Control GO/NO-GO decision making and Canadarm2 cargo capture alignment.",
                ObjectiveTr = "Görev öncesi brifingi tamamla ve robotik kenetlenme krizinde en doğru mühendislik kararını ver.",
                ObjectiveEn = "Complete flight readiness briefing and execute autonomous robotic docking.",
                Status = StageStatus.Locked,
                OxygenPercent = 94,
                PressureKpa = 98.5,
                GameUrl = "https://spaceplace.nasa.gov/explore-mars/en/"
            },
            new()
            {
                Number = 6,
                ShortName = "Derin Uzay & Kriz",
                TitleTr = "06: Derin Uzay & Simülasyon",
                TitleEn = "06: Deep Space & Simulation",
                SubtitleTr = "Güneş Fırtınası, Radyasyon & Mühendislik Liderliği",
                SubtitleEn = "Solar Flare Storm, Radiation & Command",
                DescriptionTr = "Gerçek zamanlı telemetri altında ani güneş fırtınaları (CME), yakıt sızıntıları ve soğutma arızalarında tüm kaynakları yöneterek görevi zaferle tamamla.",
                DescriptionEn = "Command the spacecraft through major solar flares, coolant leaks, and orbital insertion.",
                ObjectiveTr = "Tüm eğitim bilgilerini kullanarak derin uzay krizlerini çöz ve Altın Madalya kazan.",
                ObjectiveEn = "Resolve deep space emergencies with optimal resource balance to earn Gold.",
                Status = StageStatus.Locked,
                OxygenPercent = 99,
                PressureKpa = 101.3,
                GameUrl = "https://spaceplace.nasa.gov/"
            }
        };

        // 2. Akademi: 5 Detaylı Eğitim Modülü (Bilgi Kartları)
        InfoCards = new ObservableCollection<InfoCardItem>
        {
            new()
            {
                Index = 1,
                StageTitle = "1. Uzay Nedir?",
                Subtitle = "Kármán Hattı, Vakum & Kozmik Evren",
                VectorPathData = "M12,2 A10,10 0 1,0 22,12 A10,10 0 0,0 12,2 Z M2,12 H22 M12,2 A15.3,15.3 0 0,1 16,12 A15.3,15.3 0 0,1 12,22 A15.3,15.3 0 0,1 8,12 A15.3,15.3 0 0,1 12,2 Z",
                IsEarthCard = true,
                IsRead = true,
                IsSelected = true,
                MainTitle = "Uzayın Derinlikleri: Sonsuz Boşluk ve Büyüleyici Evren!",
                Description = "Gezegenimiz Dünya, bizi zararlı radyasyonlardan koruyan ve nefes almamızı sağlayan kalın bir atmosfer tabakasıyla çevrilidir. Ancak deniz seviyesinden yaklaşık 100 kilometre yukarıya (Kármán Hattı) çıktığınızda atmosfer tamamen biter ve sonsuz uzay boşluğu başlar! Uzayda soluyabileceğimiz hava molekülleri ve atmosfer basıncı yoktur; ses dalgalarını iletecek hiçbir ortam bulunmadığı için evren mutlak bir sessizliğe gömülmüştür.\n\nÜstelik Güneş ışığı alan yerler kavurucu +120°C sıcaklığa ulaşırken, gölgede kalan bölgeler dondurucu -150°C'ye kadar düşer. Bu büyüleyici kozmik okyanusta hayatta kalabilmek için astronotlar ileri mühendislik harikası koruyucu kıyafetler ve yaşam destek sistemleri kullanırlar.",
                DidYouKnow = "Uzayın derinlikleri o kadar akılalmaz boyutlardadır ki, ışık hızıyla (saniyede tam 300.000 kilometre!) seyahat etseniz bile Güneş Sistemimize en yakın yıldıza (Proxima Centauri) ulaşmanız 4 yıldan uzun sürer. İnsan yapımı en hızlı uzay araçlarıyla bu yolculuk tam 70.000 yıl sürerdi!",
                SourceText = "Kaynak: NASA Space Place & Kármán Line Research"
            },
            new()
            {
                Index = 2,
                StageTitle = "2. Astronot Kim Olur?",
                Subtitle = "ASCAN Seçim Süreci & Çelik İrade",
                VectorPathData = "M12,3 A8,8 0 0,1 20,11 V17 A2,2 0 0,1 18,19 H6 A2,2 0 0,1 4,17 V11 A8,8 0 0,1 12,3 Z M7.5,9.5 H16.5 V13.5 A2,2 0 0,1 14.5,15.5 H9.5 A2,2 0 0,1 7.5,13.5 Z M4,21 H20",
                IsEarthCard = false,
                MainTitle = "Bir Astronotun Doğuşu: Zihinsel ve Fiziksel Çelik İrade!",
                Description = "Bir NASA veya uluslararası uzay ajansı astronotu olmak, gezegenimizin en zorlu ve en seçici süreçlerinden biridir. Her astronot adayı (ASCAN - Astronaut Candidate) ilanına dünyanın dört bir yanından on binlerce üstün yetenekli mühendis, bilim insanı, doktor ve askeri jet pilotu başvurur.\n\nYıllar süren seçim aşamalarında adaylar sadece üstün akademik zekalarıyla değil; aşırı G-kuvveti altında bilincini koruma testleri, sıfır yerçekimi uçuşları, derin mağara izolasyon kampları ve su altı hayatta kalma eğitimleriyle sınanırlar. En kritik kriter ise psikolojik dayanıklılık ve takım ruhudur; çünkü uzayda aylar boyunca küçük bir kapsülde kapalı kalırken acil bir kriz anında paniklemeden, saniyeler içinde doğru analitik kararı verebilmek hayati önem taşır.",
                DidYouKnow = "NASA'nın astronot alım sınıfına 12.000'den fazla üst düzey aday başvurmuş, ancak bu zorlu maratonun sonunda sadece 10 kişi seçilebilmiştir! Kabul oranı %0.08'dir; bu da Harvard Üniversitesi'ne girmekten yaklaşık 50 kat daha zordur.",
                SourceText = "Kaynak: NASA Astronaut Selection & Training Facility"
            },
            new()
            {
                Index = 3,
                StageTitle = "3. Uzay İstasyonu (ISS)",
                Subtitle = "Yörüngedeki Laboratuvar & Yaşam",
                VectorPathData = "M2,9 H8 V15 H2 Z M16,9 H22 V15 H16 Z M8,12 H16 M10,8 H14 V16 H10 Z M5,6 V18 M19,6 V18",
                IsEarthCard = false,
                MainTitle = "Yörüngedeki İnsan Evi: Uluslararası Uzay İstasyonu (ISS)!",
                Description = "Uluslararası Uzay İstasyonu (ISS), insanlık tarihinin ortaklaşa inşa ettiği en karmaşık bilim mühendisliği şaheseridir. Dünya'nın yaklaşık 400 kilometre yukarısındaki alçak yörüngede süzülen bu devasa platform, saatte yaklaşık 28.000 kilometre gibi muazzam bir süratle hareket eder.\n\nBu baş döndürücü hız sayesinde istasyon Dünya etrafındaki bir tam turunu sadece 90 dakikada tamamlar; yani istasyondaki astronotlar her 24 saatlik günde tam 16 kez gün doğumu ve 16 kez gün batımı yaşarlar! Bir futbol sahası büyüklüğündeki istasyon; laboratuvarlar, uyku kabinleri, spor aletleri ve dev güneş panelleriyle donatılmıştır. Yerçekimsiz ortamda kas ve kemik erimesini engellemek için astronotlar her gün en az 2 saat özel vakumlu koşu bantlarında ve ağırlık cihazlarında egzersiz yapmak zorundadır.",
                DidYouKnow = "Gökyüzünün tamamen karardığı açık akşamlarda ISS'i çıplak gözle izleyebilirsiniz! Devasa güneş panellerinden yansıyan ışık sayesinde gökyüzünde çok parlak ve uçaklardan katbekat hızlı ilerleyen beyaz bir yıldız gibi görünür.",
                SourceText = "Kaynak: NASA ISS Program & Johnson Space Center"
            },
            new()
            {
                Index = 4,
                StageTitle = "4. Kaynak Yönetimi",
                Subtitle = "ECLSS Yaşam Destek & Geri Dönüşüm",
                VectorPathData = "M13,2 L3,14 H12 L11,22 L21,10 H12 Z",
                IsEarthCard = false,
                MainTitle = "Uzayda Yaşam Çemberi: Hiçbir Damlanın İsraf Edilmediği Dünya!",
                Description = "Gezegenimizde nefes alırken oksijenin veya musluğu açtığımızda suyun tükeneceğini düşünmeyiz. Oysa Dünya'dan yüzbinlerce kilometre uzakta uzayda her nefes, her watt elektrik ve her mililitre su paha biçilmez bir hazinedir!\n\nUzay araçlarında bulunan ECLSS (Çevresel Kontrol ve Yaşam Destek Sistemi), kapalı bir döngü ekosistemi yaratır. Astronotların nefes verirken ürettiği karbondioksit (CO2) zehirlenmeye yol açmaması için özel kimyasal filtrelerle (Sabatier reaktörleri) emilir ve oksijene dönüştürülür. Güneş panellerinden elde edilen enerji dev bataryalarda depolanır. Eğer soğutma veya elektrik sisteminde bir arıza çıkarsa saniyeler içinde önceliklendirme yapılmalı ve kritik yaşam üniteleri korunmalıdır.",
                DidYouKnow = "ISS'teki ileri arıtma sistemleri sayesinde astronotların teri, nefes buharı ve atık sıvıları filtrelenip damıtılarak %98'e varan oranda geri dönüştürülür! Elde edilen su, Dünya'daki musluk sularının çoğundan bile daha saf ve temiz bir içme suyuna dönüşür.",
                SourceText = "Kaynak: NASA Environmental Control & Life Support Systems (ECLSS)"
            },
            new()
            {
                Index = 5,
                StageTitle = "5. Görev Hazırlığı",
                Subtitle = "Kokpit Kontrolleri & GO/NO-GO Kararı",
                VectorPathData = "M12,2.5 C15.5,5.5 16.5,10 15,15 H9 C7.5,10 8.5,5.5 12,2.5 Z M9,15 L6,18.5 H9.5 M15,15 L18,18.5 H14.5 M10.5,18.5 L12,21.5 L13.5,18.5",
                IsEarthCard = false,
                MainTitle = "Kokpite Adım At: Karar Anı ve Mühendislik Liderliği!",
                Description = "Teorik eğitimlerini, sistem pratiklerini ve kaynak protokollerini başarıyla tamamladın. Şimdi gerçek bir görev komutanı olarak simülasyon kokpitine geçiyorsun! Önündeki ekranda telemetri verileri canlı olarak akacak: Oksijen seviyen, batarya enerjin, mürettebat morali, bilgi puanın ve görev zamanı.\n\nKarşılaşacağın ani güneş fırtınaları, telemetri anomalileri veya mikro-meteorit sızıntılarında soğukkanlılığını korumalısın. Vereceğin her kararın bir maliyeti ve bilimsel bir sonucu olacak; standart prosedürleri, yenilikçi mühendislik yaklaşımlarını ve risk faktörlerini tartarak en optimum yolu seçmelisin.",
                DidYouKnow = "NASA uçuş yöneticileri (Flight Controllers) bir görevi yönetirken 'GO / NO-GO' kuralını uygular. Yüzlerce kritik sistemin tamamı 'GO' onayı vermeden hiçbir roket fırlatılmaz, hiçbir kritik manevra başlatılmaz. Şimdi bu kontrol senin ellerinde!",
                SourceText = "Kaynak: NASA Mission Control & Flight Operations Directorate"
            }
        };

        _currentCard = InfoCards[0];

        // 3. Künye ve Kutlama: Madalya Slotları & 6 Temel Astronot Becerisi
        foreach (var s in Stages)
        {
            StageMedals.Add(new StageMedalItem(s.Number, s.TitleTr));
            CelebrationMedalSlots.Add(new CelebrationMedalSlot(s.Number, s.ShortName));
        }

        Skills.Add(new AstronautSkill { Id = 1, Name = "Sağlık Uzmanlığı", ShortCode = "01 • Biyometri", IconText = "01", EffectDesc = "Oksijen kaybı kararlarda %20 daha az etkiler.", StageRequired = "1. Aşama: ASCAN" });
        Skills.Add(new AstronautSkill { Id = 2, Name = "Enerji Yönetimi", ShortCode = "02 • Güç Hattı", IconText = "02", EffectDesc = "Enerji tüketimleri 5 puan daha az harcar.", StageRequired = "2. Aşama: ISS" });
        Skills.Add(new AstronautSkill { Id = 3, Name = "Hızlı Düşünme", ShortCode = "03 • Refleks", IconText = "03", EffectDesc = "İpucu (Araştır) özelliği Bilgi puanı harcamadan açılır.", StageRequired = "3. Aşama: T-38" });
        Skills.Add(new AstronautSkill { Id = 4, Name = "EVA Uzmanı", ShortCode = "04 • Uzay Yürüyüşü", IconText = "04", EffectDesc = "Zor kararlarda moral kaybını tamamen önler.", StageRequired = "4. Aşama: NBL" });
        Skills.Add(new AstronautSkill { Id = 5, Name = "Robotik Refleks", ShortCode = "05 • Canadarm2", IconText = "05", EffectDesc = "Zaman barı her turda daha yavaş azalır.", StageRequired = "5. Aşama: Canadarm2" });
        Skills.Add(new AstronautSkill { Id = 6, Name = "Tam Astronot", ShortCode = "06 • Komutan", IconText = "06", EffectDesc = "Sonraki turlarda tüm kaynaklar +10 bonus ile başlar.", StageRequired = "6. Aşama: Fırlatma" });

        // 4. Hangar: 8 NASA Space Place Uzay Oyunu
        Games = new ObservableCollection<SpaceGameItem>
        {
            new(
                1,
                "Snap It! Güneş Tutulması Macerası",
                "Snap It! An Eclipse Photo Adventure",
                "Güneş & Tutulma Gözlemi",
                "Güneş ve Ay'ın kesiştiği anları yakalayın. Tutulma evrelerini fotoğraflayarak uzay albümünüzü tamamlayın!",
                "https://spaceplace.nasa.gov/snap-it-eclipse-game/en/",
                "pack://application:,,,/DORA;component/Assets/games/snap-it.png",
                "https://spaceplace.nasa.gov/templates/featured/play/snap-it-eclipse-game.png"
            ),
            new(
                2,
                "Küp Uydu (CubeSat) Tasarımcısı",
                "CubeSat Builder: Build a NASA Spacecraft!",
                "Uzay Mühendisliği & Uydu Yapımı",
                "Güneş panelleri, kameralar, antenler ve itki sistemlerini birleştirerek kendi NASA araştırma küp uydunuzu inşa edin!",
                "https://spaceplace.nasa.gov/cubesat-builder-game/en/",
                "pack://application:,,,/DORA;component/Assets/games/cubesat.png",
                "https://spaceplace.nasa.gov/templates/featured/play/cubesat-builder-game.png"
            ),
            new(
                3,
                "Mars'ı Keşfet: Rover Sürüş ve Kodlama",
                "Explore Mars: A Mars Rover Game",
                "Gezegen Keşfi & Algoritmik Sürüş",
                "Kızıl Gezegende Mars Gezginine sıralı komutlar vererek engelleri aşın, kayaları inceleyin ve kontrol merkezine veri aktarın!",
                "https://spaceplace.nasa.gov/explore-mars/en/",
                "pack://application:,,,/DORA;component/Assets/games/explore-mars.png",
                "https://spaceplace.nasa.gov/templates/featured/play/explore-mars.png"
            ),
            new(
                4,
                "Akıntıya Kapıl: Okyanus Akıntıları Oyunu",
                "Go With the Flow: An Ocean Currents Game",
                "Dünya Bilimi & Oşinografi",
                "Okyanus sıcaklığı ve tuzluluk yoğunluğunu yöneterek su altı akıntılarını kullanın, araştırma denizaltınızı hedefe ulaştırın!",
                "https://spaceplace.nasa.gov/ocean-currents/en/",
                "pack://application:,,,/DORA;component/Assets/games/ocean-currents.png",
                "https://spaceplace.nasa.gov/templates/featured/play/ocean-currents_wide.png"
            ),
            new(
                5,
                "Derin Uzay Ağı (DSN) İletişim Hattı",
                "DSN Uplink-Downlink: A DSN Game",
                "Derin Uzay & Anten Haberleşmesi",
                "Dünyanın dört bir yanındaki dev parabolik antenleri yöneterek derin uzaydaki araçlarla sinyal bağlantısı kurun!",
                "https://spaceplace.nasa.gov/dsn-game/en/",
                "pack://application:,,,/DORA;component/Assets/games/dsn-game.png",
                "https://spaceplace.nasa.gov/templates/featured/play/dsn-game.png"
            ),
            new(
                6,
                "Helios: Güneş Enerjisi ve Füzyon",
                "Helios: How the Sun Makes Energy",
                "Astrofizik & Güneş Fiziği",
                "Güneş'in merkezindeki nükleer füzyon sürecini yönetin; protonları birleştirip helyum üreterek güneş sistemini aydınlatın!",
                "https://spaceplace.nasa.gov/helios-game/en/",
                "pack://application:,,,/DORA;component/Assets/games/helios.png",
                "https://spaceplace.nasa.gov/templates/featured/play/helios-game.png"
            ),
            new(
                7,
                "Relay: Lazerli Uzay Haberleşmesi",
                "Relay: A Laser-Based Space Communications Game",
                "Lazer Teknolojisi & İletişim",
                "NASA'nın yeni nesil lazer teknolojisiyle Dünya, Ay ve uydular arasında optik ışınları yönlendirerek ışık hızında veri aktarın!",
                "https://spaceplace.nasa.gov/js/relay-game/index.html",
                "pack://application:,,,/DORA;component/Assets/games/relay-laser.png",
                "https://spaceplace.nasa.gov/templates/featured/play/relay-laser-communications-game.png"
            ),
            new(
                8,
                "Evrenini Renklendir: Gizli Uzay Nesneleri",
                "Color Your Universe: Find the Hidden Objects",
                "Gözlem & Bilimsel Keşif",
                "Jüpiter ve uzay araçları tablosunda saklanmış bilimsel nesneleri keşfedin ve kendi uzay gözlem tablonuzu oluşturun!",
                "https://spaceplace.nasa.gov/color-your-universe-game/en/",
                "pack://application:,,,/DORA;component/Assets/games/color-universe.png",
                "https://spaceplace.nasa.gov/templates/featured/play/color-your-universe.png"
            )
        };

        _selectedGame = Games[0];

        // 5. Backend (ScoreService) verilerini yükle
        SyncWithBackendScoreService();

        _selectedStage = Stages[0];
        _selectedStage.IsSelected = true;

        UpdateOrbitMapStatusSummary();
        LoadCrisisScenario(_selectedStage.Number);
        UpdateQuizStrings();
        RefreshLeaderboardData();

        // 6. Canlı NASA DONKI uzay havası verisini arka planda çek
        _ = DonkiVeriCekAsync();
    }

    public async Task DonkiVeriCekAsync()
    {
        try
        {
            var weather = await _donkiService.GetCurrentSpaceWeatherAsync();
            SolarActivityText = $"Güneş: {weather.SolarActivity.Replace("☀️", "").Trim()}";
            DonkiSourceText = weather.SourceText;
            if (weather.HasMajorSolarFlare)
            {
                EarthMessageText = "DİKKAT: Güneş fırtınası hızlandı! Radyasyon kalkanını devrede tutun.";
            }
        }
        catch { }
    }

    private void SyncWithBackendScoreService()
    {
        try
        {
            var player = _scoreService.GetOrCreatePlayer(Session.AstronautName);
            RoundLevel = Math.Max(1, player.TurSeviyesi);
            UpdateRoundTitle();

            if (player.AsamaDetay != null && player.AsamaDetay.Count > 0)
            {
                foreach (var kvp in player.AsamaDetay)
                {
                    if (Enum.TryParse<MadalyaTuru>(kvp.Value, out var m) && m != MadalyaTuru.Yok)
                    {
                        KazanilanMadalyalar[kvp.Key] = m;
                        var stage = Stages.FirstOrDefault(s => s.Number == kvp.Key);
                        if (stage != null) stage.Medal = m;

                        var sm = StageMedals.FirstOrDefault(item => item.StageId == kvp.Key);
                        if (sm != null) sm.Medal = m;

                        var skill = Skills.FirstOrDefault(sk => sk.Id == kvp.Key);
                        if (skill != null) skill.IsUnlocked = true;
                    }
                }
            }

            RecalculateMedalStats();
            KnowledgeLevel = Math.Max(20, player.BilgiPuani);
            int earnedPoints = (CurrentScore * 2) + ((int)KnowledgeLevel * 5);
            CurrentXp = Math.Min(NextLevelXp, 1450 + earnedPoints);
            Level = Math.Max(3, 3 + (TotalGoldCount / 2));
            XpProgressPercent = Math.Round((double)CurrentXp / NextLevelXp * 100, 1);
        }
        catch { }
    }

    private void UpdateOrbitMapStatusSummary()
    {
        int completedCount = _aktifTurTamamlananAsamalar.Count;
        CompletedStagesCountText = $"Tamamlanan: {completedCount} / 6";

        var firstUncompleted = Stages.FirstOrDefault(s => !s.IsRoundCompleted);
        for (int i = 0; i < Stages.Count; i++)
        {
            var st = Stages[i];
            if (st.IsRoundCompleted)
            {
                st.Status = StageStatus.Completed;
            }
            else if (firstUncompleted != null && st.Number == firstUncompleted.Number)
            {
                st.Status = StageStatus.Active;
            }
            else
            {
                st.Status = StageStatus.Locked;
            }
        }

        ActiveStageSummaryText = firstUncompleted != null
            ? $"Sıradaki Hedef: 0{firstUncompleted.Number} {firstUncompleted.ShortName}"
            : "Tüm 6 Görev Aşaması Tamamlandı!";
    }

    private void RecalculateMedalStats()
    {
        TotalGoldCount = KazanilanMadalyalar.Values.Count(m => m == MadalyaTuru.Altin);
        TotalSilverCount = KazanilanMadalyalar.Values.Count(m => m == MadalyaTuru.Gumus);
        TotalBronzeCount = KazanilanMadalyalar.Values.Count(m => m == MadalyaTuru.Bronz);
        CurrentScore = (TotalGoldCount * 100) + (TotalSilverCount * 60) + (TotalBronzeCount * 30);
    }

    public void RefreshLeaderboardData()
    {
        try
        {
            LeaderboardScores.Clear();
            var list = _scoreService.GetLeaderboard(LeaderboardFilter);
            foreach (var item in list)
            {
                LeaderboardScores.Add(item);
            }
            int rank = _scoreService.GetPlayerRank(Session.AstronautName);
            int total = _scoreService.GetTotalPlayerCount();
            PlayerRankText = $"#{rank} / {total} Astronot";
        }
        catch { }
    }

    // ==================== SEKME GEÇİŞ KOMUTLARI ====================
    [RelayCommand]
    private void SelectTab(string tabName)
    {
        if (string.IsNullOrWhiteSpace(tabName)) return;

        if (tabName != "HANGAR" && IsPlayingGame)
        {
            IsPlayingGame = false;
        }

        if (tabName == "KOMUTA" && ActiveTab == "KOMUTA")
        {
            IsStageCockpitActive = false;
        }

        ActiveTab = tabName;

        if (tabName == "KÜNYE")
        {
            RefreshLeaderboardData();
        }
    }

    // ==================== KOMUTA & SİMÜLASYON KOKPİTİ KOMUTLARI ====================
    [RelayCommand]
    private void SelectStage(MissionStageNode? stage)
    {
        if (stage == null) return;

        foreach (var s in Stages)
        {
            s.IsSelected = (s.Number == stage.Number);
        }
        SelectedStage = stage;
        OnPropertyChanged(nameof(HasNextStage));

        LoadCrisisScenario(stage.Number, preserveAnsweredState: true);
    }

    [RelayCommand]
    private void EnterMission()
    {
        if (SelectedStage == null) return;
        LoadCrisisScenario(SelectedStage.Number, preserveAnsweredState: true);
        IsStageCockpitActive = true;
        ActiveTab = "KOMUTA";
    }

    [RelayCommand]
    private void CloseStageCockpit()
    {
        IsStageCockpitActive = false;
    }

    [RelayCommand]
    private void OpenStageTraining()
    {
        int cardIdx = Math.Clamp(SelectedStage?.Number ?? 1, 1, 5);
        var card = InfoCards.FirstOrDefault(c => c.Index == cardIdx) ?? InfoCards[0];
        SelectInfoCard(card);
        ActiveTab = "AKADEMİ";
    }

    public void LoadCrisisScenario(int stageId, bool preserveAnsweredState = false)
    {
        IsHintVisible = false;
        IsFailureDialogOpen = false;

        var stageObj = Stages.FirstOrDefault(s => s.Number == stageId);
        if (preserveAnsweredState && stageObj != null && stageObj.IsRoundCompleted)
        {
            IsCurrentStageAnswered = true;
            HasLastBadge = true;
            var medal = KazanilanMadalyalar.GetValueOrDefault(stageId, MadalyaTuru.Altin);
            LastBadgeResult = $"{medal.ToName().ToUpperInvariant()} MADALYA — Bu Aşama Tamamlandı!";
            LastBadgeMessage = "Yeni bir kriz olasılığı çözmek için 'Yenile' butonuna basabilir veya sonraki aşamaya geçebilirsin.";
            return;
        }

        IsCurrentStageAnswered = false;
        HasLastBadge = false;
        LastBadgeResult = string.Empty;
        LastBadgeMessage = string.Empty;

        if (!_senaryoHavuzu.TryGetValue(stageId, out var list) || list.Count == 0)
        {
            list = _senaryoHavuzu[1];
        }

        int lastIdx = _sonSecilenSenaryoIndex.TryGetValue(stageId, out var prev) ? prev : -1;
        int idx = _rng.Next(list.Count);
        if (list.Count > 1)
        {
            while (idx == lastIdx)
            {
                idx = _rng.Next(list.Count);
            }
        }
        _sonSecilenSenaryoIndex[stageId] = idx;
        CurrentScenarioNumberText = $"Senaryo Olasılığı #{idx + 1} / {list.Count}";

        var secilen = list[idx];
        CurrentCrisisTitle = secilen.KrizBasligi;
        CurrentCrisisText = secilen.KrizAciklamasi;

        _currentOptionA = new DecisionOption(
            Key: "A",
            Text: secilen.KararA.Metin,
            CostSummary: secilen.KararA.FormatMaliyet(),
            OxygenDelta: secilen.KararA.dOksijen,
            EnergyDelta: secilen.KararA.dEnerji,
            MoraleDelta: secilen.KararA.dMoral,
            KnowledgeDelta: secilen.KararA.dBilgi,
            TimeDelta: secilen.KararA.dZaman,
            IsWrong: secilen.KararA.Etiket == "Riskli",
            IsOptimal: secilen.KararA.Etiket == "Optimal",
            LearningNasaFact: secilen.NasaGercegi);

        _currentOptionB = new DecisionOption(
            Key: "B",
            Text: secilen.KararB.Metin,
            CostSummary: secilen.KararB.FormatMaliyet(),
            OxygenDelta: secilen.KararB.dOksijen,
            EnergyDelta: secilen.KararB.dEnerji,
            MoraleDelta: secilen.KararB.dMoral,
            KnowledgeDelta: secilen.KararB.dBilgi,
            TimeDelta: secilen.KararB.dZaman,
            IsWrong: secilen.KararB.Etiket == "Riskli",
            IsOptimal: secilen.KararB.Etiket == "Optimal",
            LearningNasaFact: secilen.NasaGercegi);

        _currentOptionC = new DecisionOption(
            Key: "C",
            Text: secilen.KararC.Metin,
            CostSummary: secilen.KararC.FormatMaliyet(),
            OxygenDelta: secilen.KararC.dOksijen,
            EnergyDelta: secilen.KararC.dEnerji,
            MoraleDelta: secilen.KararC.dMoral,
            KnowledgeDelta: secilen.KararC.dBilgi,
            TimeDelta: secilen.KararC.dZaman,
            IsWrong: secilen.KararC.Etiket == "Riskli",
            IsOptimal: secilen.KararC.Etiket == "Optimal",
            LearningNasaFact: secilen.NasaGercegi);

        HintText = secilen.IpucuMetni;
        DecisionAText = _currentOptionA.Text;
        DecisionACostText = _currentOptionA.CostSummary;
        DecisionBText = _currentOptionB.Text;
        DecisionBCostText = _currentOptionB.CostSummary;
        DecisionCText = _currentOptionC.Text;
        DecisionCCostText = _currentOptionC.CostSummary;

        JournalFact = secilen.NasaGercegi;
        JournalDeltaSummary = SelectedStage?.ObjectiveTr ?? "Telemetri göstergelerini inceleyerek en uygun mühendislik kararını seçin.";
    }

    [RelayCommand]
    private void ExecuteDecisionA()
    {
        if (_currentOptionA != null) ApplyDecision(_currentOptionA);
    }

    [RelayCommand]
    private void ExecuteDecisionB()
    {
        if (_currentOptionB != null) ApplyDecision(_currentOptionB);
    }

    [RelayCommand]
    private void ExecuteDecisionC()
    {
        if (_currentOptionC != null) ApplyDecision(_currentOptionC);
    }

    private void ApplyDecision(DecisionOption option)
    {
        // Aynı soruya 10 kez cevap verilmesini kesin olarak engelle!
        if (IsCurrentStageAnswered || IsFailureDialogOpen) return;

        OxygenLevel = Math.Clamp(OxygenLevel + option.OxygenDelta, 0.0, 100.0);
        EnergyLevel = Math.Clamp(EnergyLevel + option.EnergyDelta, 0.0, 100.0);
        MoraleLevel = Math.Clamp(MoraleLevel + option.MoraleDelta, 0.0, 100.0);
        KnowledgeLevel = Math.Clamp(KnowledgeLevel + option.KnowledgeDelta, 0.0, 100.0);
        TimeLevel = Math.Clamp(TimeLevel + option.TimeDelta, 0.0, 100.0);

        OxygenStatusBrush = GetStatusBrush(OxygenLevel);
        EnergyStatusBrush = GetStatusBrush(EnergyLevel);

        JournalFact = option.LearningNasaFact;
        JournalDeltaSummary = $"Bilgi: {(option.KnowledgeDelta >= 0 ? "+" : "")}{option.KnowledgeDelta:0} P  •  Zaman: {option.TimeDelta:0}%  •  O2: {(option.OxygenDelta >= 0 ? "+" : "")}{option.OxygenDelta:0}%  •  Enerji: {(option.EnergyDelta >= 0 ? "+" : "")}{option.EnergyDelta:0}%";

        if (OxygenLevel <= 0 || EnergyLevel <= 0 || TimeLevel <= 0)
        {
            IsFailureDialogOpen = true;
            IsCurrentStageAnswered = true;
            if (OxygenLevel <= 0)
            {
                FailureReason = "Oksijen Seviyesi Tükendi!";
                FailureMessage = "Nefes almak uzayda en değerli kaynaktır. Yaşam destek sistemini sıfırlayarak tekrar dene.";
            }
            else if (EnergyLevel <= 0)
            {
                FailureReason = "Enerji Rezervi Tükendi!";
                FailureMessage = "Uzayda her kararın bir enerji maliyeti vardır. Güç rezervlerini yenileyerek tekrar dene.";
            }
            else
            {
                FailureReason = "Görev Süresi Tükendi!";
                FailureMessage = "Zaman yönetimi uzayda hayati önem taşır. Simülasyonu sıfırlayarak tekrar dene.";
            }
            HasLastBadge = false;
            return;
        }

        if (SelectedStage != null)
        {
            int stageId = SelectedStage.Number;
            IsCurrentStageAnswered = true;
            SelectedStage.IsRoundCompleted = true;
            _aktifTurTamamlananAsamalar.Add(stageId);

            // Bu aşamanın kalıcı becerisini aç
            var skill = Skills.FirstOrDefault(sk => sk.Id == stageId);
            if (skill != null)
            {
                skill.IsUnlocked = true;
            }

            MadalyaTuru kazanilanMadalya;
            if (option.IsOptimal || (OxygenLevel > 70 && EnergyLevel > 70))
            {
                kazanilanMadalya = MadalyaTuru.Altin;
                LastBadgeResult = "★ ALTIN MADALYA KAZANDIN! — Optimum Mühendislik Kararı";
                LastBadgeMessage = skill != null ? $"Kalıcı Beceri Açıldı: {skill.Name}" : "Mükemmel kaynak yönetimi!";
            }
            else if (OxygenLevel >= 40 && EnergyLevel >= 40)
            {
                kazanilanMadalya = MadalyaTuru.Gumus;
                LastBadgeResult = "★ GÜMÜŞ MADALYA KAZANDIN! — Görev Başarıyla Tamamlandı";
                LastBadgeMessage = skill != null ? $"Kalıcı Beceri Açıldı: {skill.Name}" : "Sistemler dengede tutuldu.";
            }
            else
            {
                kazanilanMadalya = MadalyaTuru.Bronz;
                LastBadgeResult = "★ BRONZ MADALYA KAZANDIN! — Riskli Kararla Tamamlandı";
                LastBadgeMessage = "Kaynaklar kritik seviyeye indi ancak görev tamamlandı.";
            }

            if (!KazanilanMadalyalar.TryGetValue(stageId, out var existingMedal) || kazanilanMadalya > existingMedal)
            {
                KazanilanMadalyalar[stageId] = kazanilanMadalya;
            }

            SelectedStage.Medal = KazanilanMadalyalar[stageId];
            var sm = StageMedals.FirstOrDefault(m => m.StageId == stageId);
            if (sm != null)
            {
                sm.Medal = KazanilanMadalyalar[stageId];
            }

            UpdateOrbitMapStatusSummary();
            RecalculateMedalStats();

            // XP ve Seviye Güncelle
            CurrentXp = Math.Min(NextLevelXp, CurrentXp + (kazanilanMadalya == MadalyaTuru.Altin ? 150 : 80));
            Level = Math.Max(3, 3 + (TotalGoldCount / 2));
            XpProgressPercent = Math.Round((double)CurrentXp / NextLevelXp * 100, 1);
            OnPropertyChanged(nameof(LevelText));

            // Backend'e kaydet
            var unlockedSkills = Skills.Where(s => s.IsUnlocked).Select(s => s.Name).ToList();
            _scoreService.UpdatePlayerMedals(
                Session.AstronautName,
                KazanilanMadalyalar,
                CurrentScore,
                (int)KnowledgeLevel,
                (int)TimeLevel,
                RoundLevel,
                unlockedSkills);

            RefreshLeaderboardData();
            HasLastBadge = true;
            string savedBadgeResult = $"★ Aşama 0{stageId}: {LastBadgeResult}";
            string savedBadgeMsg = LastBadgeMessage;
            string savedFact = option.LearningNasaFact;
            string savedDelta = JournalDeltaSummary;

            // 6 sorunun tamamı bittiyse veya 6. aşama tamamlandıysa:
            // ASLA 1. aşamaya geri dönme! Madalya Kutlama Ekranını aç!
            if (_aktifTurTamamlananAsamalar.Count >= 6 || stageId == 6)
            {
                        IsCurrentStageAnswered = true;
                LastBadgeResult = savedBadgeResult;
                LastBadgeMessage = savedBadgeMsg;
                JournalFact = savedFact;
                JournalDeltaSummary = savedDelta;
                _ = TriggerMedalCelebrationAsync();
            }
            else
            {
                // Kullanıcı seçeneğe tıkladığı anda HİÇ BEKLEMEDEN doğrudan sonraki aşamaya geç!
                        var nextStageObj = Stages.FirstOrDefault(s => s.Number == stageId + 1 && !s.IsRoundCompleted)
                                   ?? Stages.FirstOrDefault(s => !s.IsRoundCompleted)
                                   ?? Stages.FirstOrDefault(s => s.Number == stageId + 1);

                if (nextStageObj != null)
                {
                    foreach (var s in Stages)
                    {
                        s.IsSelected = (s.Number == nextStageObj.Number);
                    }
                    SelectedStage = nextStageObj;
                    OnPropertyChanged(nameof(HasNextStage));
                    LoadCrisisScenario(nextStageObj.Number, preserveAnsweredState: false);

                    // Önceki aşamada kazanılan madalya bilgisini alt barda göster
                    HasLastBadge = true;
                    LastBadgeResult = savedBadgeResult;
                    LastBadgeMessage = savedBadgeMsg;
                    JournalFact = savedFact;
                    JournalDeltaSummary = savedDelta;
                }
            }
        }
    }

    private async Task TriggerMedalCelebrationAsync()
    {
        try
        {
            MedalCelebrationSummary = $"{TotalGoldCount} Altın • {TotalSilverCount} Gümüş • {TotalBronzeCount} Bronz Madalya ({CurrentScore} Puan)";
            int rank = _scoreService.GetPlayerRank(Session.AstronautName);
            LeaderboardRankText = $"Akademi Liderlik Tablosunda #{rank}. Sıradasın!";

            for (int i = 0; i < 6; i++)
            {
                var medal = KazanilanMadalyalar.GetValueOrDefault(i + 1, MadalyaTuru.Yok);
                CelebrationMedalSlots[i].MedalLabel = medal.ToName();
                CelebrationMedalSlots[i].BadgeColor = medal switch
                {
                    MadalyaTuru.Altin => "#B45309",
                    MadalyaTuru.Gumus => "#334155",
                    MadalyaTuru.Bronz => "#9A3412",
                    _ => "#64748B"
                };
                CelebrationMedalSlots[i].BadgeBg = medal switch
                {
                    MadalyaTuru.Altin => "#FEF3C7",
                    MadalyaTuru.Gumus => "#F1F5F9",
                    MadalyaTuru.Bronz => "#FFEDD5",
                    _ => "#F8FAFC"
                };
                CelebrationMedalSlots[i].BadgeBorder = medal switch
                {
                    MadalyaTuru.Altin => "#F59E0B",
                    MadalyaTuru.Gumus => "#94A3B8",
                    MadalyaTuru.Bronz => "#EA580C",
                    _ => "#CBD5E1"
                };
                CelebrationMedalSlots[i].IsVisible = false;
                CelebrationMedalSlots[i].Opacity = 0.0;
            }

            IsMedalCelebrationVisible = true;

            for (int i = 0; i < 6; i++)
            {
                CelebrationMedalSlots[i].IsVisible = true;
                CelebrationMedalSlots[i].Opacity = 1.0;
                await Task.Delay(120);
            }
        }
        catch { }
    }

    [RelayCommand]
    private void ShowMedalCelebration()
    {
        _ = TriggerMedalCelebrationAsync();
    }

    [RelayCommand]
    private void CloseMedalCelebration()
    {
        IsMedalCelebrationVisible = false;
    }

    [RelayCommand]
    private void OpenKunyeFromCelebration()
    {
        IsMedalCelebrationVisible = false;
        IsStageCockpitActive = false;
        SelectTab("KÜNYE");
    }

    [RelayCommand]
    private void ToggleHint()
    {
        IsHintVisible = !IsHintVisible;
    }

    /// <summary>Yenile tuşuna basıldığında mevcut aşama için 4 olasılıktan farklı birini tekrar karşımıza çıkarır.</summary>
    [RelayCommand]
    private void RefreshCrisis()
    {
        IsMedalCelebrationVisible = false;
        if (SelectedStage != null)
        {
            _aktifTurTamamlananAsamalar.Remove(SelectedStage.Number);
            SelectedStage.IsRoundCompleted = false;
            UpdateOrbitMapStatusSummary();
            LoadCrisisScenario(SelectedStage.Number, preserveAnsweredState: false);
        }
    }

    /// <summary>Harita veya aşama kartı üzerindeki Yenile tuşuna basıldığında o aşamayı rastgele yeni olasılıkla açar.</summary>
    [RelayCommand]
    private void YenileStageSenaryo(MissionStageNode? stage)
    {
        IsMedalCelebrationVisible = false;
        var target = stage ?? SelectedStage;
        if (target == null) return;

        _aktifTurTamamlananAsamalar.Remove(target.Number);
        target.IsRoundCompleted = false;
        UpdateOrbitMapStatusSummary();

        foreach (var s in Stages)
        {
            s.IsSelected = (s.Number == target.Number);
        }
        SelectedStage = target;
        OnPropertyChanged(nameof(HasNextStage));
        LoadCrisisScenario(target.Number, preserveAnsweredState: false);
    }

    [RelayCommand]
    private void NextSimulationStage()
    {
        if (SelectedStage == null) return;

        // 6. sorudan sonra ASLA 1'e dönme!
        if (SelectedStage.Number >= 6)
        {
            _ = TriggerMedalCelebrationAsync();
            return;
        }

        var target = Stages.FirstOrDefault(s => s.Number == SelectedStage.Number + 1);
        if (target != null)
        {
            SelectStage(target);
        }
    }

    [RelayCommand]
    private void ResetSimulation()
    {
        IsMedalCelebrationVisible = false;
        OxygenLevel = 85.0;
        EnergyLevel = 80.0;
        MoraleLevel = 75.0;
        TimeLevel = 100.0;
        OxygenStatusBrush = GreenBrush;
        EnergyStatusBrush = GreenBrush;
        IsFailureDialogOpen = false;
        HasLastBadge = false;
        if (SelectedStage != null)
        {
            _aktifTurTamamlananAsamalar.Remove(SelectedStage.Number);
            SelectedStage.IsRoundCompleted = false;
            UpdateOrbitMapStatusSummary();
            LoadCrisisScenario(SelectedStage.Number, preserveAnsweredState: false);
        }
    }

    private static SolidColorBrush GetStatusBrush(double value)
    {
        if (value > 60) return GreenBrush;
        if (value >= 30) return AmberBrush;
        return RedBrush;
    }

    // ==================== AKADEMİ (5 EĞİTİM MODÜLÜ) KOMUTLARI ====================
    [RelayCommand]
    private void SelectInfoCard(InfoCardItem? card)
    {
        if (card == null) return;
        foreach (var c in InfoCards)
        {
            c.IsSelected = (c.Index == card.Index);
        }

        if (!card.IsRead)
        {
            card.IsRead = true;
            CurrentXp = Math.Min(NextLevelXp, CurrentXp + 25);
            XpProgressPercent = Math.Round((double)CurrentXp / NextLevelXp * 100, 1);
        }

        CurrentCard = card;
        ReadCardsCount = InfoCards.Count(c => c.IsRead);
        OnPropertyChanged(nameof(IsCardPrevEnabled));
        OnPropertyChanged(nameof(CardNextButtonText));
    }

    [RelayCommand]
    private void PrevCard()
    {
        if (CurrentCard == null || CurrentCard.Index <= 1) return;
        var prev = InfoCards.FirstOrDefault(c => c.Index == CurrentCard.Index - 1);
        if (prev != null) SelectInfoCard(prev);
    }

    [RelayCommand]
    private void NextCard()
    {
        if (CurrentCard == null) return;
        if (CurrentCard.Index < 5)
        {
            var next = InfoCards.FirstOrDefault(c => c.Index == CurrentCard.Index + 1);
            if (next != null) SelectInfoCard(next);
        }
        else
        {
            var stage6 = Stages.FirstOrDefault(s => s.Number == 6) ?? Stages.Last();
            SelectStage(stage6);
            IsStageCockpitActive = true;
            ActiveTab = "KOMUTA";
        }
    }

    [RelayCommand]
    private void LaunchCurrentCardSimulation()
    {
        int stageNum = CurrentCard?.Index ?? 1;
        var stage = Stages.FirstOrDefault(s => s.Number == stageNum) ?? Stages[0];
        SelectStage(stage);
        IsStageCockpitActive = true;
        ActiveTab = "KOMUTA";
    }

    // ==================== HANGAR (OYUNLAR) KOMUTLARI ====================
    [RelayCommand]
    private void PlayGame(SpaceGameItem? game)
    {
        if (game == null) return;
        SelectedGame = game;
        IsPlayingGame = true;
    }

    [RelayCommand]
    public void StopGame()
    {
        IsPlayingGame = false;
    }

    // ==================== KÜNYE & LİDERLİK TABLOSU KOMUTLARI ====================
    [RelayCommand]
    private void FilterLeaderboard(string? mode)
    {
        if (string.IsNullOrWhiteSpace(mode)) return;
        LeaderboardFilter = mode;
        RefreshLeaderboardData();
    }

    // ==================== AYARLAR & TÜM GÖREVLERİ YENİLEME KOMUTLARI ====================
    [RelayCommand]
    private void SelectLocation(string? locationId)
    {
        if (locationId == "Moon")
        {
            CurrentLocation = MissionLocations.Moon;
            TemperatureText = CurrentLocation.DefaultTemperature;
        }
        else if (locationId == "Mars")
        {
            CurrentLocation = MissionLocations.Mars;
            TemperatureText = CurrentLocation.DefaultTemperature;
        }
    }

    [RelayCommand]
    private void ToggleSound()
    {
        IsSoundEnabled = !IsSoundEnabled;
    }

    [RelayCommand]
    private void TekrarOyna()
    {
        IsMedalCelebrationVisible = false;
        RoundLevel++;
        UpdateRoundTitle();
        double bonus = Skills.Any(s => s.Id == 6 && s.IsUnlocked) ? 10.0 : 0.0;
        OxygenLevel = Math.Min(100.0, 85.0 + bonus);
        EnergyLevel = Math.Min(100.0, 80.0 + bonus);
        MoraleLevel = Math.Min(100.0, 75.0 + bonus);
        TimeLevel = 100.0;
        OxygenStatusBrush = GreenBrush;
        EnergyStatusBrush = GreenBrush;
        IsFailureDialogOpen = false;
        HasLastBadge = false;

        _aktifTurTamamlananAsamalar.Clear();
        foreach (var s in Stages)
        {
            s.IsRoundCompleted = false;
        }
        UpdateOrbitMapStatusSummary();
        SelectStage(Stages[0]);
        LoadCrisisScenario(1, preserveAnsweredState: false);
    }

    [RelayCommand]
    private void TumGorevleriYenile()
    {
        IsMedalCelebrationVisible = false;
        _aktifTurTamamlananAsamalar.Clear();
        foreach (var s in Stages)
        {
            s.IsRoundCompleted = false;
        }
        OxygenLevel = 85.0;
        EnergyLevel = 80.0;
        MoraleLevel = 75.0;
        TimeLevel = 100.0;
        OxygenStatusBrush = GreenBrush;
        EnergyStatusBrush = GreenBrush;
        IsFailureDialogOpen = false;
        HasLastBadge = false;

        UpdateOrbitMapStatusSummary();
        SelectStage(Stages[0]);
        LoadCrisisScenario(1, preserveAnsweredState: false);
    }

    private void UpdateRoundTitle()
    {
        RoundTitle = RoundLevel switch
        {
            1 => "1. Tur: Aday Astronot",
            2 => "2. Tur: Kıdemli Kadet",
            3 => "3. Tur: Görev Uzmanı",
            _ => $"{RoundLevel}. Tur: Uzay Komutanı"
        };
    }

    // ==================== GÜNÜN MİNİ TESTİ & DİL KOMUTLARI ====================
    partial void OnIsEnglishChanged(bool value)
    {
        OnPropertyChanged(nameof(AstronautRoleTitle));
        OnPropertyChanged(nameof(RoleBadgeText));
        OnPropertyChanged(nameof(AppSubtitle));
        OnPropertyChanged(nameof(TabKomuta));
        OnPropertyChanged(nameof(TabAkademi));
        OnPropertyChanged(nameof(TabHangar));
        OnPropertyChanged(nameof(TabKunye));
        OnPropertyChanged(nameof(TabAyarlar));
        OnPropertyChanged(nameof(MapTitle));
        OnPropertyChanged(nameof(MapSubtitle));
        OnPropertyChanged(nameof(BriefingHeading));
        OnPropertyChanged(nameof(EnterMissionButtonText));
        OnPropertyChanged(nameof(OpenTrainingButtonText));
        OnPropertyChanged(nameof(ProgressHeading));
        OnPropertyChanged(nameof(LevelText));
        OnPropertyChanged(nameof(AchievementsLabel));
        OnPropertyChanged(nameof(QuizHeading));
        UpdateQuizStrings();
    }

    private void UpdateQuizStrings()
    {
        QuizQuestion = IsEnglish
            ? "What fraction of Earth's gravity is experienced on the Moon's surface?"
            : "Ay yörüngesinde ve yüzeyinde yerçekimi Dünya'nın kaçta biridir?";

        QuizOptionA = IsEnglish ? "1/6th (~16.6%)" : "1/6'sı (Yaklaşık %16.6)";
        QuizOptionB = IsEnglish ? "1/3rd (~33.3%)" : "1/3'ü (Yaklaşık %33.3)";

        if (QuizAnswered)
        {
            QuizFeedback = QuizIsSuccess
                ? (IsEnglish ? "✓ Correct Answer! (+50 XP Awarded)" : "✓ Doğru Cevapladınız! (+50 XP Kazandınız)")
                : (IsEnglish ? "✗ Answered! Correct: 1/6th (~16.6%)" : "✗ Cevap Hakkınız Doldu — Doğru Yanıt: 1/6'sı (%16.6)");
        }
    }

    [RelayCommand]
    private void AnswerQuiz(string option)
    {
        // Günün sorusu için sadece 1 cevap hakkı vardır; cevaplandığında seçenekler gizlenir.
        if (QuizAnswered) return;

        QuizAnswered = true;
        if (option == "A")
        {
            QuizIsSuccess = true;
            QuizFeedback = IsEnglish
                ? "✓ Correct Answer! (+50 XP Awarded)"
                : "✓ Doğru Cevapladınız! (+50 XP Kazandınız)";

            CurrentXp = Math.Min(NextLevelXp, CurrentXp + 50);
            XpProgressPercent = Math.Round((double)CurrentXp / NextLevelXp * 100, 1);
            KnowledgeLevel = Math.Min(100, KnowledgeLevel + 10);

            try
            {
                var unlockedSkills = Skills.Where(s => s.IsUnlocked).Select(s => s.Name).ToList();
                _scoreService.UpdatePlayerMedals(
                    Session.AstronautName,
                    KazanilanMadalyalar,
                    CurrentScore,
                    (int)KnowledgeLevel,
                    (int)TimeLevel,
                    RoundLevel,
                    unlockedSkills);
                RefreshLeaderboardData();
            }
            catch { }
        }
        else
        {
            QuizIsSuccess = false;
            QuizFeedback = IsEnglish
                ? "✗ Answered! Correct: 1/6th (~16.6%)"
                : "✗ Cevap Hakkınız Doldu — Doğru Yanıt: 1/6'sı (%16.6)";
        }
    }

    [RelayCommand]
    private void SetLanguage(string lang)
    {
        IsEnglish = (lang == "EN");
    }

    [RelayCommand]
    private void Logout()
    {
        IsPlayingGame = false;
        _onLogout();
    }

    private static string GetRoleDisplayTr(string r) => r.ToLowerInvariant() switch
    {
        "engineer" or "muhendis" => "Mühendis",
        "explorer" or "kasif" => "Kaşif",
        _ => "Pilot"
    };

    private static string GetRoleDisplayEn(string r) => r.ToLowerInvariant() switch
    {
        "engineer" or "muhendis" => "Engineer",
        "explorer" or "kasif" => "Specialist",
        _ => "Pilot"
    };
}
