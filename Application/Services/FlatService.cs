﻿using Application.Interfaces;
using Application.Interfaces.Repository;
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
            var lastCheckDate = _channelInfoRepository.ReadLastCheckDate(channelId);

            var newAdjaraFlats = new List<FlatInfoModel>();

            var countPagesForScrap = 10;

            for (var i = 1; i < countPagesForScrap; i++)
            {
                var batumiFlats = _scraperSsDotGe.ScrapPageWithAllFlats(AdjaraMunicipallyLinksSsDotGe.GetBatumiLink(i), lastCheckDate);
                newAdjaraFlats.AddRange(batumiFlats);

                var kobuletiFlats = _scraperSsDotGe.ScrapPageWithAllFlats(AdjaraMunicipallyLinksSsDotGe.GetKobuletiLink(i), lastCheckDate);
                newAdjaraFlats.AddRange(kobuletiFlats);

                var khelvachauriFlats = _scraperSsDotGe.ScrapPageWithAllFlats(AdjaraMunicipallyLinksSsDotGe.GetKhelvachauriLink(i), lastCheckDate);
                newAdjaraFlats.AddRange(khelvachauriFlats);
            }

            _flatRepository.CreateFlats(newAdjaraFlats);
            
            _channelInfoRepository.UpdateLastCheckDate(channelId,DateTime.UtcNow);
        }
    }
}
