// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using Microsoft.Extensions.Logging;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Pipelines;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    /// Defines a class that implements the <see cref="IImportEntityPipeline"/> interface. This pipeline
    /// imports updates to a <see cref="SellableItem"/> from a Content Hub instance.
    /// </summary>
    public class ImportEntityPipeline : CommercePipeline<ImportEntityArgument, ImportEntityArgument>, IImportEntityPipeline
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImportEntityPipeline"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="IPipelineConfiguration"/> for this pipeline.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> instance to be used for logging.</param>
        public ImportEntityPipeline(IPipelineConfiguration<IImportEntityPipeline> configuration, ILoggerFactory loggerFactory) : base(configuration, loggerFactory)
        {
        }
    }
}
