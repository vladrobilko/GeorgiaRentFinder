using Application.Models;

namespace Application.Interfaces
{
    public interface IFlatFindService
    {
        void FindAndSaveSuitAdjaraFlats(long channelId);

        void FindAndSaveSuitImeretiFlats(long channelId);
    }
}