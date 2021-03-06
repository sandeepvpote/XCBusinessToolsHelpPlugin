// © 2016 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Braintree;
using Braintree.Exceptions;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Orders;
using Sitecore.Commerce.Plugin.Payments;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;

namespace Plugin.Sample.Payments.Braintree
{
    /// <summary>
    ///  Defines a block which creates a payment service transaction.
    /// </summary>  
    /// <seealso>
    ///   <cref>
    /// Sitecore.Framework.Pipelines.PipelineBlock{Sitecore.Commerce.Plugin.Orders.CartEmailArgument, Sitecore.Commerce.Plugin.Orders.CartEmailArgument, Sitecore.Commerce.Core.CommercePipelineExecutionContext}
    /// </cref>
    /// </seealso>
    [PipelineDisplayName(PaymentsBraintreeConstants.CreateFederatedPaymentBlock)]
    public class CreateFederatedPaymentBlock : AsyncPipelineBlock<CartEmailArgument, CartEmailArgument, CommercePipelineExecutionContext>
    {
        /// <summary>
        /// Runs the specified argument.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="context">The context.</param>
        /// <returns>
        /// A cart with federate payment component
        /// </returns>
        public override async Task<CartEmailArgument> RunAsync(CartEmailArgument arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{Name}: The cart can not be null");

            var cart = arg.Cart;
            if (!cart.HasComponent<FederatedPaymentComponent>())
            {
                return arg;
            }

            var payment = cart.GetComponent<FederatedPaymentComponent>();
            if (string.IsNullOrEmpty(payment.PaymentMethodNonce))
            {
                context.Abort(
                    await context.CommerceContext.AddMessage(
                        context.GetPolicy<KnownResultCodes>().Error,
                        "InvalidOrMissingPropertyValue",
                        new object[]
                        {
                            "PaymentMethodNonce"
                        },
                        "Invalid or missing value for property 'PaymentMethodNonce'.").ConfigureAwait(false),
                    context);
                return arg;
            }

            var braintreeClientPolicy = context.GetPolicy<BraintreeClientPolicy>();
            if (!(await braintreeClientPolicy.IsValid(context.CommerceContext).ConfigureAwait(false)))
            {
                return arg;
            }

            try
            {
                var gateway = new BraintreeGateway(braintreeClientPolicy.Environment, braintreeClientPolicy.MerchantId, braintreeClientPolicy.PublicKey, braintreeClientPolicy.PrivateKey);

                var request = new TransactionRequest
                {
                    Amount = payment.Amount.Amount,
                    PaymentMethodNonce = payment.PaymentMethodNonce,
                    BillingAddress = ComponentsHelper.TranslatePartyToAddressRequest(payment.BillingParty),
                    Options = new TransactionOptionsRequest
                    {
                        SubmitForSettlement = false
                    }
                };

                var result = await gateway.Transaction.SaleAsync(request).ConfigureAwait(false);
                if (result.IsSuccess())
                {
                    var transaction = result.Target;
                    payment.TransactionId = transaction?.Id;
                    payment.TransactionStatus = transaction?.Status?.ToString();
                    payment.PaymentInstrumentType = transaction?.PaymentInstrumentType?.ToString();

                    var cc = transaction?.CreditCard;
                    payment.MaskedNumber = cc?.MaskedNumber;
                    payment.CardType = cc?.CardType?.ToString();
                    if (cc?.ExpirationMonth != null)
                    {
                        payment.ExpiresMonth = int.Parse(cc.ExpirationMonth, CultureInfo.InvariantCulture);
                    }

                    if (cc?.ExpirationYear != null)
                    {
                        payment.ExpiresYear = int.Parse(cc.ExpirationYear, CultureInfo.InvariantCulture);
                    }
                }
                else
                {
                    var errorMessages = string.Concat(result.Message, " ", result.Errors.DeepAll().Aggregate(string.Empty, (current, error) => current + ("Error: " + (int) error.Code + " - " + error.Message + "\n")));
                    context.Abort(
                        await context.CommerceContext.AddMessage(
                            context.GetPolicy<KnownResultCodes>().Error,
                            "CreatePaymentFailed",
                            new object[]
                            {
                                "PaymentMethodNonce"
                            },
                            $"{Name}. Create payment failed :{errorMessages}").ConfigureAwait(false),
                        context);
                }

                return arg;
            }
            catch (BraintreeException ex)
            {
                context.Abort(
                    await context.CommerceContext.AddMessage(
                        context.GetPolicy<KnownResultCodes>().Error,
                        "CreatePaymentFailed",
                        new object[]
                        {
                            "PaymentMethodNonce",
                            ex
                        },
                        $"{Name}. Create payment failed.").ConfigureAwait(false),
                    context);
                return arg;
            }
        }
    }
}
