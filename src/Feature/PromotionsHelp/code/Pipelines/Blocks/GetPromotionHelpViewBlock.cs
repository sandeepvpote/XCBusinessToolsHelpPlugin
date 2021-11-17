using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.BusinessTools.Entities;
using Sitecore.Commerce.Plugin.Promotions;
using Sitecore.Commerce.Plugin.PromotionsHelp.Policies;
using Sitecore.Framework.Conditions;

namespace Sitecore.Commerce.Plugin.PromotionsHelp.Pipelines.Blocks
{
    public class GetPromotionHelpViewBlock : PipelineBlock<EntityView, EntityView,
            CommercePipelineExecutionContext>
    {
        private readonly IFindEntityPipeline _findEntityPipeline;


        public GetPromotionHelpViewBlock(IFindEntityPipeline findEntityPipeline) : base()
        {
            _findEntityPipeline = findEntityPipeline;
        }

        public override Task<EntityView> RunBlock(EntityView arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{Name}: {Constants.Pipelines.Block.ArgumentNullMessage}");

            var viewsPolicy = context.GetPolicy<KnownPromotionHelpViewsPolicy>();

            var entityViewArgument = context.CommerceContext.GetObjects<EntityViewArgument>()
                .FirstOrDefault();

            if (string.IsNullOrEmpty(entityViewArgument?.ViewName)
                //|| !entityViewArgument.ViewName.Equals(viewsPolicy.PromotionImportSearchMediaItems,
                //    StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(entityViewArgument.ForAction) 
                ||
                !entityViewArgument.ForAction.Equals(viewsPolicy.PromotionListHelp,
                    StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(arg);
            }

            var helpId = $"{CommerceEntity.IdPrefix<BusinessToolsHelp>()}Promotion-{arg.Name}";

            var findHelpEntity = _findEntityPipeline.RunAsync(
                new FindEntityArgument(typeof(BusinessToolsHelp), helpId),
                new CommercePipelineExecutionContextOptions(context.CommerceContext)).Result;

            var helpEntity = findHelpEntity as BusinessToolsHelp;

            if(helpEntity== null) return Task.FromResult(arg);

            List<ViewProperty> properties1 = arg?.Properties;
            properties1?.Add(new ViewProperty()
            {
                Name = "HelpDetails",
                RawValue = helpEntity.HelpDescription,
                IsReadOnly = true,
                IsRequired = true,
                IsHidden = false, 
                Value = helpEntity.HelpDescription
            });


            return Task.FromResult(arg);
        }
    }
}
