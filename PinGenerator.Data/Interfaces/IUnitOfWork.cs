namespace PinGenerator.Data.Interfaces
{
    public interface IUnitOfWork
    {
        IPinRepository Pins { get; }
    }
}
