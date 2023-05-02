using Application.Models;
using WebScraper.Models;

namespace Application.Interfaces.Repository
{
    public interface IFlatRepository
    {
        void CreateFlats(List<FlatInfoModel> flats);

        long ReadCountNotViewedFlats();

        FlatInfoClientModel ReadOldestNotViewedFlatInfoClientModel();
    }
}
