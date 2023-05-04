namespace DataManagement.Models;

public partial class FlatDateInfoDto
{
    public long Id { get; set; }

    public long FlatInfoId { get; set; }

    public DateTime SitePublication { get; set; }

    public DateTime? TelegramPublication { get; set; }

    public DateTime? RefusePublication { get; set; }

    public virtual FlatInfoDto FlatInfo { get; set; } = null!;
}
