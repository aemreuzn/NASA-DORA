namespace NasaAstronautTraining.Models;

public record SpaceGameItem(
    int Id,
    string TurkishTitle,
    string OriginalTitle,
    string Category,
    string Description,
    string Url,
    string ImagePath,
    string RemoteImageUrl);
