namespace Application.Interfaces.Repository
{
    public interface IChannelInfoRepository
    {
        public DateTime ReadLastCheckDate(long channelId);

        public void UpdateLastCheckDate(long channelId, DateTime lastCheckDate);

        public DateTime ReadLatestCheckDateFromAllChannels();

        public long ReadIdChannelWithLastCheckDate();
    }
}
