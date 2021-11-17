// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System.Globalization;
using System.Threading.Tasks;
using Stylelabs.M.Sdk.Contracts.Base;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    /// Defines extension methods for <see cref="IEntity"/>.
    /// </summary>
    public static class IEntityExtensions
    {
        /// <summary>
        /// Gets the value of the property identified by <paramref name="property"/>
        /// for the culture specified by <paramref name="cultureInfo"/>.
        /// </summary>
        /// <typeparam name="T">The type of the property to return</typeparam>
        /// <param name="entity">The instance of the <see cref="IEntity"/> to get the property from.</param>
        /// <param name="property">The name of the property.</param>
        /// <param name="cultureInfo">The culture for which the property should be retrieved.</param>
        /// <returns>The value of the property, or default(T) if the property does not exist.</returns>
        public static async Task<T> GetMultiLanguagePropertyValueAsync<T>(this IEntity entity, string property, CultureInfo cultureInfo)
        {
            var contentHubProperty = await entity.GetPropertyAsync(property).ConfigureAwait(false);
            var propertyValue = default(T);

            if (contentHubProperty != null)
            {
                if (contentHubProperty.IsMultiLanguage)
                {
                    propertyValue = await entity.GetPropertyValueAsync<T>(property, cultureInfo).ConfigureAwait(false);
                }
                else
                {
                    propertyValue = await entity.GetPropertyValueAsync<T>(property).ConfigureAwait(false);
                }
            }

            return propertyValue;
        }

        /// <summary>
        /// Gets the value of the property identified by <paramref name="property"/>
        /// for the culture specified by <paramref name="culture"/>.
        /// </summary>
        /// <typeparam name="T">The type of the property to return</typeparam>
        /// <param name="entity">The instance of the <see cref="IEntity"/> to get the property from.</param>
        /// <param name="property">The name of the property.</param>
        /// <param name="culture">The string representation of the culture for which the property should be retrieved.</param>
        /// <returns>The value of the property, or default(T) if the property does not exist.</returns>
        public static async Task<T> GetMultiLanguagePropertyValueAsync<T>(this IEntity entity, string property, string culture)
        {
            return await GetMultiLanguagePropertyValueAsync<T>(entity, property, new CultureInfo(culture)).ConfigureAwait(false);
        }
    }
}
