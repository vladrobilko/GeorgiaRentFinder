namespace WebScraper.Models;

public class FlatCoordinateModel
{
    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public FlatCoordinateModel(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    public FlatCoordinateModel() { }

    public FlatCoordinateModel GetDefaultFlatCoordinate()
    {
        return new FlatCoordinateModel(0, 0);
    }

    public string ToStringForView()
    {
        return $"Latitude: {Latitude} + Longitude: {Longitude}";
    }
}