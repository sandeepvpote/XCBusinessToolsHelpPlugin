// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using Microsoft.Extensions.Logging;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    /// Defines a pipeline that imports updates from CH PublicLinks to <see cref="ContentHubImagesComponent"/>.
    /// </summary>
    public class ImportPublicLinkEntityPipeline : CommercePipeline<ImportEntityArgument, ImportEntityArgument>, IImportPublicLinkEntityPipeline
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImportPublicLinkEntityPipeline"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="IPipelineConfiguration"/> for this pipeline.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> instance to be used for logging.</param>
        public ImportPublicLinkEntityPipeline(IPipelineConfiguration<IImportPublicLinkEntityPipeline> configuration, ILoggerFactory loggerFactory) : base(configuration, loggerFactory)
        {
        }
    }
}
