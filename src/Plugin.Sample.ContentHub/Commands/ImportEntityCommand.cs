// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Framework.Conditions;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    ///     Represents a <see cref="CommerceCommand"/> that imports changes of
    ///     <see cref="CommerceEntity"/> done in Content Hub.
    /// </summary>
    public class ImportEntityCommand : CommerceCommand
    {
        private readonly IImportEntityPipeline _importEntityPipeline;
        private readonly ILogger<ImportEntityCommand> _logger;
        private readonly CommerceEnvironment _globalEnvironment;
        private readonly TelemetryClient _telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportEntityCommand"/> class.
        /// </summary>
        /// <param name="importEntityPipeline">
        ///     The <see cref="IImportEntityPipeline"/> to import changes of <see cref="CommerceEntity"/> done in Content Hub.
        /// </param>
        /// <param name="logger">The <see cref="ILogger"/> to log diagnostic information.</param>
        /// <param name="telemetryClient">
        ///     The <see cref="TelemetryClient"/>, required for <see cref="CommerceContext"/>.
        /// </param>
        /// <param name="globalEnvironment">
        ///     The global <see cref="CommerceEnvironment"/>, required for <see cref="CommerceContext"/>.
        /// </param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> that contains additional dependencies.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="importEntityPipeline"/>, <paramref name="globalEnvironment"/>, <paramref name="logger"/>, or
        ///     <paramref name="telemetryClient"/> is <see langword="null"/>.
        /// </exception>
        public ImportEntityCommand(IImportEntityPipeline importEntityPipeline, ILogger<ImportEntityCommand> logger,
            TelemetryClient telemetryClient, CommerceEnvironment globalEnvironment,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Condition.Requires(importEntityPipeline, nameof(importEntityPipeline)).IsNotNull();
            Condition.Requires(logger, nameof(logger)).IsNotNull();
            Condition.Requires(telemetryClient, nameof(telemetryClient)).IsNotNull();
            Condition.Requires(globalEnvironment, nameof(globalEnvironment)).IsNotNull();

            _importEntityPipeline = importEntityPipeline;
            _logger = logger;
            _globalEnvironment = globalEnvironment;
            _telemetryClient = telemetryClient;
        }

        /// <summary>
        ///     Imports changes of <see cref="CommerceEntity"/> done in Content Hub.
        /// </summary>
        /// <param name="environment">
        ///     The <see cref="CommerceEnvironment"/> within which <see cref="IImportEntityPipeline"/> will be executed.
        /// </param>
        /// <param name="serviceBusMessage">
        ///     The <see cref="ServiceBusMessage"/> that contains information about changed <see cref="CommerceEntity"/>.
        /// </param>
        /// <returns>
        ///     A <see cref="Task"/> that represents an asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="environment"/> or <paramref name="serviceBusMessage"/> is <see langword="null"/>.
        /// </exception>
        public virtual async Task Process(CommerceEnvironment environment, ServiceBusMessage serviceBusMessage)
        {
            Condition.Requires(environment, nameof(environment)).IsNotNull();
            Condition.Requires(serviceBusMessage, nameof(serviceBusMessage)).IsNotNull();

            var context = new CommerceContext(_logger, _telemetryClient)
            {
                GlobalEnvironment = _globalEnvironment,
                Environment = environment
            };

            if (!context.HasPolicy<ContentHubClientPolicy>())
            {
                _logger.LogError($"{nameof(ImportEntityCommand)}: {nameof(ContentHubClientPolicy)} is not found. Execution failed.");
                IsFaulted = true;
                return;
            }

            if (!context.HasPolicy<SynchronizationPolicy>())
            {
                _logger.LogError($"{nameof(ImportEntityCommand)}: {nameof(SynchronizationPolicy)} is not found. Execution failed.");
                IsFaulted = true;
                return;
            }

            if (!context.HasPolicy<ProductSynchronizationPolicy>() && !context.HasPolicy<AssetSynchronizationPolicy>())
            {
                _logger.LogError($"{nameof(ImportEntityCommand)}: {nameof(ProductSynchronizationPolicy)} and {nameof(AssetSynchronizationPolicy)} are not found. Execution failed.");
                IsFaulted = true;
                return;
            }

            try
            {
                using (CommandActivity.Start(context, this))
                {
                    var clientPolicy = context.GetPolicy<ContentHubClientPolicy>();
                    var synchronizationPolicy = context.GetPolicy<SynchronizationPolicy>();

                    ProductSynchronizationPolicy productSynchronizationPolicy = null;
                    AssetSynchronizationPolicy assetSynchronizationPolicy = null;

                    if (context.HasPolicy<ProductSynchronizationPolicy>())
                    {
                        productSynchronizationPolicy = context.GetPolicy<ProductSynchronizationPolicy>();
                    }

                    if (context.HasPolicy<AssetSynchronizationPolicy>())
                    {
                        assetSynchronizationPolicy = context.GetPolicy<AssetSynchronizationPolicy>();
                    }

                    ContentHubClientFactory.RetryCount = clientPolicy.RetryCount;
                    ContentHubClientFactory.Timeout = TimeSpan.FromSeconds(clientPolicy.TimeoutInSeconds);

                    var argument = new ImportEntityArgument(serviceBusMessage, clientPolicy, synchronizationPolicy, productSynchronizationPolicy, assetSynchronizationPolicy);
                    await _importEntityPipeline.RunAsync(argument, context.PipelineContextOptions).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error during {nameof(ImportEntityCommand)} execution");
                IsFaulted = true;
            }
        }
    }
}
