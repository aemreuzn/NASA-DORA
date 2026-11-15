using System.Collections.Generic;

namespace NasaAstronautTraining.Models;

public record DecisionOption(
    string Key,
    string Text,
    string CostSummary,
    double OxygenDelta,
    double EnergyDelta,
    double MoraleDelta,
    double KnowledgeDelta,
    double TimeDelta,
    bool IsWrong,
    bool IsOptimal,
    string LearningNasaFact
);

public class Karar
{
    public string Metin { get; set; } = string.Empty;
    public int dOksijen { get; set; }
    public int dEnerji { get; set; }
    public int dMoral { get; set; }
    public int dBilgi { get; set; }
    public int dZaman { get; set; }
    public string Etiket { get; set; } = "Standart"; // "Standart" / "Riskli" / "Optimal"

    public string FormatMaliyet()
    {
        var parts = new List<string>();
        if (dOksijen != 0) parts.Add($"Oksijen {(dOksijen > 0 ? "+" : "")}{dOksijen}");
        if (dEnerji != 0) parts.Add($"Enerji {(dEnerji > 0 ? "+" : "")}{dEnerji}");
        if (dMoral != 0) parts.Add($"Moral {(dMoral > 0 ? "+" : "")}{dMoral}");
        if (dBilgi != 0) parts.Add($"Bilgi {(dBilgi > 0 ? "+" : "")}{dBilgi}");
        if (dZaman != 0) parts.Add($"Zaman {(dZaman > 0 ? "+" : "")}{dZaman}");
        return string.Join("  •  ", parts);
    }
}

public class Senaryo
{
    public string KrizBasligi { get; set; } = string.Empty;
    public string KrizAciklamasi { get; set; } = string.Empty;
    public Karar KararA { get; set; } = new();
    public Karar KararB { get; set; } = new();
    public Karar KararC { get; set; } = new();
    public string NasaGercegi { get; set; } = string.Empty;
    public string IpucuMetni { get; set; } = string.Empty;
}

