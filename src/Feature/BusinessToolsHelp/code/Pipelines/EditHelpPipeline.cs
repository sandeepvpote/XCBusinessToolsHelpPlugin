using Microsoft.Extensions.Logging;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.BusinessTools.Pipelines.Arguments;
using Sitecore.Framework.Pipelines;

namespace Sitecore.Commerce.Plugin.BusinessTools.Pipelines
{
    /// <summary>
    ///     Class EditHelppPipeline.
    /// </summary>
    public class EditHelpPipeline :
        CommercePipeline<EditHelpArgument, HelpContentArgument>,
        IEditHelpPipeline,
        IPipeline<EditHelpArgument, HelpContentArgument, CommercePipelineExecutionContext>,
        IPipelineBlock<EditHelpArgument, HelpContentArgument, CommercePipelineExecutionContext>,
        IPipeline, IPipelineBlock
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="EditHelpPipeline" /> class.
        /// </summary>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="configuration">The configuration.</param>
        public EditHelpPipeline(
            ILoggerFactory loggerFactory,
            IPipelineConfiguration<IEditHelpPipeline> configuration) : base(configuration, loggerFactory)
        {
        }
    }
}