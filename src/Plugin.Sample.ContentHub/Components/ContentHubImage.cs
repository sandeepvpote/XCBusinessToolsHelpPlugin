// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Conditions;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    ///     Represents a class that contains information about the image, received from Content Hub.
    /// </summary>
    public class ContentHubImage : Component
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ContentHubImage" /> class.
        /// </summary>
        public ContentHubImage()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ContentHubImage" /> class.
        /// </summary>
        /// <param name="url">The <see cref="Uri"/> of the image.</param>
        /// <param name="width">The width (in pixels) of the image.</param>
        /// <param name="height">The height (in pixels) of the image.</param>
        /// <param name="alternateText">The alternative information for an image if a user cannot view it.</param>
        /// <param name="isMaster">
        ///     The <see cref="bool"/> value that defines whether the image is master (main) or not.
        /// </param>
        /// <param name="linkId">The related link id.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="url"/> or <paramref name="alternateText"/> is <see langword="null"/>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="width"/> or <paramref name="height"/> is less or equal to 0.
        /// </exception>
        public ContentHubImage(Uri url, int width, int height, string alternateText, bool isMaster, long? linkId)
        {
            Condition.Requires(url, nameof(url)).IsNotNull();
            Condition.Requires(width, nameof(width)).IsGreaterThan(0);
            Condition.Requires(height, nameof(height)).IsGreaterThan(0);
            Condition.Requires(linkId, nameof(linkId)).IsNotNull();

            Url = url;
            Width = width;
            Height = height;
            IsMaster = isMaster;
            AlternateText = alternateText ?? string.Empty;
            Id = linkId.ToString();
        }

        /// <summary>
        ///     Gets the image URL.
        /// </summary>
        public Uri Url { get; set; }

        /// <summary>
        ///     Gets the image width.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        ///     Gets the image height.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        ///     Gets the image Master status.
        /// </summary>
        public bool IsMaster { get; set; }

        /// <summary>
        ///     Gets the alternative information for an image if a user cannot view it.
        /// </summary>
        public string AlternateText { get; set; }
    }
}
