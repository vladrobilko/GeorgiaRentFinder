namespace DataManagement.Models;

public partial class FlatInfoDto
{
    public long Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public long FlatPhoneId { get; set; }

    public long Cost { get; set; }

    public string? PageLink { get; set; }

    public long? ViewsOnSite { get; set; }

    public string? AdditionalInformation { get; set; }

    public virtual ICollection<FlatCoordinateDto> FlatCoordinateDtos { get; set; } = new List<FlatCoordinateDto>();

    public virtual ICollection<FlatDateInfoDto> FlatDateInfoDtos { get; set; } = new List<FlatDateInfoDto>();

    public virtual ICollection<FlatLinkImage> FlatLinkImages { get; set; } = new List<FlatLinkImage>();

    public virtual FlatPhoneDto FlatPhone { get; set; } = null!;
}
