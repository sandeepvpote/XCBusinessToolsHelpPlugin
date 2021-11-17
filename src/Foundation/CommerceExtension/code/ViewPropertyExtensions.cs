using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;

namespace Sitecore.Commerce.Plugin.CommerceExtension
{
    public static class ViewPropertyExtensions
    {

        public static List<ViewProperty> AddProperty(this List<ViewProperty> properties, string name, object rawValue, bool isRequired)
        {
            ViewProperty viewProperty = new ViewProperty();

            viewProperty.Name = name;
            viewProperty.RawValue = rawValue;
            viewProperty.IsRequired = isRequired;

            properties.Add(viewProperty);

            return properties;
        }

        public static List<ViewProperty> AddProperty(this List<ViewProperty> properties, string name, object rawValue, bool isRequired, bool isHidden)
        {
            ViewProperty viewProperty = new ViewProperty();

            viewProperty.Name = name;
            viewProperty.RawValue = rawValue;
            viewProperty.IsRequired = isRequired;
            viewProperty.IsHidden = isHidden;

            properties.Add(viewProperty);

            return properties;
        }

        public static List<ViewProperty> AddProperty(this List<ViewProperty> properties, string name, object rawValue, bool isRequired, bool isHidden, bool isReadOnly)
        {
            ViewProperty viewProperty = new ViewProperty();

            viewProperty.Name = name;
            viewProperty.RawValue = rawValue;
            viewProperty.IsRequired = isRequired;
            viewProperty.IsHidden = isHidden;
            viewProperty.IsReadOnly = isReadOnly;

            properties.Add(viewProperty);

            return properties;
        }

        public static List<ViewProperty> AddViewProperty(this List<ViewProperty> properties, string name, object rawValue, bool isReadOnly, bool isHidden, string uiType = null, string originalType =null)
        {
            ViewProperty viewProperty = new ViewProperty();

            viewProperty.Name = name;

            viewProperty.IsReadOnly = isReadOnly;
            viewProperty.IsHidden = isHidden;
            viewProperty.RawValue = rawValue;
            if (!string.IsNullOrEmpty(uiType))
            {
                viewProperty.UiType = uiType;
            }

            if (!string.IsNullOrEmpty(originalType))
            {
                viewProperty.OriginalType = originalType;
            }

            properties.Add(viewProperty);

            return properties;
        }

        public static List<ViewProperty> AddViewProperty(this List<ViewProperty> properties, string name, object rawValue, bool isReadOnly, bool isHidden, bool isRequired, string uiType = null, string originalType = null)
        {
            ViewProperty viewProperty = new ViewProperty();

            viewProperty.Name = name;

            viewProperty.IsReadOnly = isReadOnly;
            viewProperty.IsHidden = isHidden;
            viewProperty.RawValue = rawValue;
            viewProperty.IsRequired = isRequired;
            if (!string.IsNullOrEmpty(uiType))
            {
                viewProperty.UiType = uiType;
            }

            if (!string.IsNullOrEmpty(originalType))
            {
                viewProperty.OriginalType = originalType;
            }

            properties.Add(viewProperty);

            return properties;
        }

        public static List<ViewProperty> AddDropdownProperty(this List<ViewProperty> properties, string name, List<KeyValuePair<string, string>> dropdownList, string rawValue, bool isReadOnly, bool isHidden, bool isRequired)
        {
            ViewProperty viewProperty = new ViewProperty();

            viewProperty.Name = name;
            viewProperty.UiType = "Dropdown";

            var availableSelectionsPolicy = new AvailableSelectionsPolicy();

            foreach (var item in dropdownList)
            {
                availableSelectionsPolicy.List.Add(new Selection() { DisplayName = item.Value, Name = item.Key, IsDefault = item.Key.Equals(rawValue, StringComparison.OrdinalIgnoreCase) });
            }

            viewProperty.Policies = new List<Policy>() { availableSelectionsPolicy };
            viewProperty.RawValue =
                viewProperty.GetPolicy<AvailableSelectionsPolicy>().List.FirstOrDefault(x => x.IsDefault)?.Name ?? string.Empty;

            viewProperty.IsReadOnly = isReadOnly;
            viewProperty.IsHidden = isHidden;
            viewProperty.IsRequired = isRequired;

            properties.Add(viewProperty);

            return properties;
        }

        public static List<ViewProperty> AddTagProperty(this List<ViewProperty> properties, string name, List<string> value, bool isReadOnly, bool isHidden)
        {
            ViewProperty viewProperty = new ViewProperty();

            viewProperty.Name = name;

            viewProperty.IsReadOnly = isReadOnly;
            viewProperty.IsHidden = isHidden;
            viewProperty.UiType = "Tags";
            viewProperty.OriginalType = "List";
            if (value == null)
            {
                viewProperty.RawValue = new string[]{};
            }
            else
            {
                viewProperty.RawValue = value;
            }
            
            
            properties.Add(viewProperty);

            return properties;
        }

        public static List<ViewProperty> AddProperty(this List<ViewProperty> properties, string name, object rawValue, string displayName = null, string uiType = null)
        {
            ViewProperty viewProperty = new ViewProperty();

            viewProperty.Name = name;
            viewProperty.DisplayName = displayName;
            viewProperty.RawValue = rawValue;
            viewProperty.UiType = uiType;

            properties.Add(viewProperty);

            return properties;
        }


    }
}