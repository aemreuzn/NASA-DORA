namespace NasaAstronautTraining.Models;

public enum MadalyaTuru
{
    Yok = 0,
    Bronz = 1,
    Gumus = 2,
    Altin = 3
}

public static class MadalyaExtensions
{
    public static string ToEmoji(this MadalyaTuru turu) => turu switch
    {
        MadalyaTuru.Altin => "🥇",
        MadalyaTuru.Gumus => "🥈",
        MadalyaTuru.Bronz => "🥉",
        _ => "—"
    };

    public static string ToName(this MadalyaTuru turu) => turu switch
    {
        MadalyaTuru.Altin => "Altın",
        MadalyaTuru.Gumus => "Gümüş",
        MadalyaTuru.Bronz => "Bronz",
        _ => "Henüz Yok"
    };

    public static string ToHexColor(this MadalyaTuru turu) => turu switch
    {
        MadalyaTuru.Altin => "#FFD700",
        MadalyaTuru.Gumus => "#C0C0C0",
        MadalyaTuru.Bronz => "#CD7F32",
        _ => "#475569"
    };
}
