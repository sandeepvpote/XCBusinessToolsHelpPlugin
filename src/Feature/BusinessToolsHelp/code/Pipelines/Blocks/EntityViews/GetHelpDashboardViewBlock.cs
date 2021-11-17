using System;
using System.Linq;
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
    ///     Class GetHelpProfileDashboardViewBlock.
    /// </summary>
    [PipelineDisplayName("Help.Block.GetDashboardViewBlock")]
    public class
        GetHelpDashboardViewBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly IFindEntitiesInListPipeline _findEntitiesInListPipeline;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetHelpDashboardViewBlock" /> class.
        /// </summary>
        /// <param name="findEntitiesInListPipeline">The find entities in list pipeline.</param>
        public GetHelpDashboardViewBlock(IFindEntitiesInListPipeline findEntitiesInListPipeline)
        {
            _findEntitiesInListPipeline = findEntitiesInListPipeline;
        }

        /// <summary>
        ///     Runs the specified argument.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="context">The context.</param>
        /// <returns>Task&lt;EntityView&gt;.</returns>
        public override async Task<EntityView> RunBlock(EntityView arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{Name} {Constants.Pipelines.Block.ArgumentNullMessage}");
            var viewsPolicy = context.GetPolicy<KnownHelpViewsPolicy>();

            var entityViewArgument = context.CommerceContext.GetObjects<EntityViewArgument>().FirstOrDefault();

            if (string.IsNullOrEmpty(entityViewArgument?.ViewName)
                || !entityViewArgument.ViewName.Equals(viewsPolicy.Help,
                    StringComparison.OrdinalIgnoreCase)
                && !entityViewArgument.ViewName.Equals(viewsPolicy.HelpDashboard,
                    StringComparison.OrdinalIgnoreCase))
            {
                return await Task.FromResult(arg);
            }

            var helpView = new EntityView
            {
                Name = viewsPolicy.Helps,
                ItemId = viewsPolicy.Helps,
                EntityId = arg.EntityId,
                UiHint = "Table"
            };

            arg.ChildViews.Add(helpView);


            var listName  = CommerceEntity.ListName<Entities.BusinessToolsHelp>();
            var storeResult = await _findEntitiesInListPipeline.RunAsync(
                new FindEntitiesInListArgument(typeof(Entities.BusinessToolsHelp),
                    listName, 0, int.MaxValue), context);

            if (storeResult?.List?.Items == null)
            {
                return await Task.FromResult(arg);
            }

            foreach (var helpEntity in storeResult.List.Items)
            {
                var help = helpEntity as Entities.BusinessToolsHelp;

                var summaryEntityView = new EntityView
                {
                    EntityId = helpEntity?.Id,
                    ItemId = helpEntity?.Id,
                    DisplayName = helpEntity?.DisplayName,
                    Name = "Summary"
                };

                var viewProperties = summaryEntityView.Properties;

                viewProperties.AddViewProperty(viewsPolicy.HelpName, help?.HelpName, true, false,
                    "EntityLink");
                viewProperties.AddViewProperty(viewsPolicy.EntityName, help?.EntityName, true, false);
                viewProperties.AddViewProperty(viewsPolicy.EntityViewName, help?.EntityViewName, true, false);
                
                helpView.ChildViews.Add(summaryEntityView);
            }

            return await Task.FromResult(arg);
        }
    }
}