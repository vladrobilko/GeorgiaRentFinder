namespace WebScraper;

public class Coordinate
{
    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public Coordinate(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    public Coordinate() { }

    public Coordinate GetDefaultCoordinate()
    {
        return new Coordinate(0, 0);
    }

    public string ToStringForView()
    {
        return $"Latitude: {Latitude} + Longitude: {Longitude}";
    }
}