using Application.Interfaces.Repository;
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

        public DateTime GetLastCheckDate(long channelId)
        {
            return _context.ChannelInfosDto.Single(c => c.Id == channelId).LastCheckDate;
        }
    }
}
