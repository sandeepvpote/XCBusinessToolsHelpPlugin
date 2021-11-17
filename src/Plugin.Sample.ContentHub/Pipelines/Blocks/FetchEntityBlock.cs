// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using Stylelabs.M.Framework.Essentials.LoadConfigurations;
using Stylelabs.M.Sdk.Contracts.Base;
using Stylelabs.M.Sdk.WebClient;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    ///     Defines a block that fetches the Content Hub <see cref="IEntity"/>.
    /// </summary>
    [PipelineDisplayName(ContentHubConstants.FetchEntityBlock)]
    public class FetchEntityBlock : AsyncPipelineBlock<ImportEntityArgument, ImportEntityArgument, CommercePipelineExecutionContext>
    {
        private readonly ContentHubClientFactory _clientFactory;

        private readonly IImportProductEntityPipeline _importProductEntityPipeline;

        private readonly IImportAssetEntityPipeline _importAssetEntityPipeline;

        private readonly IImportPublicLinkEntityPipeline _importPublicLinkEntityPipeline;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FetchEntityBlock"/> class.
        /// </summary>
        /// <param name="clientFactory">The <see cref="ContentHubClientFactory"/> to get <see cref="IWebMClient"/>.</param>
        /// <param name="importProductEntityPipeline">
        ///     The <see cref="ImportProductEntityPipeline"/> to update <see cref="SellableItem"/> from Content Hub product.
        /// </param>
        /// <param name="importAssetEntityPipeline">
        ///     The <see cref="ImportAssetEntityPipeline"/> to update <see cref="ContentHubImagesComponent"/> of all
        ///     related <see cref="SellableItem"/> from Content Hub asset.
        /// </param>
        /// <param name="importPublicLinkEntityPipeline">
        ///     The <see cref="ImportPublicLinkEntityPipeline"/> to update <see cref="ContentHubImagesComponent"/> from Content Hub public link.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="clientFactory"/>, <paramref name="importProductEntityPipeline"/>, <paramref name="importAssetEntityPipeline"/> or
        ///     <paramref name="importPublicLinkEntityPipeline"/> is <see langword="null"/>.
        /// </exception>
        public FetchEntityBlock(ContentHubClientFactory clientFactory, IImportProductEntityPipeline importProductEntityPipeline, IImportAssetEntityPipeline importAssetEntityPipeline,
            IImportPublicLinkEntityPipeline importPublicLinkEntityPipeline)
        {
            Condition.Requires(clientFactory, nameof(clientFactory)).IsNotNull();
            Condition.Requires(importProductEntityPipeline, nameof(importProductEntityPipeline)).IsNotNull();
            Condition.Requires(importAssetEntityPipeline, nameof(importAssetEntityPipeline)).IsNotNull();
            Condition.Requires(importPublicLinkEntityPipeline, nameof(importPublicLinkEntityPipeline)).IsNotNull();

            _clientFactory = clientFactory;
            _importProductEntityPipeline = importProductEntityPipeline;
            _importAssetEntityPipeline = importAssetEntityPipeline;
            _importPublicLinkEntityPipeline = importPublicLinkEntityPipeline;
        }

        /// <summary>
        ///     Retrieves <see cref="IEntity"/> from Content Hub using <see cref="IWebMClient"/> and checks the <see cref="ServiceBusMessage.TargetDefinition"/>
        ///     to call the specific <see cref="IPipeline"/>.
        /// </summary>
        /// <param name="arg">
        ///     The <see cref="ImportEntityArgument"/> which contains information about the <see cref="IEntity"/> to load.
        /// </param>
        /// <param name="context">The <see cref="CommercePipelineExecutionContext"/> for current block execution.</param>
        /// <returns>The <paramref name="arg"/> that contains the loaded <see cref="IEntity"/>.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="arg"/> or <paramref name="context"/> is <see langword="null"/>.
        /// </exception>
        public override async Task<ImportEntityArgument> RunAsync(ImportEntityArgument arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg, nameof(arg)).IsNotNull();
            Condition.Requires(context, nameof(context)).IsNotNull();

            context.CommerceContext.Logger.LogDebug($"{Name}: TargetId: {arg.TargetId}, TargetIdentifier: {arg.TargetIdentifier}, EventType: {arg.EventType}, Version: {arg.Version}");

            if (!arg.Message.TargetDefinition.EqualsOrdinalIgnoreCase(arg.SynchronizationPolicy.ProductEntityType) &&
                !arg.Message.TargetDefinition.EqualsOrdinalIgnoreCase(arg.SynchronizationPolicy.AssetEntityType) &&
                !arg.Message.TargetDefinition.EqualsOrdinalIgnoreCase(arg.SynchronizationPolicy.PublicLinkEntityType))
            {
                var message = $"{ContentHubConstants.ContentHubEntityMappingMismatch} {Name}: The Entity (id: {arg.TargetId}) " +
                    $"Entity Definition ({arg.Message.TargetDefinition}) does not match the configured Policy Entity mapping " +
                    $"({arg.SynchronizationPolicy.ProductEntityType}, {arg.SynchronizationPolicy.AssetEntityType} or " +
                    $"{arg.SynchronizationPolicy.PublicLinkEntityType}).";
                context.Abort(message, context, LogLevel.Error);

                return arg;
            }

            if (arg.Message.TargetDefinition.EqualsOrdinalIgnoreCase(arg.SynchronizationPolicy.ProductEntityType) && arg.ProductSynchronizationPolicy == null)
            {
                var message = $"{Name}: {nameof(ProductSynchronizationPolicy)} is not configured. Exiting block.";
                context.Abort(message, context, LogLevel.Warning);

                return arg;
            }

            if ((arg.Message.TargetDefinition.EqualsOrdinalIgnoreCase(arg.SynchronizationPolicy.AssetEntityType) ||
                arg.Message.TargetDefinition.EqualsOrdinalIgnoreCase(arg.SynchronizationPolicy.PublicLinkEntityType)) && arg.AssetSynchronizationPolicy == null)
            {
                var message = $"{Name}: {nameof(AssetSynchronizationPolicy)} is not configured. Exiting block.";
                context.Abort(message, context, LogLevel.Warning);

                return arg;
            }

            var mClient = _clientFactory.CreateClient(arg.ClientPolicy);
            var entity = await mClient.Entities.GetAsync(arg.TargetId, EntityLoadConfiguration.Full).ConfigureAwait(false);

            if (entity == null)
            {
                var message = $"{ContentHubConstants.ContentHubEntityNotFoundError} {Name}: The Entity (id: {arg.TargetId}) was not found in Content Hub.";
                context.Abort(message, context, LogLevel.Warning);

                return arg;
            }

            arg.Entity = entity;

            if (arg.Message.TargetDefinition.EqualsOrdinalIgnoreCase(arg.SynchronizationPolicy.ProductEntityType))
            {
                await _importProductEntityPipeline.RunAsync(arg, context).ConfigureAwait(false);
            }
            else if (arg.Message.TargetDefinition.EqualsOrdinalIgnoreCase(arg.SynchronizationPolicy.AssetEntityType))
            {
                await _importAssetEntityPipeline.RunAsync(arg, context).ConfigureAwait(false);
            }
            else if (arg.Message.TargetDefinition.EqualsOrdinalIgnoreCase(arg.SynchronizationPolicy.PublicLinkEntityType))
            {
                await _importPublicLinkEntityPipeline.RunAsync(arg, context).ConfigureAwait(false);
            }

            return arg;
        }
    }
}
