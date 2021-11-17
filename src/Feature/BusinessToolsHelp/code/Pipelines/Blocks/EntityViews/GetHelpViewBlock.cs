using System;
using System.Threading.Tasks;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.BusinessTools.Policies;
using Sitecore.Commerce.Plugin.CommerceExtension;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;

namespace Sitecore.Commerce.Plugin.BusinessTools.Pipelines.Blocks.EntityViews
{
    /// <summary>
    ///     Class GetHelpViewBlock.
    /// </summary>
    /// <seealso
    [PipelineDisplayName("Help.Block.GetViewBlock")]
    public class GetHelpViewBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly ViewCommander _viewCommander;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetHelpViewBlock" /> class.
        /// </summary>
        /// <param name="viewCommander">The view commander.</param>
        public GetHelpViewBlock(ViewCommander viewCommander)
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
            Condition.Requires(arg).IsNotNull($"{Name}: {Constants.Pipelines.Block.ArgumentNullMessage}");

            var viewsPolicy = context.GetPolicy<KnownHelpViewsPolicy>();
            var actionsPolicy = context.GetPolicy<KnownHelpActionsPolicy>();
            var request = _viewCommander.CurrentEntityViewArgument(context.CommerceContext);

            if (!arg.Name.Equals(viewsPolicy.Helps, StringComparison.OrdinalIgnoreCase)
                && !arg.Name.Equals(viewsPolicy.Master, StringComparison.OrdinalIgnoreCase)
                && !arg.Name.Equals(viewsPolicy.Details, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(arg);
            }

            var isAddView = !string.IsNullOrEmpty(arg.Action) && arg.Action.Equals(actionsPolicy.AddHelp,
                                StringComparison.OrdinalIgnoreCase);

            // On add action 
            if (isAddView)
            {
                arg.Properties.AddProperty(viewsPolicy.HelpName, string.Empty, true);
                arg.Properties.AddProperty(viewsPolicy.EntityName, string.Empty, true);
                arg.Properties.AddProperty(viewsPolicy.EntityViewName, string.Empty, true);
                arg.Properties.AddProperty(viewsPolicy.HelpDescription, string.Empty, true);
                return Task.FromResult(arg);
            }

            var isEditView = !string.IsNullOrEmpty(arg.Action) &&
                             arg.Action.Equals(actionsPolicy.EditHelp,
                                 StringComparison.OrdinalIgnoreCase);

            if (!(request.Entity is Entities.BusinessToolsHelp help))
            {
                return Task.FromResult(arg);
            }

            var entityView = arg;
            // On details view
            if (!isEditView)
            {
                var view = new EntityView
                {
                    Name = viewsPolicy.Details,
                    ItemId = viewsPolicy.Details,
                    EntityId = entityView.EntityId,
                    UiHint = "Vertical"
                };
                arg.ChildViews.Add(view);

                entityView = view;
            }

            // Details view and edit action
            entityView.Properties.AddViewProperty(viewsPolicy.HelpName, help?.HelpName ?? string.Empty, true, false);
            entityView.Properties.AddProperty(viewsPolicy.EntityName, string.Empty, true);
            entityView.Properties.AddProperty(viewsPolicy.EntityViewName, string.Empty, true);
            entityView.Properties.AddProperty(viewsPolicy.HelpDescription, string.Empty, true);



            return Task.FromResult(arg);
        }
    }
}