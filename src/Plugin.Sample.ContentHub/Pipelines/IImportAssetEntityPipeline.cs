// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;
using Stylelabs.M.Sdk.Contracts.Base;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    /// Defines an interface for a pipeline that imports content hub <see cref="IEntity"/> updates.
    /// </summary>
    [PipelineDisplayName(ContentHubConstants.ImportAssetEntityPipeline)]
    public interface IImportAssetEntityPipeline : IPipeline<ImportEntityArgument, ImportEntityArgument, CommercePipelineExecutionContext>
    {
    }
}
