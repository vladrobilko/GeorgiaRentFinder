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

        public bool IsPostedSameFlatLastHourAndIncreaseNumberOfMentionedPhoneIsPosted(FlatInfoClientModel flat)
        {
            var date = DateTime.Now.AddHours(-1);

            var result = _flatRepository
                .ReadLastHourPostedFlats(date)
                .FirstOrDefault(x => x.cost == flat.Cost && x.phoneNumber == flat.FlatPhoneClientModel.PhoneNumber);

            if (!result.Equals(default))
                _flatRepository.UpdatePhoneNumberWithDecreaseNumberOfMention(flat.FlatPhoneClientModel.PhoneNumber);

            return !result.Equals(default);
        }
    }
}