using System;
using System.Threading.Tasks;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.BusinessTools.Pipelines;
using Sitecore.Commerce.Plugin.BusinessTools.Pipelines.Arguments;

namespace Sitecore.Commerce.Plugin.BusinessTools.Commands
{

    public class CreateHelpCommand : CommerceCommand
    {
        private readonly ICreateHelpPipeline _createHelpPipeline;

        public CreateHelpCommand(
            ICreateHelpPipeline createHelpPipeline,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _createHelpPipeline = createHelpPipeline;
        }

      
        public async Task<Entities.BusinessToolsHelp> Process(CommerceContext commerceContext,
            CreateHelpArgument createHelpArgument)
        {
            var createAccommodationStoreCommand = this;
            Entities.BusinessToolsHelp help = null;

            using (CommandActivity.Start(commerceContext, createAccommodationStoreCommand))
            {
                await createAccommodationStoreCommand.PerformTransaction(commerceContext, async () =>
                {
                    var accommodationStoreContentArgument = await _createHelpPipeline.RunAsync(createHelpArgument, commerceContext.PipelineContextOptions)
                        .ConfigureAwait(false);
                    help = accommodationStoreContentArgument?.Help;
                }).ConfigureAwait(false);
            }

            return help;
        }
    }
}