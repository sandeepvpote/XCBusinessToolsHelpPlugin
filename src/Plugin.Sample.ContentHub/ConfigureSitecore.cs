// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Management;
using Sitecore.Framework.Configuration;
using Sitecore.Framework.Pipelines.Definitions.Extensions;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    ///     The configure class for <see cref="Plugin.Sample.ContentHub"/>.
    /// </summary>
    /// <seealso cref="IConfigureSitecore" />
    [ExcludeFromCodeCoverage] // cannot check the pipelines configurations
    public class ConfigureSitecore : IConfigureSitecore
    {
        /// <summary>
        ///     Configure services necessary for <see cref="Plugin.Sample.ContentHub"/>.
        /// </summary>
        /// <param name="services">
        ///     The <see cref="IServiceCollection"/> to register necessary services.
        /// </param>
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient(typeof(ContentHubClientFactory));
            services.AddTransient(typeof(ServiceBusSubscriptionClientFactory));

            var assembly = Assembly.GetExecutingAssembly();
            services.RegisterAllPipelineBlocks(assembly);

            services.Sitecore().Pipelines(config => config
                .ConfigurePipeline<IStartEnvironmentPipeline>(p => p
                    .Add<InitializeServiceBusBlock>())
                .ConfigurePipeline<IConfigureServiceApiPipeline>(p => p
                    .Add<ConfigureServiceApiBlock>())
                .AddPipeline<IImportEntityPipeline, ImportEntityPipeline>(p => p
                    .Add<FetchEntityBlock>())
                .AddPipeline<IImportProductEntityPipeline, ImportProductEntityPipeline>(p => p
                    .Add<FindSellableItemBlock>()
                    .Add<MapContentHubPropertiesBlock>()
                    .Add<UpdateSellableItemPropertiesBlock>()
                    .Add<UpdateLocalizedPropertiesBlock>()
                    .Add<FetchAssetEntitiesBlock>()
                    .Add<FetchImagePublicLinksBlock>()
                    .Add<BuildImagesComponentBlock>()
                    .Add<SetImagesComponentBlock>()
                    .Add<UpdateLocalizedComponentsBlock>()
                    .Add<PersistLocalizationsBlock>()
                    .Add<PersistSellableItemBlock>())
                .AddPipeline<IImportPublicLinkEntityPipeline, ImportPublicLinkEntityPipeline>(p => p
                    .Add<FetchPublicLinkAssetEntitiesBlock>())
                .AddPipeline<IImportAssetEntityPipeline, ImportAssetEntityPipeline>(p => p
                    .Add<FetchAssetProductEntitiesBlock>())
                .ConfigurePipeline<IGetEntityViewPipeline>(p => p
                    .Add<GetSellableItemContentHubImagesViewBlock>().After<GetSellableItemImagesViewBlock>())
            );

            services.RegisterAllCommands(assembly);
        }
    }
}
