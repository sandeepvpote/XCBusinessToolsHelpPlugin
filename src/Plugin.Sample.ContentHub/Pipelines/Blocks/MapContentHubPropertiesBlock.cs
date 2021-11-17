// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    /// Defines a block that maps the ContentHub property values their corresponding values.
    /// </summary>
    [PipelineDisplayName(ContentHubConstants.MapContentHubPropertiesBlock)]
    public class MapContentHubPropertiesBlock : AsyncPipelineBlock<ImportEntityArgument, ImportEntityArgument, CommercePipelineExecutionContext>
    {
        /// <summary>
        /// Maps the ContentHub product properties to their corresponding values.
        /// </summary>
        /// <param name="arg">The <see cref="ImportEntityArgument"/>.</param>
        /// <param name="context">The <see cref="CommercePipelineExecutionContext"/>The <see cref="CommercePipelineExecutionContext"/>.</param>
        /// <returns>The <see cref="ImportEntityArgument"/> containing the mappings.</returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="arg"/>
        /// </exception>
        /// <remarks>
        /// Property mappings are defined in the <see cref="ProductSynchronizationPolicy"/>.
        /// When the method completes successfully, the <see cref="ImportEntityArgument.Properties"/> property will contain the mappings.
        /// </remarks>
        public override async Task<ImportEntityArgument> RunAsync(ImportEntityArgument arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg, nameof(arg)).IsNotNull();
            Condition.Requires(arg.CommerceEntity, nameof(arg.CommerceEntity)).IsNotNull();

            if (arg.ProductSynchronizationPolicy == null)
            {
                context.CommerceContext.Logger.LogWarning($"{Name}: {nameof(ProductSynchronizationPolicy)} is not configured. Exiting block.");
                return arg;
            }

            if (!arg.ProductSynchronizationPolicy.PropertyMapping.Any())
            {
                context.CommerceContext.Logger.LogWarning($"{Name}: {nameof(ProductSynchronizationPolicy)} has no mappings configured. Exiting block.");
                return arg;
            }

            var contentHubProperties = arg.ProductSynchronizationPolicy.PropertyMapping.Select(x => x.FromProperty).Distinct();
            FilterDefaultTokenCulture(arg);

            foreach (var culture in arg.EditedCultures)
            {
                var propertyValuePairs = new Dictionary<string, string>();

                if (!IsCultureSupported(culture, arg, context))
                {
                    context.CommerceContext.Logger.LogWarning($"{Name} Culture {culture} is not supported. Entity {arg.CommerceEntity.Id} " +
                        $"will not be updated for this culture.");
                    continue;
                }

                foreach (var propertyKey in contentHubProperties)
                {
                    var propertyValue = string.Empty;

                    if (propertyKey.EqualsOrdinalIgnoreCase(ContentHubConstants.ContentHubEntityVersionProperty))
                    {
                        propertyValue = arg.Entity.Version.ToString(CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        propertyValue = await arg.Entity.GetMultiLanguagePropertyValueAsync<string>(propertyKey, culture).ConfigureAwait(false);
                    }

                    propertyValuePairs.Add(propertyKey, propertyValue);
                }

                if (!arg.Properties.ContainsKey(culture))
                {
                    arg.Properties.Add(culture, propertyValuePairs);
                }
                else
                {
                    arg.Properties[culture] = propertyValuePairs;
                }
            }

            return arg;
        }

        private static bool IsCultureSupported(string culture, ImportEntityArgument arg, CommercePipelineExecutionContext context)
        {
            if (culture.EqualsOrdinalIgnoreCase(arg.SynchronizationPolicy.DefaultCulture))
            {
                return true;
            }

            var globalPolicy = context.GetPolicy<GlobalEnvironmentPolicy>();
            return globalPolicy.Languages.FirstOrDefault(l => l.EqualsOrdinalIgnoreCase(culture)) != null;
        }

        private static void FilterDefaultTokenCulture(ImportEntityArgument arg)
        {
            var cultures = arg.EditedCultures;

            if (cultures == null || cultures.Count == 0)
            {
                return;
            }

            // If cultures contains both (Default) and DefaultCulture, remove default
            // Otherwise, if cultures contains (Default) but not DefaultCulture, overwrite (Default) with DefaultCulture
            var containsDefaultToken = cultures.Contains<string>(ContentHubConstants.DefaultLanguageToken);
            var containsDefaultCulture = cultures.Contains<string>(arg.SynchronizationPolicy.DefaultCulture);

            if (containsDefaultToken)
            {
                if (containsDefaultCulture)
                {
                    cultures.Remove(ContentHubConstants.DefaultLanguageToken);
                }
                else
                {
                    var index = cultures.IndexOf(ContentHubConstants.DefaultLanguageToken);
                    if (index != -1)
                    {
                        cultures[index] = arg.SynchronizationPolicy.DefaultCulture;
                    }
                }
            }
        }
    }
}
