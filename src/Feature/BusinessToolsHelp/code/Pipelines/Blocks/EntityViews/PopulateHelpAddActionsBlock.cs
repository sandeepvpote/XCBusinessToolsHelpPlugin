using System;
using System.Threading.Tasks;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.BusinessTools.Policies;
using Sitecore.Framework.Pipelines;

namespace Sitecore.Commerce.Plugin.BusinessTools.Pipelines.Blocks.EntityViews
{
    /// <summary>
    ///     Class PopulateHelpAddActionsBlock.
    /// </summary>
    [PipelineDisplayName("Help.Blocks.PopulateHelpAddViewBlock")]
    public class PopulateHelpAddActionsBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        /// <summary>
        ///     Runs the specified argument.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="context">The context.</param>
        /// <returns>Task&lt;EntityView&gt;.</returns>
        public override Task<EntityView> RunBlock(EntityView arg, CommercePipelineExecutionContext context)
        {
            var viewsPolicy = context.GetPolicy<KnownHelpViewsPolicy>();
            var actionsPolicy = context.GetPolicy<KnownHelpActionsPolicy>();

            if (string.IsNullOrEmpty(arg?.Name) ||
                !(arg.Name.Equals(viewsPolicy.Helps, StringComparison.OrdinalIgnoreCase) &&
                  arg.ItemId.Equals(viewsPolicy.Helps, StringComparison.OrdinalIgnoreCase)))
                return Task.FromResult(arg);

            var actionPolicy = arg.GetPolicy<ActionsPolicy>();

            var actionView = new EntityActionView
            {
                Name = actionsPolicy.AddHelp,
                IsEnabled = true,
                EntityView = arg.Name,
                Icon = Constants.Pipelines.Block.AddIcon
            };

            actionPolicy.AddAction(actionView);

            return Task.FromResult(arg);
        }
    }
}