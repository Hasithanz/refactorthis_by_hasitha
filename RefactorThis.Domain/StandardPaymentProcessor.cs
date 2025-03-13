using System.Linq;
using RefactorThis.Persistence;

namespace RefactorThis.Domain
{
    public class StandardPaymentProcessor : IPaymentProcessor
    {
        private readonly IHandleInvoiceHelper _handleInvoiceHelper;
        private readonly IHandlePaymentHelper _handlePaymentHelper;
        public StandardPaymentProcessor(IHandleInvoiceHelper handleInvoiceHelper, IHandlePaymentHelper handlePaymentHelper)
        {
            _handleInvoiceHelper = handleInvoiceHelper;
            _handlePaymentHelper = handlePaymentHelper;
        }

        public string ProcessPayment(Invoice invoice, Payment payment)
        {
            if (invoice.Amount == 0)
            {
                return _handleInvoiceHelper.HandleZeroAmountInvoice(invoice, payment);
            }

            if(invoice.Payments == null || !invoice.Payments.Any())
            {
                return _handlePaymentHelper.HandleNoPayments(invoice, payment);
            }

            if (invoice.Payments.Sum(x => x.Amount) != 0 && invoice.Amount == invoice.Payments.Sum(x => x.Amount))
            {
                return "invoice was already fully paid";
            }
            else if (invoice.Payments.Sum(x => x.Amount) != 0 && payment.Amount > (invoice.Amount - invoice.AmountPaid))
            {
                return "the payment is greater than the partial amount remaining";
            }
            else
            {
                if ((invoice.Amount - invoice.AmountPaid) == payment.Amount)
                {
                    invoice.AmountPaid += payment.Amount;
                    invoice.Payments.Add(payment);
                    return "final partial payment received, invoice is now fully paid";
                }
                else
                {
                    invoice.AmountPaid += payment.Amount;
                    invoice.Payments.Add(payment);
                    return "another partial payment received, still not fully paid";
                }
            }
        }
    }
}
