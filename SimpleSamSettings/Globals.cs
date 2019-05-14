using System;
using System.IO;

namespace SimpleSamSettings
{
    /// <summary>
    /// A settings file for all of your settings!
    /// </summary>
    public static class Globals
    {
        static Globals()
        {
            SettingsFileBasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PROVIDE_A_REAL_PATH");
        }

        /// <summary>
        /// Do not interact with the disk at all for any settings class
        ///
        /// All settings classes are new'd up and none are saved to disk
        /// </summary>
        public static bool NoPersistence { get; set; } // e.g. in a unit test

        /// <summary>
        /// Base path for settings file persistence (a company name or something)
        /// </summary>
        public static string SettingsFileBasePath { get; set; }
    }
}