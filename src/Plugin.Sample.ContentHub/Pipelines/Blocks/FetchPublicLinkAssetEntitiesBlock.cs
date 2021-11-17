// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using Stylelabs.M.Framework.Essentials.LoadConfigurations;
using Stylelabs.M.Sdk.Contracts.Base;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    ///     Represents a block which loads the related assets of a public link and run the <see cref="ImportAssetEntityPipeline"/>
    ///     on each of the related assets.
    /// </summary>
    [PipelineDisplayName(ContentHubConstants.FetchPublicLinkAssetEntitiesBlock)]
    public class FetchPublicLinkAssetEntitiesBlock : AsyncPipelineBlock<ImportEntityArgument, ImportEntityArgument, CommercePipelineExecutionContext>
    {
        private readonly ContentHubClientFactory _clientFactory;

        private readonly IImportAssetEntityPipeline _importAssetPipeline;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FetchPublicLinkAssetEntitiesBlock" />.
        /// </summary>
        /// <param name="clientFactory">The <see cref="ContentHubClientFactory"/> to fetch <see cref="IEntity"/>.</param>
        /// <param name="importAssetPipeline">The <see cref="IImportAssetEntityPipeline"/> to import a content hub entity asset.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="clientFactory"/> or <paramref name="importAssetPipeline"/> is <see langword="null"/>.
        /// </exception>
        public FetchPublicLinkAssetEntitiesBlock(ContentHubClientFactory clientFactory, IImportAssetEntityPipeline importAssetPipeline)
        {
            Condition.Requires(clientFactory, nameof(clientFactory)).IsNotNull();
            Condition.Requires(importAssetPipeline, nameof(importAssetPipeline)).IsNotNull();

            _clientFactory = clientFactory;
            _importAssetPipeline = importAssetPipeline;
        }

        /// <summary>
        ///     Ensures that for each related asset in the <see cref="ImportEntityArgument.Entity">public link</see> in <paramref name="arg"/>
        ///     the <see cref="ImportAssetEntityPipeline"/> is run.
        /// </summary>
        /// <param name="arg">The <see cref="ImportEntityArgument"/> that contains <see cref="IEntity"/>.</param>
        /// <param name="context">The <see cref="CommercePipelineExecutionContext"/> to execute current block.</param>
        /// <returns><paramref name="arg"/>.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="arg"/>, <paramref name="context"/>, or <see cref="ImportEntityArgument.Entity"/>
        ///     of <paramref name="arg"/> is <see langword="null"/>.
        /// </exception>
        public override async Task<ImportEntityArgument> RunAsync(ImportEntityArgument arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg, nameof(arg)).IsNotNull();
            Condition.Requires(context, nameof(context)).IsNotNull();
            Condition.Requires(arg.Entity, nameof(arg.Entity)).IsNotNull();

            if (arg.AssetSynchronizationPolicy == null)
            {
                context.CommerceContext.Logger.LogWarning($"{Name}: {nameof(AssetSynchronizationPolicy)} is not configured. Exiting block.");
                return arg;
            }

            var ids = (await arg.Entity.GetRelationAsync<IChildToManyParentsRelation>
                (arg.AssetSynchronizationPolicy.AssetToPublicLinkRelationName).ConfigureAwait(false))?.Parents ?? new List<long>();

            if (!ids.Any())
            {
                var message = $"{ContentHubConstants.ContentHubEntityMappingMismatch} {Name}: The Public Link Entity (id: {arg.Entity.Id}) " +
                    $"does not have related assets.";
                context.Abort(message, context, LogLevel.Error);

                return arg;
            }

            var mClient = _clientFactory.CreateClient(arg.ClientPolicy);
            var entities = await mClient.Entities.GetManyAsync(ids, EntityLoadConfiguration.Full).ConfigureAwait(false);

            var tasks = new List<Task>();
            foreach (var e in entities)
            {
                var productArg = new ImportEntityArgument(
                    arg.Message,
                    arg.ClientPolicy,
                    arg.SynchronizationPolicy,
                    arg.ProductSynchronizationPolicy,
                    arg.AssetSynchronizationPolicy
                )
                {
                    Entity = e
                };

                tasks.Add(_importAssetPipeline.RunAsync(productArg, context));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            return arg;
        }
    }
}
