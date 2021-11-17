// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    ///     Defines a block that updates the properties of a <see cref="SellableItem"/> as a result of
    ///     a Content Hub update to that item.
    /// </summary>
    [PipelineDisplayName(ContentHubConstants.UpdateSellableItemPropertiesBlock)]
    public class UpdateSellableItemPropertiesBlock : SyncPipelineBlock<ImportEntityArgument, ImportEntityArgument, CommercePipelineExecutionContext>
    {
        /// <summary>
        ///     Updates <see cref="SellableItem"/> with the changes, received from Content Hub.
        /// </summary>
        /// <param name="arg">
        ///     The <see cref="ImportEntityArgument"/> that contains <see cref="SellableItem"/> to update in
        ///     <see cref="ImportEntityArgument.CommerceEntity"/>.
        /// </param>
        /// <param name="context">The <see cref="CommercePipelineExecutionContext"/> to execute current block.</param>
        /// <returns>The <paramref name="arg"/> with updated <see cref="SellableItem"/>.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="arg"/>, <paramref name="context"/>, or <see cref="ImportEntityArgument.CommerceEntity"/>
        ///     in <paramref name="arg"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <see cref="ImportEntityArgument.CommerceEntity"/> in <paramref name="arg"/> is not of <see cref="SellableItem"/> type.
        /// </exception>
        public override ImportEntityArgument Run(ImportEntityArgument arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg, nameof(arg)).IsNotNull();
            Condition.Requires(context, nameof(context)).IsNotNull();
            Condition.Requires(arg.CommerceEntity, nameof(arg.CommerceEntity)).IsNotNull().IsOfType(typeof(SellableItem));

            if (arg.ProductSynchronizationPolicy == null)
            {
                context.CommerceContext.Logger.LogWarning($"{Name}: {nameof(ProductSynchronizationPolicy)} is not configured. Exiting block.");
                return arg;
            }

            if (arg.ProductSynchronizationPolicy.PropertyMapping.Count <= 0)
            {
                context.CommerceContext.Logger.LogWarning($"{Name}: No sellable item properties will be updated because no mappings have been defined.");
                return arg;
            }

            try
            {
                UpdateSellableItemProperties(arg, context);
                context.CommerceContext.Logger.LogDebug($"{Name}: Updated SellableItem properties (id: {arg.CommerceEntity.Id}).");
            }
            catch (Exception ex)
            {
                var errorMsg = $"Error occured during update of {nameof(SellableItem)} with id {arg.CommerceEntity.Id}";
                context.CommerceContext.Logger.LogError(ex, $"{Name}: {errorMsg}");
            }

            return arg;
        }

        /// <summary>
        /// Validates an incoming property value from a Content Hub update.
        /// </summary>
        /// <param name="propertyName">The name of the property being updated.</param>
        /// <param name="propertyValue">The new value of the property.</param>
        /// <param name="arg">The <see cref="ImportEntityArgument"/> containing information about the update.</param>
        /// <returns>
        /// <c>true</c> is the value is valid; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// If this method returns <c>false</c>, the sellable item property denoted by <paramref name="propertyName"/>
        /// will not be updated. This method can be overridden by derived classes to provide custom validation for
        /// properties being updated by Content Hub.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        ///      <paramref name="propertyName"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///      <paramref name="propertyName"/> is empty or all whitespace.
        /// </exception>
        protected virtual bool ValidatePropertyValue(string propertyName, string propertyValue, ImportEntityArgument arg = null)
        {
            Condition.Requires(propertyName, nameof(propertyName)).IsNotNullOrWhiteSpace();

            // Disallow the nulling out of DisplayName - SF PDP errors out in this case
            if (propertyName.EqualsOrdinalIgnoreCase(nameof(SellableItem.DisplayName)))
            {
                return !string.IsNullOrWhiteSpace(propertyValue);
            }

            return true;
        }

        private static IDictionary<string, string> GetSellableItemProperties(IDictionary<string, IDictionary<string, string>> contentHubProperties,
            SynchronizationPolicy synchronizationPolicy, ProductSynchronizationPolicy mappingPolicy)
        {
            // Contains propertyName | propertyValue
            var commerceProperties = new Dictionary<string, string>();
            var propertiesMapping = mappingPolicy.PropertyMapping;

            // Get SellableItem properties mapping
            var sellableItemPropertiesMapping = propertiesMapping.Where(x => !x.ToProperty.Contains(".", StringComparison.OrdinalIgnoreCase));
            var defaultCultureProperties = contentHubProperties[synchronizationPolicy.DefaultCulture];

            // Populate the properties
            foreach (var propertyMap in sellableItemPropertiesMapping)
            {
                if (defaultCultureProperties.ContainsKey(propertyMap.FromProperty))
                {
                    var propertyValue = defaultCultureProperties[propertyMap.FromProperty];

                    commerceProperties.Add(propertyMap.ToProperty.GetToPropertyName(), propertyValue);
                }
            }

            return commerceProperties;
        }

        private void UpdateSellableItemProperties(ImportEntityArgument arg, CommercePipelineExecutionContext context)
        {
            var sellableItem = arg.CommerceEntity;

            // Get a list of SellableItem properties for edit
            var itemProperties = GetSellableItemProperties(arg.Properties, arg.SynchronizationPolicy, arg.ProductSynchronizationPolicy);

            if (itemProperties.Any())
            {
                var type = sellableItem.GetType();

                foreach (var mapping in itemProperties)
                {
                    PropertyInfo property = type.GetProperty(mapping.Key);
                    if (property != null)
                    {
                        if (ValidatePropertyValue(mapping.Key, mapping.Value, arg))
                        {
                            property.SetValue(sellableItem, mapping.Value, null);
                        }
                        else
                        {
                            context.Logger.LogWarning($"{Name}: Content Hub update of {sellableItem.Id} - property {mapping.Key} " +
                                $"will not be updated as value '{mapping.Value}' is invalid");
                        }
                    }
                }
            }
        }
    }
}
