namespace DataManagement.Models;

public partial class FlatPhoneDto
{
    public long Id { get; set; }

    public string Number { get; set; } = null!;

    public long NumberMentionsOnSite { get; set; }

    public virtual ICollection<FlatInfoDto> FlatsInfoDto { get; set; } = new List<FlatInfoDto>();
}
