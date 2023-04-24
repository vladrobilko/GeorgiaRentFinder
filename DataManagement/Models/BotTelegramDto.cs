namespace DataManagement.Models;

public partial class BotTelegramDto
{
    public long Id { get; set; }

    public string? Token { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<BotAdminDto> BotAdminsDto { get; set; } = new List<BotAdminDto>();

    public virtual ICollection<ChannelInfoDto> ChannelInfosDto { get; set; } = new List<ChannelInfoDto>();
}
