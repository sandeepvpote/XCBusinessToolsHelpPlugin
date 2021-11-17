// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Conditions;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    /// Defines a class that represents information needed to synchronize digital assets
    /// (DAM data) from Content Hub.
    /// </summary>
    public class AssetSynchronizationPolicy : Policy
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AssetSynchronizationPolicy" /> class.
        /// </summary>
        public AssetSynchronizationPolicy()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetSynchronizationPolicy"/> class.
        /// </summary>
        /// <param name="entityToMasterAssetRelationName">The name of the relation between entity and master asset.</param>
        /// <param name="entityToAssetRelationName">The name of the relation between entity and asset.</param>
        /// <param name="assetToPublicLinkRelationName">The name of the relation between asset and public link.</param>
        /// <param name="assetsAlternateTextFieldName">The name of the property that represents the alternative text asset field on Content Hub.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="entityToMasterAssetRelationName"/>, <paramref name="entityToAssetRelationName"/>,
        ///     <paramref name="assetToPublicLinkRelationName"/>, or <paramref name="assetsAlternateTextFieldName"/> is <see langword="null" />
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="entityToMasterAssetRelationName"/>, <paramref name="entityToAssetRelationName"/>,
        ///     <paramref name="assetToPublicLinkRelationName"/>, or <paramref name="assetsAlternateTextFieldName"/> is an empty string or contains only whitespace.
        /// </exception>
        public AssetSynchronizationPolicy(string entityToMasterAssetRelationName, string entityToAssetRelationName, string assetToPublicLinkRelationName, string assetsAlternateTextFieldName)
        {
            Condition.Requires(entityToMasterAssetRelationName, nameof(entityToMasterAssetRelationName)).IsNotNullOrWhiteSpace();
            Condition.Requires(entityToAssetRelationName, nameof(entityToAssetRelationName)).IsNotNullOrWhiteSpace();
            Condition.Requires(assetToPublicLinkRelationName, nameof(assetToPublicLinkRelationName)).IsNotNullOrWhiteSpace();
            Condition.Requires(assetsAlternateTextFieldName, nameof(assetsAlternateTextFieldName)).IsNotNullOrWhiteSpace();

            EntityToMasterAssetRelationName = entityToMasterAssetRelationName;
            EntityToAssetRelationName = entityToAssetRelationName;
            AssetToPublicLinkRelationName = assetToPublicLinkRelationName;
            AssetsAlternateTextFieldName = assetsAlternateTextFieldName;
        }

        /// <summary>
        /// Gets the name of the relation between entity and master asset.
        /// </summary>
        public string EntityToMasterAssetRelationName { get; set; }

        /// <summary>
        /// Gets the name of the relation between entity and asset.
        /// </summary>
        public string EntityToAssetRelationName { get; set; }

        /// <summary>
        /// Gets the name of the relation between asset and public link.
        /// </summary>
        public string AssetToPublicLinkRelationName { get; set; }

        /// <summary>
        /// Gets the name of the property that represents the alternate text foreign key between Content Hub and Commerce Engine.
        /// </summary>
        public string AssetsAlternateTextFieldName { get; set; }
    }
}
