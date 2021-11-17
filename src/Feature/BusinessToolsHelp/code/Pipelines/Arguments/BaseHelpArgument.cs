using Sitecore.Commerce.Core;

namespace Sitecore.Commerce.Plugin.BusinessTools.Pipelines.Arguments
{
   
    public class BaseHelpArgument : PipelineArgument
    {
     
        public BaseHelpArgument()
        {
            EntityName = string.Empty;
            EntityViewName = string.Empty;
            HelpDescription = string.Empty;
            HelpName = string.Empty;
        }

        public string EntityName { get; set; }
        public string EntityViewName { get; set; }
        public string HelpName { get; set; }
        public string HelpDescription { get; set; }

    }
}