// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    ///     Defines a block which subscribes to service bus messages
    /// </summary>
    [PipelineDisplayName(ContentHubConstants.InitializeServiceBusBlock)]
    public class InitializeServiceBusBlock : AsyncPipelineBlock<CommerceEnvironment, CommerceEnvironment, CommercePipelineExecutionContext>
    {
        private readonly CommerceCommander _commerceCommander;
        private readonly ServiceBusSubscriptionClientFactory _serviceBusClientsFactory;

        private ISubscriptionClient _subscriptionClient;
        private ILogger _logger;
        private CommerceEnvironment _environment;

        /// <summary>
        /// Initializes a new instance of the <see cref="InitializeServiceBusBlock"/> class.
        /// </summary>
        /// <param name="commerceCommander">
        ///     The <see cref="CommerceCommander"/> to get <see cref="ImportEntityCommand"/> for
        ///     <see cref="CommerceEntity"/> changes import into Commerce Engine.
        /// </param>
        /// <param name="serviceBusClientsFactory">
        ///     The <see cref="ServiceBusSubscriptionClientFactory"/> to communicate with Azure Service Bus.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="commerceCommander"/> or <paramref name="serviceBusClientsFactory"/> is <see langword="null"/>.
        /// </exception>
        public InitializeServiceBusBlock(CommerceCommander commerceCommander, ServiceBusSubscriptionClientFactory serviceBusClientsFactory)
        {
            Condition.Requires(commerceCommander, nameof(commerceCommander)).IsNotNull();
            Condition.Requires(serviceBusClientsFactory, nameof(serviceBusClientsFactory)).IsNotNull();

            _commerceCommander = commerceCommander;
            _serviceBusClientsFactory = serviceBusClientsFactory;
        }

        /// <summary>
        /// Configures message handlers for Azure Service Bus. 
        /// </summary>
        /// <param name="arg">
        ///     The current <see cref="CommerceEnvironment"/> to use for <see cref="ImportEntityCommand"/>.
        /// </param>
        /// <param name="context">The current <see cref="CommercePipelineExecutionContext"/>.</param>
        /// <returns><paramref name="arg"/></returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="arg"/> or <paramref name="context"/> is <see langword="null"/>.
        /// </exception>
        public override async Task<CommerceEnvironment> RunAsync(CommerceEnvironment arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg, nameof(arg)).IsNotNull();
            Condition.Requires(context, nameof(context)).IsNotNull();

            context.CommerceContext.Logger.LogDebug($"{Name}: Azure Service Bus initialization started");

            AzureServiceBusPolicy serviceBusPolicy;

            if (arg.IsPolicyConfigured<AzureServiceBusPolicy>())
            {
                serviceBusPolicy = arg.GetConfiguredPolicy<AzureServiceBusPolicy>();
            }
            else
            {
                context.CommerceContext.Logger.LogInformation($"{Name}: Azure Service Bus will not be initialized because {nameof(AzureServiceBusPolicy)} " +
                    $"is not configured for environment {arg.Name}");
                return arg;
            }

            if (!arg.IsPolicyConfigured<SynchronizationPolicy>())
            {
                context.CommerceContext.Logger.LogWarning($"{Name}: Azure Service Bus will not be initialized because {nameof(SynchronizationPolicy)} " +
                    $"is not configured for environment {arg.Name}");
                return arg;
            }

            if (!arg.IsPolicyConfigured<ProductSynchronizationPolicy>() && !arg.IsPolicyConfigured<AssetSynchronizationPolicy>())
            {
                context.CommerceContext.Logger.LogWarning($"{Name}: Azure Service Bus will not be initialized because neither " +
                    $"{nameof(ProductSynchronizationPolicy)} nor {nameof(AssetSynchronizationPolicy)} is not configured for environment " +
                    $"{arg.Name}. Nothing to do.");
                return arg;
            }

            _environment = arg;
            _logger = context.CommerceContext.Logger;

            try
            {
                _subscriptionClient = _serviceBusClientsFactory.CreateSubscriptionClient(serviceBusPolicy.ConnectionString,
                    serviceBusPolicy.TopicName, serviceBusPolicy.SubscriptionName);

                var messageHandlerOptions = new MessageHandlerOptions(ProcessReceivedExceptionAsync)
                {
                    MaxConcurrentCalls = 1,
                    AutoComplete = false
                };

                _subscriptionClient.RegisterMessageHandler(ProcessMessageAsync, messageHandlerOptions);
            }
            catch (Exception ex)
            {
                var message = await context.CommerceContext.AddMessage(
                    context.GetPolicy<KnownResultCodes>().Error,
                    "ServiceBusMessageHandlerError",
                    new object[]
                    {
                        "ContentHubAzureServiceBus",
                        ex
                    },
                    $"{Name}: Error receiving message from service bus."
                ).ConfigureAwait(false);
                context.Abort(message, context, LogLevel.Error);
            }

            return arg;
        }

        private async Task ProcessMessageAsync(Message message, CancellationToken token)
        {
            var model = JsonConvert.DeserializeObject<ServiceBusMessage>(Encoding.UTF8.GetString(message.Body));

            var command = _commerceCommander.Command<ImportEntityCommand>();
            await command.Process(_environment, model).ConfigureAwait(false);

            if (!command.IsFaulted)
            {
                await _subscriptionClient.CompleteAsync(message.SystemProperties.LockToken).ConfigureAwait(false);
            }
        }

        private Task ProcessReceivedExceptionAsync(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            var exContext = exceptionReceivedEventArgs.ExceptionReceivedContext;

            var message = $"{Name}: Azure Service Bus message handler encountered an error. Endpoint: {exContext.Endpoint}; Entity Path: {exContext.EntityPath}; Executing Action: {exContext.Action}";

            if (exContext.Action.EqualsOrdinalIgnoreCase("RenewLock"))
            {
                _logger.LogWarning(exceptionReceivedEventArgs.Exception, message);
            }
            else
            {
                _logger.LogError(exceptionReceivedEventArgs.Exception, message);
            }

            return Task.CompletedTask;
        }
    }
}
