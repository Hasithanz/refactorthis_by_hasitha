using RefactorThis.Persistence;

namespace RefactorThis.Domain
{
    public interface IHandleInvoiceHelper
    {
        string HandleZeroAmountInvoice(Invoice invoice, Payment payment);
    }
}
