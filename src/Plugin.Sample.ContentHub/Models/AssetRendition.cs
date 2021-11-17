// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System;
using System.Collections.Generic;
using Sitecore.Framework.Conditions;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    /// Represents a rendition for asset.
    /// </summary>
    public class AssetRendition
    {
        /// <summary>
        /// Initializes a new instance of <see cref="AssetRendition"/>.
        /// </summary>
        /// <param name="status">The status of the rendition.</param>
        /// <param name="properties">
        ///     The <see cref="IReadOnlyDictionary{TKey,TValue}"/> where key and value are the name and value of
        ///     the property, respectively.
        /// </param>
        /// <param name="locations">
        ///     The <see cref="IReadOnlyDictionary{TKey, TValue}"/> where key is the name of the location and value
        ///     is the <see cref="Array"/> of location values.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="status"/> or <paramref name="locations"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="status"/> is <see cref="string.Empty"/> or contains only whitespace characters.
        /// </exception>
        public AssetRendition(string status, IReadOnlyDictionary<string, string> properties, IReadOnlyDictionary<string, string[]> locations)
        {
            Condition.Requires(status, nameof(status)).IsNotNullOrWhiteSpace();
            Condition.Requires(locations, nameof(locations)).IsNotNull();

            Status = status;
            Properties = properties;
            Locations = locations;
        }

        /// <summary>
        /// Gets the status of the rendition.
        /// </summary>
        public string Status { get; }

        /// <summary>
        ///     Gets the <see cref="IReadOnlyDictionary{TKey, TValue}"/> where key and value are the name and value of
        ///     the property, respectively.
        /// </summary>
        public IReadOnlyDictionary<string, string> Properties { get; }

        /// <summary>
        ///     Gets the <see cref="IReadOnlyDictionary{TKey, TValue}"/> where key is the name of the location and value
        ///     is the <see cref="Array"/> of location values.
        /// </summary>
        public IReadOnlyDictionary<string, string[]> Locations { get; }
    }
}
