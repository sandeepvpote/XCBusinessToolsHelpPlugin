// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System;
using Sitecore.Framework.Conditions;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    /// Represents a ratio of <see cref="ConversionConfiguration"/>.
    /// </summary>
    public class Ratio
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Ratio"/>.
        /// </summary>
        /// <param name="name">The name of the <see cref="Ratio"/>.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="name"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="name"/> is <see cref="string.Empty"/> or contains only whitespace characters.
        /// </exception>
        public Ratio(string name)
        {
            Condition.Requires(name, nameof(name)).IsNotNullOrWhiteSpace();
            Name = name;
        }

        /// <summary>
        /// Gets the name of the <see cref="Ratio"/>.
        /// </summary>
        public string Name { get; }
    }
}
