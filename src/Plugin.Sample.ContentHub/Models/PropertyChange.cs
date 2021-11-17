// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System;
using Sitecore.Framework.Conditions;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    /// Defines a class that represents a property change on a
    /// Content Hub entity.
    /// </summary>
    public class PropertyChange
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyChange"/> class.
        /// </summary>
        /// <param name="property">The name of the changed property.</param>
        /// <param name="type">The data type of the property.</param>
        /// <param name="culture">The culture of the property.</param>
        /// <param name="originalValue">The original value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="property"/>, <paramref name="type"/>, or <paramref name="culture"/>,
        ///     is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="property"/>, <paramref name="type"/>, or <paramref name="culture"/>
        ///      is <see cref="string.Empty"/> or contains only whitespace.
        /// </exception>
        public PropertyChange(string property, string type, string culture, string originalValue, object newValue)
        {
            Condition.Requires(property, nameof(property)).IsNotNullOrWhiteSpace();
            Condition.Requires(type, nameof(type)).IsNotNullOrWhiteSpace();
            Condition.Requires(culture, nameof(culture)).IsNotNullOrWhiteSpace();

            Property = property;
            Type = type;
            Culture = culture;
            OriginalValue = originalValue;
            NewValue = newValue;
        }

        /// <summary>
        /// The culture in which the property value was changed.
        /// </summary>
        public string Culture { get; set; }

        /// <summary>
        /// The name of the property whose value was changed.
        /// </summary>
        public string Property { get; set; }

        /// <summary>
        /// The data type of the property whose value was changed.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The original value of the property before it was changed.
        /// </summary>
        public string OriginalValue { get; set; }

        /// <summary>
        /// The updated value of the property.
        /// </summary>
        public object NewValue { get; set; }
    }
}
