using System;
using RefactorThis.Persistence;

namespace RefactorThis.Domain
{
    public class InvoiceService
    {
        // InvoiceService class was depending directly on a concrete implementation of InvoiceRepository, which is a violation of the Dependency Inversion Principle.
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IPaymentProcessorFactory _paymentProcessorFactory;

        public InvoiceService(IInvoiceRepository invoiceRepository, IPaymentProcessorFactory paymentProcessorFactory)
        {
            _invoiceRepository = invoiceRepository;
            _paymentProcessorFactory = paymentProcessorFactory;
        }
                
        public string ProcessPayment(Payment payment)
        {
            // Changed the variable names to be more descriptive and to enhance readability.
            var invoice = _invoiceRepository.GetInvoice(payment.Reference);

            if (invoice == null)
            {
                throw new InvalidOperationException("There is no invoice matching this payment");
            }

            // Introduced the PaymentProcessorFactory to create the appropriate payment processor based on the invoice type.
            // Created two separate payment processors for Commercial and Standard invoices. Which supports the Open/Closed Principle.
            // If a new invoice type is introduced, a new payment processor can be created without modifying the existing code.
            var paymentProcessor = _paymentProcessorFactory.Create(invoice.Type);
            var responseMessage = paymentProcessor.ProcessPayment(invoice, payment);

            invoice.Save();

            return responseMessage;
        }
    }
}