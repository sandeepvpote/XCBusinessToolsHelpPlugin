// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    /// Defines an interface for a pipeline that imports updates from CH PublicLinks to <see cref="ContentHubImagesComponent"/>.
    /// </summary>
    [PipelineDisplayName(ContentHubConstants.IImportPublicLinkEntityPipeline)]
    public interface IImportPublicLinkEntityPipeline : IPipeline<ImportEntityArgument, ImportEntityArgument, CommercePipelineExecutionContext>
    {
    }
}
