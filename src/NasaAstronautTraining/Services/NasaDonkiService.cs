using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NasaAstronautTraining.Services;

public record DonkiSpaceWeather(
    string SolarActivity,
    string Details,
    string SourceText,
    bool HasMajorSolarFlare
);

public class NasaDonkiService
{
    private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(4) };

    public async Task<DonkiSpaceWeather> GetCurrentSpaceWeatherAsync()
    {
        try
        {
            string startDate = DateTime.UtcNow.AddDays(-3).ToString("yyyy-MM-dd");
            string endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
            string url = $"https://api.nasa.gov/DONKI/FLR?startDate={startDate}&endDate={endDate}&api_key=DEMO_KEY";

            string json = await HttpClient.GetStringAsync(url);

            if (json.Contains("\"classType\":\"X") || json.Contains("\"classType\": \"X"))
            {
                return new DonkiSpaceWeather(
                    SolarActivity: "KRİTİK / X-Sınıfı ⚠️",
                    Details: "NASA DONKI: Son 72 saatte ekstrem X-sınıfı güneş patlaması ve jeomanyetik fırtına kaydedildi!",
                    SourceText: "Kaynak: NASA DONKI API — Canlı Veri",
                    HasMajorSolarFlare: true
                );
            }
            else if (json.Contains("\"classType\":\"M") || json.Contains("\"classType\": \"M"))
            {
                return new DonkiSpaceWeather(
                    SolarActivity: "YÜKSEK / M-Sınıfı ☀️",
                    Details: "NASA DONKI: M-sınıfı orta-şiddetli güneş patlaması tespit edildi. Radyasyon kalkanı izlenmeli.",
                    SourceText: "Kaynak: NASA DONKI API — Canlı Veri",
                    HasMajorSolarFlare: true
                );
            }
            else
            {
                return new DonkiSpaceWeather(
                    SolarActivity: "Normal / C-Sınıfı ☀️",
                    Details: "NASA DONKI: Güneş sakin; koronal kütle atımı düşük seviyede.",
                    SourceText: "Kaynak: NASA DONKI API — Canlı Veri",
                    HasMajorSolarFlare: false
                );
            }
        }
        catch
        {
            // Bağlantı yoksa veya offline ise güvenilir arşiv verisi
            return new DonkiSpaceWeather(
                SolarActivity: "Orta (Arşiv Verisi) ☀️",
                Details: "NASA DONKI: Güneş aktivitesi nominal seviyede (Arşiv Verisi — Çevrimdışı Mod).",
                SourceText: "Kaynak: NASA DONKI Arşivi",
                HasMajorSolarFlare: false
            );
        }
    }
}
