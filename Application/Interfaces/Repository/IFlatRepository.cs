using Application.Models;
using WebScraper.Models;

namespace Application.Interfaces.Repository
{
    public interface IFlatRepository
    {
        void CreateFlats(List<FlatInfoModel> flats);

        void UpdateFlatDateInfoTelegramPublication(long flatId, DateTime timeOfPublic);

        void UpdateFlatDateInfoRefusePublication(long flatId, DateTime timeOfRefuse);

        void UpdateFlatDateInfoTelegramException(long flatId, DateTime time);

        long ReadCountNotViewedFlats();

        FlatInfoClientModel ReadOldestNotViewedFlat();

        FlatInfoClientModel ReadFlatById(long flatId);

        IEnumerable<(string phoneNumber, long cost)> ReadLastPostedFlatsFromDate(DateTime time);

        void UpdatePhoneNumberWithDecreaseNumberOfMention(string phoneNumber);
    }
}