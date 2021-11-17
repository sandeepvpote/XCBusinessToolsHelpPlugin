using System;
using System.Threading.Tasks;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.BusinessTools.Policies;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;

namespace Sitecore.Commerce.Plugin.BusinessTools.Pipelines.Blocks.EntityViews
{
    /// <summary>
    ///     Class GetHelpNavigationViewBlock.
    /// </summary>
    [PipelineDisplayName("Help.Block.GetNavigationViewBlock")]
    public class
        GetHelpNavigationViewBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        /// <summary>
        ///     Runs the specified argument.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="context">The context.</param>
        /// <returns>Task&lt;EntityView&gt;.</returns>
        public override Task<EntityView> RunBlock(EntityView arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{Name}: {Constants.Pipelines.Block.ArgumentNullMessage}");

            var viewsPolicy = context.GetPolicy<KnownHelpViewsPolicy>();

            if (string.IsNullOrEmpty(arg?.Name)
                || !arg.Name.Equals(viewsPolicy.ToolsNavigation, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(arg);
            }

            var accommodationGroupView = new EntityView
            {
                Name = viewsPolicy.HelpDashboard,
                ItemId = viewsPolicy.HelpDashboard,
                Icon = Constants.Pipelines.Block.DashboardIcon,
                DisplayRank = Constants.Pipelines.Block.DisplayRank
            };

            arg.ChildViews.Add(accommodationGroupView);

            return Task.FromResult(arg);
        }
    }
}