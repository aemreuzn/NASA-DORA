namespace NasaAstronautTraining.Models;

public record MissionLocation(
    string Id,
    string Name,
    string RegionName,
    string Description,
    string Difficulty,
    string DefaultTemperature,
    string AccentColor,
    string BackgroundBrushHex,
    string Icon
);

public static class MissionLocations
{
    public static readonly MissionLocation Moon = new(
        Id: "Moon",
        Name: "Ay — Güney Kutbu",
        RegionName: "Artemis III / Shackleton Krateri",
        Description: "Karanlık kraterlerde su buzu keşfi ve kalıcı ay üssü kurulumu.",
        Difficulty: "Orta",
        DefaultTemperature: "-130°C",
        AccentColor: "#93C5FD",
        BackgroundBrushHex: "#08101E",
        Icon: "🌕"
    );

    public static readonly MissionLocation Mars = new(
        Id: "Mars",
        Name: "Mars — Jezero Krateri",
        RegionName: "Perseverance İniş Noktası",
        Description: "Antik nehir deltası tortularında mikrobiyal yaşam izleri arayışı.",
        Difficulty: "Zor",
        DefaultTemperature: "-63°C",
        AccentColor: "#FB923C",
        BackgroundBrushHex: "#160C0A",
        Icon: "🔴"
    );
}
