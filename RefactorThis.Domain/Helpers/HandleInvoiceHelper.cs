using System;
using System.Linq;
using RefactorThis.Persistence;

namespace RefactorThis.Domain.Helpers
{
    public class HandleInvoiceHelper : IHandleInvoiceHelper
    {
        public string HandleZeroAmountInvoice(Invoice invoice, Payment payment)
        {
            if (invoice.Payments == null || !invoice.Payments.Any())
            {
                return "no payment needed";
            }
            else
            {
                throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");
            }
        }
    }
}
