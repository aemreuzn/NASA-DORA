using System;
using System.Collections.Generic;

namespace NasaAstronautTraining.Models;

public class OyuncuKayit
{
    public string KullaniciAdi { get; set; } = string.Empty;
    public int AltinSayisi { get; set; }
    public int GumusSayisi { get; set; }
    public int BronzSayisi { get; set; }
    public int ToplamPuan { get; set; }
    public int BilgiPuani { get; set; }
    public int KalanZaman { get; set; } = 100;
    public int TurSeviyesi { get; set; } = 1;
    public List<string> KazanilanBeceriler { get; set; } = new();
    public Dictionary<int, string> AsamaDetay { get; set; } = new(); // "1" -> "Altin"
    public DateTime SonOynanma { get; set; } = DateTime.Now;
}
