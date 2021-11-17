// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System;
using Microsoft.AspNet.OData.Builder;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    /// Defines a block which configures the OData model for the <see cref="Plugin.Sample.ContentHub"/> plugin
    /// </summary>
    [PipelineDisplayName(ContentHubConstants.ConfigureServiceApiBlock)]
    public class ConfigureServiceApiBlock : SyncPipelineBlock<ODataConventionModelBuilder, ODataConventionModelBuilder, CommercePipelineExecutionContext>
    {
        /// <summary>
        ///     Runs the specified model builder.
        /// </summary>
        /// <param name="arg">The model builder.</param>
        /// <param name="context">The context.</param>
        /// <returns>The <see cref="ODataConventionModelBuilder"/> with new types.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="arg"/> is <see langword="null"/>.
        /// </exception>
        public override ODataConventionModelBuilder Run(ODataConventionModelBuilder arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg, nameof(arg)).IsNotNull();

            var uri = arg.ComplexType<Uri>();
            uri.Name = "OdataUri";
            uri.Property(u => u.AbsoluteUri);
            uri.Property(u => u.OriginalString);
            uri.Property(u => u.PathAndQuery);

            var imageComponent = arg.EntityType<ContentHubImage>();
            imageComponent.Property(c => c.Width);
            imageComponent.Property(c => c.Height);
            imageComponent.Property(c => c.IsMaster);
            imageComponent.Property(c => c.AlternateText);
            imageComponent.ComplexProperty(c => c.Url);

            return arg;
        }
    }
}
