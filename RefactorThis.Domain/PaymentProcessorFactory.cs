using System;
using RefactorThis.Persistence;

namespace RefactorThis.Domain
{
    public class PaymentProcessorFactory : IPaymentProcessorFactory
    {
        private readonly IHandleInvoiceHelper _handleInvoiceHelper;
        private readonly IHandlePaymentHelper _handlePaymentHelper;

        public PaymentProcessorFactory(IHandleInvoiceHelper handleInvoiceHelper, IHandlePaymentHelper handlePaymentHelper) 
        {
            _handleInvoiceHelper = handleInvoiceHelper;
            _handlePaymentHelper = handlePaymentHelper;
        }

        public IPaymentProcessor Create(InvoiceType type)
        {
            switch (type)
            {
                case InvoiceType.Standard:
                    return new StandardPaymentProcessor(_handleInvoiceHelper, _handlePaymentHelper);
                case InvoiceType.Commercial:
                    return new CommercialPaymentProcessor(_handleInvoiceHelper, _handlePaymentHelper);
                default:
                    throw new ArgumentException("Invalid invoice type");
            }
        }
    }
}
