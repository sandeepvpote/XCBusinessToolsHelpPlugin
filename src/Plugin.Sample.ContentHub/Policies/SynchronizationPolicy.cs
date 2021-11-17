// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Conditions;
using Stylelabs.M.Sdk.Contracts.Base;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    /// Defines a policy that contains basic common information needed to synchronize
    /// any kind of content from Content Hub. This policy must be configured in order
    /// to synchronize PCM or DAM data.
    /// </summary>
    public class SynchronizationPolicy : Policy
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SynchronizationPolicy" /> class.
        /// </summary>
        public SynchronizationPolicy()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SynchronizationPolicy"/> class.
        /// </summary>
        /// <param name="productEntityType">A string representation of the type of the product entity</param>
        /// <param name="assetEntityType">A string representation of the type of the asset entity</param>
        /// <param name="publicLinkEntityType">A string representation of the type of the public link entity</param>
        /// <param name="entityForeignKey">The name of the property that represents a foreign key between Content Hub and Commerce Engine.</param>
        /// <param name="defaultCulture">The Content Hub culture that should be mapped to the default culture in Commerce Engine.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="productEntityType"/>, <paramref name="assetEntityType"/>, <paramref name="publicLinkEntityType"/>, <paramref name="entityForeignKey"/>,
        ///     or <paramref name="defaultCulture"/> is <see langword="null" />
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="productEntityType"/>, <paramref name="assetEntityType"/>, <paramref name="publicLinkEntityType"/>, <paramref name="entityForeignKey"/>,
        ///     or <paramref name="defaultCulture"/> is an empty string or contains only whitespace.
        /// </exception>
        public SynchronizationPolicy(string productEntityType, string assetEntityType, string publicLinkEntityType, string entityForeignKey, string defaultCulture)
        {
            Condition.Requires(productEntityType, nameof(productEntityType)).IsNotNullOrWhiteSpace();
            Condition.Requires(assetEntityType, nameof(assetEntityType)).IsNotNullOrWhiteSpace();
            Condition.Requires(publicLinkEntityType, nameof(publicLinkEntityType)).IsNotNullOrWhiteSpace();
            Condition.Requires(entityForeignKey, nameof(entityForeignKey)).IsNotNullOrWhiteSpace();
            Condition.Requires(defaultCulture, nameof(defaultCulture)).IsNotNullOrWhiteSpace();

            ProductEntityType = productEntityType;
            AssetEntityType = assetEntityType;
            PublicLinkEntityType = publicLinkEntityType;
            EntityForeignKey = entityForeignKey;
            DefaultCulture = defaultCulture;
        }

        /// <summary>
        /// Gets the Content Hub culture that corresponds to the default culture in Commerce Engine.
        /// </summary>
        public string DefaultCulture { get; set; }

        /// <summary>
        /// Gets the name of the property that represents a foreign key between Content Hub and Commerce Engine.
        /// </summary>
        public string EntityForeignKey { get; set; }

        /// <summary>
        /// Gets a string representation of the product <see cref="IEntity"/> type in Content Hub.
        /// </summary>
        public string ProductEntityType { get; set; }

        /// <summary>
        /// Gets a string representation of the asset <see cref="IEntity"/> type in Content Hub.
        /// </summary>
        public string AssetEntityType { get; set; }

        /// <summary>
        /// Gets a string representation of the public link <see cref="IEntity"/> type in Content Hub.
        /// </summary>
        public string PublicLinkEntityType { get; set; }
    }
}
