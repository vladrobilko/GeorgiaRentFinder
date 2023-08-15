using Application.Interfaces;
using Application.Interfaces.Repository;

namespace Application.Services;

public class FlatPublicationService : IFlatPublicationService
{
    private readonly IFlatRepository _flatRepository;

    public FlatPublicationService(IFlatRepository flatRepository)
    {
        _flatRepository = flatRepository;
    }

    public void AddDateOfTelegramPublication(long flatId, DateTime timeOfPublic)
    {
        _flatRepository.UpdateFlatDateInfoTelegramPublication(flatId, timeOfPublic);
    }

    public void AddDateOfRefusePublication(long flatId, DateTime timeOfPublic)
    {
        _flatRepository.UpdateFlatDateInfoRefusePublication(flatId, timeOfPublic);
    }

    public void AddDatesForTelegramException(long flatId, DateTime time)
    {
        _flatRepository.UpdateFlatDateInfoTelegramException(flatId, time);
    }
}