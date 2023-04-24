namespace DataManagement.Models;

public partial class FlatLinkImage
{
    public long Id { get; set; }

    public long? FlatInfoId { get; set; }

    public string? Link { get; set; }

    public virtual FlatInfoDto? FlatInfo { get; set; }
}
