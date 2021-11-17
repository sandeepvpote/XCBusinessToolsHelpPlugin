using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.BusinessTools.Pipelines.Arguments;
using Sitecore.Framework.Pipelines;

namespace Sitecore.Commerce.Plugin.BusinessTools.Pipelines
{
    /// <summary>
    ///     Interface ICreateHelpPipeline
    /// </summary>
    [PipelineDisplayName("Help.Pipeline.CreateHelp")]
    public interface ICreateHelpPipeline :
        IPipeline<CreateHelpArgument, HelpContentArgument, CommercePipelineExecutionContext>,
        IPipelineBlock<CreateHelpArgument, HelpContentArgument, CommercePipelineExecutionContext>,
        IPipeline, IPipelineBlock
    {
    }
}