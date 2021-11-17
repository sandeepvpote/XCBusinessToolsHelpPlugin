using Microsoft.Extensions.Logging;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.BusinessTools.Pipelines.Arguments;
using Sitecore.Framework.Pipelines;

namespace Sitecore.Commerce.Plugin.BusinessTools.Pipelines
{
    /// <summary>
    ///     Class CreateHelpPipeline.
    /// </summary>
    public class CreateHelpPipeline :
        CommercePipeline<CreateHelpArgument, HelpContentArgument>,
        ICreateHelpPipeline,
        IPipeline<CreateHelpArgument, HelpContentArgument, CommercePipelineExecutionContext>,
        IPipelineBlock<CreateHelpArgument, HelpContentArgument, CommercePipelineExecutionContext>,
        IPipeline, IPipelineBlock
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CreateHelpPipeline" /> class.
        /// </summary>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="configuration">The configuration.</param>
        public CreateHelpPipeline(
            ILoggerFactory loggerFactory,
            IPipelineConfiguration<ICreateHelpPipeline> configuration) : base(configuration, loggerFactory)
        {
        }
    }
}