// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Conditions;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    ///     Defines the <see cref="Policy"/> to work with Azure Service Bus.
    /// </summary>
    public class AzureServiceBusPolicy : Policy
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AzureServiceBusPolicy" /> class.
        /// </summary>
        public AzureServiceBusPolicy()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureServiceBusPolicy"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string to connect to Azure Service Bus.</param>
        /// <param name="subscriptionName">The name of Azure Service Bus subscription.</param>
        /// <param name="topicName">The topic name for incoming messages.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="connectionString"/>,
        ///     <paramref name="subscriptionName"/>, or <paramref name="topicName"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="connectionString"/>,
        /// <paramref name="subscriptionName"/>, or <paramref name="topicName"/> is <see cref="string.Empty"/>
        ///     or contains whitespace characters only.
        /// </exception>
        public AzureServiceBusPolicy(string connectionString, string subscriptionName,
            string topicName)
        {
            Condition.Requires(connectionString, nameof(connectionString)).IsNotNullOrWhiteSpace();
            Condition.Requires(subscriptionName, nameof(subscriptionName)).IsNotNullOrWhiteSpace();
            Condition.Requires(topicName, nameof(topicName)).IsNotNullOrWhiteSpace();

            ConnectionString = connectionString;
            SubscriptionName = subscriptionName;
            TopicName = topicName;
        }

        /// <summary>
        ///     Gets the connection string to connect to Azure Service Bus.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        ///     Gets the topic name for incoming messages.
        /// </summary>
        public string TopicName { get; set; }

        /// <summary>
        ///     Gets the name of Azure Service Bus subscription.
        /// </summary>
        public string SubscriptionName { get; set; }
    }
}
