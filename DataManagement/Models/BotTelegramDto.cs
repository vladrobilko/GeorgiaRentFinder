using System;
using System.Collections.Generic;

namespace DataManagement.Models;

public partial class BotTelegramDto
{
    public long Id { get; set; }

    public string? Token { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<BotAdminDto> BotAdminDtos { get; set; } = new List<BotAdminDto>();

    public virtual ICollection<ChannelInfoDto> ChannelInfoDtos { get; set; } = new List<ChannelInfoDto>();
}
