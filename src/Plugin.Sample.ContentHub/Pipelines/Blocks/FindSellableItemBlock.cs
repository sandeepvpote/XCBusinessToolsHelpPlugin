// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using Stylelabs.M.Sdk.Contracts.Base;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    ///     Represents a block which ensures that for received Content Hub <see cref="IEntity"/>
    ///     there is appropriate <see cref="CommerceEntity"/>.
    /// </summary>
    [PipelineDisplayName(ContentHubConstants.FindSellableItemBlock)]
    public class FindSellableItemBlock : AsyncPipelineBlock<ImportEntityArgument, ImportEntityArgument, CommercePipelineExecutionContext>
    {
        private readonly IFindEntityPipeline _findEntityPipeline;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FindSellableItemBlock" />.
        /// </summary>
        /// <param name="findEntityPipeline">The <see cref="IFindEntityPipeline"/> to get <see cref="SellableItem"/>.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="findEntityPipeline"/> is <see langword="null"/>.
        /// </exception>
        public FindSellableItemBlock(IFindEntityPipeline findEntityPipeline)
        {
            Condition.Requires(findEntityPipeline, nameof(findEntityPipeline)).IsNotNull();

            _findEntityPipeline = findEntityPipeline;
        }

        /// <summary>
        /// Ensures that for <see cref="ImportEntityArgument.Entity"/> in <paramref name="arg"/> there is appropriate
        /// <see cref="CommerceEntity"/> with the same value of <see cref="SynchronizationPolicy.EntityForeignKey"/>
        /// property.
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

            context.CommerceContext.Logger.LogDebug($"{Name}: Started.");

            var foreignKey = (await arg.Entity.GetPropertyValueAsync(arg.SynchronizationPolicy.EntityForeignKey).ConfigureAwait(false)) as string;
            if (string.IsNullOrEmpty(foreignKey))
            {
                var message = $"{ContentHubConstants.ContentHubProductIdMissingOrInvalid} {Name}: The Content Hub Entity (id: {arg.TargetId}) " +
                    $"does not contain a valid value for mapped property ID value ({arg.SynchronizationPolicy.EntityForeignKey}).";
                context.Abort(message, context, LogLevel.Error);

                return arg;
            }

            var findEntityArgument = new FindEntityArgument(typeof(SellableItem), foreignKey.ToEntityId<SellableItem>());
            var sellableItem = (await _findEntityPipeline.RunAsync(findEntityArgument, context).ConfigureAwait(false)) as SellableItem;

            if (sellableItem == null)
            {
                var message = $"{ContentHubConstants.ContentHubSellableItemMissing} {Name}: The Content Hub Entity (id: {arg.TargetId}) " +
                    $"references a Sellable Item referenced by ({arg.SynchronizationPolicy.EntityForeignKey}: {foreignKey}). Either the " +
                    $"property ID key is incorrect or the Commerce Catalog is out of date.";
                context.Abort(message, context, LogLevel.Error);

                return arg;
            }

            arg.CommerceEntity = sellableItem;
            arg.LocalizationEntity = findEntityArgument.LocalizationEntity;

            return arg;
        }
    }
}
