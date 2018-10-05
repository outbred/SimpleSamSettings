using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace SimpleSamSettings
{
    /// <summary>
    /// Persists a settings file using the BinaryFormatter
    /// </summary>
    public static class BinaryPersistor
    {
        /// <summary>
        ///     Saves the settingsObjects provided out to disk, if possible.
        /// </summary>
        /// <typeparam name="TSettings">The settings object type to be saved</typeparam>
        /// <param name="instance">Any serializable object</param>
        /// <param name="fileExistsAlready">Lets the user know if the settings file was overwritten</param>
        /// <param name="nameMarker">
        ///     Additional filename specifier for the case where many of the same types of settings are used
        ///     for the same instance in the same class
        /// </param>
        /// <param name="overwrite">If the file already exists, must be true to persist the current settings.</param>
        /// <returns>True if successful, false if not</returns>
        public static bool Save<TSettings>(TSettings instance, out bool fileExistsAlready, string nameMarker = null, bool overwrite = true)
            where TSettings : class
        {
            Contract.Requires(typeof(TSettings).IsSerializable);
            Contract.Requires(instance != null);
            fileExistsAlready = false;
            if (SettingsGlobals.NoPersistence)
                return false;

            try
            {
                // get the filename 
                var fileName = BuildFileName<TSettings>(nameMarker);
                fileExistsAlready = File.Exists(fileName);
                if (fileExistsAlready && !overwrite) return false;

                if (!fileExistsAlready)
                    if (!Directory.Exists(Path.GetDirectoryName(fileName)))
                        Directory.CreateDirectory(Path.GetDirectoryName(fileName));

                IFormatter formatter = new BinaryFormatter();
                // serialize the instance into the file; save in local user appdata dir
                using (var stream = new FileStream(fileName + @".new", FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    formatter.Serialize(stream, instance);
                }

                if (fileExistsAlready) 
                { 
                    try  { File.Delete(fileName); } 
                    catch (Exception) { return false; }
                }

                File.Move(fileName + @".new", fileName);

                return true;
            }
            catch (Exception)
            {
                if (Debugger.IsAttached)
                    Debugger.Break();

                return false;
            }
        }

        /// <summary>
        ///     Retrieves the settings object specified, if it exists on disk.
        /// </summary>
        /// <typeparam name="TCallingClass">The calling class's type - used to generate a unique settings filename</typeparam>
        /// <typeparam name="TSettings">The settings object type to be retrieved</typeparam>
        /// <param name="result">The settings object to be returned</param>
        /// <param name="nameMarker">
        ///     Additional filename specifier for the case where many of the same types of settings are used
        ///     for the same instance in the same class
        /// </param>
        /// <returns>True if retrieved from file, false if empty object</returns>
        public static bool Retrieve<TSettings>(out TSettings result, string nameMarker = null)
            where TSettings : class, new()
        {
            Contract.Requires(typeof(TSettings).IsSerializable);
            Contract.Ensures(result != null);
            if (SettingsGlobals.NoPersistence)
            {
                result = new TSettings();
                return false;
            }

            try
            {
                var fileName = BuildFileName<TSettings>(nameMarker);
                if (File.Exists(fileName))
                {
                    IFormatter formatter = new BinaryFormatter();
                    using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                        result = formatter.Deserialize(stream) as TSettings;

                    return true;
                }

                result = new TSettings();
                return false;
            }
            catch (Exception)
            {
                try
                {
                    var fileName = BuildFileName<TSettings>(nameMarker);
                    if (File.Exists(fileName))
                        File.Delete(fileName);
                }
                catch { }

                result = new TSettings();
                return false;
            }
        }

        private static string BuildFileName<TSettingsClass>(string nameMarker = null)
            where TSettingsClass : class
        {
            Contract.Ensures(Contract.Result<string>() != null);
            // create the file name: "(CallingClass.Name).(SettingsClass.Name).nameMarker (if supplied).settings"
            string fileName = null;
            if (!string.IsNullOrWhiteSpace(nameMarker))
                fileName = $"{typeof(TSettingsClass).Name}.{nameMarker}.settings";
            else
                fileName = $"{typeof(TSettingsClass).Name}.settings";

            return Path.Combine(SettingsGlobals.SettingsFileBasePath, fileName);
        }
    }
}