// © 2016 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Braintree;
using Braintree.Exceptions;
using Microsoft.Extensions.Logging;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.ManagedLists;
using Sitecore.Commerce.Plugin.Orders;
using Sitecore.Commerce.Plugin.Payments;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;

namespace Plugin.Sample.Payments.Braintree
{
    /// <summary>
    /// Defines a refund federated paymentBlock.
    /// </summary>
    /// <seealso>
    /// <cref>
    ///    Sitecore.Framework.Pipelines.PipelineBlock{Sitecore.Commerce.Plugin.Payments.OrderPaymentsArgument, 
    ///    Sitecore.Commerce.Plugin.Payments.Order, Sitecore.Commerce.Core.CommercePipelineExecutionContext}
    /// </cref>
    /// </seealso>
    [PipelineDisplayName(PaymentsBraintreeConstants.RefundFederatedPaymentBlock)]
    public class RefundFederatedPaymentBlock : AsyncPipelineBlock<OrderPaymentsArgument, Order, CommercePipelineExecutionContext>
    {
        private readonly IPersistEntityPipeline _persistPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="RefundFederatedPaymentBlock"/> class.
        /// </summary>
        /// <param name="persistEntityPipeline">The persist entity pipeline.</param>
        public RefundFederatedPaymentBlock(IPersistEntityPipeline persistEntityPipeline)
        {
            _persistPipeline = persistEntityPipeline;
        }

        /// <summary>
        /// Runs the specified argument.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="context">The context.</param>
        /// <returns>
        /// An OrderPaymentsArgument with order and Federated existingPayment info
        /// </returns>
        public override async Task<Order> RunAsync(OrderPaymentsArgument arg, CommercePipelineExecutionContext context)
        {
            if (arg == null)
            {
                return null;
            }

            Condition.Requires(arg).IsNotNull($"{Name}: The arg can not be null");
            Condition.Requires(arg.Order).IsNotNull($"{Name}: The order can not be null");

            var order = arg.Order;
            if (!order.Status.Equals(context.GetPolicy<KnownOrderStatusPolicy>().Completed, StringComparison.OrdinalIgnoreCase))
            {
                var invalidOrderStateMessage = $"{Name}: Expected order in '{context.GetPolicy<KnownOrderStatusPolicy>().Completed}' status but order was in '{order.Status}' status";
                await context.CommerceContext.AddMessage(
                    context.GetPolicy<KnownResultCodes>().ValidationError,
                    "InvalidOrderState",
                    new object[]
                    {
                        context.GetPolicy<KnownOrderStatusPolicy>().OnHold,
                        order.Status
                    },
                    invalidOrderStateMessage).ConfigureAwait(false);
                return null;
            }

            if (!order.HasComponent<FederatedPaymentComponent>())
            {
                return order;
            }

            var braintreeClientPolicy = context.GetPolicy<BraintreeClientPolicy>();
            if (!(await braintreeClientPolicy.IsValid(context.CommerceContext).ConfigureAwait(false)))
            {
                return null;
            }

            try
            {
                var existingPayment = order.GetComponent<FederatedPaymentComponent>();
                if (!(arg.Payments.FirstOrDefault(p => p.Id.Equals(existingPayment.Id, StringComparison.OrdinalIgnoreCase)) is FederatedPaymentComponent paymentToRefund))
                {
                    return order;
                }

                if (existingPayment.Amount.Amount < paymentToRefund.Amount.Amount)
                {
                    await context.CommerceContext.AddMessage(
                        context.GetPolicy<KnownResultCodes>().Error,
                        "IllegalRefundOperation",
                        new object[]
                        {
                            order.Id,
                            existingPayment.Id
                        },
                        "Order Federated Payment amount is less than refund amount").ConfigureAwait(false);
                    return null;
                }

                var gateway = new BraintreeGateway(braintreeClientPolicy?.Environment, braintreeClientPolicy.MerchantId, braintreeClientPolicy?.PublicKey, braintreeClientPolicy.PrivateKey);
                var result = gateway.Transaction.Refund(existingPayment.TransactionId, paymentToRefund.Amount.Amount);
                if (result.IsSuccess())
                {
                    context.Logger.LogInformation($"{Name} - Refund Payment succeeded:{paymentToRefund.Id}");
                    existingPayment.TransactionStatus = result.Target.Status.ToString();
                }
                else
                {
                    var errorMessages = result.Errors.DeepAll().Aggregate(string.Empty, (current, error) => current + ("Error: " + (int) error.Code + " - " + error.Message + "\n"));

                    await context.CommerceContext.AddMessage(
                        context.GetPolicy<KnownResultCodes>().Error,
                        "PaymentRefundFailed",
                        new object[]
                        {
                            existingPayment.TransactionId
                        },
                        $"{Name}. Payment refund failed for transaction {existingPayment.TransactionId}: {errorMessages}").ConfigureAwait(false);

                    return null;
                }

                if (existingPayment.Amount.Amount == paymentToRefund.Amount.Amount)
                {
                    order.RemoveComponents(existingPayment);
                }
                else
                {
                    // Reduce existing existingPayment by the amount being refunded
                    existingPayment.Amount.Amount -= paymentToRefund.Amount.Amount;
                }

                await GenerateSalesActivity(order, existingPayment, paymentToRefund, result.Target.Id, context).ConfigureAwait(false);
            }
            catch (BraintreeException ex)
            {
                await context.CommerceContext.AddMessage(
                    context.GetPolicy<KnownResultCodes>().Error,
                    "PaymentRefundFailed",
                    new object[]
                    {
                        order.Id,
                        ex
                    },
                    $"{Name}. Payment refund failed.").ConfigureAwait(false);
                return null;
            }

            return order;
        }

