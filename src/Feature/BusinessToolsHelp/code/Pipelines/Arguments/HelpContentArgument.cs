using Sitecore.Commerce.Core;

namespace Sitecore.Commerce.Plugin.BusinessTools.Pipelines.Arguments
{
    /// <summary>
    ///     Class HelpProfileContentArgument.
    /// </summary>
    public class HelpContentArgument : PipelineArgument
    {
        /// <summary>
        ///     Gets or sets the help.
        /// </summary>
        /// <value>The help.</value>
        public Entities.BusinessToolsHelp Help { get; set; }
    }
}