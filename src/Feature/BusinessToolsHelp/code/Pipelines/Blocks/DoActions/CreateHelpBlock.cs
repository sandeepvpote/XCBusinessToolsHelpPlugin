using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.BusinessTools.Entities;
using Sitecore.Commerce.Plugin.BusinessTools.Pipelines.Arguments;
using Sitecore.Commerce.Plugin.BusinessTools.Policies;
using Sitecore.Commerce.Plugin.CommerceExtension;
using Sitecore.Commerce.Plugin.ManagedLists;
using Sitecore.Framework.Pipelines;

namespace Sitecore.Commerce.Plugin.BusinessTools.Pipelines.Blocks.DoActions
{
    /// <summary>
    ///     Class CreateHelpBlock.
    /// </summary>
    [PipelineDisplayName("Help.Block.CreateHelpBlock")]
    public class CreateHelpBlock : PipelineBlock<CreateHelpArgument, HelpContentArgument, CommercePipelineExecutionContext>
    {
        private readonly IDoesEntityExistPipeline _doesEntityExistPipeline;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CreateHelpBlock" /> class.
        /// </summary>
        /// <param name="doesEntityExistPipeline">The does entity exist pipeline.</param>
        public CreateHelpBlock(IDoesEntityExistPipeline doesEntityExistPipeline)
        {
            _doesEntityExistPipeline = doesEntityExistPipeline;
        }

        /// <summary>
        ///     Runs the specified argument.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="context">The context.</param>
        /// <returns>Task&lt;HelpContentArgument&gt;.</returns>
        public override async Task<HelpContentArgument> RunBlock(CreateHelpArgument arg,
            CommercePipelineExecutionContext context)
        {
            var createHelpBlock = this;
            var helpViewsPolicy = context.GetPolicy<KnownHelpViewsPolicy>();


            var helpId = $"{CommerceEntity.IdPrefix<Entities.BusinessToolsHelp>()}{arg.EntityName}-{arg.EntityViewName}";

            var helpExists = createHelpBlock._doesEntityExistPipeline
                .RunAsync(new FindEntityArgument(typeof(Entities.BusinessToolsHelp), helpId), context)
                .ConfigureAwait(false).GetAwaiter().GetResult();

            if (helpExists)
            {
                CommerceContextHelper.AbortExecution(context.CommerceContext, "helpNameAlreadyInUse",
                    $"Help name {arg.HelpName} is already in use.", arg.HelpName);
                return null;
            }

            var contentArgument = new HelpContentArgument();

            var help = new Entities.BusinessToolsHelp
            {
                Id = helpId,
                Name = arg.HelpName,
                DisplayName = arg.HelpName,
                HelpName = arg.HelpName,
                HelpDescription = arg.HelpDescription,
                EntityName = arg.EntityName,
                EntityViewName = arg.EntityViewName
            };

           

            //var component = contentArgument.Help.GetComponent<ListMembershipsComponent>();
            //component.Memberships.Add(CommerceEntity.ListName<BusinessToolsHelp>() ?? "");
            //component.Memberships.Add(helpViewsPolicy.BusinessToolsHelpItems);

            help.SetComponent((Component)new ListMembershipsComponent((IEnumerable<string>)new List<string>()
            {
                CommerceEntity.ListName<BusinessToolsHelp>(),
                helpViewsPolicy.BusinessToolsHelpItems
            }));

            contentArgument.Help = help;

            return await Task.FromResult(contentArgument).ConfigureAwait(false);
        }
    }
}