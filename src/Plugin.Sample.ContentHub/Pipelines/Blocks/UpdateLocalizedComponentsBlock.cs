// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    /// Defines a block that updates the <see cref="LocalizationEntity"/> <see cref="ContentHubImagesComponent"/> corresponding to
    /// a <see cref="SellableItem"/> that has been updated in Content Hub.
    /// </summary>
    [PipelineDisplayName(ContentHubConstants.UpdateLocalizedComponentsBlock)]
    public class UpdateLocalizedComponentsBlock : AsyncPipelineBlock<ImportEntityArgument, ImportEntityArgument, CommercePipelineExecutionContext>
    {
        /// <summary>
        /// Updates the <see cref="LocalizationEntity"/> ComponentsProperties
        /// </summary>
        /// <param name="arg">The <see cref="ImportEntityArgument"/>, containing information about the update.</param>
        /// <param name="context">The <see cref="CommercePipelineExecutionContext"/></param>
        /// <returns>The <paramref name="arg"/>.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="arg"/>, <paramref name="arg.CommerceEntity"/>,
        ///     or <paramref name="context"/> is <see langword="null"/>.
        /// </exception>
        public override async Task<ImportEntityArgument> RunAsync(ImportEntityArgument arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg, nameof(arg)).IsNotNull();
            Condition.Requires(arg.CommerceEntity, nameof(arg.CommerceEntity)).IsNotNull();
            Condition.Requires(context, nameof(context)).IsNotNull();

            if (arg.AssetSynchronizationPolicy == null)
            {
                context.CommerceContext.Logger.LogWarning($"{Name}: {nameof(AssetSynchronizationPolicy)} is not configured. Exiting block.");
                return arg;
            }

            if (arg.PublicLinkToImageMappings == null || !arg.PublicLinkToImageMappings.Any())
            {
                context.CommerceContext.Logger.LogWarning($"{Name}: No localizations to persist. Exiting block.");
                return arg;
            }

            var localizationEntity = arg.LocalizationEntity;
            if (localizationEntity == null)
            {
                context.CommerceContext.Logger.LogError($"{Name}: LocalizationEntity for sellable item {arg.CommerceEntity.Id} could not be found.");
                return arg;
            }

            var policy = LocalizeEntityPolicy.GetPolicyByType(context.CommerceContext, arg.CommerceEntity.GetType());
            if (policy == null)
            {
                context.CommerceContext.Logger.LogError($"{Name}: LocalizedEntityPolicy was not found for type {arg.CommerceEntity.GetType().FullName}.");
                return arg;
            }

            var componentPolicy = policy.GetItemComponentPolicyByName(nameof(ContentHubImage));
            if (componentPolicy == null)
            {
                context.CommerceContext.Logger.LogError($"{Name}: LocalizeEntityComponentPolicy was not found for type {nameof(ContentHubImage)}.");
                return arg;
            }

            arg.LocalizationsUpdated = true;
            var defaultCulture = arg.SynchronizationPolicy.DefaultCulture;
            var altTextFieldName = arg.AssetSynchronizationPolicy.AssetsAlternateTextFieldName;

            foreach (var link in arg.PublicLinkToImageMappings.Keys)
            {
                var asset = arg.PublicLinkToImageMappings[link];

                var localizedValues = new List<Parameter>();
                foreach (var culture in link.Cultures)
                {
                    if (culture.Name.EqualsOrdinalIgnoreCase(defaultCulture))
                    {
                        continue;
                    }

                    var status = await link.GetMultiLanguagePropertyValueAsync<string>(ContentHubConstants.PublicLinkStatusPropertyName, culture).ConfigureAwait(false);
                    if (status != null && status.EqualsOrdinalIgnoreCase(ContentHubConstants.PublicLinkCompletedStatusName))
                    {
                        var altText = await asset.GetMultiLanguagePropertyValueAsync<string>(altTextFieldName, culture).ConfigureAwait(false);
                        if (!string.IsNullOrEmpty(altText))
                        {
                            localizedValues.Add(new Parameter(culture.Name, altText));
                        }
                    }
                }

                if (localizedValues.Any())
                {
                    localizationEntity.AddOrUpdateComponentValue(componentPolicy.Path, link.Id.ToString(), altTextFieldName, localizedValues);
                }
            }

            return arg;
        }
    }
}
