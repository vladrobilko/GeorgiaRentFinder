using Application.Models;

namespace Application.Interfaces
{
    public interface IFlatInfoService
    {
        FlatInfoClientModel GetAvailableFlat();

        FlatInfoClientModel GetFlatById(long flatId);

        long GetCountNotViewedFlats();

        string GetIdChannelWithLastCheckDate();
    }
}
