namespace WebScraper.Models;

public class FlatCoordinate
{
    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public FlatCoordinate(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    public FlatCoordinate() { }

    public FlatCoordinate GetDefaultFlatCoordinate()
    {
        return new FlatCoordinate(0, 0);
    }

    public string ToStringForView()
    {
        return $"Latitude: {Latitude} + Longitude: {Longitude}";
    }
}