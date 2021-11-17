using Sitecore.Commerce.Core;

namespace Sitecore.Commerce.Plugin.BusinessTools.Entities
{
    public class BusinessToolsHelp : CommerceEntity
    {
        public string EntityName { get; set; }
        public string EntityViewName { get; set; }
        public string HelpName { get; set; }
        public string HelpDescription { get; set; }
    }
}