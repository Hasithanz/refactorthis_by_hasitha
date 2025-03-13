using RefactorThis.Persistence;

namespace RefactorThis.Domain
{
    public interface IPaymentProcessorFactory
    {
        IPaymentProcessor Create(InvoiceType type);
    }
}
