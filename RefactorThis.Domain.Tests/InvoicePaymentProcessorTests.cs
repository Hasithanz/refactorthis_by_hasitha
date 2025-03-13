using Moq;
using System;
using NUnit.Framework;
using RefactorThis.Persistence;
using System.Collections.Generic;
using RefactorThis.Domain.Helpers;

namespace RefactorThis.Domain.Tests
{
	[TestFixture]
	public class InvoicePaymentProcessorTests
	{
        private Mock<IInvoiceRepository> _invoiceRepository;
        private Mock<IPaymentProcessorFactory> _paymentProcessorFactory;
        private InvoiceService _invoiceService;

        [SetUp]
        public void Setup()
        {
            _invoiceRepository = new Mock<IInvoiceRepository>();
            _paymentProcessorFactory = new Mock<IPaymentProcessorFactory>();
            _invoiceService = new InvoiceService(_invoiceRepository.Object, _paymentProcessorFactory.Object);
        }

        [Test]
		public void ProcessPayment_Should_ThrowException_When_NoInoiceFoundForPaymentReference( )
		{
            var payment = new Payment();
            var failureMessage = "";

            _invoiceRepository.Setup(repository => repository.GetInvoice("REF00001")).Returns((Invoice)null);

            try
            {
                var result = _invoiceService.ProcessPayment(payment);
            }
            catch (InvalidOperationException e)
            {
                failureMessage = e.Message;
            }

            Assert.That(failureMessage, Is.EqualTo("There is no invoice matching this payment"));
        }

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_NoPaymentNeeded( )
		{
            var invoice = new Invoice(_invoiceRepository.Object)
			{
				Amount = 0,
				AmountPaid = 0,
				Payments = null
			};

            IHandlePaymentHelper handlePaymentHelper = new HandlePaymentHelper();
            IHandleInvoiceHelper handleInvoiceHelper = new HandleInvoiceHelper();

            _invoiceRepository.Setup(repository => repository.GetInvoice(It.IsAny<string>())).Returns(invoice);
			_paymentProcessorFactory.Setup(factory => factory.Create(It.IsAny<InvoiceType>())).Returns(new StandardPaymentProcessor(handleInvoiceHelper, handlePaymentHelper));  

            var result = _invoiceService.ProcessPayment(new Payment());

			_invoiceRepository.Verify(repository => repository.SaveInvoice(It.IsAny<Invoice>()), Times.Once);

            Assert.AreEqual( "no payment needed", result );
		}

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaid( )
		{
            IInvoiceRepository invoiceRepository = new InvoiceRepository();
            IHandlePaymentHelper handlePaymentHelper = new HandlePaymentHelper();
            IHandleInvoiceHelper handleInvoiceHelper = new HandleInvoiceHelper();
            IPaymentProcessorFactory paymentProcessorFactory = new PaymentProcessorFactory(handleInvoiceHelper, handlePaymentHelper);

            var invoice = new Invoice(invoiceRepository)
			{
				Amount = 10,
				AmountPaid = 10,
				Payments = new List<Payment>
				{
					new Payment
					{
						Amount = 10
					}
				}
			};
            invoiceRepository.Add( invoice );

			var paymentProcessor = new InvoiceService(invoiceRepository, paymentProcessorFactory);

			var payment = new Payment( );

			var result = paymentProcessor.ProcessPayment( payment );

			Assert.AreEqual( "invoice was already fully paid", result );
		}

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue( )
		{
            IInvoiceRepository invoiceRepository = new InvoiceRepository();
            IHandlePaymentHelper handlePaymentHelper = new HandlePaymentHelper();
            IHandleInvoiceHelper handleInvoiceHelper = new HandleInvoiceHelper();
            IPaymentProcessorFactory paymentProcessorFactory = new PaymentProcessorFactory(handleInvoiceHelper, handlePaymentHelper);

            var invoice = new Invoice(invoiceRepository)
			{
				Amount = 10,
				AmountPaid = 5,
				Payments = new List<Payment>
				{
					new Payment
					{
						Amount = 5
					}
				}
			};
            invoiceRepository.Add( invoice );

			var paymentProcessor = new InvoiceService( invoiceRepository, paymentProcessorFactory );

			var payment = new Payment( )
			{
				Amount = 6
			};

			var result = paymentProcessor.ProcessPayment( payment );

			Assert.AreEqual( "the payment is greater than the partial amount remaining", result );
		}

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount( )
		{
            IInvoiceRepository invoiceRepository = new InvoiceRepository();
            IHandlePaymentHelper handlePaymentHelper = new HandlePaymentHelper();
            IHandleInvoiceHelper handleInvoiceHelper = new HandleInvoiceHelper();
            IPaymentProcessorFactory paymentProcessorFactory = new PaymentProcessorFactory(handleInvoiceHelper, handlePaymentHelper);

            var invoice = new Invoice( invoiceRepository )
			{
				Amount = 5,
				AmountPaid = 0,
				Payments = new List<Payment>( )
			};
			invoiceRepository.Add( invoice );

			var paymentProcessor = new InvoiceService( invoiceRepository, paymentProcessorFactory );

			var payment = new Payment( )
			{
				Amount = 6
			};

			var result = paymentProcessor.ProcessPayment( payment );

			Assert.AreEqual( "the payment is greater than the invoice amount", result );
		}

