// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System;
using Sitecore.Framework.Conditions;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    ///     Represents a message that contains information about changed
    ///     Content Hub entity, received as part of <see cref="ServiceBusMessage"/>.
    /// </summary>
    public class SaveEntityMessage : EntityMessage
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SaveEntityMessage"/> class.
        /// </summary>
        /// <param name="eventType">The type of the current event.</param>
        /// <param name="targetDefinition">The definition of the target Content Hub entity.</param>
        /// <param name="targetId">The id of the target Content Hub entity.</param>
        /// <param name="targetIdentifier">The identifier of the target Content Hub entity.</param>
        /// <param name="isNew">Specifies, whether the Content Hub entity is new or not.</param>
        /// <param name="timestamp">The timestamp of when event has happened.</param>
        /// <param name="userId">The id of the user, who made the changes.</param>
        /// <param name="version">The version of the Content Hub entity.</param>
        /// <param name="changeSet">The <see cref="ChangeSet"/> with details of changed properties.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="eventType"/>, <paramref name="targetDefinition"/>, <paramref name="targetIdentifier"/>,
        ///     or <paramref name="changeSet"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="eventType"/>, <paramref name="targetDefinition"/>, or <paramref name="targetIdentifier"/>
        ///     is <see cref="string.Empty"/> or contains only whitespace characters.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="targetId"/>, <paramref name="userId"/>, or <paramref name="version"/>
        ///     is less than or equal to zero.
        /// </exception>
        public SaveEntityMessage(string eventType, string targetDefinition, long targetId, string targetIdentifier,
            bool isNew, DateTime timestamp, int userId, int version, Changeset changeSet)
            : base(eventType, targetDefinition, targetId, targetIdentifier)
        {
            Condition.Requires(changeSet, nameof(changeSet)).IsNotNull();
            Condition.Requires(userId, nameof(userId)).IsGreaterThan(0);
            Condition.Requires(version, nameof(version)).IsGreaterThan(0);

            IsNew = isNew;
            Timestamp = timestamp;
            UserId = userId;
            Version = version;
            ChangeSet = changeSet;
        }

        /// <summary>
        /// Gets the value that specifies, whether the Content Hub entity is new or not.
        /// </summary>
        public bool IsNew { get; }

        /// <summary>
        /// Gets the timestamp of when event has happened.
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// Gets the id of the user, who made the changes.
        /// </summary>
        public int UserId { get; }

        /// <summary>
        /// Gets the version of the Content Hub entity.
        /// </summary>
        public int Version { get; }

        /// <summary>
        /// Gets the <see cref="ChangeSet"/> with details of changed properties.
        /// </summary>
        public Changeset ChangeSet { get; }
    }
}
