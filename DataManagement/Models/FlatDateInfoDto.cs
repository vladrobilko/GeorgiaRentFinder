namespace DataManagement.Models;

public partial class FlatDateInfoDto
{
    public long FlatInfoId { get; set; }

    public DateTime SitePublication { get; set; }

    public DateTime? TelegramPublication { get; set; }

    public DateTime? RefusePublication { get; set; }

    public virtual FlatInfoDto? FlatInfoDto { get; set; }
}
