using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Promotions;
using Sitecore.Commerce.Plugin.PromotionsHelp.Policies;
using Sitecore.Framework.Pipelines;

namespace Sitecore.Commerce.Plugin.PromotionsHelp.Pipelines.Blocks
{
    /// <summary>
    ///     Class PopulateCompanyAddActionsBlock.
    /// </summary>
    [PipelineDisplayName("Plugin.Promotions.Import.PopulatePromotionHelpViewBlock")]
    public class PopulatePromotionHelpViewBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        /// <summary>
        ///     Runs the specified argument.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="context">The context.</param>
        /// <returns>Task&lt;EntityView&gt;.</returns>
        public override Task<EntityView> RunBlock(EntityView arg, CommercePipelineExecutionContext context)
        {
            var viewsPolicy = context.GetPolicy<KnownPromotionHelpViewsPolicy>();
            if (string.IsNullOrEmpty(arg?.Name) || !arg.Name.Equals(context.GetPolicy<KnownPromotionsViewsPolicy>().Details, StringComparison.OrdinalIgnoreCase) 
                                                || !string.IsNullOrEmpty(arg.Action))
                return Task.FromResult<EntityView>(arg);
            if (!(context.CommerceContext.GetObject<EntityViewArgument>()?.Entity is PromotionBook))
                return Task.FromResult<EntityView>(arg);
            var actionPolicy = arg.GetPolicy<ActionsPolicy>();

            var actionView = new EntityActionView
            {
                Name = viewsPolicy.PromotionListHelp,
                IsEnabled = true,
                EntityView = context.GetPolicy<KnownPromotionsViewsPolicy>().Details,
                Icon = Constants.Pipelines.Block.HelpIcon
            };


            actionPolicy.AddAction(actionView);

            return Task.FromResult(arg);
        }
        
    }
}