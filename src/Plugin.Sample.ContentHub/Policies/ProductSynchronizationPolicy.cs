// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System;
using System.Collections.Generic;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Conditions;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    /// Defines a class that represents information needed to synchronize products
    /// (PCM data) from Content Hub.
    /// </summary>
    public class ProductSynchronizationPolicy : Policy
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ProductSynchronizationPolicy" /> class.
        /// </summary>
        public ProductSynchronizationPolicy()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductSynchronizationPolicy"/> class.
        /// </summary>
        /// <param name="propertyMapping">The list of property mappings between Content Hub and Commerce Engine entity properties.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="propertyMapping"/> is <see langword="null" />
        /// </exception>
        public ProductSynchronizationPolicy(IReadOnlyList<PropertyMap> propertyMapping)
        {
            Condition.Requires(propertyMapping, nameof(propertyMapping)).IsNotNull();

            PropertyMapping = propertyMapping;
        }

        /// <summary>
        /// Gets the collection of property mappings between Content Hub and Commerce Engine entities.
        /// </summary>
        public IReadOnlyList<PropertyMap> PropertyMapping { get; set; }
    }
}
