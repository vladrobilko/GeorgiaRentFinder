using Application.Models;
using WebScraper.Models;

namespace Application.Interfaces
{
    public interface IFlatService
    {
        void FindAndSaveSuitAdjaraFlats(long channelId);

        FlatInfoClientModel GetAvailableFlat(long channelId);

        long GetCountNotViewedFlats();
    }
}
