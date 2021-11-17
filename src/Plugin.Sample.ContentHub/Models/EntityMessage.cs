// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System;
using Sitecore.Framework.Conditions;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    ///     Represents a message that contains information about Content Hub entity, received as part of <see cref="ServiceBusMessage"/>.
    /// </summary>
    public class EntityMessage
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="EntityMessage"/> class.
        /// </summary>
        /// <param name="eventType">The type of the current event.</param>
        /// <param name="targetDefinition">The definition of the target Content Hub entity.</param>
        /// <param name="targetId">The id of the target Content Hub entity.</param>
        /// <param name="targetIdentifier">The identifier of the target Content Hub entity.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="eventType"/>, <paramref name="targetDefinition"/>, or <paramref name="targetIdentifier"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="eventType"/>, <paramref name="targetDefinition"/>, or <paramref name="targetIdentifier"/>
        ///     is <see cref="string.Empty"/> or contains only whitespace characters.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="targetId"/> is less than or equal to zero.
        /// </exception>
        public EntityMessage(string eventType, string targetDefinition, long targetId, string targetIdentifier)
        {
            Condition.Requires(eventType, nameof(eventType)).IsNotNullOrWhiteSpace();
            Condition.Requires(targetDefinition, nameof(targetDefinition)).IsNotNullOrWhiteSpace();
            Condition.Requires(targetIdentifier, nameof(targetIdentifier)).IsNotNullOrWhiteSpace();
            Condition.Requires(targetId, nameof(targetId)).IsGreaterThan(0);

            EventType = eventType;
            TargetDefinition = targetDefinition;
            TargetId = targetId;
            TargetIdentifier = targetIdentifier;
        }

        /// <summary>
        /// Gets the type of the current event.
        /// </summary>
        public string EventType { get; }

        /// <summary>
        /// Gets the definition of the target Content Hub entity.
        /// </summary>
        public string TargetDefinition { get; }

        /// <summary>
        /// Gets the id of the target Content Hub entity.
        /// </summary>
        public long TargetId { get; }

        /// <summary>
        /// Gets the identifier of the target Content Hub entity.
        /// </summary>
        public string TargetIdentifier { get; }
    }
}
