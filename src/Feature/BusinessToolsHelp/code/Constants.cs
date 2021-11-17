namespace Sitecore.Commerce.Plugin.BusinessTools
{
    public class Constants
    {
        public static class Pipelines
        {
            public static class Block
            {
                public const string DashboardIcon = "id_cards";

                public const string AddIcon = "add";
                public const string EditIcon = "edit";

                public const int DisplayRank = 1;

                public const string ArgumentNullMessage = "Argument cannot be null.";

                public static class Fields
                {
                    public const string EmailInvalid = nameof(EmailInvalid);
                }
            }
        }
    }
}