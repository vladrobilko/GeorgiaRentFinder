using Application.Interfaces;
using Application.Interfaces.Repository;
using Application.Models;

namespace Application.Services
{
    public class FlatInfoService : IFlatInfoService
    {
        private readonly IFlatRepository _flatRepository;

        private readonly IChannelInfoRepository _channelInfoRepository;

        public FlatInfoService(IFlatRepository flatRepository, IChannelInfoRepository channelInfoRepository)
        {
            _flatRepository = flatRepository;
            _channelInfoRepository = channelInfoRepository;
        }
        public FlatInfoClientModel GetAvailableFlat()
        {
            return _flatRepository.ReadOldestNotViewedFlat();
        }

        public FlatInfoClientModel GetFlatById(long flatId)
        {
            return _flatRepository.ReadFlatById(flatId);
        }

        public long GetCountNotViewedFlats()
        {
            return _flatRepository.ReadCountNotViewedFlats();
        }

        public string GetIdChannelWithLastCheckDate()
        {
            return _channelInfoRepository.ReadIdChannelWithLastCheckDate();
        }
    }
}
