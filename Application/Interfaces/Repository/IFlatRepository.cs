using Application.Models;
using WebScraper.Models;

namespace Application.Interfaces.Repository
{
    public interface IFlatRepository
    {
        void CreateFlats(List<FlatInfoModel> flats);

        void UpdateFlatDateInfoToTelegramPublication(long flatId, DateTime timeOfPublic);

        void UpdateFlatDateInfoToRefusePublication(long flatId, DateTime timeOfRefuse);

        long ReadCountNotViewedFlats();

        FlatInfoClientModel ReadOldestNotViewedFlatInfoClientModel();

        FlatInfoClientModel ReadFlatById(long flatId);
    }
}