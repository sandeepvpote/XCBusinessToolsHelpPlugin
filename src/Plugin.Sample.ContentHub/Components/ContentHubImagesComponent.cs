// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;
using Sitecore.Commerce.Core;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    /// Represents a <see cref="Component"/> that contains images from Content Hub.
    /// </summary>
    public class ContentHubImagesComponent : Component
    {
        /// <summary>
        /// Gets the images retrieved from Content Hub.
        /// </summary>
        [NotMapped]
        [JsonIgnore]
        public IList<ContentHubImage> ContentHubImages => ChildComponents.OfType<ContentHubImage>().ToList();
    }
}
