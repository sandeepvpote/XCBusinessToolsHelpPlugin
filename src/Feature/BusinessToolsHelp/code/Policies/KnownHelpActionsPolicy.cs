using Sitecore.Commerce.Core;

namespace Sitecore.Commerce.Plugin.BusinessTools.Policies
{
    /// <summary>
    ///     Class KnownHelpActionsPolicy.
    /// </summary>
    /// <seealso cref="Sitecore.Commerce.Core.Policy" />
    public class KnownHelpActionsPolicy : Policy
    {
        /// <summary>
        ///     Gets the add help profile.
        /// </summary>
        /// <value>The add help profile.</value>
        public string AddHelp { get; internal set; } = nameof(AddHelp);

        /// <summary>
        ///     Gets the edit help profile.
        /// </summary>
        /// <value>The edit help profile.</value>
        public string EditHelp { get; internal set; } = nameof(EditHelp);
    }
}