public static class SenaryoHavuzuData
{
    public static Dictionary<int, List<Senaryo>> Olustur()
    {
        return new Dictionary<int, List<Senaryo>>
        {
            // ==================== 1. ASCAN SEÇİMİ (4 Senaryo) ====================
            [1] = new List<Senaryo>
            {
                new Senaryo
                {
                    KrizBasligi = "Biyometrik Uyarı",
                    KrizAciklamasi = "Kalp atış hızın limite yaklaştı!",
                    KararA = new Karar { Metin = "Nefes egzersizi yap", dOksijen = -10, dMoral = 10, Etiket = "Standart" },
                    KararB = new Karar { Metin = "Zorla devam et", dEnerji = -25, dBilgi = 10, Etiket = "Riskli" },
                    KararC = new Karar { Metin = "Görevi ertele, dinlen", dZaman = -20, dOksijen = 15, dMoral = 15, Etiket = "Optimal" },
                    NasaGercegi = "Astronotlar stres altında kalp hızını biofeedback ile kontrol etmeyi öğrenir.",
                    IpucuMetni = "Biyolojik sınırları zorlamak yerine kontrollü dinlenme ve diyafram nefesi kalp ritmini hızla dengeler."
                },
                new Senaryo
                {
                    KrizBasligi = "Ekip Stresi",
                    KrizAciklamasi = "Takım arkadaşın simülasyonda panikliyor!",
                    KararA = new Karar { Metin = "Onu yatıştır, duraksın", dZaman = -15, dMoral = 20, Etiket = "Standart" },
                    KararB = new Karar { Metin = "Göreve devam et, o toparlasın", dMoral = -20, dBilgi = 10, Etiket = "Riskli" },
                    KararC = new Karar { Metin = "Görevi böl, ona başka iş ver", dZaman = -10, dMoral = 15, dBilgi = 5, Etiket = "Optimal" },
                    NasaGercegi = "ISS’de psikolojik dayanışma, teknik beceri kadar kritik sayılır.",
                    IpucuMetni = "Ekip yönetiminde (CRM) panikleyen üyeye odaklanabileceği yapılandırılmış bir alt görev vermek en etkili yöntemdir."
                },
                new Senaryo
                {
                    KrizBasligi = "Nöromotor Refleks",
                    KrizAciklamasi = "Refleks testinde sol elin yavaş tepki verdi!",
                    KararA = new Karar { Metin = "Testi yeniden iste", dZaman = -20, dMoral = -10, dBilgi = 15, Etiket = "Standart" },
                    KararB = new Karar { Metin = "Devam et, görmezden gel", dOksijen = -5, dEnerji = -10, Etiket = "Riskli" },
                    KararC = new Karar { Metin = "Doktora bildir, şeffaf ol", dZaman = -10, dMoral = 20, dBilgi = 20, Etiket = "Optimal" },
                    NasaGercegi = "NASA’da sağlık şeffaflığı, gizlemekten her zaman daha değerli kabul edilir.",
                    IpucuMetni = "Uçuş cerrahlarına (Flight Surgeons) her belirtiyi şeffafça bildirmek hem güvenliği hem ekip güvenini artırır."
                },
                new Senaryo
                {
                    KrizBasligi = "Su Altı Hayatta Kalma",
                    KrizAciklamasi = "Su altı hayatta kalma testinde ekipman arızalandı!",
                    KararA = new Karar { Metin = "Testten çık, güvenliği seç", dZaman = -25, dMoral = 10, Etiket = "Standart" },
                    KararB = new Karar { Metin = "Arızalı ekipmanla devam et", dOksijen = -30, dBilgi = 20, Etiket = "Riskli" },
                    KararC = new Karar { Metin = "Yedek ekipman iste, kısa mola", dZaman = -15, dOksijen = 5, dBilgi = 15, Etiket = "Optimal" },
                    NasaGercegi = "NBL’de her ekipman arızası gerçek EVA’daki gibi protokole bağlıdır.",
                    IpucuMetni = "Yedek regülatör ve ekipman istemek hem eğitimi kesintiye uğratmaz hem de oksijen güvenliğini korur."
                }
            },

            // ==================== 2. ISS SİSTEMLERİ (4 Senaryo) ====================
            [2] = new List<Senaryo>
            {
                new Senaryo
                {
                    KrizBasligi = "Mikro-Meteorit Çarpması",
                    KrizAciklamasi = "Güneş panellerine mikro meteorit çarptı!",
                    KararA = new Karar { Metin = "Isıtıcıları kapat", dOksijen = -20, dEnerji = 10, Etiket = "Standart" },
                    KararB = new Karar { Metin = "Yedek batarya aç", dEnerji = -10, dMoral = 10, Etiket = "Riskli" },
                    KararC = new Karar { Metin = "Paneli ekiple onar", dEnerji = -15, dBilgi = 25, dZaman = -20, Etiket = "Optimal" },
                    NasaGercegi = "ISS güneş panelleri toplamda 2500 m² alan kaplar, dünyanın en büyük uzay güç sistemidir.",
                    IpucuMetni = "Kalıcı enerji kaybını önlemek için ekiple birlikte hasarlı hücre hattını onarmak en yüksek bilimsel kazanımı sağlar."
                },
                new Senaryo
                {
                    KrizBasligi = "Yaşam Desteği (ECLSS)",
                    KrizAciklamasi = "Hava filtresi tıkandı, CO₂ yükseliyor!",
                    KararA = new Karar { Metin = "Yedek filtreyi devreye al", dEnerji = -15, dOksijen = 20, Etiket = "Standart" },
                    KararB = new Karar { Metin = "Tüm aktiviteyi durdur, bekle", dZaman = -20, dOksijen = 10, dMoral = -15, Etiket = "Riskli" },
                    KararC = new Karar { Metin = "Filtreyi ekiple temizle", dBilgi = 20, dOksijen = 15, dZaman = -15, Etiket = "Optimal" },
                    NasaGercegi = "ISS’de hava filtresi bakımı haftada 2 kez yapılır, her astronot bunu öğrenir.",
                    IpucuMetni = "CDRA karbondioksit filtrelerini ekiple temizlemek enerji harcamadan temiz hava döngüsünü geri kazandırır."
                },
                new Senaryo
                {
                    KrizBasligi = "Su Geri Dönüşümü",
                    KrizAciklamasi = "Su geri dönüşüm sistemi %40 kapasitede çalışıyor!",
                    KararA = new Karar { Metin = "Su tayınını azalt", dMoral = -20, dOksijen = 10, Etiket = "Riskli" },
                    KararB = new Karar { Metin = "Sistemi kapat, onar", dZaman = -25, dBilgi = 20, dOksijen = 15, Etiket = "Optimal" },
                    KararC = new Karar { Metin = "Dünya’ya bildir, talimat bekle", dZaman = -30, dMoral = 10, dBilgi = 10, Etiket = "Standart" },
                    NasaGercegi = "ISS su sistemi idrarı da dahil tüm suyu %93 oranında geri dönüştürür.",
                    IpucuMetni = "Su elektroliz yoluyla oksijen üretiminde de kullanıldığı için sistemi onarmak hem su hem O₂ kazandırır."
                },
                new Senaryo
                {
                    KrizBasligi = "Radyasyon Alarmı",
                    KrizAciklamasi = "Güneş fırtınası geliyor, radyasyon artıyor!",
                    KararA = new Karar { Metin = "Kalkanı aç", dEnerji = -30, dOksijen = 5, Etiket = "Standart" },
                    KararB = new Karar { Metin = "Zırhlı modüle çekil", dZaman = -20, dMoral = -10, dOksijen = 15, Etiket = "Optimal" },
                    KararC = new Karar { Metin = "Bilimi sürdür, veri topla", dBilgi = 40, dOksijen = -15, dEnerji = -10, Etiket = "Riskli" },
                    NasaGercegi = "Güneş fırtınası sırasında astronotlar ISS’in merkezi modülüne sığınır.",
                    IpucuMetni = "Korunaklı modüle çekilmek veya kalkan açmak radyasyon dozunu düşürür; veri toplamak ise yüksek bilgi verir ama kaynak tüketir."
                }
            },

            // ==================== 3. T-38 UÇUŞU (4 Senaryo) ====================
            [3] = new List<Senaryo>
            {
                new Senaryo
                {
                    KrizBasligi = "Kritik Yakıt Seviyesi",
                    KrizAciklamasi = "Yakıt azalıyor, hedefe 200 km kaldı!",
                    KararA = new Karar { Metin = "İrtifayı düşür", dOksijen = -15, dZaman = -10, Etiket = "Standart" },
                    KararB = new Karar { Metin = "Hızı artır", dEnerji = -30, dBilgi = 10, Etiket = "Riskli" },
                    KararC = new Karar { Metin = "Acil iniş planı yap", dZaman = -20, dMoral = 15, dBilgi = 20, Etiket = "Optimal" },
                    NasaGercegi = "T-38 pilotları her uçuşta 3 farklı acil iniş planı hazır tutar.",
                    IpucuMetni = "Yakıt kritikken hızı artırmak yakıtı hızla bitirir; alternatif pist ve acil iniş planı en güvenli seçenektir."
                },
                new Senaryo
                {
                    KrizBasligi = "Aviyonik Kararma",
                    KrizAciklamasi = "Kokpitte elektrik arızası, ekranlar karardı!",
                    KararA = new Karar { Metin = "Analog aletlere geç", dBilgi = 25, dZaman = -15, Etiket = "Optimal" },
                    KararB = new Karar { Metin = "Sistemi yeniden başlat", dEnerji = -20, dZaman = -20, Etiket = "Riskli" },
                    KararC = new Karar { Metin = "Yardım çağır, konum bildir", dMoral = 10, dZaman = -25, dBilgi = 15, Etiket = "Standart" },
                    NasaGercegi = "Tüm NASA pilotları analog navigasyonu dijital sistemler olmadan yapabilmek zorundadır.",
                    IpucuMetni = "Mekanik jiroskop ve basınç altimetresi elektrik kesintisinde bile çalışmaya devam eder."
                },
                new Senaryo
                {
                    KrizBasligi = "Şiddetli Türbülans",
                    KrizAciklamasi = "Türbülansa girdin, ekipman sallanıyor!",
                    KararA = new Karar { Metin = "Yavaşla, stabil hava ara", dZaman = -20, dOksijen = 10, Etiket = "Standart" },
                    KararB = new Karar { Metin = "Hızla geç, süreyi koru", dOksijen = -20, dEnerji = -15, Etiket = "Riskli" },
                    KararC = new Karar { Metin = "İrtifa değiştir, türbülanstan çık", dBilgi = 20, dZaman = -15, dEnerji = -10, Etiket = "Optimal" },
                    NasaGercegi = "Uzay araçları da atmosfere girerken benzer türbülans kuvvetleriyle karşılaşır.",
                    IpucuMetni = "Jet akımı türbülansları dar irtifa bantlarında oluşur; irtifa değiştirmek sarsıntıyı hızla keser."
                },
                new Senaryo
                {
                    KrizBasligi = "Sıfır Görüş",
                    KrizAciklamasi = "Görüş sıfır, sis bastırdı!",
                    KararA = new Karar { Metin = "ILS sistemiyle in", dBilgi = 20, dMoral = 10, dZaman = -10, Etiket = "Optimal" },
                    KararB = new Karar { Metin = "Dön, alternatif havaalanına git", dZaman = -30, dMoral = -10, Etiket = "Riskli" },
                    KararC = new Karar { Metin = "Yüksekliği koru, sisin geçmesini bekle", dEnerji = -20, dZaman = -25, dBilgi = 10, Etiket = "Standart" },
                    NasaGercegi = "Astronotlar kör iniş eğitimini ISS docking simülasyonlarında da kullanır.",
                    IpucuMetni = "ILS (Aletli İniş Sistemi) radyo sinyalleriyle sıfır görüşte bile uçağı piste santimetrik indirir."
                }
            },

            // ==================== 4. NBL HAVUZU (4 Senaryo) ====================
            [4] = new List<Senaryo>
            {
                new Senaryo
                {
                    KrizBasligi = "EVA Basınç Kaybı",
                    KrizAciklamasi = "Uzay kıyafetinde küçük sızıntı!",
                    KararA = new Karar { Metin = "EVA’yı durdur", dOksijen = -5, dEnerji = -10, dZaman = -15, Etiket = "Standart" },
                    KararB = new Karar { Metin = "Hızla bitir", dOksijen = -35, dBilgi = 15, Etiket = "Riskli" },
                    KararC = new Karar { Metin = "Sızıntıyı bantla, devam et", dOksijen = -10, dBilgi = 25, dZaman = -10, Etiket = "Optimal" },
                    NasaGercegi = "EVA kıyafetleri 8 katmandan oluşur, küçük sızıntılar protokolle yönetilebilir.",
                    IpucuMetni = "Acil durum yama kiti ve yedek basınç regülatörü küçük sızıntıları güvenli sınırda tutar."
                },
                new Senaryo
                {
                    KrizBasligi = "Serbest Nesne",
                    KrizAciklamasi = "Alet çantası elinizden kaçtı, yüzüyor!",
                    KararA = new Karar { Metin = "Peşinden git", dZaman = -20, dOksijen = -10, Etiket = "Riskli" },
                    KararB = new Karar { Metin = "Yedek aletle devam et", dBilgi = -10, dMoral = -15, dZaman = -10, Etiket = "Standart" },
                    KararC = new Karar { Metin = "Görevi bitir, sonra topla", dBilgi = 15, dZaman = -5, dOksijen = 5, Etiket = "Optimal" },
                    NasaGercegi = "ISS dışında kaybolan nesneler orbital hız kazanarak tehlikeli moloza dönüşebilir.",
                    IpucuMetni = "Emniyet halatını (tether) çözüp sürüklenen nesnenin peşinden atlamak yasaktır; önce güvenlik ve görev gelir."
                },
                new Senaryo
                {
                    KrizBasligi = "Yön Kaybı",
                    KrizAciklamasi = "Ortak, karanlık bölgede yönünü kaybetti!",
                    KararA = new Karar { Metin = "Git ve bul", dZaman = -25, dMoral = 20, Etiket = "Standart" },
                    KararB = new Karar { Metin = "Sesle yönlendir", dZaman = -15, dBilgi = 15, dMoral = 15, Etiket = "Optimal" },
                    KararC = new Karar { Metin = "Mission Control’ü ara", dZaman = -20, dMoral = 10, dBilgi = 20, Etiket = "Standart" },
                    NasaGercegi = "Uzay yürüyüşlerinde her astronot çifti sürekli görsel temas halinde olmak zorundadır.",
                    IpucuMetni = "Telsiz üzerinden sakin sesli yönlendirme ve kask feneri referansı ortağı en hızlı şekilde rotaya döndürür."
                },
                new Senaryo
                {
                    KrizBasligi = "Termal Düşüş",
                    KrizAciklamasi = "Su sıcaklığı düştü, hipotermi riski!",
                    KararA = new Karar { Metin = "Hemen çık", dZaman = -10, dOksijen = 20, dMoral = 10, Etiket = "Standart" },
                    KararB = new Karar { Metin = "Isıtıcı kıyafet giy, devam et", dEnerji = -20, dBilgi = 20, Etiket = "Riskli" },
                    KararC = new Karar { Metin = "Görevi kısalt, kritik parçaları bitir", dZaman = -15, dOksijen = 10, dBilgi = 15, Etiket = "Optimal" },
                    NasaGercegi = "NBL havuzu 86°F’de tutulur, gerçek uzayda kıyafet içi ısı sistemi hayati önem taşır.",
                    IpucuMetni = "Termal anomali durumunda yalnızca birincil kritik montaj adımlarını tamamlayıp güvenli çıkış yapmak en doğrusudur."
                }
            },

            // ==================== 5. CANADARM2 (4 Senaryo) ====================
            [5] = new List<Senaryo>
            {
                new Senaryo
                {
                    KrizBasligi = "Koordinat Kilitlenmesi",
                    KrizAciklamasi = "Robotik kol yanlış koordinata kilitlendi!",
                    KararA = new Karar { Metin = "Sıfırla", dEnerji = -20, dZaman = -15, Etiket = "Standart" },
                    KararB = new Karar { Metin = "Manuel override", dOksijen = -10, dEnerji = -40, Etiket = "Riskli" },
                    KararC = new Karar { Metin = "Yazılımı güncelle", dBilgi = 25, dZaman = -25, dEnerji = -10, Etiket = "Optimal" },
                    NasaGercegi = "Canadarm2 saniyede 37 cm hareket edebilir, hassas görevlerde milimetre hassasiyeti gerekir.",
                    IpucuMetni = "Kinematik yazılım tablolarını güncellemek kolu zorlamadan doğru koordinata oturtur."
                },
                new Senaryo
                {
                    KrizBasligi = "Aşırı Kütle Ataleti",
                    KrizAciklamasi = "Kargo modülü beklenenden 2 ton ağır!",
                    KararA = new Karar { Metin = "Yavaşça taşı, güç artır", dEnerji = -30, dZaman = -20, Etiket = "Standart" },
                    KararB = new Karar { Metin = "Modülü böl, iki seferde taşı", dZaman = -35, dBilgi = 20, Etiket = "Riskli" },
                    KararC = new Karar { Metin = "Fizik hesapla, momentum kullan", dBilgi = 30, dEnerji = -15, dZaman = -15, Etiket = "Optimal" },
                    NasaGercegi = "Uzayda kütle korunur, ağırlık kaybolur. Canadarm2 milyonlarca dolar değerinde yük taşır.",
                    IpucuMetni = "Ağırlıksız ortamda bile kütle ataleti (momentum) vardır; düşük ivmeli momentum hesabı enerjiyi korur."
                },
                new Senaryo
                {
                    KrizBasligi = "Harmonik Titreşim",
                    KrizAciklamasi = "Kol ekleminde titreşim tespit edildi!",
                    KararA = new Karar { Metin = "Görevi durdur, bakım yap", dZaman = -25, dBilgi = 20, Etiket = "Standart" },
                    KararB = new Karar { Metin = "Titreşimi görmezden gel", dEnerji = -10, dMoral = -20, dBilgi = -10, Etiket = "Riskli" },
                    KararC = new Karar { Metin = "Hızı yarıya indir, tamamla", dZaman = -20, dBilgi = 15, dEnerji = -10, Etiket = "Optimal" },
                    NasaGercegi = "Canadarm2'nin sensörleri titreşimi 0.001mm hassasiyetle algılar.",
                    IpucuMetni = "Açısal hızı yarıya indirmek rezonans titreşimini sönümler ve yükü güvenle kenetler."
                },
                new Senaryo
                {
                    KrizBasligi = "Optik Kesinti",
                    KrizAciklamasi = "Kamera görüntüsü dondu, kör çalışıyorsun!",
                    KararA = new Karar { Metin = "Dur, kamerayı yeniden başlat", dZaman = -20, dMoral = -10, Etiket = "Standart" },
                    KararB = new Karar { Metin = "Yedek kameraya geç", dBilgi = 15, dZaman = -10, dEnerji = -10, Etiket = "Optimal" },
                    KararC = new Karar { Metin = "Sensör verileriyle devam et", dBilgi = 30, dOksijen = -10, dZaman = -15, Etiket = "Riskli" },
                    NasaGercegi = "ISS dışındaki kameralar kozmik radyasyona karşı özel olarak korunur.",
                    IpucuMetni = "Yedek kamera açısı veya kuvvet-moment sensör telemetrisi görüntüsüz anlarda güvenli rehberlik sağlar."
                }
            },

            // ==================== 6. FIRLATMA (4 Senaryo) ====================
            [6] = new List<Senaryo>
            {
                new Senaryo
                {
                    KrizBasligi = "Sensör Dalgalanması",
                    KrizAciklamasi = "Geri sayımda hava basınç sensöründe dalgalanma!",
                    KararA = new Karar { Metin = "Geri sayımı durdur", dZaman = -15, dMoral = 10, dBilgi = 15, Etiket = "Standart" },
                    KararB = new Karar { Metin = "Fırlatmaya devam", dOksijen = -40, dEnerji = -40, dMoral = -30, Etiket = "Riskli" },
                    KararC = new Karar { Metin = "GO/NO-GO protokolü", dOksijen = 15, dEnerji = 10, dMoral = 25, dBilgi = 30, dZaman = -10, Etiket = "Optimal" },
                    NasaGercegi = "GO/NO-GO protokolü Apollo 13'te hayat kurtardı.",
                    IpucuMetni = "GO/NO-GO protokolüyle yedek sensör kanalını doğrulamak hem güvenliği hem kalkış penceresini kurtarır."
                },
                new Senaryo
                {
                    KrizBasligi = "Kriyojenik Sızıntı",
                    KrizAciklamasi = "Yakıt dolumu sırasında küçük sızıntı!",
                    KararA = new Karar { Metin = "Dolumu durdur, onar", dZaman = -30, dBilgi = 20, Etiket = "Standart" },
                    KararB = new Karar { Metin = "Kabul edilebilir say, devam et", dOksijen = -20, dMoral = -25, Etiket = "Riskli" },
                    KararC = new Karar { Metin = "NASA mühendisleriyle analiz et", dZaman = -20, dBilgi = 30, dMoral = 15, Etiket = "Optimal" },
                    NasaGercegi = "Challenger felaketinin nedeni de ‘kabul edilebilir risk’ olarak geçilen bir anormallikti.",
                    IpucuMetni = "Sıvı hidrojen sızıntılarında mühendislik ekibiyle termal conta analizi yapmak en doğru protokoldür."
                },
                new Senaryo
                {
                    KrizBasligi = "Meteoroloji Uyarısı",
                    KrizAciklamasi = "Fırlatmadan 3 saat önce hava kötüleşti!",
                    KararA = new Karar { Metin = "Fırlatmayı ertele", dZaman = -40, dMoral = -15, dBilgi = 10, Etiket = "Standart" },
                    KararB = new Karar { Metin = "Hava penceresini zorla", dOksijen = -30, dEnerji = -30, dMoral = -20, Etiket = "Riskli" },
                    KararC = new Karar { Metin = "Meteoroloji verisini analiz et, karar ver", dBilgi = 25, dZaman = -25, dMoral = 10, Etiket = "Optimal" },
                    NasaGercegi = "NASA’nın fırlatma için 11 ayrı hava kriteri vardır, biri tutmazsa ertelenir.",
                    IpucuMetni = "Yüksek irtifa rüzgar balonlarından gelen verileri analiz ederek güvenli kalkış koridoru bulunabilir."
                },
                new Senaryo
                {
                    KrizBasligi = "Yörünge Sapması",
                    KrizAciklamasi = "Fırlatma sonrası yörüngede küçük sapma var!",
                    KararA = new Karar { Metin = "Düzeltme yakıtı kullan", dEnerji = -25, dOksijen = 10, Etiket = "Standart" },
                    KararB = new Karar { Metin = "Sapmayla devam et, görmezden gel", dMoral = -30, dBilgi = -20, Etiket = "Riskli" },
                    KararC = new Karar { Metin = "Yörüngeyi hesapla, optimal düzeltme", dBilgi = 35, dEnerji = -15, dZaman = -20, Etiket = "Optimal" },
                    NasaGercegi = "Apollo misyonlarında yörünge düzeltmeleri saniyenin binde biri hassasiyetle yapıldı.",
                    IpucuMetni = "Yörünge mekaniği (Orbital Mechanics) hesaplamasıyla en tepe noktada (apogee) yapılan kısa ateşleme yakıtı korur."
                }
            }
        };
    }
}
