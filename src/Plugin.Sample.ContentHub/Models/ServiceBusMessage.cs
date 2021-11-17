// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using Newtonsoft.Json;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    /// Represents a message received from Azure Service Bus.
    /// </summary>
    public class ServiceBusMessage
    {
        /// <summary>
        ///     Gets the definition of the target Content Hub entity.
        /// </summary>
        public string TargetDefinition => EntityMessage.TargetDefinition;

        /// <summary>
        ///     Gets the id of the target Content Hub entity.
        /// </summary>
        public long TargetId => EntityMessage.TargetId;

        /// <summary>
        ///     Gets, Sets the <see cref="ContentHub.EntityMessage"/> that contains information
        ///     about changed Content Hub entity.
        /// </summary>
        public EntityMessage EntityMessage { get; set; }

        /// <summary>
        ///     Gets the <see cref="ContentHub.SaveEntityMessage"/> that contains information
        ///     about changed Content Hub entity.
        /// </summary>
        [JsonProperty("saveEntityMessage")]
        public SaveEntityMessage SaveEntityMessage
        {
            get => EntityMessage as SaveEntityMessage;
            set => EntityMessage = value;
        }

        /// <summary>
        ///     Gets the <see cref="ContentHub.SaveEntityMessage"/> that contains information
        ///     about changed Content Hub entity.
        /// </summary>
        [JsonProperty("deleteEntityMessage")]
        public DeleteEntityMessage DeleteEntityMessage
        {
            get => EntityMessage as DeleteEntityMessage;
            set => EntityMessage = value;
        }

        /// <summary>
        ///     Gets the context of the current <see cref="ServiceBusMessage"/>.
        /// </summary>
        [JsonProperty("context")]
        public ServiceBusMessageContext Context { get; set; }
    }
}
