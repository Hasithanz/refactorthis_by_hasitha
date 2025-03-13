using RefactorThis.Persistence;

namespace RefactorThis.Domain.Helpers
{
    public class HandlePaymentHelper : IHandlePaymentHelper
    {
        public string HandleNoPayments(Invoice invoice, Payment payment)
        {
            if (payment.Amount > invoice.Amount)
            {
                return "the payment is greater than the invoice amount";
            }
            else if (invoice.Amount == payment.Amount)
            {
                invoice.AmountPaid = payment.Amount;
                invoice.TaxAmount = payment.Amount * 0.14m;
                invoice.Payments.Add(payment);
                return "invoice is now fully paid";
            }
            else
            {
                invoice.AmountPaid = payment.Amount;
                invoice.TaxAmount = payment.Amount * 0.14m;
                invoice.Payments.Add(payment);
                return "invoice is now partially paid";
            }
        }
    }
}
