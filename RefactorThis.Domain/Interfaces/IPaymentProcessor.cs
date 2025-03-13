using RefactorThis.Persistence;

namespace RefactorThis.Domain
{
    public interface IPaymentProcessor
    {
        string ProcessPayment(Invoice invoice, Payment payment);
    }
}
