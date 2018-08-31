namespace Espresso
{

    /// <summary>
    /// Main library entry point.
    /// </summary>
    public static class Entry
    {

        static readonly object sync = new object();

        /// <summary>
        /// Runs an instance.
        /// </summary>
        public static void Run()
        {
            lock (sync)
                RunMain();
        }

        /// <summary>
        /// Dispatches to the main method.
        /// </summary>
        static void RunMain()
        {
            Native.main(2, new string[] { "espresso", "-h" });
        }

    }

}
