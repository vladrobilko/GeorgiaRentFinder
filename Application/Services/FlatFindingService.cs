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
    public class FlatFindingService : IFlatFindService
    {
        private readonly IFlatRepository _flatRepository;

        private readonly IChannelInfoRepository _channelInfoRepository;

        public FlatFindingService(IFlatRepository flatRepository, IChannelInfoRepository channelInfoRepository)
        {
            _flatRepository = flatRepository;
            _channelInfoRepository = channelInfoRepository;
        }

        public void FindAndSaveSuitAdjaraFlats(long channelId)
        {
            var lastCheckDate = _channelInfoRepository.ReadLastCheckDateById(channelId);

            var newAdjaraFlats = new List<FlatInfoModel>();

            var countPagesForScrap = 5;

            var scraperMyHomeGe = new FlatsScraper(new MyHomeDotGeFlatScraper(), 24, 100, 460);

            FindMyHomeDotGeAdjaraFLats(countPagesForScrap, scraperMyHomeGe, lastCheckDate, newAdjaraFlats);

            var scraperSsDotGe = new FlatsScraper(new SsDotGeFlatScraper(), 20, 100, 460);

            FindSsDotGeAdjaraFLats(countPagesForScrap, scraperSsDotGe, lastCheckDate, newAdjaraFlats);

            _flatRepository.CreateFlats(newAdjaraFlats);

            _channelInfoRepository.UpdateLastCheckDate(channelId, DateTime.Now);
        }

        public void FindAndSaveSuitImeretiFlats(long channelId)
        {
            var lastCheckDate = _channelInfoRepository.ReadLastCheckDateById(channelId);

            var imeretiFlats = new List<FlatInfoModel>();

            var countPagesForScrap = 5;

            var scraperMyHomeDotGe = new FlatsScraper(new MyHomeDotGeFlatScraper(), 24, 100, 410);

            FindMyHomeDotGeImeretiFLats(countPagesForScrap, scraperMyHomeDotGe, lastCheckDate, imeretiFlats);

            var scraperSsDotGe = new FlatsScraper(new SsDotGeFlatScraper(), 20, 100, 410);

            FindSsDotGeImeretiFLats(countPagesForScrap, scraperSsDotGe, lastCheckDate, imeretiFlats);

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