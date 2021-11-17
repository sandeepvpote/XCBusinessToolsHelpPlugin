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
    /// Class DoActionAddHelpBlock.
    /// </summary>
    [PipelineDisplayName("Help.Block.AddHelpBlock")]
    public class
        DoActionAddHelpBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly CreateHelpCommand _createHelpCommand;

        /// <summary>
        /// Initializes a new instance of the <see cref="DoActionAddHelpBlock" /> class.
        /// </summary>
        /// <param name="createHelpCommand">The create help  command.</param>
        public DoActionAddHelpBlock(CreateHelpCommand createHelpCommand)
        {
            _createHelpCommand = createHelpCommand;
        }

        /// <summary>
        /// Runs the specified argument.
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
            if (string.IsNullOrEmpty(arg.Action) || !arg.Action.Equals(helpActionsPolicy.AddHelp,
                    StringComparison.OrdinalIgnoreCase))
            {
                return await Task.FromResult(arg);
            }

            var createHelpArgument = new CreateHelpArgument();

            var helpName = arg.Properties.FirstOrDefault(x =>
                x.Name.Equals(helpViewsPolicy.HelpName, StringComparison.OrdinalIgnoreCase));

            if (helpName?.Value != null)
            {
                createHelpArgument.HelpName = helpName.Value;
            }

            var entityName = arg.Properties.FirstOrDefault(x =>
                x.Name.Equals(helpViewsPolicy.EntityName, StringComparison.OrdinalIgnoreCase));

            if (entityName?.Value != null)
            {
                createHelpArgument.EntityName = entityName.Value;
            }

            var entityViewName = arg.Properties.FirstOrDefault(x =>
                x.Name.Equals(helpViewsPolicy.EntityViewName, StringComparison.OrdinalIgnoreCase));

            if (entityViewName?.Value != null)
            {
                createHelpArgument.EntityViewName = entityViewName.Value;
            }

            var helpDescription = arg.Properties.FirstOrDefault(x =>
                x.Name.Equals(helpViewsPolicy.HelpDescription, StringComparison.OrdinalIgnoreCase));

            if (helpDescription?.Value != null)
            {
                createHelpArgument.HelpDescription = helpDescription?.Value;
            }

            var help = await _createHelpCommand
                .Process(context.CommerceContext, createHelpArgument).ConfigureAwait(false);

            return await Task.FromResult(arg);
        }
    }
}