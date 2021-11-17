// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    /// Defines a block that updates the <see cref="LocalizationEntity"/> corresponding to
    /// a <see cref="SellableItem"/> that has been updated in Content Hub.
    /// </summary>
    [PipelineDisplayName(ContentHubConstants.UpdateLocalizedPropertiesBlock)]
    public class UpdateLocalizedPropertiesBlock : SyncPipelineBlock<ImportEntityArgument, ImportEntityArgument, CommercePipelineExecutionContext>
    {
        /// <summary>
        /// Updates the <see cref="LocalizationEntity"/>
        /// </summary>
        /// <param name="arg">The <see cref="ImportEntityArgument"/>, containing information about the update.</param>
        /// <param name="context">The <see cref="CommercePipelineExecutionContext"/></param>
        /// <returns>The <paramref name="arg"/>.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="arg"/>, <paramref name="arg.CommerceEntity"/>,
        ///     or <paramref name="context"/> is <see langword="null"/>.
        /// </exception>
        public override ImportEntityArgument Run(ImportEntityArgument arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg, nameof(arg)).IsNotNull();
            Condition.Requires(arg.CommerceEntity, nameof(arg.CommerceEntity)).IsNotNull();
            Condition.Requires(context, nameof(context)).IsNotNull();

            if (arg.ProductSynchronizationPolicy == null)
            {
                context.CommerceContext.Logger.LogWarning($"{Name}: {nameof(ProductSynchronizationPolicy)} is not configured. Exiting block.");
                return arg;
            }

            if (!UpdateIncludesLocalizations(arg))
            {
                arg.LocalizationsUpdated = false;
                context.CommerceContext.Logger.LogWarning($"{Name}: No localizations to persist. Exiting block.");
                return arg;
            }

            var localizationPolicy = LocalizeEntityPolicy.GetPolicyByType(context.CommerceContext, typeof(SellableItem));
            if (localizationPolicy?.Properties == null || !localizationPolicy.Properties.Any())
            {
                arg.LocalizationsUpdated = false;
                return arg;
            }

            arg.LocalizationsUpdated = true;

            var localizationEntity = arg.LocalizationEntity;
            if (localizationEntity == null)
            {
                context.CommerceContext.Logger.LogError($"{Name}: LocalizationEntity for sellable item {arg.CommerceEntity.Id} could not be found.");
                return arg;
            }

            var propertyMappings = arg.ProductSynchronizationPolicy.PropertyMapping.Where(m => !m.ToProperty.Contains(".", StringComparison.OrdinalIgnoreCase));
            if (propertyMappings.Any())
            {
                foreach (var mapping in propertyMappings)
                {
                    if (!IsCommercePropertyLocalizable(mapping.ToProperty, localizationPolicy))
                    {
                        context.CommerceContext.Logger.LogDebug($"{Name} Localized values for property {mapping.ToProperty} could not be updated because " +
                            $"this property is not currently localized. Refer to the {nameof(LocalizeEntityPolicy)}.");
                        continue;
                    }

                    foreach (var lang in arg.Properties.Keys)
                    {
                        if (lang.EqualsOrdinalIgnoreCase(arg.SynchronizationPolicy.DefaultCulture))
                        {
                            continue;
                        }

                        var props = arg.Properties[lang];
                        if (props != null && props.ContainsKey(mapping.FromProperty))
                        {
                            var updatedValue = props[mapping.FromProperty];
                            localizationEntity.AddOrUpdatePropertyValue(mapping.ToProperty, new List<Parameter>()
                            {
                                new Parameter(lang, updatedValue)
                            });
                        }
                        else
                        {
                            context.CommerceContext.Logger.LogDebug($"{Name} Property {mapping.ToProperty} could not be updated for culture {lang} " +
                                $"because its value was not returned in the content hub query.");
                        }
                    }
                }
            }

            return arg;
        }

        private static bool UpdateIncludesLocalizations(ImportEntityArgument arg)
        {
            foreach (var c in arg.Properties.Keys)
            {
                if (c.EqualsOrdinalIgnoreCase(arg.SynchronizationPolicy.DefaultCulture) == false &&
                    arg.Properties[c] != null &&
                    arg.Properties[c].Any())
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsCommercePropertyLocalizable(string propertyName, LocalizeEntityPolicy localizationPolicy)
        {
            return localizationPolicy.Properties.FirstOrDefault(p => p != null && p.EqualsOrdinalIgnoreCase(propertyName)) != null;
        }
    }
}
