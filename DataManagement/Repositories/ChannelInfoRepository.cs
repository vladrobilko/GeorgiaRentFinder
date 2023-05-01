﻿using Application.Interfaces.Repository;
using DataManagement.Models;

namespace DataManagement.Repositories
{
    public class ChannelInfoRepository : IChannelInfoRepository
    {
        private readonly RentFinderDbContext _context;

        public ChannelInfoRepository(RentFinderDbContext context)
        {
            _context = context;
        }

        public DateTime ReadLastCheckDate(long channelId)
        {
            return _context.ChannelInfosDto.Single(c => c.Id == channelId).LastCheckDate;
        }

        public void UpdateLastCheckDate(long channelId, DateTime lastCheckDate)
        {
            var channelInfo = _context.ChannelInfosDto.Single(c => c.Id == channelId);

            channelInfo.LastCheckDate = lastCheckDate;

            _context.SaveChanges();
        }

        public DateTime ReadLatestCheckDateFromAllChannels()
        {
            return _context.ChannelInfosDto
                .OrderByDescending(c => c.LastCheckDate)
                .Select(i => i)
                .First()
                .LastCheckDate;
        }

        public long ReadIdChannelWithLastCheckDate()
        {
            return _context.ChannelInfosDto
                .OrderByDescending(c => c.LastCheckDate)
                .Select(i => i)
                .First().Id;
        }
    }
}
