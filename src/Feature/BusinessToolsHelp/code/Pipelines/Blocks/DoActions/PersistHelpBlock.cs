using System.Threading.Tasks;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.BusinessTools.Pipelines.Arguments;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;

namespace Sitecore.Commerce.Plugin.BusinessTools.Pipelines.Blocks.DoActions
{
    /// <summary>
    ///     Class PersistHelpBlock.
    /// </summary>
    [PipelineDisplayName("Help.Block.PersistHelpBlock")]
    public class PersistHelpBlock : PipelineBlock<HelpContentArgument, HelpContentArgument,
        CommercePipelineExecutionContext>
    {
        private readonly IPersistEntityPipeline _persistEntityPipeline;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PersistHelpBlock" /> class.
        /// </summary>
        /// <param name="persistEntityPipeline">The persist entity pipeline.</param>
        public PersistHelpBlock(IPersistEntityPipeline persistEntityPipeline)
        {
            _persistEntityPipeline = persistEntityPipeline;
        }

        /// <summary>
        ///     Runs the specified argument.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="context">The context.</param>
        /// <returns>Task&lt;HelpContentArgument&gt;.</returns>
        public override async Task<HelpContentArgument> RunBlock(HelpContentArgument arg,
            CommercePipelineExecutionContext context)
        {
            var persistAccommodationStoreBlock = this;

            Condition.Requires(arg)
                .IsNotNull(persistAccommodationStoreBlock.Name + ": The help can not be null");

            await persistAccommodationStoreBlock._persistEntityPipeline
                .RunAsync(new PersistEntityArgument(arg.Help), context).ConfigureAwait(false);

            context.CommerceContext.AddEntity(arg.Help);

            return arg;
        }
    }
}