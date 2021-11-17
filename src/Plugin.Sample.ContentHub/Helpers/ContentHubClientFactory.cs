// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System;
using System.Linq;
using Sitecore.Framework.Conditions;
using Stylelabs.M.Sdk.WebClient;
using Stylelabs.M.Sdk.WebClient.Authentication;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    /// Represents a factory to create <see cref="IWebMClient"/> instances.
    /// </summary>
    public class ContentHubClientFactory
    {
        /// <summary>
        ///     Gets or sets amount of retries that should be done by any
        ///     <see cref="IWebMClient"/> created by <see cref="ContentHubClientFactory"/>.
        /// </summary>
        public static int RetryCount
        {
            get => WebDefaults.RetryPolicy.RetryCount;
            set => WebDefaults.RetryPolicy.RetryCount = value;
        }

        /// <summary>
        ///     Gets or sets a <see cref="TimeSpan"/> before requests done by any
        ///     <see cref="IWebMClient"/> created by <see cref="ContentHubClientFactory"/> fail with timeout.
        /// </summary>
        public static TimeSpan Timeout
        {
            get => WebDefaults.TimeoutPolicy.Timeout;
            set => WebDefaults.TimeoutPolicy.Timeout = value;
        }

        /// <summary>
        ///     Creates the <see cref="IWebMClient"/> using information from <paramref name="policy"/>.
        /// </summary>
        /// <param name="policy">
        ///     The <see cref="ContentHubClientPolicy"/> that contains information to instantiate <see cref="IWebMClient"/>.
        /// </param>
        /// <returns>An instance of <see cref="IWebMClient"/>.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="policy"/> is <see langword="null"/>.
        /// </exception>
        public virtual IWebMClient CreateClient(ContentHubClientPolicy policy)
        {
            Condition.Requires(policy, nameof(policy)).IsNotNull();

            var auth = new OAuthPasswordGrant
            {
                ClientId = policy.ClientId,
                ClientSecret = policy.ClientSecret,
                UserName = policy.UserName,
                Password = policy.Password
            };

            var client = MClientFactory.CreateMClient(new Uri(policy.EndpointUrl), auth);

            if (!string.IsNullOrEmpty(policy.KnownSSORedirects))
            {
                client.SetKnownSSoRedirects(policy.KnownSSORedirects.Split(';').Select(s => new Uri(s)));
            }

            return client;
        }
    }
}
