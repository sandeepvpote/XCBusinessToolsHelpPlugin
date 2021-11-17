using Sitecore.Commerce.Core;

namespace Sitecore.Commerce.Plugin.CommerceExtension
{
    /// <summary>
    /// Class CommerceContextHelper.
    /// </summary>
    public static class CommerceContextHelper
    {
        /// <summary>
        /// Aborts the execution.
        /// </summary>
        /// <param name="commerceContext">The commerce context.</param>
        /// <param name="commerceTermKey">The commerce term key.</param>
        /// <param name="defaultMessage">The default message.</param>
        /// <param name="name">The name.</param>
        public static void AbortExecution(CommerceContext commerceContext, string commerceTermKey,
            string defaultMessage, string name)
        {
            var executionContext = commerceContext.PipelineContext;
            var validationError = executionContext.GetPolicy<KnownResultCodes>().ValidationError;
            var args = new object[1]
            {
                name
            };
            executionContext.Abort(
                commerceContext.AddMessage(validationError, commerceTermKey, args, defaultMessage).Result,
                executionContext);
            executionContext = null;
        }

        //public static bool CheckEnvironment(CommerceContext commerceContext)
        //{
        //    return Constants.EnvironmentName.Select(name => commerceContext.Environment.Name.Contains(name)).Any(exists => exists);
        //}
    }
}
