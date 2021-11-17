// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    /// Defines a block that persists localized properties of a <see cref="SellableItem"/>
    /// using a <see cref="LocalizationEntity"/> in response to a Content Hub update.
    /// </summary>
    [PipelineDisplayName(ContentHubConstants.PersistLocalizationsBlock)]
    public class PersistLocalizationsBlock : AsyncPipelineBlock<ImportEntityArgument, ImportEntityArgument, CommercePipelineExecutionContext>
    {
        private readonly IPersistEntityPipeline _persistEntityPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistLocalizationsBlock"/> class.
        /// </summary>
        /// <param name="persistEntityPipeline">An instance of <see cref="IPersistEntityPipeline"/>, used to persist updated localizations.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="persistEntityPipeline"/> is <see langword="null"/>.
        /// </exception>
        public PersistLocalizationsBlock(IPersistEntityPipeline persistEntityPipeline)
        {
            Condition.Requires(persistEntityPipeline, nameof(persistEntityPipeline)).IsNotNull();

            _persistEntityPipeline = persistEntityPipeline;
        }

        /// <summary>
        ///     Persists the updated localized properties.
        /// </summary>
        /// <param name="arg">The <see cref="ImportEntityArgument"/>, containing information about the update.</param>
        /// <param name="context">The <see cref="CommercePipelineExecutionContext"/></param>
        /// <returns>The <paramref name="arg"/>.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="arg"/>, <paramref name="arg.LocalizationEntity"/>, or <paramref name="context"/>
        ///     is <see langword="null"/>.
        /// </exception>
        public override async Task<ImportEntityArgument> RunAsync(ImportEntityArgument arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg, nameof(arg)).IsNotNull();
            Condition.Requires(context, nameof(context)).IsNotNull();

            if (!arg.LocalizationsUpdated)
            {
                context.CommerceContext.Logger.LogDebug($"{Name}: No localizations to persist. Exiting block.");
                return arg;
            }

            await _persistEntityPipeline.RunAsync(new PersistEntityArgument(arg.LocalizationEntity), context).ConfigureAwait(false);

            return arg;
        }
    }
}
