namespace Sitecore.Commerce.Plugin.BusinessTools.Pipelines.Arguments
{
    /// <summary>
    ///     Class EditHelpArgument.
    /// </summary>
    /// <seealso cref="BaseHelpArgument" />
    public class EditHelpArgument : BaseHelpArgument
    {
        /// <summary>
        ///     Gets or sets the help.
        /// </summary>
        /// <value>The help.</value>
        public Entities.BusinessToolsHelp Help { get; set; }

        /// <summary>
        ///     Gets or sets the entity identifier.
        /// </summary>
        /// <value>The entity identifier.</value>
        public string EntityId { get; set; }
    }
}