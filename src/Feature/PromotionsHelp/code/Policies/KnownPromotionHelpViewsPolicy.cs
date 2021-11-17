using Sitecore.Commerce.Core;

namespace Sitecore.Commerce.Plugin.PromotionsHelp.Policies
{
    public class KnownPromotionHelpViewsPolicy : Policy
    {
        public string PromotionListHelp { get; internal set; } = nameof(PromotionListHelp);


    }
}
