// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using Microsoft.Extensions.Logging;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Pipelines;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    /// Defines a pipeline that implements the <see cref="IImportAssetEntityPipeline"/> interface. This pipeline
    /// imports updates to a <see cref="ContentHubImagesComponent"/>  of the <see cref="SellableItem"/> from a Content Hub asset instance.
    /// </summary>
    public class ImportAssetEntityPipeline : CommercePipeline<ImportEntityArgument, ImportEntityArgument>, IImportAssetEntityPipeline
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImportAssetEntityPipeline"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="IPipelineConfiguration"/> for this pipeline.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> instance to be used for logging.</param>
        public ImportAssetEntityPipeline(IPipelineConfiguration<IImportAssetEntityPipeline> configuration, ILoggerFactory loggerFactory) : base(configuration, loggerFactory)
        {
        }
    }
}
