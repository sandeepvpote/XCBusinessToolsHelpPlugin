using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.BusinessTools.Pipelines.Arguments;
using Sitecore.Framework.Pipelines;

namespace Sitecore.Commerce.Plugin.BusinessTools.Pipelines
{
    /// <summary>
    ///     Interface IEditHelpPipeline
    /// </summary>
    [PipelineDisplayName("Help.Pipeline.EditHelp")]
    public interface IEditHelpPipeline :
        IPipeline<EditHelpArgument, HelpContentArgument, CommercePipelineExecutionContext>,
        IPipelineBlock<EditHelpArgument, HelpContentArgument, CommercePipelineExecutionContext>,
        IPipeline, IPipelineBlock
    {
    }
}