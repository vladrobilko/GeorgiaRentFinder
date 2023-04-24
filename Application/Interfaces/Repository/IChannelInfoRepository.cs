namespace Application.Interfaces.Repository
{
    public interface IChannelInfoRepository
    {
        public DateTime GetLastCheckDate(long channelId);
    }
}
