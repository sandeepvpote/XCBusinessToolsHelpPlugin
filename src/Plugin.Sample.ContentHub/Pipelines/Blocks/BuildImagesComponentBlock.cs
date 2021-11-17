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
using Stylelabs.M.Sdk.Contracts.Base;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    ///     Defines a block that builds <see cref="ContentHubImagesComponent"/> based on the information,
    ///     provided by <see cref="ImportEntityArgument"/>.
    /// </summary>
    [PipelineDisplayName(ContentHubConstants.BuildImagesComponentBlock)]
    public class BuildImagesComponentBlock : AsyncPipelineBlock<ImportEntityArgument, ImportEntityArgument, CommercePipelineExecutionContext>
    {
        /// <summary>
        ///     Builds an <see cref="ContentHubImagesComponent"/> based on the information, provided in <paramref name="arg"/>.
        /// </summary>
        /// <param name="arg">
        ///     The <see cref="ImportEntityArgument"/> which contains information, required to build <see cref="ContentHubImagesComponent"/>.
        /// </param>
        /// <param name="context">The <see cref="CommercePipelineExecutionContext"/> for current block execution.</param>
        /// <returns><paramref name="arg"/> with built <see cref="ImportEntityArgument.ContentHubImagesComponent"/>.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="arg"/>, <paramref name="arg.Entity"/> or <paramref name="context"/> is <see langword="null"/>.
        /// </exception>
        public override async Task<ImportEntityArgument> RunAsync(ImportEntityArgument arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg, nameof(arg)).IsNotNull();
            Condition.Requires(arg.Entity, nameof(arg.Entity)).IsNotNull();
            Condition.Requires(context, nameof(context)).IsNotNull();

            if (arg.AssetSynchronizationPolicy == null)
            {
                context.CommerceContext.Logger.LogWarning($"{Name}: {nameof(AssetSynchronizationPolicy)} is not configured. Exiting block.");
                return arg;
            }

            context.CommerceContext.Logger.LogDebug($"{Name}: Building {nameof(ContentHubImagesComponent)}");

            if (arg.PublicLinkToImageMappings == null || !arg.PublicLinkToImageMappings.Any())
            {
                return arg;
            }

            var defaultCulture = arg.SynchronizationPolicy.DefaultCulture;
            var altTextFieldName = arg.AssetSynchronizationPolicy.AssetsAlternateTextFieldName;
            var imagesComponent = new ContentHubImagesComponent();

            foreach (var link in arg.PublicLinkToImageMappings.Keys)
            {
                var status = await link.GetMultiLanguagePropertyValueAsync<string>(ContentHubConstants.PublicLinkStatusPropertyName, defaultCulture).ConfigureAwait(false);
                if (status != null && status.EqualsOrdinalIgnoreCase(ContentHubConstants.PublicLinkCompletedStatusName))
                {
                    var image = arg.PublicLinkToImageMappings[link];

                    var url = await GenerateLinkUrlAsync(link, arg.ClientPolicy.EndpointUrl, defaultCulture).ConfigureAwait(false);
                    var dimensions = await GetImageDimensions(image, link, defaultCulture).ConfigureAwait(false);
                    var altText = await image.GetMultiLanguagePropertyValueAsync<string>(altTextFieldName, defaultCulture).ConfigureAwait(false);
                    var masterAssetRelation = await image.GetRelationAsync<IChildToManyParentsRelation>(arg.AssetSynchronizationPolicy.EntityToMasterAssetRelationName).ConfigureAwait(false);
                    bool isMaster = masterAssetRelation != null && masterAssetRelation.Parents.Any(id => id == arg.Entity.Id);

                    imagesComponent.ChildComponents.Add(new ContentHubImage(url, dimensions.Item1, dimensions.Item2, altText, isMaster, link.Id));
                }
            }

            arg.ContentHubImagesComponent = imagesComponent;

            return arg;
        }

        private static async Task<FileProperties> GetAssetFilePropertiesAsync(IEntity asset, string culture)
        {
            var filePropsJson = await asset.GetMultiLanguagePropertyValueAsync<JToken>(ContentHubConstants.AssetFilePropertiesPropertyName, culture).ConfigureAwait(false);
            return filePropsJson[ContentHubConstants.FilePropertiesFieldName]?.ToObject<FileProperties>();
        }

        private static async Task<Tuple<int, int>> GetImageDimensions(IEntity image, IEntity link, string culture)
        {
            var conversionConfigJson = await link.GetMultiLanguagePropertyValueAsync<JToken>(ContentHubConstants.PublicLinkConversionConfigurationPropertyName, culture).ConfigureAwait(false);
            var config = conversionConfigJson?.ToObject<ConversionConfiguration>();

            if (config != null && config.OriginalWidth > 0 && config.OriginalHeight > 0)
            {
                return config.CroppingConfiguration == null ? new Tuple<int, int>(config.OriginalWidth, config.OriginalHeight) : new Tuple<int, int>(config.CroppingConfiguration.Width, config.CroppingConfiguration.Height);
            }

            var renditionsJson = await image.GetMultiLanguagePropertyValueAsync<JToken>(ContentHubConstants.AssetRenditionsPropertyName, culture).ConfigureAwait(false);
            var renditions = renditionsJson?.ToObject<Dictionary<string, AssetRendition>>().Values.Where(ar => ar.Locations != null && ar.Locations.Any()).ToDictionary(v => v.Locations.First().Value.First());

            if (renditions != null)
            {
                var linkRendition = await link.GetMultiLanguagePropertyValueAsync<string>(ContentHubConstants.PublicLinkFileKeyPropertyName, culture).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(linkRendition) && renditions.ContainsKey(linkRendition))
                {
                    var renditionProps = renditions[linkRendition].Properties;

                    int width = 0;
                    int height = 0;

                    var dimensionsParsed = renditionProps.ContainsKey(ContentHubConstants.RenditionWidthPropertyName) &&
                        int.TryParse(renditionProps[ContentHubConstants.RenditionWidthPropertyName], out width) &&
                        renditionProps.ContainsKey(ContentHubConstants.RenditionHeightPropertyName) &&
                        int.TryParse(renditionProps[ContentHubConstants.RenditionHeightPropertyName], out height);

                    if (dimensionsParsed)
                    {
                        return new Tuple<int, int>(width, height);
                    }
                }

                if (linkRendition.EqualsOrdinalIgnoreCase(ContentHubConstants.DownloadOriginalRenditionName))
                {
                    var fileProps = await GetAssetFilePropertiesAsync(image, culture).ConfigureAwait(false);
                    return new Tuple<int, int>(fileProps.Width, fileProps.Height);
                }
            }

            var fileProperties = await GetAssetFilePropertiesAsync(image, culture).ConfigureAwait(false);
            return new Tuple<int, int>(fileProperties.Width, fileProperties.Height);
        }

        private static async Task<Uri> GenerateLinkUrlAsync(IEntity linkEntity, string endpointUrl, string culture)
        {
            var relativeUrl = await linkEntity.GetMultiLanguagePropertyValueAsync<string>(ContentHubConstants.PublicLinkRelativeUrl, culture).ConfigureAwait(false);
            var versionHash = await linkEntity.GetMultiLanguagePropertyValueAsync<string>(ContentHubConstants.PublicLinkVersionHash, culture).ConfigureAwait(false);

            return new Uri($"{endpointUrl.TrimEnd('/')}/api/public/content/{relativeUrl}?v={versionHash}");
        }
    }
}
