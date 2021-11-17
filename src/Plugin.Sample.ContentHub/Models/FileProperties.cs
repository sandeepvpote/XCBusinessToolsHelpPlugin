// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System;
using Newtonsoft.Json;
using Sitecore.Framework.Conditions;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    /// Represents a set of file properties of a Content Hub Asset.
    /// </summary>
    public class FileProperties
    {
        /// <summary>
        /// Initializes a new instance of <see cref="FileProperties"/>.
        /// </summary>
        /// <param name="group">The name of the group the asset belongs to.</param>
        /// <param name="extension">The extension of the asset file.</param>
        /// <param name="width">The width of the asset.</param>
        /// <param name="height">The height of the asset.</param>
        /// <param name="resolution">The resolution of the asset.</param>
        /// <param name="colorSpace">The name of color space used in asset.</param>
        /// <param name="megapixels">The amount of megapixels in the asset.</param>
        /// <param name="application">The name of the application used for asset.</param>
        /// <param name="contentType">The MIME content type for asset.</param>
        public FileProperties(string group, string extension, int width, int height, string resolution, string colorSpace, double megapixels, string application, string contentType)
        {
            Group = group;
            Extension = extension;
            Width = width;
            Height = height;
            Resolution = resolution;
            ColorSpace = colorSpace;
            Megapixels = megapixels;
            Application = application;
            ContentType = contentType;
        }

        /// <summary>
        /// Gets the name of the group the asset belongs to.
        /// </summary>
        public string Group { get; }

        /// <summary>
        /// Gets the extension of the asset file.
        /// </summary>
        public string Extension { get; }

        /// <summary>
        /// Gets the width of the asset.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Gets the height of the asset.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Gets the resolution of the asset.
        /// </summary>
        public string Resolution { get; }

        /// <summary>
        /// Gets the name of color space used in asset.
        /// </summary>
        public string ColorSpace { get; }

        /// <summary>
        /// Gets the amount of megapixels in the asset.
        /// </summary>
        public double Megapixels { get; }

        /// <summary>
        /// Gets the name of the application used for asset.
        /// </summary>
        public string Application { get; }

        /// <summary>
        /// Gets the MIME content type for asset.
        /// </summary>
        [JsonProperty("content_type")]
        public string ContentType { get; }
    }
}
