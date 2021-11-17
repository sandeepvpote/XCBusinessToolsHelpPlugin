// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    /// Defines a block that sets or removes the <see cref="ContentHubImagesComponent"/> on a <see cref="SellableItem"/>,
    /// depending on whether there are any associated Content Hub images.
    /// </summary>
    [PipelineDisplayName(ContentHubConstants.SetImagesComponentBlock)]
    public class SetImagesComponentBlock : SyncPipelineBlock<ImportEntityArgument, ImportEntityArgument, CommercePipelineExecutionContext>
    {
        /// <summary>
        /// Sets or removes the <see cref="ContentHubImagesComponent"/> of a <see cref="SellableItem"/>.
        /// </summary>
        /// <param name="arg">
        ///     The <see cref="ImportEntityArgument"/> that contains the <see cref="SellableItem"/> to update in
        ///     the <see cref="ImportEntityArgument.CommerceEntity"/> property.
        /// </param>
        /// <param name="context">The <see cref="CommercePipelineExecutionContext"/> to execute current block.</param>
        /// <returns>The <paramref name="arg"/> with updated <see cref="SellableItem"/>.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="arg"/>, <paramref name="context"/>, or <see cref="ImportEntityArgument.CommerceEntity"/>
        ///     in <paramref name="arg"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <see cref="ImportEntityArgument.CommerceEntity"/> in <paramref name="arg"/> is not of <see cref="SellableItem"/> type.
        /// </exception>
        public override ImportEntityArgument Run(ImportEntityArgument arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg, nameof(arg)).IsNotNull();
            Condition.Requires(context, nameof(context)).IsNotNull();
            Condition.Requires(arg.CommerceEntity, nameof(arg.CommerceEntity)).IsNotNull().IsOfType(typeof(SellableItem));

            if (arg.AssetSynchronizationPolicy == null)
            {
                context.CommerceContext.Logger.LogWarning($"{Name}: {nameof(AssetSynchronizationPolicy)} is not configured. Exiting block.");
                return arg;
            }

            var sellableItem = (SellableItem) arg.CommerceEntity;
            if (arg.ContentHubImagesComponent != null && arg.ContentHubImagesComponent.ContentHubImages.Any())
            {
                sellableItem.SetComponent(arg.ContentHubImagesComponent);
            }
            else
            {
                sellableItem.RemoveComponent(typeof(ContentHubImagesComponent));
            }

            return arg;
        }
    }
}
