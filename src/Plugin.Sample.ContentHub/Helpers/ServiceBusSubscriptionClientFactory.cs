// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System;
using Microsoft.Azure.ServiceBus;
using Sitecore.Framework.Conditions;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    /// Represents a factory to create <see cref="ISubscriptionClient"/> and <see cref="ITopicClient"/> instances.
    /// </summary>
    public class ServiceBusSubscriptionClientFactory
    {
        /// <summary>
        /// Initializes a new concrete implementation of the <see cref="ISubscriptionClient"/> interface.
        /// </summary>
        /// <param name="connectionString">The connection string to connect to Azure Service Bus.</param>
        /// <param name="topicName">The name of the topic to connect to.</param>
        /// <param name="subscriptionName">The name of the subscription to connect to.</param>
        /// <returns>New <see cref="ISubscriptionClient"/>.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="connectionString"/>, <paramref name="topicName"/>, or
        ///     <paramref name="subscriptionName"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="connectionString"/>, <paramref name="topicName"/>, or
        ///     <paramref name="subscriptionName"/> is <see cref="string.Empty"/> or contains only whitespace characters.
        /// </exception>
        public virtual ISubscriptionClient CreateSubscriptionClient(string connectionString, string topicName, string subscriptionName)
        {
            Condition.Requires(connectionString, nameof(connectionString)).IsNotNullOrWhiteSpace();
            Condition.Requires(topicName, nameof(topicName)).IsNotNullOrWhiteSpace();
            Condition.Requires(subscriptionName, nameof(subscriptionName)).IsNotNullOrWhiteSpace();

            return new SubscriptionClient(connectionString, topicName, subscriptionName);
        }
    }
}
