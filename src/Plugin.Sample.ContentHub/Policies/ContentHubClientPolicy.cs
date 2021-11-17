// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using System;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Conditions;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    ///     Represents a <see cref="Policy"/> that contains connection information about target Content Hub instance.
    /// </summary>
    public class ContentHubClientPolicy : Policy
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ContentHubClientPolicy" /> class.
        /// </summary>
        public ContentHubClientPolicy()
        {
        }

#pragma warning disable CA1054 // Uri parameters should not be strings
        /// <summary>
        ///     Initializes a new instance of the <see cref="ContentHubClientPolicy"/> class.
        /// </summary>
        /// <param name="endpointUrl">The url to Content Hub endpoint.</param>
        /// <param name="userName">The username to use for authentication with Content Hub endpoint.</param>
        /// <param name="password">The password to use for authentication with Content Hub endpoint.</param>
        /// <param name="clientId">The client id to use for authentication with Content Hub endpoint.</param>
        /// <param name="clientSecret">The client secret to use for authentication with Content Hub endpoint.</param>
        /// <param name="knownSSORedirects">The known SSO redirect URls that must trigger security token refreshes.</param>
        /// <param name="timeoutInSeconds">The amount of seconds before requests to Content Hub endpoint time out.</param>
        /// <param name="retryCount">The amount of attempts to execute request to Content Hub endpoint before throwing error.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="endpointUrl"/>, <paramref name="userName"/>, <paramref name="password"/>, <paramref name="clientId"/>,
        ///     <paramref name="clientSecret"/>, or <paramref name="knownSSORedirects"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="endpointUrl"/>, <paramref name="userName"/>, <paramref name="password"/>, <paramref name="clientId"/>,
        ///     or <paramref name="clientSecret"/> is <see cref="string.Empty"/> or contains only whitespace characters.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="timeoutInSeconds"/> or <paramref name="timeoutInSeconds"/> is less than or equal to zero.
        /// </exception>
        public ContentHubClientPolicy(string endpointUrl, string userName, string password, string clientId,
#pragma warning restore CA1054 // Uri parameters should not be strings
            string clientSecret, string knownSSORedirects = "", int timeoutInSeconds = 90, int retryCount = 3)
        {
            Condition.Requires(endpointUrl, nameof(endpointUrl)).IsNotNullOrWhiteSpace();
            Condition.Requires(userName, nameof(userName)).IsNotNullOrWhiteSpace();
            Condition.Requires(password, nameof(password)).IsNotNullOrWhiteSpace();
            Condition.Requires(clientId, nameof(clientId)).IsNotNullOrWhiteSpace();
            Condition.Requires(clientSecret, nameof(clientSecret)).IsNotNullOrWhiteSpace();
            Condition.Requires(knownSSORedirects, nameof(knownSSORedirects)).IsNotNull();
            Condition.Requires(timeoutInSeconds, nameof(timeoutInSeconds)).IsGreaterThan(0);
            Condition.Requires(retryCount, nameof(retryCount)).IsGreaterThan(0);

            EndpointUrl = endpointUrl;
            UserName = userName;
            Password = password;
            ClientId = clientId;
            ClientSecret = clientSecret;
            KnownSSORedirects = knownSSORedirects;
            TimeoutInSeconds = timeoutInSeconds;
            RetryCount = retryCount;
        }

#pragma warning disable CA1056 // Uri properties should not be strings
        /// <summary>
        /// Gets the url to Content Hub endpoint.
        /// </summary>
        public string EndpointUrl { get; set; }
#pragma warning restore CA1056 // Uri properties should not be strings

        /// <summary>
        /// Gets the username to use for authentication with Content Hub endpoint.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets the password to use for authentication with Content Hub endpoint.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets the client id to use for authentication with Content Hub endpoint.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets the client secret to use for authentication with Content Hub endpoint.
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets the known SSO redirect URls that must trigger security token refreshes.
        /// </summary>
        public string KnownSSORedirects { get; set; }

        /// <summary>
        /// Gets the amount of seconds before requests to Content Hub endpoint time out.
        /// </summary>
        public int TimeoutInSeconds { get; set; }

        /// <summary>
        /// Gets the amount of attempts to execute request to Content Hub endpoint before throwing error.
        /// </summary>
        public int RetryCount { get; set; }
    }
}
