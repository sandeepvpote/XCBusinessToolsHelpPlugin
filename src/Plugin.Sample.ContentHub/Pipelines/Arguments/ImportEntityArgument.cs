// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Conditions;
using Stylelabs.M.Sdk.Contracts.Base;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    ///     Represents a <see cref="PipelineArgument"/> for <see cref="IImportEntityPipeline"/>.
    /// </summary>
    public class ImportEntityArgument : PipelineArgument
    {
        /// <summary>
        /// Initializes a new an instance of the <see cref="ImportEntityArgument"/> class.
        /// </summary>
        /// <param name="message">The <see cref="ServiceBusMessage"/> that contains information about changed entity.</param>
        /// <param name="clientPolicy">The <see cref="ContentHubClientPolicy"/> that contains connection information for the target Content Hub instance.</param>
        /// <param name="synchronizationPolicy">The <see cref="ContentHub.SynchronizationPolicy"/> that contains basic information needed to synchronize data from Content Hub.</param>
        /// <param name="productSynchronizationPolicy">The <see cref="ContentHub.ProductSynchronizationPolicy"/> that contains information needed to synchronize product data from Content Hub.</param>
        /// <param name="assetSynchronizationPolicy">The <see cref="ContentHub.AssetSynchronizationPolicy"/> that contains information needed to synchronize asset data from Content Hub.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="message"/>, <paramref name="clientPolicy"/>, or <paramref name="synchronizationPolicy"/>
        ///     is <see langword="null"/>.
        /// </exception>
        public ImportEntityArgument(ServiceBusMessage message, ContentHubClientPolicy clientPolicy,
            SynchronizationPolicy synchronizationPolicy, ProductSynchronizationPolicy productSynchronizationPolicy, AssetSynchronizationPolicy assetSynchronizationPolicy)
        {
            Condition.Requires(message, nameof(message)).IsNotNull();
            Condition.Requires(clientPolicy, nameof(clientPolicy)).IsNotNull();
            Condition.Requires(synchronizationPolicy, nameof(synchronizationPolicy)).IsNotNull();

            Message = message;
            ClientPolicy = clientPolicy;
            SynchronizationPolicy = synchronizationPolicy;
            ProductSynchronizationPolicy = productSynchronizationPolicy;
            AssetSynchronizationPolicy = assetSynchronizationPolicy;
            TargetId = message.EntityMessage.TargetId;
            TargetIdentifier = message.EntityMessage.TargetIdentifier;
            EventType = message.EntityMessage.EventType;
            Version = message.SaveEntityMessage?.Version ?? 0;
            EditedCultures = message.SaveEntityMessage?.ChangeSet.Cultures.ToList() ?? new List<string>();

            // Add default culture to properties so most blocks will not have to ensure key exists
            Properties.Add(SynchronizationPolicy.DefaultCulture, new Dictionary<string, string>());
        }

        /// <summary>
        /// The <see cref="ServiceBusMessage"/> received.
        /// </summary>
        public ServiceBusMessage Message { get; }

        /// <summary>
        /// Gets the <see cref="ContentHubClientPolicy"/> that contains information about target Content Hub instance.
        /// </summary>
        public ContentHubClientPolicy ClientPolicy { get; }

        /// <summary>
        /// Gets the <see cref="ContentHub.SynchronizationPolicy"/> that contains basic information about synchronizing
        /// Commerce Engine with Content Hub.
        /// </summary>
        public SynchronizationPolicy SynchronizationPolicy { get; }

        /// <summary>
        /// Gets the id of the target Content Hub entity.
        /// </summary>
        public long TargetId { get; }

        /// <summary>
        /// Gets the identifier of the target Content Hub entity.
        /// </summary>
        public string TargetIdentifier { get; }

        /// <summary>
        /// Gets the type of the current event.
        /// </summary>
        public string EventType { get; }

        /// <summary>
        /// Gets the version of the Content Hub entity.
        /// </summary>
        public int Version { get; }

        /// <summary>
        /// Gets or sets the Content Hub <see cref="IEntity"/>.
        /// </summary>
        public IEntity Entity { get; set; }

        /// <summary>
        /// Gets or sets the assets related to <see cref="Entity"/>.
        /// </summary>
        public IList<IEntity> Assets { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ContentHub.ContentHubImagesComponent"/> that contains image links with additional metadata.
        /// </summary>
        public ContentHubImagesComponent ContentHubImagesComponent { get; set; }

        /// <summary>
        /// Gets or sets the properties related to <see cref="Entity"/>.
        /// </summary>
        public IDictionary<string, IDictionary<string, string>> Properties { get; set; } = new Dictionary<string, IDictionary<string, string>>();

        /// <summary>
        ///     Gets or sets the <see cref="IDictionary{TKey, TValue}"/>, where key is a Public Link
        ///     and value is a corresponding image.
        /// </summary>
        public IDictionary<IEntity, IEntity> PublicLinkToImageMappings { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Sitecore.Commerce.Core.CommerceEntity"/> that corresponds to current <see cref="Entity"/>.
        /// </summary>
        public CommerceEntity CommerceEntity { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Sitecore.Commerce.Core.LocalizationEntity"/> for <see cref="ImportEntityArgument.CommerceEntity"/>.
        /// </summary>
        public LocalizationEntity LocalizationEntity { get; set; }

        /// <summary>
        /// Gets or sets the list of cultures affected by the Content Hub update.
        /// </summary>
        public IList<string> EditedCultures { get; set; }

        /// <summary>
        /// Gets or sets a value that reflects whether this update includes localizations.
        /// </summary>
        public bool LocalizationsUpdated { get; set; }

        /// <summary>
        /// Gets the <see cref="ContentHub.ProductSynchronizationPolicy"/> containing information used to synchronize product data.
        /// </summary>
        public ProductSynchronizationPolicy ProductSynchronizationPolicy { get; }

        /// <summary>
        /// Gets the <see cref="ContentHub.AssetSynchronizationPolicy"/> containing information used to synchronize asset data.
        /// </summary>
        public AssetSynchronizationPolicy AssetSynchronizationPolicy { get; }
    }
}
