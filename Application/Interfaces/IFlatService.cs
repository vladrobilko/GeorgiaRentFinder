using Application.Models;

namespace Application.Interfaces
{
    public interface IFlatService
    {
        void FindAndSaveSuitAdjaraFlats(long channelId);

        void AddDateOfTelegramPublication(long flatId, DateTime timeOfPublic);

        void AddDateOfRefusePublication(long flatId, DateTime timeOfPublic);

        FlatInfoClientModel GetAvailableFlat(long channelId);

        FlatInfoClientModel GetFlatById(long flatId);

        long GetCountNotViewedFlats();
    }
}
