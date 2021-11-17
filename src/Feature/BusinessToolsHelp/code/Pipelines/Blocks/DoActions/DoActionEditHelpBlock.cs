using System;
using System.Linq;
using System.Threading.Tasks;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.BusinessTools.Commands;
using Sitecore.Commerce.Plugin.BusinessTools.Pipelines.Arguments;
using Sitecore.Commerce.Plugin.BusinessTools.Policies;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;

namespace Sitecore.Commerce.Plugin.BusinessTools.Pipelines.Blocks.DoActions
{
    /// <summary>
    ///     Class DoActionEditHelpBlock.
    /// </summary>
    [PipelineDisplayName("Help.Block.EditHelpBlock")]
    public class
        DoActionEditHelpBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly EditHelpCommand _editHelpCommand;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DoActionEditHelpBlock" /> class.
        /// </summary>
        /// <param name="editHelpCommand">The edit help  command.</param>
        public DoActionEditHelpBlock(EditHelpCommand editHelpCommand)
        {
            _editHelpCommand = editHelpCommand;
        }

        /// <summary>
        ///     Runs the specified argument.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="context">The context.</param>
        /// <returns>Task&lt;EntityView&gt;.</returns>
        public override async Task<EntityView> RunBlock(EntityView arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{Name}: {Constants.Pipelines.Block.ArgumentNullMessage}");

            var helpActionsPolicy = context.GetPolicy<KnownHelpActionsPolicy>();
            var helpViewsPolicy = context.GetPolicy<KnownHelpViewsPolicy>();

            // Only proceed if the AddAccommodationStore action was invoked
            if (string.IsNullOrEmpty(arg.Action) || !arg.Action.Equals(helpActionsPolicy.EditHelp,
                    StringComparison.OrdinalIgnoreCase))
            {
                return await Task.FromResult(arg);
            }

            var editHelpArgument = new EditHelpArgument();

            editHelpArgument.EntityId = arg.EntityId;

            var helpName = arg.Properties.FirstOrDefault(x =>
                x.Name.Equals(helpViewsPolicy.HelpName, StringComparison.OrdinalIgnoreCase));

            if (helpName?.Value != null)
            {
                editHelpArgument.HelpName = helpName.Value;
            }

            var entityName = arg.Properties.FirstOrDefault(x =>
                x.Name.Equals(helpViewsPolicy.EntityName, StringComparison.OrdinalIgnoreCase));

            if (entityName?.Value != null)
            {
                editHelpArgument.EntityName = entityName.Value;
            }

            var entityViewName = arg.Properties.FirstOrDefault(x =>
                x.Name.Equals(helpViewsPolicy.EntityViewName, StringComparison.OrdinalIgnoreCase));

            if (entityViewName?.Value != null)
            {
                editHelpArgument.EntityViewName = entityViewName.Value;
            }

            var helpDescription = arg.Properties.FirstOrDefault(x =>
                x.Name.Equals(helpViewsPolicy.HelpDescription, StringComparison.OrdinalIgnoreCase));

            if (helpDescription?.Value != null)
            {
                editHelpArgument.HelpDescription = helpDescription?.Value;
            }


            var help = await _editHelpCommand
                .Process(context.CommerceContext, editHelpArgument).ConfigureAwait(false);

            return await Task.FromResult(arg);
        }
    }
}