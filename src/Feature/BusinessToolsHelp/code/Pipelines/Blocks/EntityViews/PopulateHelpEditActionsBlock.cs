using System;
using System.Threading.Tasks;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.BusinessTools.Policies;
using Sitecore.Framework.Pipelines;

namespace Sitecore.Commerce.Plugin.BusinessTools.Pipelines.Blocks.EntityViews
{
    /// <summary>
    ///     Class PopulateHelpEditActionsBlock.
    /// </summary>
    [PipelineDisplayName("Help.Blocks.PopulateHelpEditViewBlock")]
    public class
        PopulateHelpEditActionsBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly ViewCommander _viewCommander;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PopulateHelpEditActionsBlock" /> class.
        /// </summary>
        /// <param name="viewCommander">The view commander.</param>
        public PopulateHelpEditActionsBlock(ViewCommander viewCommander)
        {
            _viewCommander = viewCommander;
        }

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
            var request = _viewCommander.CurrentEntityViewArgument(context.CommerceContext);

            if (string.IsNullOrEmpty(arg?.Name) ||
                !arg.Name.Equals(viewsPolicy.Details, StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(arg);

            if (!(request?.Entity is Entities.BusinessToolsHelp))
            {
                return Task.FromResult(arg);
            }

            if (request?.Entity is Entities.BusinessToolsHelp && !string.IsNullOrEmpty(request.ItemId))
            {
                return Task.FromResult(arg);
            }

            var actionPolicy = arg.GetPolicy<ActionsPolicy>();

            var actionView = new EntityActionView
            {
                Name = actionsPolicy.EditHelp,
                IsEnabled = true,
                EntityView = arg.Name,
                Icon = Constants.Pipelines.Block.EditIcon
            };

            actionPolicy.AddAction(actionView);

            return Task.FromResult(arg);
        }
    }
}