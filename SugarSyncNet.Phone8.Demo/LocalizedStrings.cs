using SugarSyncNet.Phone8.Demo.Resources;

namespace SugarSyncNet.Phone8.Demo
{
    /// <summary>
    /// Enables access to string resources.
    /// </summary>
    public class LocalizedStrings
    {
        /// <summary>
        /// The localized resources
        /// </summary>
        private static AppResources localizedResources = new AppResources();

        /// <summary>
        /// Gets the localized resources
        /// </summary>
        public AppResources LocalizedResources
        {
            get { return localizedResources; }
        }
    }
}