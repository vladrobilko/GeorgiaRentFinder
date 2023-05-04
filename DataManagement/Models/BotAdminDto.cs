namespace DataManagement.Models;

public partial class BotAdminDto
{
    public long Id { get; set; }

    public long? BotTelegramId { get; set; }

    public string? Name { get; set; }

    public virtual BotTelegramDto? BotTelegram { get; set; }
}
