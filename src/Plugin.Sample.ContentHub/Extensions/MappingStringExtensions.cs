// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System.Linq;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    /// Defines extension methods for <see cref="PropertyMap"/> properties.
    /// </summary>
    public static class MappingStringExtensions
    {
        /// <summary>
        /// Extract the view name of the property.
        /// </summary>
        /// <param name="property">The string property that contains the view name.</param>
        /// <returns>The view name</returns>
        public static string GetToPropertyViewName(this string property)
        {
            if (string.IsNullOrEmpty(property))
            {
                return property;
            }

            return property.Split('.').First();
        }

        /// <summary>
        /// Extract the name of the property.
        /// </summary>
        /// <param name="property">The string property that contains the name.</param>
        /// <returns>The name</returns>
        public static string GetToPropertyName(this string property)
        {
            if (string.IsNullOrEmpty(property))
            {
                return property;
            }

            return property.Split('.').Last();
        }
    }
}
