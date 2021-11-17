// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    ///     Defines a block that populates a <see cref="EntityView"/> for <see cref="SellableItem"/> with images information from Content Hub.
    /// </summary>
    [PipelineDisplayName(ContentHubConstants.GetSellableItemContentHubImagesViewBlock)]
    public class GetSellableItemContentHubImagesViewBlock : SyncPipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        /// <summary>
        ///     Updates <paramref name="arg"/> with information received from Content Hub.
        /// </summary>
        /// <param name="arg">
        ///     The <see cref="EntityView"/> to populate if <see cref="SellableItem"/> has <see cref="ContentHubImagesComponent"/>.
        /// </param>
        /// <param name="context">The <see cref="CommercePipelineExecutionContext"/> to execute current block.</param>
        /// <returns>The <paramref name="arg"/> with updated <see cref="EntityView"/>.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="arg"/>, <paramref name="context"/> is <see langword="null"/>.
        /// </exception>
        public override EntityView Run(EntityView arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg, nameof(arg)).IsNotNull();
            Condition.Requires(context, nameof(context)).IsNotNull();

            if (!context.HasPolicy<BizFxPolicy>())
            {
                return arg;
            }

            var viewsPolicy = context.GetPolicy<KnownCatalogViewsPolicy>();
            var request = context.CommerceContext.GetObjects<EntityViewArgument>().FirstOrDefault();

            if (!(request?.Entity is SellableItem) ||
                !request.ViewName.EqualsOrdinalIgnoreCase(viewsPolicy.Master) &&
                !request.ViewName.EqualsOrdinalIgnoreCase(viewsPolicy.Variant))
            {
                return arg;
            }

            var sellableItem = request.Entity as SellableItem;
            if (!sellableItem.HasComponent<ContentHubImagesComponent>())
            {
                return arg;
            }

            var contentHubImagesComponent = sellableItem.GetComponent<ContentHubImagesComponent>(arg.ItemId);
            if (!contentHubImagesComponent.ContentHubImages.Any())
            {
                return arg;
            }

            var contentHubImagesEntityView = new EntityView
            {
                DisplayName = ContentHubConstants.ImagesEntityViewDisplayName,
                Name = ContentHubConstants.ImagesEntityViewName,
                UiHint = CoreConstants.TableUiHint,
                EntityId = arg.EntityId,
                EntityVersion = arg.EntityVersion,
                ItemId = arg.ItemId
            };
            arg.ChildViews.Add(contentHubImagesEntityView);

            foreach (var image in contentHubImagesComponent.ContentHubImages)
            {
                var imageView = GetImageView(arg, image, context.GetPolicy<GlobalImagePolicy>());
                contentHubImagesEntityView.ChildViews.Add(imageView);
            }

            return arg;
        }

        private static EntityView GetImageView(EntityView parentView, ContentHubImage image, GlobalImagePolicy globalImagePolicy)
        {
            return new EntityView
            {
                Name = ContentHubConstants.ImagesEntityViewImageViewPropertyName,
                ItemId = string.Concat(!string.IsNullOrEmpty(parentView.ItemId) ? parentView.ItemId : string.Empty, "|", image),
                Properties = new List<ViewProperty>
                {
                    new ViewProperty
                    {
                        Name = ContentHubConstants.ImagesEntityViewImageViewPropertyName,
                        IsHidden = false,
                        IsRequired = false,
                        IsReadOnly = true,
                        RawValue = string.Format(CultureInfo.InvariantCulture, ContentHubConstants.ImageViewPropertyRawValueStringFormat, image.AlternateText, globalImagePolicy.DefaultImage.Height, globalImagePolicy.DefaultImage.Width, image.Url),
                        UiType = ContentHubConstants.EntityViewPropertyUITypeHtml,
                        OriginalType = ContentHubConstants.EntityViewPropertyUITypeHtml
                    },
                    new ViewProperty
                    {
                        Name = ContentHubConstants.ImagesEntityViewPublicLinkViewPropertyName,
                        IsHidden = false,
                        IsRequired = false,
                        IsReadOnly = true,
                        RawValue = string.Format(CultureInfo.InvariantCulture, ContentHubConstants.PublicLinkViewPropertyRawValueStringFormat, image.Url),
                        UiType = ContentHubConstants.EntityViewPropertyUITypeHtml
                    },
                    new ViewProperty
                    {
                        Name = ContentHubConstants.ImagesEntityViewWidthViewPropertyName,
                        IsHidden = false,
                        IsRequired = false,
                        IsReadOnly = true,
                        RawValue = image.Width.ToString(CultureInfo.InvariantCulture),
                        OriginalType = image.Width.GetType().FullName
                    },
                    new ViewProperty
                    {
                        Name = ContentHubConstants.ImagesEntityViewHeightViewPropertyName,
                        IsHidden = false,
                        IsRequired = false,
                        IsReadOnly = true,
                        RawValue = image.Height.ToString(CultureInfo.InvariantCulture),
                        OriginalType = image.Height.GetType().FullName
                    },
                    new ViewProperty
                    {
                        Name = ContentHubConstants.ImagesEntityViewIsMasterFileViewPropertyName,
                        IsHidden = false,
                        IsRequired = false,
                        IsReadOnly = true,
                        RawValue = image.IsMaster.ToString(CultureInfo.InvariantCulture),
                        OriginalType = image.IsMaster.GetType().FullName
                    }
                }
            };
        }
    }
}