		[Test]
		public void ProcessPayment_Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue( )
		{
            IInvoiceRepository invoiceRepository = new InvoiceRepository();
            IHandlePaymentHelper handlePaymentHelper = new HandlePaymentHelper();
            IHandleInvoiceHelper handleInvoiceHelper = new HandleInvoiceHelper();
            IPaymentProcessorFactory paymentProcessorFactory = new PaymentProcessorFactory(handleInvoiceHelper, handlePaymentHelper);

            var invoice = new Invoice( invoiceRepository )
			{
				Amount = 10,
				AmountPaid = 5,
				Payments = new List<Payment>
				{
					new Payment
					{
						Amount = 5
					}
				}
			};
			invoiceRepository.Add( invoice );

			var paymentProcessor = new InvoiceService(invoiceRepository, paymentProcessorFactory);

			var payment = new Payment( )
			{
				Amount = 5
			};

			var result = paymentProcessor.ProcessPayment( payment );

			Assert.AreEqual( "final partial payment received, invoice is now fully paid", result );
		}

		[Test]
		public void ProcessPayment_Should_ReturnFullyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidEqualsInvoiceAmount( )
		{
            IInvoiceRepository invoiceRepository = new InvoiceRepository();
            IHandlePaymentHelper handlePaymentHelper = new HandlePaymentHelper();
            IHandleInvoiceHelper handleInvoiceHelper = new HandleInvoiceHelper();
            IPaymentProcessorFactory paymentProcessorFactory = new PaymentProcessorFactory(handleInvoiceHelper, handlePaymentHelper);

            var invoice = new Invoice( invoiceRepository )
			{
				Amount = 10,
				AmountPaid = 0,
				Payments = new List<Payment>( ) { new Payment( ) { Amount = 10 } }
			};
			invoiceRepository.Add( invoice );

			var paymentProcessor = new InvoiceService(invoiceRepository, paymentProcessorFactory);

			var payment = new Payment( )
			{
				Amount = 10
			};

			var result = paymentProcessor.ProcessPayment( payment );

			Assert.AreEqual( "invoice was already fully paid", result );
		}

		[Test]
		public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue( )
		{
            IInvoiceRepository invoiceRepository = new InvoiceRepository();
            IHandlePaymentHelper handlePaymentHelper = new HandlePaymentHelper();
            IHandleInvoiceHelper handleInvoiceHelper = new HandleInvoiceHelper();
            IPaymentProcessorFactory paymentProcessorFactory = new PaymentProcessorFactory(handleInvoiceHelper, handlePaymentHelper);

            var invoice = new Invoice( invoiceRepository )
			{
				Amount = 10,
				AmountPaid = 5,
				Payments = new List<Payment>
				{
					new Payment
					{
						Amount = 5
					}
				}
			};
			invoiceRepository.Add( invoice );

			var paymentProcessor = new InvoiceService( invoiceRepository, paymentProcessorFactory );

			var payment = new Payment( )
			{
				Amount = 1
			};

			var result = paymentProcessor.ProcessPayment( payment );

			Assert.AreEqual( "another partial payment received, still not fully paid", result );
		}

		[Test]
		public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount( )
		{
            IInvoiceRepository invoiceRepository = new InvoiceRepository();
            IHandlePaymentHelper handlePaymentHelper = new HandlePaymentHelper();
            IHandleInvoiceHelper handleInvoiceHelper = new HandleInvoiceHelper();
            IPaymentProcessorFactory paymentProcessorFactory = new PaymentProcessorFactory(handleInvoiceHelper, handlePaymentHelper);

            var invoice = new Invoice( invoiceRepository )
			{
				Amount = 10,
				AmountPaid = 0,
				Payments = new List<Payment>( )
			};
			invoiceRepository.Add( invoice );

			var paymentProcessor = new InvoiceService( invoiceRepository, paymentProcessorFactory );

			var payment = new Payment( )
			{
				Amount = 1
			};

			var result = paymentProcessor.ProcessPayment( payment );

			Assert.AreEqual( "invoice is now partially paid", result );
		}
	}
}