// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using Newtonsoft.Json;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    ///     Represents a class with information about the conversion of a Content Hub Asset from original to
    ///     another form, accessible via Public Link.
    /// </summary>
    public class ConversionConfiguration
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ConversionConfiguration"/>.
        /// </summary>
        /// <param name="originalWidth">The original width of an Asset.</param>
        /// <param name="originalHeight">The original height of an Asset.</param>
        /// <param name="ratio">The ratio of the conversion with respect to its original.</param>
        /// <param name="croppingConfiguration">The optional <see cref="CroppingConfiguration"/> of an Asset.</param>
        /// <param name="width">The optional width, if different from <paramref name="originalWidth"/>.</param>
        /// <param name="height">The optional height, if different from <paramref name="originalHeight"/>.</param>
        public ConversionConfiguration(int originalWidth, int originalHeight, Ratio ratio,
            CroppingConfiguration croppingConfiguration, int? width = null, int? height = null)
        {
            OriginalWidth = originalWidth;
            OriginalHeight = originalHeight;
            Ratio = ratio;
            CroppingConfiguration = croppingConfiguration;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Gets the optional width, if different from <see cref="OriginalWidth"/>.
        /// </summary>
        public int? Width { get; }

        /// <summary>
        /// Gets the optional height, if different from <see cref="OriginalHeight"/>.
        /// </summary>
        public int? Height { get; }

        /// <summary>
        /// Gets the optional <see cref="CroppingConfiguration"/> of an Asset.
        /// </summary>
        [JsonProperty("cropping_configuration")]
        public CroppingConfiguration CroppingConfiguration { get; }

        /// <summary>
        /// Gets the original width of an Asset.
        /// </summary>
        [JsonProperty("original_width")]
        public int OriginalWidth { get; }

        /// <summary>
        /// Gets the original height of an Asset.
        /// </summary>
        [JsonProperty("original_height")]
        public int OriginalHeight { get; }

        /// <summary>
        /// Gets the ratio of an Asset.
        /// </summary>
        public Ratio Ratio { get; }
    }
}
