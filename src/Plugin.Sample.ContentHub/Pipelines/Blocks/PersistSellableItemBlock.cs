// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    /// Defines a block that persists a <see cref="SellableItem"/> that has been updated in response to
    /// a Content Hub update.
    /// </summary>
    [PipelineDisplayName(ContentHubConstants.PersistSellableItemBlock)]
    public class PersistSellableItemBlock : AsyncPipelineBlock<ImportEntityArgument, ImportEntityArgument, CommercePipelineExecutionContext>
    {
        private readonly IPersistEntityPipeline _persistPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistSellableItemBlock"/> class.
        /// </summary>
        /// <param name="persistPipeline">An instance of <see cref="IPersistEntityPipeline"/> used to persist updates to a <see cref="SellableItem"/>.</param>
        public PersistSellableItemBlock(IPersistEntityPipeline persistPipeline)
        {
            Condition.Requires(persistPipeline, nameof(persistPipeline)).IsNotNull();

            _persistPipeline = persistPipeline;
        }

        /// <summary>
        ///     Persists the updated <see cref="SellableItem"/>.
        /// </summary>
        /// <param name="arg">The <see cref="ImportEntityArgument"/> that contains information regarding the update from Content Hub.</param>
        /// <param name="context">The <see cref="CommercePipelineExecutionContext"/>.</param>
        /// <returns>The <paramref name="arg"/> argument.</returns>
        public override async Task<ImportEntityArgument> RunAsync(ImportEntityArgument arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg, nameof(arg)).IsNotNull();
            Condition.Requires(context, nameof(context)).IsNotNull();

            context.Logger.LogDebug($"{Name}: Persisting sellable item with Id: {arg.CommerceEntity.Id}");

            await _persistPipeline.RunAsync(new PersistEntityArgument(arg.CommerceEntity), context).ConfigureAwait(false);

            return arg;
        }
    }
}
