using System.Threading.Tasks;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.BusinessTools.Pipelines.Arguments;
using Sitecore.Commerce.Plugin.CommerceExtension;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;

namespace Sitecore.Commerce.Plugin.BusinessTools.Pipelines.Blocks.DoActions
{
    /// <summary>
    ///     Class EditHelpBlock.
    /// </summary>
    [PipelineDisplayName("Help.Block.EditHelpBlock")]
    public class EditHelpBlock : PipelineBlock<EditHelpArgument, HelpContentArgument,
        CommercePipelineExecutionContext>
    {
        /// <summary>
        ///     Runs the specified argument.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="context">The context.</param>
        /// <returns>Task&lt;HelpContentArgument&gt;.</returns>
        public override async Task<HelpContentArgument> RunBlock(EditHelpArgument arg,
            CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{Name}: {Constants.Pipelines.Block.ArgumentNullMessage}");

            var help = arg.Help;

            if (help == null)
            {
                CommerceContextHelper.AbortExecution(context.CommerceContext, "HelpDoseNotExists",
                    $"Help {arg.HelpName} does not exists.", arg.HelpName);

                return null;
            }

            help.Name = arg.HelpName;
            help.DisplayName = arg.HelpName;
            help.HelpName = arg.HelpName;
            help.HelpDescription = arg.HelpDescription;
            help.EntityName = arg.EntityName;
            help.EntityViewName = arg.EntityViewName;

            var contentArgument = new HelpContentArgument
            {
                Help = help
            };

            return await Task.FromResult(contentArgument).ConfigureAwait(false);
        }
    }
}