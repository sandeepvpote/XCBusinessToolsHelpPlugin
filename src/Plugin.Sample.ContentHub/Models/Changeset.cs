// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System;
using System.Collections.Generic;
using Sitecore.Framework.Conditions;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    /// Represents a set of changes received as part of <see cref="SaveEntityMessage"/>.
    /// </summary>
    public class Changeset
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Changeset"/> class.
        /// </summary>
        /// <param name="propertyChanges">The collection of changed properties.</param>
        /// <param name="cultures">The collection of the cultures within changed Content Hub entity.</param>
        /// <param name="relationChanges">The collection of changes in entity's relationships.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="propertyChanges"/>, <paramref name="cultures"/>, or <paramref name="relationChanges"/> is <see langword="null"/>.
        /// </exception>
        public Changeset(IReadOnlyList<PropertyChange> propertyChanges, IReadOnlyList<string> cultures,
            IReadOnlyList<RelationChange> relationChanges)
        {
            Condition.Requires(propertyChanges, nameof(propertyChanges)).IsNotNull();
            Condition.Requires(cultures, nameof(cultures)).IsNotNull();
            Condition.Requires(relationChanges, nameof(relationChanges)).IsNotNull();

            PropertyChanges = propertyChanges;
            Cultures = cultures;
            RelationChanges = relationChanges;
        }

        /// <summary>
        /// Gets the collection of changed properties.
        /// </summary>
        public IReadOnlyList<PropertyChange> PropertyChanges { get; }

        /// <summary>
        /// Gets the collection of the cultures within changed Content Hub entity.
        /// </summary>
        public IReadOnlyList<string> Cultures { get; }

        /// <summary>
        /// Gets the collection of changes in entity's relationships.
        /// </summary>
        public IReadOnlyList<RelationChange> RelationChanges { get; }
    }
}
