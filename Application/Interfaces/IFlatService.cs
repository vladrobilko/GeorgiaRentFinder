using WebScraper.Models;

namespace Application.Interfaces
{
    public interface IFlatService
    {
        void FindAndSaveSuitAdjaraFlats(long channelId);

        FlatInfoModel GetAvailableFlat(long channelId);

        long GetCountNotViewedFlats();
    }
}
