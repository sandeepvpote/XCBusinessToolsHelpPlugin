// © 2020 Sitecore Corporation A/S. All rights reserved. Sitecore® is a registered trademark of Sitecore Corporation A/S.

using Sitecore.Commerce.Core;

namespace Plugin.Sample.ContentHub
{
    /// <summary>
    /// Extension methods for <see cref="CommerceEnvironment"/>
    /// </summary>
    public static class EnvironmentExtensions
    {
        /// <summary>
        /// Check if and specific <see cref="Policy"/> is configured on the <see cref="CommerceEnvironment"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="Policy"/> type.</typeparam>
        /// <param name="environment">The <see cref="CommerceEnvironment"/> to check.</param>
        /// <returns>True if the <see cref="CommerceEnvironment"/> contains the <see cref="Policy"/>, otherwise false.</returns>
        public static bool IsPolicyConfigured<T>(this CommerceEnvironment environment) where T : Policy
        {
            return environment.GetConfiguredPolicy<T>() != null;
        }

        /// <summary>
        /// Get the configured <see cref="Policy"/> from the <see cref="CommerceEnvironment"/>
        /// </summary>
        /// <typeparam name="T">The <see cref="Policy"/> type.</typeparam>
        /// <param name="environment">The <see cref="CommerceEnvironment"/> to check.</param>
        /// <returns>The <see cref="Policy"/></returns>
        public static T GetConfiguredPolicy<T>(this CommerceEnvironment environment) where T : Policy
        {
            if (environment.HasPolicy<T>())
            {
                return environment.GetPolicy<T>();
            }

            if (environment.HasComponent<PolicySetsComponent>() && environment.GetComponent<PolicySetsComponent>().HasPolicy<T>())
            {
                return environment.GetComponent<PolicySetsComponent>().GetPolicy<T>();
            }

            return null;
        }
    }
}
