namespace DataManagement.Models;

public partial class ChannelInfoDto
{
    public long Id { get; set; }

    public long? BotTelegramId { get; set; }

    public string ChannelName { get; set; }

    public DateTime LastCheckDate { get; set; }

    public virtual BotTelegramDto? BotTelegram { get; set; }
}
