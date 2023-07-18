namespace Application.Interfaces
{
    public interface IFlatPublicationService
    {
        void AddDateOfTelegramPublication(long flatId, DateTime timeOfPublic);

        void AddDateOfRefusePublication(long flatId, DateTime timeOfPublic);

        void AddDatesForTelegramException(long flatId, DateTime time);
    }
}