// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Pipelines;
using Stylelabs.M.Sdk.Contracts.Base;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    /// The constants for <see cref="ContentHub"/> plugin.
    /// </summary>
    public static class ContentHubConstants
    {
        /// <summary>
        /// The <see cref="ContentHub.InitializeServiceBusBlock"/> block name.
        /// </summary>
        public const string InitializeServiceBusBlock = BlockPrefix + nameof(ContentHub.InitializeServiceBusBlock);

        /// <summary>
        /// The <see cref="ContentHub.FetchEntityBlock"/> block name.
        /// </summary>
        public const string FetchEntityBlock = BlockPrefix + nameof(ContentHub.FetchEntityBlock);

        /// <summary>
        /// The <see cref="ContentHub.FetchAssetEntitiesBlock"/> block name.
        /// </summary>
        public const string FetchAssetEntitiesBlock = BlockPrefix + nameof(ContentHub.FetchAssetEntitiesBlock);

        /// <summary>
        /// The <see cref="ContentHub.FetchImagePublicLinksBlock"/> block name.
        /// </summary>
        public const string FetchImagePublicLinksBlock = BlockPrefix + nameof(ContentHub.FetchImagePublicLinksBlock);

        /// <summary>
        /// The <see cref="ContentHub.FindSellableItemBlock"/> block name.
        /// </summary>
        public const string FindSellableItemBlock = BlockPrefix + nameof(ContentHub.FindSellableItemBlock);

        /// <summary>
        /// The <see cref="ContentHub.FetchAssetProductEntitiesBlock"/> block name.
        /// </summary>
        public const string FetchAssetProductEntitiesBlock = BlockPrefix + nameof(ContentHub.FetchAssetProductEntitiesBlock);

        /// <summary>
        /// The <see cref="ContentHub.FetchPublicLinkAssetEntitiesBlock"/> block name.
        /// </summary>
        public const string FetchPublicLinkAssetEntitiesBlock = BlockPrefix + nameof(ContentHub.FetchPublicLinkAssetEntitiesBlock);

        /// <summary>
        /// The <see cref="ContentHub.UpdateSellableItemPropertiesBlock"/> block name.
        /// </summary>
        public const string UpdateSellableItemPropertiesBlock = BlockPrefix + nameof(ContentHub.UpdateSellableItemPropertiesBlock);

        /// <summary>
        /// The <see cref="ContentHub.UpdateLocalizedPropertiesBlock"/> block name.
        /// </summary>
        public const string UpdateLocalizedPropertiesBlock = BlockPrefix + nameof(ContentHub.UpdateLocalizedPropertiesBlock);

        /// <summary>
        /// The <see cref="ContentHub.UpdateLocalizedComponentsBlock"/> block name.
        /// </summary>
        public const string UpdateLocalizedComponentsBlock = BlockPrefix + nameof(ContentHub.UpdateLocalizedComponentsBlock);

        /// <summary>
        /// The <see cref="ContentHub.MapContentHubPropertiesBlock"/> block name.
        /// </summary>
        public const string MapContentHubPropertiesBlock = BlockPrefix + nameof(ContentHub.MapContentHubPropertiesBlock);

        /// <summary>
        /// The <see cref="ContentHub.PersistSellableItemBlock"/> block name.
        /// </summary>
        public const string PersistSellableItemBlock = BlockPrefix + nameof(ContentHub.PersistSellableItemBlock);

        /// <summary>
        /// The <see cref="ContentHub.PersistLocalizationsBlock"/> block name.
        /// </summary>
        public const string PersistLocalizationsBlock = BlockPrefix + nameof(ContentHub.PersistLocalizationsBlock);

        /// <summary>
        /// The <see cref="ContentHub.BuildImagesComponentBlock"/> block name.
        /// </summary>
        public const string BuildImagesComponentBlock = BlockPrefix + nameof(ContentHub.BuildImagesComponentBlock);

        /// <summary>
        /// The <see cref="ContentHub.ConfigureServiceApiBlock"/> block name.
        /// </summary>
        public const string ConfigureServiceApiBlock = BlockPrefix + nameof(ContentHub.ConfigureServiceApiBlock);

        /// <summary>
        /// The <see cref="ContentHub.GetSellableItemContentHubImagesViewBlock"/> block name.
        /// </summary>
        public const string GetSellableItemContentHubImagesViewBlock = BlockPrefix + nameof(ContentHub.GetSellableItemContentHubImagesViewBlock);

        /// <summary>
        /// The <see cref="ContentHub.SetImagesComponentBlock"/> block name.
        /// </summary>
        public const string SetImagesComponentBlock = BlockPrefix + nameof(ContentHub.SetImagesComponentBlock);

        /// <summary>
        /// The <see cref="ContentHub.ImportEntityPipeline"/> block name.
        /// </summary>
        public const string ImportEntityPipeline = PipelinePrefix + nameof(ContentHub.ImportEntityPipeline);

        /// <summary>
        /// The <see cref="ContentHub.ImportProductEntityPipeline"/> block name.
        /// </summary>
        public const string ImportProductEntityPipeline = PipelinePrefix + nameof(ContentHub.ImportProductEntityPipeline);

        /// <summary>
        /// The <see cref="ContentHub.ImportAssetEntityPipeline"/> block name.
        /// </summary>
        public const string ImportAssetEntityPipeline = PipelinePrefix + nameof(ContentHub.ImportAssetEntityPipeline);

        /// <summary>
        /// The <see cref="ContentHub.IImportPublicLinkEntityPipeline"/> block name.
        /// </summary>
        public const string IImportPublicLinkEntityPipeline = PipelinePrefix + nameof(ContentHub.IImportPublicLinkEntityPipeline);

        /// <summary>
        /// Public Link Relative URL
        /// </summary>
        public const string PublicLinkRelativeUrl = "RelativeUrl";

        /// <summary>
        /// Public Link Version Hash
        /// </summary>
        public const string PublicLinkVersionHash = "VersionHash";

        /// <summary>
        /// Name of the status for completed Public link
        /// </summary>
        public const string PublicLinkCompletedStatusName = "Completed";

        /// <summary>
        /// Content Hub Version Property Name
        /// </summary>
        public const string ContentHubEntityVersionProperty = "Version";

        /// <summary>
        /// Name of the Status property of a Content Hub public link <see cref="IEntity"/>.
        /// </summary>
        public const string PublicLinkStatusPropertyName = "Status";

        /// <summary>
        /// Name of the FileKey property of a Content Hub public link <see cref="IEntity"/>.
        /// </summary>
        public const string PublicLinkFileKeyPropertyName = "FileKey";

        /// <summary>
        /// Name of the ConversionConfiguration property of a Content Hub public link <see cref="IEntity"/>.
        /// </summary>
        public const string PublicLinkConversionConfigurationPropertyName = "ConversionConfiguration";

        /// <summary>
        /// Name of the FileProperties property of a Content Hub asset <see cref="IEntity"/>.
        /// </summary>
        public const string AssetFilePropertiesPropertyName = "FileProperties";

        /// <summary>
        /// Name of the Renditions property of a Content Hub asset <see cref="IEntity"/>.
        /// </summary>
        public const string AssetRenditionsPropertyName = "Renditions";

        /// <summary>
        /// Name of the DownloadOriginal Content Hub <see cref="IRendition"/>.
        /// </summary>
        public const string DownloadOriginalRenditionName = "downloadOriginal";

        /// <summary>
        /// Name of the Width property of a Content Hub <see cref="IRendition"/>.
        /// </summary>
        public const string RenditionWidthPropertyName = "width";

        /// <summary>
        /// Name of the Height property of a Content Hub <see cref="IRendition"/>.
        /// </summary>
        public const string RenditionHeightPropertyName = "height";

        /// <summary>
        /// Name of the Properties field in Json for <see cref="FileProperties"/>.
        /// </summary>
        public const string FilePropertiesFieldName = "properties";

        /// <summary>
        /// Name of the images group provided in <see cref="FileProperties.Group"/>.
        /// </summary>
        public const string ImagesGroupName = "images";

        /// <summary>
        /// The default display name for the Sitecore DAM Assets <see cref="EntityView"/>.
        /// </summary>
        public const string ImagesEntityViewDisplayName = "Sitecore DAM Assets";

        /// <summary>
        /// The view name for the Sitecore DAM Assets <see cref="EntityView"/>.
        /// </summary>
        public const string ImagesEntityViewName = "SitecoreDAMAssets";

        /// <summary>
        /// The <see cref="ViewProperty"/> name for the image's public link.
        /// </summary>
        public const string ImagesEntityViewPublicLinkViewPropertyName = "Public link";

        /// <summary>
        /// The <see cref="ViewProperty"/> name for the image's alternate text.
        /// </summary>
        public const string ImagesEntityViewAlternateTextViewPropertyName = "Alternate Text";

        /// <summary>
        /// The <see cref="ViewProperty"/> name for the image.
        /// </summary>
        public const string ImagesEntityViewImageViewPropertyName = "Image";

        /// <summary>
        /// The <see cref="ViewProperty"/> name for the image width.
        /// </summary>
        public const string ImagesEntityViewWidthViewPropertyName = "Width";

        /// <summary>
        /// The <see cref="ViewProperty"/> name for the image height.
        /// </summary>
        public const string ImagesEntityViewHeightViewPropertyName = "Height";

        /// <summary>
        /// The <see cref="ViewProperty"/> name for the IsMasterFile property.
        /// </summary>
        public const string ImagesEntityViewIsMasterFileViewPropertyName = "IsMasterFile";

        /// <summary>
        /// The raw value string format of the image to be rendered.
        /// </summary>
        public const string ImageViewPropertyRawValueStringFormat = "<img alt='{0}' height={1} width={2} src='{3}'/>";

        /// <summary>
        /// The <see cref="ViewProperty"/> name for the image's public link that is to be rendered.
        /// </summary>
        public const string PublicLinkViewPropertyRawValueStringFormat = "<a href='{0}' target='_blank'>{0}<a/>";

        /// <summary>
        /// Defines the HTML <see cref="ViewProperty"/> UI type.
        /// </summary>
        public const string EntityViewPropertyUITypeHtml = "Html";

        /// <summary>
        /// Defines the default language token content hub uses to represent as the culture
        /// for non-localizable properties.
        /// </summary>
        public const string DefaultLanguageToken = "(Default)";

        /// <summary>
        /// The value to log messages when a entity not found error occur.
        /// </summary>
        public const string ContentHubEntityNotFoundError = "ContentHubEntityNotFoundError";

        /// <summary>
        /// The value to log messages when a mismatch error occur.
        /// </summary>
        public const string ContentHubEntityMappingMismatch = "ContentHubEntityMappingMismatch";

        /// <summary>
        /// The value to log messages when missing or invalid id error occur.
        /// </summary>
        public const string ContentHubProductIdMissingOrInvalid = "ContentHubProductIdMissingOrInvalid";

        /// <summary>
        /// The value to log messages when missing <see cref="SellableItem"/> error occur.
        /// </summary>
        public const string ContentHubSellableItemMissing = "ContentHubSellableItemMissing";

        /// <summary>
        /// Defines the <see cref="PipelineBlock"/> prefix for Content Hub blocks.
        /// </summary>
        private const string BlockPrefix = "ContentHub.block.";

        /// <summary>
        /// Defines the <see cref="IPipeline"/> prefix for Content Hub pipelines.
        /// </summary>
        private const string PipelinePrefix = "ContentHub.pipeline.";
    }
}
