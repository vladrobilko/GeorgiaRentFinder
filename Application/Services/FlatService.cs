using Application.Interfaces;
using Application.Interfaces.Repository;
using System.Collections.Generic;
using WebScraper.Models;
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

            var result = new List<FlatInfoModel>();

            for (int i = 1; i < 2; i++)
            {
                var batumi = _scraperSsDotGe.ScrapPageWithAllFlats(AdjaraMunicipallyLinksSsDotGe.GetBatumiLink(i), lastCheckDate);
                result.AddRange(batumi);

                var kobuleti = _scraperSsDotGe.ScrapPageWithAllFlats(AdjaraMunicipallyLinksSsDotGe.GetKobuletiLink(i), lastCheckDate);
                result.AddRange(kobuleti);

                var khelvachauri = _scraperSsDotGe.ScrapPageWithAllFlats(AdjaraMunicipallyLinksSsDotGe.GetKhelvachauriLink(i), lastCheckDate);
                result.AddRange(khelvachauri);
            }

            _flatRepository.CreateFlats(result);

            //renew time

        }
    }
}
