namespace Automod
{
    /// <summary>
    /// For global variables that have to be seen/modified by multiple parts of the code.
    /// </summary>
    static class Globals
    {
        /// <summary>
        /// Used by the Channels, CreateChannel, and Program classes.
        /// Prevents the automatic channel deletion from deleting newly-created channels.
        /// </summary>
        public static bool deleteGuard;

        static Globals()
        {
            deleteGuard = false;
        }
    }
}
