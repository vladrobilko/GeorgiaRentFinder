using System;
using System.Collections.Generic;

namespace DataManagement.Models;

public partial class FlatPhoneDto
{
    public long Id { get; set; }

    public string Number { get; set; } = null!;

    public long NumberMentionsOnSite { get; set; }

    public virtual ICollection<FlatInfoDto> FlatInfoDtos { get; set; } = new List<FlatInfoDto>();
}
