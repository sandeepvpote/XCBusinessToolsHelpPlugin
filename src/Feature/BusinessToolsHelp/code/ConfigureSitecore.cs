// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.BusinessTools.Pipelines;
using Sitecore.Commerce.Plugin.BusinessTools.Pipelines.Blocks.DoActions;
using Sitecore.Commerce.Plugin.BusinessTools.Pipelines.Blocks.EntityViews;
using Sitecore.Commerce.Plugin.BusinessUsers;
using Sitecore.Framework.Configuration;
using Sitecore.Framework.Pipelines.Definitions;
using Sitecore.Framework.Pipelines.Definitions.Extensions;

namespace Sitecore.Commerce.Plugin.BusinessTools
{
    /// <summary>
    /// The configure sitecore class.
    /// </summary>
    public class ConfigureSitecore : IConfigureSitecore
    {
        /// <summary>
        /// The configure services.
        /// </summary>
        /// <param name="services">
        /// The services.
        /// </param>
        public void ConfigureServices(IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            services.RegisterAllPipelineBlocks(assembly);
            services.RegisterAllCommands(assembly);

            services.Sitecore().Pipelines(config => config
                .AddPipeline<ICreateHelpPipeline, CreateHelpPipeline>((Action<PipelineDefinition<ICreateHelpPipeline>>)(d =>
                {
                    d.Add<CreateHelpBlock>();
                    d.Add<PersistHelpBlock>();
                }))
                .AddPipeline<IEditHelpPipeline, EditHelpPipeline>((Action<PipelineDefinition<IEditHelpPipeline>>)(d =>
                {
                    d.Add<EditHelpBlock>();
                    d.Add<PersistHelpBlock>();
                }))
                .ConfigurePipeline<IBizFxNavigationPipeline>(configure => configure.Add<GetHelpNavigationViewBlock>().After<GetNavigationViewBlock>())
                .ConfigurePipeline<IGetEntityViewPipeline>(configure => configure.Add<GetHelpDashboardViewBlock>().After<PopulateEntityVersionBlock>())
                .ConfigurePipeline<IGetEntityViewPipeline>(configure => configure.Add<GetHelpViewBlock>().After<GetHelpDashboardViewBlock>())
                .ConfigurePipeline<IPopulateEntityViewActionsPipeline>(configure => configure.Add<PopulateHelpAddActionsBlock>().After<InitializeEntityViewActionsBlock>())
                .ConfigurePipeline<IPopulateEntityViewActionsPipeline>(configure => configure.Add<PopulateHelpEditActionsBlock>().After<InitializeEntityViewActionsBlock>())
                .ConfigurePipeline<IDoActionPipeline>(c =>
                {
                    c.Add<DoActionAddHelpBlock>().After<ValidateEntityVersionBlock>();
                    c.Add<DoActionEditHelpBlock>().After<ValidateEntityVersionBlock>();
                })
                .ConfigurePipeline<IConfigureServiceApiPipeline>(configure => configure.Add<ConfigureServiceApiBlock>()));
        }
    }
}
