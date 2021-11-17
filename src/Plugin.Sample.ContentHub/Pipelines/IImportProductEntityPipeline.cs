// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Pipelines;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    /// Defines an interface for a pipeline that imports product updates to a <see cref="SellableItem"/>.
    /// </summary>
    [PipelineDisplayName(ContentHubConstants.ImportProductEntityPipeline)]
    public interface IImportProductEntityPipeline : IPipeline<ImportEntityArgument, ImportEntityArgument, CommercePipelineExecutionContext>
    {
    }
}
