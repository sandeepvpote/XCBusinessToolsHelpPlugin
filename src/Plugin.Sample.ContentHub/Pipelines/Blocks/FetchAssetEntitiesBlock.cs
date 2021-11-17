// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using Stylelabs.M.Framework.Essentials.LoadConfigurations;
using Stylelabs.M.Sdk.Contracts.Base;
using Stylelabs.M.Sdk.WebClient;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    /// Defines a block that fetches Content Hub asset entities.
    /// </summary>
    [PipelineDisplayName(ContentHubConstants.FetchAssetEntitiesBlock)]
    public class FetchAssetEntitiesBlock : AsyncPipelineBlock<ImportEntityArgument, ImportEntityArgument, CommercePipelineExecutionContext>
    {
        private readonly ContentHubClientFactory _clientFactory;

        /// <summary>
        ///     Initializes an instance of the <see cref="FetchAssetEntitiesBlock"/>.
        /// </summary>
        /// <param name="clientFactory">The <see cref="ContentHubClientFactory"/> to get <see cref="IWebMClient"/>.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="clientFactory"/> is <see langword="null"/>.
        /// </exception>
        public FetchAssetEntitiesBlock(ContentHubClientFactory clientFactory)
        {
            Condition.Requires(clientFactory, nameof(clientFactory)).IsNotNull();

            _clientFactory = clientFactory;
        }

        /// <summary>
        ///     Retrieves <see cref="IEntity"/> asset instances, related to
        ///     <see cref="ImportEntityArgument.Entity"/> of <paramref name="arg"/>.
        /// </summary>
        /// <param name="arg">
        ///     The <see cref="ImportEntityArgument"/> which contains <see cref="IEntity"/>.
        /// </param>
        /// <param name="context">The <see cref="CommercePipelineExecutionContext"/> for current block execution.</param>
        /// <returns><paramref name="arg"/> with retrieved <see cref="ImportEntityArgument.Assets"/>.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="arg"/>, <paramref name="context"/>, or
        ///     <see cref="ImportEntityArgument.Entity"/> of <paramref name="arg"/> is <see langword="null"/>.
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

            context.CommerceContext.Logger.LogDebug($"{Name}: Loading assets. Entity id: {arg.Entity.Id}");

            var assetIds = (await arg.Entity.GetRelationAsync<IParentToManyChildrenRelation>
                (arg.AssetSynchronizationPolicy.EntityToAssetRelationName).ConfigureAwait(false))?.Children;

            if (assetIds != null && assetIds.Any())
            {
                IWebMClient mClient = _clientFactory.CreateClient(arg.ClientPolicy);
                var assets = await mClient.Entities.GetManyAsync(assetIds, EntityLoadConfiguration.Full).ConfigureAwait(false);
                arg.Assets = assets;
            }

            return arg;
        }
    }
}
