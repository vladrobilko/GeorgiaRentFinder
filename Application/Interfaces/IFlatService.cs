using Application.Models;

namespace Application.Interfaces
{
    public interface IFlatService
    {
        void FindAndSaveSuitAdjaraFlats(long channelId);

        void FindAndSaveSuitImeretiFlats(long channelId);

        void AddDateOfTelegramPublication(long flatId, DateTime timeOfPublic);

        void AddDateOfRefusePublication(long flatId, DateTime timeOfPublic);

        FlatInfoClientModel GetAvailableFlat();

        FlatInfoClientModel GetFlatById(long flatId);

        long GetCountNotViewedFlats();

        string GetIdChannelWithLastCheckDate();

        void AddDatesForTelegramException(long flatId, DateTime time);
    }
}
