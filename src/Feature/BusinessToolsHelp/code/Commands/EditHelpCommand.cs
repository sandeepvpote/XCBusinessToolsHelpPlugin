using System;
using System.Threading.Tasks;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.BusinessTools.Pipelines;
using Sitecore.Commerce.Plugin.BusinessTools.Pipelines.Arguments;

namespace Sitecore.Commerce.Plugin.BusinessTools.Commands
{

    public class EditHelpCommand : CommerceCommand
    {
        private readonly IEditHelpPipeline _editHelpPipeline;
        private readonly IFindEntityPipeline _findEntity;

        public EditHelpCommand(
            IEditHelpPipeline editHelpPipeline,
            IFindEntityPipeline findEntity,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _editHelpPipeline = editHelpPipeline;
            _findEntity = findEntity;
        }

        public async Task<Entities.BusinessToolsHelp> Process(CommerceContext commerceContext,
            EditHelpArgument editHelpArgument)
        {
            var editAccommodationStoreCommand = this;
            Entities.BusinessToolsHelp help = null;

            var helpEntityIdPrefix = CommerceEntity.IdPrefix<Entities.BusinessToolsHelp>();

            if (!editHelpArgument.EntityId.StartsWith(helpEntityIdPrefix,
                StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var helpEntity =
                commerceContext.GetObject<Entities.BusinessToolsHelp>(x =>
                    x.Id.Equals(editHelpArgument.EntityId, StringComparison.OrdinalIgnoreCase));

            if (helpEntity == null)
            {
                var findHelpEntity = await _findEntity.RunAsync(
                    new FindEntityArgument(typeof(Entities.BusinessToolsHelp), editHelpArgument.EntityId),
                    new CommercePipelineExecutionContextOptions(commerceContext));
                helpEntity = findHelpEntity as Entities.BusinessToolsHelp;
            }

            editHelpArgument.Help = helpEntity;

            using (CommandActivity.Start(commerceContext, editAccommodationStoreCommand))
            {
                await editAccommodationStoreCommand.PerformTransaction(commerceContext, async () =>
                {
                    var accommodationStoreContentArgument = await _editHelpPipeline
                        .RunAsync(editHelpArgument, commerceContext.PipelineContextOptions)
                        .ConfigureAwait(false);
                    help = accommodationStoreContentArgument?.Help;
                }).ConfigureAwait(false);
            }

            return help;
        }
    }
}