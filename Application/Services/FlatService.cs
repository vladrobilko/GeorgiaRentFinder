using Application.Interfaces;
using Application.Interfaces.Repository;
using WebScraper.SsDotGe;

namespace Application.Services
{
    public class FlatService : IFlatService
    {
        private readonly IFlatRepository _flatRepository;
        private readonly IChannelInfoRepository _channelInfoRepository;

        private readonly FlatScraperSsDotGe _scraperSsDotGe;

        public FlatService(IFlatRepository flatRepository, IChannelInfoRepository channelInfoRepository)
        {
            _scraperSsDotGe = new FlatScraperSsDotGe();
            _flatRepository = flatRepository;
            _channelInfoRepository = channelInfoRepository;
        }

        public void FindAndSaveSuitAdjaraFlats(long channelId)
        {
            var lastCheckDate = _channelInfoRepository.GetLastCheckDate(channelId);
        }
    }
}
