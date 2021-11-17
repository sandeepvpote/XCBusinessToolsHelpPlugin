// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using Stylelabs.M.Framework.Essentials.LoadConfigurations;
using Stylelabs.M.Sdk.Contracts.Base;
using Stylelabs.M.Sdk.WebClient;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    /// Defines a block that fetches the Content Hub asset public links.
    /// </summary>
    [PipelineDisplayName(ContentHubConstants.FetchImagePublicLinksBlock)]
    public class FetchImagePublicLinksBlock : AsyncPipelineBlock<ImportEntityArgument, ImportEntityArgument, CommercePipelineExecutionContext>
    {
        private readonly ContentHubClientFactory _clientFactory;

        /// <summary>
        ///     Initializes an instance of the <see cref="FetchImagePublicLinksBlock"/>.
        /// </summary>
        /// <param name="clientFactory">The <see cref="ContentHubClientFactory"/> to get <see cref="IWebMClient"/>.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="clientFactory"/> is <see langword="null"/>.
        /// </exception>
        public FetchImagePublicLinksBlock(ContentHubClientFactory clientFactory)
        {
            Condition.Requires(clientFactory, nameof(clientFactory)).IsNotNull();

            _clientFactory = clientFactory;
        }

        /// <summary>
        ///     Retrieves <see cref="IEntity"/> Public Link instances, related to
        ///     <see cref="ImportEntityArgument.Assets"/> of <paramref name="arg"/>.
        /// </summary>
        /// <param name="arg">
        ///     The <see cref="ImportEntityArgument"/>, which contains <see cref="ImportEntityArgument.Assets"/>.
        /// </param>
        /// <param name="context">The <see cref="CommercePipelineExecutionContext"/> for current block execution.</param>
        /// <returns>
        ///     <paramref name="arg"/> with retrieved <see cref="ImportEntityArgument.PublicLinkToImageMappings"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="arg"/> or <paramref name="context"/> is <see langword="null"/>.
        /// </exception>
        public override async Task<ImportEntityArgument> RunAsync(ImportEntityArgument arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg, nameof(arg)).IsNotNull();
            Condition.Requires(context, nameof(context)).IsNotNull();

            if (arg.AssetSynchronizationPolicy == null)
            {
                context.CommerceContext.Logger.LogWarning($"{Name}: {nameof(AssetSynchronizationPolicy)} is not configured. Exiting block.");
                return arg;
            }

            context.CommerceContext.Logger.LogDebug($"{Name}: Loading public links for assets");

            if (arg.Assets == null || !arg.Assets.Any())
            {
                return arg;
            }

            var images = await GetImagesAsync(arg.Assets, arg.SynchronizationPolicy.DefaultCulture).ConfigureAwait(false);

            if (!images.Any())
            {
                return arg;
            }

            var mClient = _clientFactory.CreateClient(arg.ClientPolicy);

            var linkIdToEntityMappings = await GetPublicLinkRelationIdsAsync(images, arg.AssetSynchronizationPolicy.AssetToPublicLinkRelationName).ConfigureAwait(false);

            if (linkIdToEntityMappings.Any())
            {
                var linkEntities = await mClient.Entities.GetManyAsync(linkIdToEntityMappings.Keys, EntityLoadConfiguration.Full).ConfigureAwait(false);

                arg.PublicLinkToImageMappings = linkEntities?.Where(link => link.Id.HasValue)
                    .ToDictionary(link => link, link => linkIdToEntityMappings[link.Id.Value]);
            }

            return arg;
        }

        private static async Task<IList<IEntity>> GetImagesAsync(IList<IEntity> assets, string culture)
        {
            var images = new List<IEntity>();

            foreach (var asset in assets)
            {
                var filePropsJson = await asset.GetMultiLanguagePropertyValueAsync<JToken>(ContentHubConstants.AssetFilePropertiesPropertyName, culture).ConfigureAwait(false);
                var fileProperties = filePropsJson?[ContentHubConstants.FilePropertiesFieldName]?.ToObject<FileProperties>();
                if (fileProperties != null && fileProperties.Group.EqualsOrdinalIgnoreCase(ContentHubConstants.ImagesGroupName))
                {
                    images.Add(asset);
                }
            }

            return images;
        }

        private static async Task<IDictionary<long, IEntity>> GetPublicLinkRelationIdsAsync(IEnumerable<IEntity> images,
            string assetToPublicLinkRelationName)
        {
            var ids = new Dictionary<long, IEntity>();

            foreach (var entity in images)
            {
                var relation = await entity.GetRelationAsync<IParentToManyChildrenRelation>(assetToPublicLinkRelationName).ConfigureAwait(false);
                var children = relation?.Children;
                if (children != null && children.Any())
                {
                    foreach (var linkId in children)
                    {
                        ids.Add(linkId, entity);
                    }
                }
            }

            return ids;
        }
    }
}
