using RefactorThis.Persistence;

namespace RefactorThis.Domain
{
    public interface IHandlePaymentHelper
    {
        string HandleNoPayments(Invoice invoice, Payment payment);
    }
}
