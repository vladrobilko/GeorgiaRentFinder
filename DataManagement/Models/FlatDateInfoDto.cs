using System;
using System.Collections.Generic;

namespace DataManagement.Models;

public partial class FlatDateInfoDto
{
    public long FlatInfoId { get; set; }

    public DateOnly SitePublication { get; set; }

    public DateOnly? TelegramPublication { get; set; }

    public DateOnly? RefusePublication { get; set; }

    public virtual FlatInfoDto? FlatInfoDto { get; set; }
}
