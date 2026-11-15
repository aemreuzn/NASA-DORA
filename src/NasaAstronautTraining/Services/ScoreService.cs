using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using NasaAstronautTraining.Models;

namespace NasaAstronautTraining.Services;

public class ScoreService
{
    private static readonly string ScoreFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dora_skorlar.json");
    private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(3) };

    public ScoreService()
    {
        EnsureDatabaseInitialized();
    }

    private void EnsureDatabaseInitialized()
    {
        try
        {
            var existing = GetAllScores();
            // Eğer dosya yoksa veya eski rastgele test isimleri (Yusuf, Ayşe, Can, Zeynep vb.) varsa temizle
            bool hasRandomNames = existing.Any(s => s.KullaniciAdi.Contains("Yusuf") || s.KullaniciAdi.Contains("Can T.") || s.KullaniciAdi.Contains("Zeynep S.") || s.KullaniciAdi == "Neil A.");

            if (!File.Exists(ScoreFilePath) || hasRandomNames || existing.Count == 0 || existing.Any(s => s.AltinSayisi > 0 && (s.KullaniciAdi == "Alper Gezeravcı" || s.KullaniciAdi == "Tuva Cihangir Atasever" || s.KullaniciAdi == "Artemis Adayı")))
            {
                // Tüm astronotlar 0 altınla başlar; o hesaptan görevler yapıldıkça altınlar kazanılır ve sıralama güncellenir
                var initialScores = new List<OyuncuKayit>
                {
                    new()
                    {
                        KullaniciAdi = "Neil Armstrong",
                        AltinSayisi = 0,
                        GumusSayisi = 0,
                        BronzSayisi = 0,
                        ToplamPuan = 0,
                        AsamaDetay = new(),
                        SonOynanma = DateTime.Now
                    },
                    new()
                    {
                        KullaniciAdi = "Alper Gezeravcı",
                        AltinSayisi = 0,
                        GumusSayisi = 0,
                        BronzSayisi = 0,
                        ToplamPuan = 0,
                        AsamaDetay = new(),
                        SonOynanma = DateTime.Now.AddMinutes(-5)
                    },
                    new()
                    {
                        KullaniciAdi = "Tuva Cihangir Atasever",
                        AltinSayisi = 0,
                        GumusSayisi = 0,
                        BronzSayisi = 0,
                        ToplamPuan = 0,
                        AsamaDetay = new(),
                        SonOynanma = DateTime.Now.AddMinutes(-10)
                    },
                    new()
                    {
                        KullaniciAdi = "Artemis Adayı",
                        AltinSayisi = 0,
                        GumusSayisi = 0,
                        BronzSayisi = 0,
                        ToplamPuan = 0,
                        AsamaDetay = new(),
                        SonOynanma = DateTime.Now.AddMinutes(-15)
                    }
                };

                SaveAll(initialScores);
            }
        }
        catch
        {
            // Ignore init fallback
        }
    }

    public List<OyuncuKayit> GetAllScores()
    {
        try
        {
            if (File.Exists(ScoreFilePath))
            {
                string json = File.ReadAllText(ScoreFilePath);
                var list = JsonSerializer.Deserialize<List<OyuncuKayit>>(json);
                if (list != null) return list;
            }
        }
        catch { }

        return new List<OyuncuKayit>();
    }

    public void SaveAll(List<OyuncuKayit> scores)
    {
        try
        {
            string json = JsonSerializer.Serialize(scores, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ScoreFilePath, json);
        }
        catch { }
    }

    public OyuncuKayit GetOrCreatePlayer(string username)
    {
        var scores = GetAllScores();
        var existing = scores.FirstOrDefault(s => string.Equals(s.KullaniciAdi, username, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            return existing;
        }

        var newRecord = new OyuncuKayit
        {
            KullaniciAdi = username,
            AltinSayisi = 0,
            GumusSayisi = 0,
            BronzSayisi = 0,
            SonOynanma = DateTime.Now
        };
        scores.Add(newRecord);
        SaveAll(scores);
        return newRecord;
    }

    public void UpdatePlayerMedals(string username, Dictionary<int, MadalyaTuru> stageMedals, int totalScore, int knowledgeScore = 0, int remainingTime = 100, int roundLevel = 1, List<string>? skills = null)
    {
        var scores = GetAllScores();
        var record = scores.FirstOrDefault(s => string.Equals(s.KullaniciAdi, username, StringComparison.OrdinalIgnoreCase));
        if (record == null)
        {
            record = new OyuncuKayit { KullaniciAdi = username };
            scores.Add(record);
        }

        // Madalyaları güncelle
        foreach (var kvp in stageMedals)
        {
            record.AsamaDetay[kvp.Key] = kvp.Value.ToString();
        }

        record.AltinSayisi = record.AsamaDetay.Values.Count(v => v == nameof(MadalyaTuru.Altin));
        record.GumusSayisi = record.AsamaDetay.Values.Count(v => v == nameof(MadalyaTuru.Gumus));
        record.BronzSayisi = record.AsamaDetay.Values.Count(v => v == nameof(MadalyaTuru.Bronz));
        record.ToplamPuan = Math.Max(record.ToplamPuan, totalScore);
        record.BilgiPuani = Math.Max(record.BilgiPuani, knowledgeScore);
        record.KalanZaman = Math.Max(record.KalanZaman, remainingTime);
        record.TurSeviyesi = Math.Max(record.TurSeviyesi, roundLevel);
        if (skills != null)
        {
            record.KazanilanBeceriler = skills.Distinct().ToList();
        }
        record.SonOynanma = DateTime.Now;

        SaveAll(scores);

        // Arka planda merkezi REST API'ye post etmeyi dene (asenkron / fire-and-forget)
        _ = SendScoreToApiAsync(record);
    }

    public List<LeaderboardItem> GetLeaderboard(string filterMode = "madalya")
    {
        var scores = GetAllScores();
        IEnumerable<OyuncuKayit> query = filterMode.ToLowerInvariant() switch
        {
            "bilgi" => scores.OrderByDescending(s => s.BilgiPuani).ThenByDescending(s => s.AltinSayisi),
            "hiz" => scores.OrderByDescending(s => s.KalanZaman).ThenByDescending(s => s.AltinSayisi),
            _ => scores.OrderByDescending(s => s.AltinSayisi).ThenByDescending(s => s.BilgiPuani).ThenByDescending(s => s.ToplamPuan)
        };

        var ordered = query.Take(10).ToList();
        var result = new List<LeaderboardItem>();
        for (int i = 0; i < ordered.Count; i++)
        {
            var item = ordered[i];
            result.Add(new LeaderboardItem(
                Rank: i + 1,
                DisplayName: item.KullaniciAdi,
                GoldCount: item.AltinSayisi,
                SilverCount: item.GumusSayisi,
                BronzeCount: item.BronzSayisi,
                KnowledgeScore: item.BilgiPuani,
                RemainingTime: item.KalanZaman,
                RoundLevel: Math.Max(1, item.TurSeviyesi),
                TotalScore: item.ToplamPuan,
                IsFirst: (i == 0)
            ));
        }

        return result;
    }

    public int GetPlayerRank(string username)
    {
        var scores = GetAllScores();
        var ordered = scores
            .OrderByDescending(s => s.AltinSayisi)
            .ThenByDescending(s => s.BilgiPuani)
            .ThenByDescending(s => s.ToplamPuan)
            .ToList();

        int index = ordered.FindIndex(s => string.Equals(s.KullaniciAdi, username, StringComparison.OrdinalIgnoreCase));
        return index >= 0 ? index + 1 : ordered.Count + 1;
    }

    public int GetTotalPlayerCount()
    {
        return Math.Max(3, GetAllScores().Count);
    }

    public async Task SendScoreToApiAsync(OyuncuKayit record)
    {
        try
        {
            string url = "https://dora-api.example.com/skor";
            string payload = JsonSerializer.Serialize(record);
            using var content = new StringContent(payload, Encoding.UTF8, "application/json");
            await HttpClient.PostAsync(url, content);
        }
        catch
        {
            // API henüz hazır değilse veya offline ise sessizce yerel skor korunur
        }
    }
}

public record LeaderboardItem(
    int Rank,
    string DisplayName,
    int GoldCount,
    int SilverCount,
    int BronzeCount,
    int KnowledgeScore,
    int RemainingTime,
    int RoundLevel,
    int TotalScore,
    bool IsFirst = false);
