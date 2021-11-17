// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    ///     Represents a message that contains information about deleted
    ///     Content Hub entity, received as part of <see cref="ServiceBusMessage"/>.
    /// </summary>
    public class DeleteEntityMessage : EntityMessage
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DeleteEntityMessage"/> class.
        /// </summary>
        /// <param name="eventType">The type of the current event.</param>
        /// <param name="targetDefinition">The definition of the target Content Hub entity.</param>
        /// <param name="targetId">The id of the target Content Hub entity.</param>
        /// <param name="targetIdentifier">The identifier of the target Content Hub entity.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="eventType"/>, <paramref name="targetDefinition"/>, or <paramref name="targetIdentifier"/>is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="eventType"/>, <paramref name="targetDefinition"/>, or <paramref name="targetIdentifier"/>
        ///     is <see cref="string.Empty"/> or contains only whitespace characters.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="targetId"/> is less than or equal to zero.
        /// </exception>
        public DeleteEntityMessage(string eventType, string targetDefinition, long targetId, string targetIdentifier)
            : base(eventType, targetDefinition, targetId, targetIdentifier)
        {
        }
    }
}
