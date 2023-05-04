namespace DataManagement.Models;

public partial class FlatCoordinateDto
{
    public long Id { get; set; }

    public long FlatInfoId { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public virtual FlatInfoDto FlatInfo { get; set; } = null!;
}
