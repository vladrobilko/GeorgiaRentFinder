namespace Application.Interfaces.Repository
{
    public interface IChannelInfoRepository
    {
        public DateTime ReadLastCheckDateById(long channelId);

        public void UpdateLastCheckDate(long channelId, DateTime lastCheckDate);

        public string ReadIdChannelWithLastCheckDate();
    }
}
