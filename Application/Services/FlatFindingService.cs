using Application.Interfaces;
using Application.Interfaces.Repository;
using WebScraper;
using WebScraper.Models;
using WebScraper.MyHomeDotGe;
using WebScraper.MyHomeDotGe.Links;
using WebScraper.SsDotGe;
using WebScraper.SsDotGe.LInks;

namespace Application.Services
{
    public class FlatFindingService : IFlatFindService
    {
        private readonly IFlatRepository _flatRepository;

        private readonly IChannelInfoRepository _channelInfoRepository;

        private const int FlatsOnPageSsGe = 20;
        private const int CountPagesForScrap = 10;
        private const int FlatsOnPageMyHomeGe = 24;
        private const int FlatLowestPriceAdjara = 150;
        private const int FlatHighestPriceAdjara = 410;
        private const int FlatLowestPriceImereti = 150;
        private const int FlatHighestPriceImereti = 410;
        
        public FlatFindingService(IFlatRepository flatRepository, IChannelInfoRepository channelInfoRepository)
        {
            _flatRepository = flatRepository;
            _channelInfoRepository = channelInfoRepository;
        }

        public void FindAndSaveSuitAdjaraFlats(long channelId)
        {
            var lastCheckDate = _channelInfoRepository.ReadLastCheckDateById(channelId);

            var newAdjaraFlats = new List<FlatInfoModel>();

            var scraperMyHomeGe = new FlatsScraper(new MyHomeDotGeFlatScraper(), FlatsOnPageMyHomeGe, FlatLowestPriceAdjara, FlatHighestPriceAdjara);

            FindMyHomeDotGeAdjaraFLats(CountPagesForScrap, scraperMyHomeGe, lastCheckDate, newAdjaraFlats);

            var scraperSsDotGe = new FlatsScraper(new SsDotGeFlatScraper(), FlatsOnPageSsGe, FlatLowestPriceAdjara, FlatHighestPriceAdjara);

            FindSsDotGeAdjaraFLats(CountPagesForScrap, scraperSsDotGe, lastCheckDate, newAdjaraFlats);

            _flatRepository.CreateFlats(newAdjaraFlats);

            _channelInfoRepository.UpdateLastCheckDate(channelId, DateTime.Now);
        }

        public void FindAndSaveSuitImeretiFlats(long channelId)
        {
            var lastCheckDate = _channelInfoRepository.ReadLastCheckDateById(channelId);

            var imeretiFlats = new List<FlatInfoModel>();

            var scraperMyHomeDotGe = new FlatsScraper(new MyHomeDotGeFlatScraper(), FlatsOnPageMyHomeGe, FlatLowestPriceImereti, FlatHighestPriceImereti);

            FindMyHomeDotGeImeretiFLats(CountPagesForScrap, scraperMyHomeDotGe, lastCheckDate, imeretiFlats);

            var scraperSsDotGe = new FlatsScraper(new SsDotGeFlatScraper(), FlatsOnPageSsGe, FlatLowestPriceImereti, FlatHighestPriceImereti);

            FindSsDotGeImeretiFLats(CountPagesForScrap, scraperSsDotGe, lastCheckDate, imeretiFlats);

            _flatRepository.CreateFlats(imeretiFlats);

            _channelInfoRepository.UpdateLastCheckDate(channelId, DateTime.Now);
        }

        private static void FindSsDotGeAdjaraFLats(int countPagesForScrap, FlatsScraper scraperSsDotGe, DateTime lastCheckDate,
            List<FlatInfoModel> newAdjaraFlats)
        {
            for (var i = 1; i < countPagesForScrap; i++)
            {
                var batumiFlats =
                    scraperSsDotGe.ScrapAllPagesWithAllFlats(AdjaraMunicipallyLinksSsDotGe.GetBatumiLink(i), lastCheckDate);
                newAdjaraFlats.AddRange(batumiFlats);

                var kobuletiFlats =
                    scraperSsDotGe.ScrapAllPagesWithAllFlats(AdjaraMunicipallyLinksSsDotGe.GetKobuletiLink(i), lastCheckDate);
                newAdjaraFlats.AddRange(kobuletiFlats);

                var khelvachauriFlats =
                    scraperSsDotGe.ScrapAllPagesWithAllFlats(AdjaraMunicipallyLinksSsDotGe.GetKhelvachauriLink(i),
                        lastCheckDate);
                newAdjaraFlats.AddRange(khelvachauriFlats);
            }
        }

        private static void FindMyHomeDotGeAdjaraFLats(int countPagesForScrap, FlatsScraper scraperMyHomeGe, DateTime lastCheckDate,
            List<FlatInfoModel> newAdjaraFlats)
        {
            for (var i = 1; i < countPagesForScrap; i++)
            {
                var batumiFlats =
                    scraperMyHomeGe.ScrapAllPagesWithAllFlats(AdjaraMunicipallyLinksMyHomeDotGe.GetBatumiLink(i),
                        lastCheckDate);
                newAdjaraFlats.AddRange(batumiFlats);

                var kobuletiFlats =
                    scraperMyHomeGe.ScrapAllPagesWithAllFlats(AdjaraMunicipallyLinksMyHomeDotGe.GetKobuletiLink(i),
                        lastCheckDate);
                newAdjaraFlats.AddRange(kobuletiFlats);

                var khelvachauriFlats =
                    scraperMyHomeGe.ScrapAllPagesWithAllFlats(AdjaraMunicipallyLinksMyHomeDotGe.GetKhelvachauriLink(i),
                        lastCheckDate);
                newAdjaraFlats.AddRange(khelvachauriFlats);
            }
        }

        private static void FindSsDotGeImeretiFLats(int countPagesForScrap, FlatsScraper scraperSsDotGe, DateTime lastCheckDate,
            List<FlatInfoModel> imeretiFlats)
        {
            for (var i = 1; i < countPagesForScrap; i++)
            {
                var imeretiSsDotGeFlats =
                    scraperSsDotGe.ScrapAllPagesWithAllFlats(ImeretiMunicipallyLinksSsDotGe.GetKutaisiLink(i), lastCheckDate);
                imeretiFlats.AddRange(imeretiSsDotGeFlats);
            }
        }

        private static void FindMyHomeDotGeImeretiFLats(int countPagesForScrap, FlatsScraper scraperMyHomeDotGe,
            DateTime lastCheckDate, List<FlatInfoModel> imeretiFlats)
        {
            for (var i = 1; i < countPagesForScrap; i++)
            {
                var imeretiMyHomeDotGeFlats =
                    scraperMyHomeDotGe.ScrapAllPagesWithAllFlats(ImeretiMunicipallyLinksMyHomeDotGe.GetKutaisiLink(i),
                        lastCheckDate);
                imeretiFlats.AddRange(imeretiMyHomeDotGeFlats);
            }
        }
    }
}