        /// <summary>
        /// Generates the sales activity.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <param name="existingPayment">The existingPayment.</param>
        /// <param name="paymentToRefund">The payment to refund</param>
        /// <param name="refundTransactionId">The refund transaction identifier.</param>
        /// <param name="context">The context.</param>
        /// <returns>
        /// A <see cref="Task" />
        /// </returns>
        protected virtual async Task GenerateSalesActivity(Order order, PaymentComponent existingPayment, PaymentComponent paymentToRefund, string refundTransactionId, CommercePipelineExecutionContext context)
        {
            var salesActivity = new SalesActivity
            {
                Id = $"{CommerceEntity.IdPrefix<SalesActivity>()}{Guid.NewGuid():N}",
                ActivityAmount = new Money(existingPayment.Amount.CurrencyCode, paymentToRefund.Amount.Amount * -1),
                Customer = new EntityReference
                {
                    EntityTarget = order.EntityComponents.OfType<ContactComponent>().FirstOrDefault()?.CustomerId
                },
                Order = new EntityReference
                {
                    EntityTarget = order.Id,
                    EntityTargetUniqueId = order.UniqueId
                },
                Name = "Refund the Federated Payment",
                PaymentStatus = context.GetPolicy<KnownSalesActivityStatusesPolicy>().Completed
            };

            salesActivity.SetComponent(new ListMembershipsComponent
            {
                Memberships = new List<string>
                {
                    CommerceEntity.ListName<SalesActivity>(),
                    context.GetPolicy<KnownOrderListsPolicy>().SalesCredits,
                    string.Format(CultureInfo.InvariantCulture, context.GetPolicy<KnownOrderListsPolicy>().OrderSalesActivities, order.FriendlyId)
                }
            });

            if (existingPayment.Amount.Amount != paymentToRefund.Amount.Amount)
            {
                salesActivity.SetComponent(existingPayment);
            }

            if (!string.IsNullOrEmpty(refundTransactionId))
            {
                salesActivity.SetComponent(new TransactionInformationComponent(refundTransactionId));
            }

            var salesActivities = order.SalesActivity.ToList();
            salesActivities.Add(new EntityReference
            {
                EntityTarget = salesActivity.Id,
                EntityTargetUniqueId = salesActivity.UniqueId
            });
            order.SalesActivity = salesActivities;

            await _persistPipeline.RunAsync(new PersistEntityArgument(salesActivity), context).ConfigureAwait(false);
        }
    }
}
