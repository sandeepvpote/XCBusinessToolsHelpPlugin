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
    ///     Defines a block that loads the <see cref="IEntity">product entities</see> related to a Content Hub <see cref="IEntity">asset</see>.
    /// </summary>
    [PipelineDisplayName(ContentHubConstants.FetchAssetProductEntitiesBlock)]
    public class FetchAssetProductEntitiesBlock : AsyncPipelineBlock<ImportEntityArgument, ImportEntityArgument, CommercePipelineExecutionContext>
    {
        private readonly ContentHubClientFactory _clientFactory;

        private readonly IImportProductEntityPipeline _importProductPipeline;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FetchAssetProductEntitiesBlock" />.
        /// </summary>
        /// <param name="clientFactory">The <see cref="ContentHubClientFactory"/> to fetch the related products <see cref="IEntity"/>.</param>
        /// <param name="importProductPipeline">The <see cref="IImportProductEntityPipeline"/> to import a content hub entity product.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="clientFactory"/> or <paramref name="importProductPipeline"/> is <see langword="null"/>.
        /// </exception>
        public FetchAssetProductEntitiesBlock(ContentHubClientFactory clientFactory, IImportProductEntityPipeline importProductPipeline)
        {
            Condition.Requires(clientFactory, nameof(clientFactory)).IsNotNull();
            Condition.Requires(importProductPipeline, nameof(importProductPipeline)).IsNotNull();

            _clientFactory = clientFactory;
            _importProductPipeline = importProductPipeline;
        }

        /// <summary>
        ///     Ensures that for each related product in the <see cref="ImportEntityArgument.Entity">asset</see> in <paramref name="arg"/>
        ///     and the removed ids from the <see cref="Changeset.RelationChanges"/> the <see cref="ImportAssetEntityPipeline"/> is run.
        /// property.
        /// </summary>
        /// <param name="arg">The <see cref="ImportEntityArgument"/> that contains <see cref="IEntity"/>.</param>
        /// <param name="context">The <see cref="CommercePipelineExecutionContext"/> to execute current block.</param>
        /// <returns><paramref name="arg"/>.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="arg"/>, <paramref name="context"/>, or <see cref="ImportEntityArgument.Entity"/> is <see langword="null"/>.
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

            context.CommerceContext.Logger.LogDebug($"{Name}: Started.");

            RelationChange relation = null;
            if (arg.Message.SaveEntityMessage != null)
            {
                relation = arg.Message.SaveEntityMessage.ChangeSet.RelationChanges
                    .FirstOrDefault(t => t.Relation.EqualsOrdinalIgnoreCase(arg.AssetSynchronizationPolicy.EntityToAssetRelationName));
            }

            var removedIds = relation?.RemovedValues ?? new List<int>();

            var relatedIds = (await arg.Entity.GetRelationAsync<IChildToManyParentsRelation>
                (arg.AssetSynchronizationPolicy.EntityToAssetRelationName).ConfigureAwait(false))?.Parents ?? new List<long>();

            var ids = relatedIds.Union(removedIds.Select(t => (long) t)).ToList();
            if (!ids.Any())
            {
                var message = $"{ContentHubConstants.ContentHubEntityMappingMismatch} {Name}: The Asset Entity (id: {arg.Entity.Id}) " +
                    $"does not have related products.";
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

                tasks.Add(_importProductPipeline.RunAsync(productArg, context));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            return arg;
        }
    }
}
