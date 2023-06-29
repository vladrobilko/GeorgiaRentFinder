using Application.Interfaces;
using Application.Interfaces.Repository;
using Application.Models;
using WebScraper;
using WebScraper.Models;
using WebScraper.MyHomeDotGe;
using WebScraper.MyHomeDotGe.Links;
using WebScraper.SsDotGe;
using WebScraper.SsDotGe.LInks;

namespace Application.Services
{
    public class FlatService : IFlatService
    {
        private readonly IFlatRepository _flatRepository;

        private readonly IChannelInfoRepository _channelInfoRepository;

        public FlatService(IFlatRepository flatRepository, IChannelInfoRepository channelInfoRepository)
        {
            _flatRepository = flatRepository;
            _channelInfoRepository = channelInfoRepository;
        }

        public void FindAndSaveSuitAdjaraFlats(long channelId)
        {
            var lastCheckDate = _channelInfoRepository.ReadLastCheckDateById(channelId);

            var newAdjaraFlats = new List<FlatInfoModel>();

            var countPagesForScrap = 5;

            var scraperMyHomeGe = new FlatsScraper(new MyHomeDotGeFlatScraper(), 24, 0, 510);

            for (var i = 1; i < countPagesForScrap; i++)
            {
                var batumiFlats = scraperMyHomeGe.ScrapAllPagesWithAllFlats(AdjaraMunicipallyLinksMyHomeDotGe.GetBatumiLink(i), lastCheckDate);
                newAdjaraFlats.AddRange(batumiFlats);

                var kobuletiFlats = scraperMyHomeGe.ScrapAllPagesWithAllFlats(AdjaraMunicipallyLinksMyHomeDotGe.GetKobuletiLink(i), lastCheckDate);
                newAdjaraFlats.AddRange(kobuletiFlats);

                var khelvachauriFlats = scraperMyHomeGe.ScrapAllPagesWithAllFlats(AdjaraMunicipallyLinksMyHomeDotGe.GetKhelvachauriLink(i), lastCheckDate);
                newAdjaraFlats.AddRange(khelvachauriFlats);
            }
            

            var scraperSsDotGe = new FlatsScraper(new SsDotGeFlatScraper(), 20, 0, 510);

            for (var i = 1; i < countPagesForScrap; i++)
            {
                var batumiFlats = scraperSsDotGe.ScrapAllPagesWithAllFlats(AdjaraMunicipallyLinksSsDotGe.GetBatumiLink(i), lastCheckDate);
                newAdjaraFlats.AddRange(batumiFlats);

                var kobuletiFlats = scraperSsDotGe.ScrapAllPagesWithAllFlats(AdjaraMunicipallyLinksSsDotGe.GetKobuletiLink(i), lastCheckDate);
                newAdjaraFlats.AddRange(kobuletiFlats);

                var khelvachauriFlats = scraperSsDotGe.ScrapAllPagesWithAllFlats(AdjaraMunicipallyLinksSsDotGe.GetKhelvachauriLink(i), lastCheckDate);
                newAdjaraFlats.AddRange(khelvachauriFlats);
            }

            _flatRepository.CreateFlats(newAdjaraFlats);
            
            _channelInfoRepository.UpdateLastCheckDate(channelId,DateTime.Now);
        }

        public void FindAndSaveSuitImeretiFlats(long channelId)
        {
            var lastCheckDate = _channelInfoRepository.ReadLastCheckDateById(channelId);

            var imeretiFlats = new List<FlatInfoModel>();

            var countPagesForScrap = 5;

            var scraperMyHomeDotGe = new FlatsScraper(new MyHomeDotGeFlatScraper(), 24, 0, 360);

            for (var i = 1; i < countPagesForScrap; i++)
            {
                var imeretiMyHomeDotGeFlats = scraperMyHomeDotGe.ScrapAllPagesWithAllFlats(ImeretiMunicipallyLinksMyHomeDotGe.GetKutaisiLink(i), lastCheckDate);
                imeretiFlats.AddRange(imeretiMyHomeDotGeFlats);
            }

            var scraperSsDotGe = new FlatsScraper(new SsDotGeFlatScraper(),20, 0, 360);

            for (var i = 1; i < countPagesForScrap; i++)
            {
                var imeretiSsDotGeFlats = scraperSsDotGe.ScrapAllPagesWithAllFlats(ImeretiMunicipallyLinksSsDotGe.GetKutaisiLink(i), lastCheckDate);
                imeretiFlats.AddRange(imeretiSsDotGeFlats);
            }

            _flatRepository.CreateFlats(imeretiFlats);

            _channelInfoRepository.UpdateLastCheckDate(channelId, DateTime.Now);
        }

        public void AddDateOfTelegramPublication(long flatId, DateTime timeOfPublic)
        {
            _flatRepository.UpdateFlatDateInfoTelegramPublication(flatId, timeOfPublic);
        }

        public void AddDateOfRefusePublication(long flatId, DateTime timeOfPublic)
        {
            _flatRepository.UpdateFlatDateInfoRefusePublication(flatId, timeOfPublic);
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

        public void AddDatesForTelegramException(long flatId, DateTime time)
        {
            _flatRepository.UpdateFlatDateInfoTelegramException(flatId, time);
        }
    }
}
