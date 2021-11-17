using Sitecore.Commerce.Core;

namespace Sitecore.Commerce.Plugin.BusinessTools.Policies
{
    /// <summary>
    ///     Class KnownHelpViewsPolicy.
    /// </summary>
    /// <seealso cref="Sitecore.Commerce.Core.Policy" />
    public class KnownHelpViewsPolicy : Policy
    {
        public string EntityName { get; set; } = nameof(EntityName);
        public string EntityViewName { get; internal set; } = nameof(EntityViewName);
        public string HelpDashboard { get; internal set; } = nameof(HelpDashboard);
        public string Help { get; internal set; } = nameof(Help);

        public string HelpItems { get; internal set; } = nameof(HelpItems);

        public string BusinessToolsHelpItems { get; internal set; } = nameof(BusinessToolsHelpItems);

        public string Helps { get; internal set; } = nameof(Helps);
        public string HelpName { get; set; } = nameof(HelpName);
        public string HelpDescription { get; set; } = nameof(HelpDescription);
        public string Details { get; internal set; } = nameof(Details);
        public string Master { get; internal set; } = nameof(Master);
        public string ToolsNavigation { get; internal set; } = nameof(ToolsNavigation);
    }
}