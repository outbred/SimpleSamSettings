using System;
using System.Diagnostics.Contracts;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SimpleSamSettings
{
    /// <summary>
    /// Persists a file using the NewtonSoft JsonConverter in JSON
    /// </summary>
    public static class JsonPersistor
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
            where TSettings : class, ISettingsBase
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

                if (fileExistsAlready)
                {
                    try
                    {
                        File.Delete(fileName);
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Directory.Exists(Path.GetDirectoryName(fileName))) Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                }

                var memoryTraceWriter = new MemoryTraceWriter();
                var json = JsonConvert.SerializeObject(instance, Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        TraceWriter = memoryTraceWriter,
                        TypeNameHandling = TypeNameHandling.Auto,
                        TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                        PreserveReferencesHandling = PreserveReferencesHandling.All
                    });
                File.WriteAllText(fileName, json);

                return true;
            }
            catch (Exception ex)
            {
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
            where TSettings : class, ISettingsBase, new()
        {
            Contract.Ensures(result != null);
            if (SettingsGlobals.NoPersistence)
            {
                result = new TSettings();
                return false;
            }

            // build the file name
            try
            {
                var fileName = BuildFileName<TSettings>(nameMarker);
                if (File.Exists(fileName))
                {
                    result = new TSettings();

                    string text = null;
                    text = File.ReadAllText(fileName);
                    result = JsonConvert.DeserializeObject<TSettings>(text,
                        new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.Auto,
                            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                            PreserveReferencesHandling = PreserveReferencesHandling.All,
                            CheckAdditionalContent = false
                        });

                    if (result == null)
                        result = new TSettings();
                    return true;
                }

                result = new TSettings();
                return false;
            }
            catch (Exception)
            {
                result = new TSettings();
                return false;
            }
        }

        public static string BuildFileName<TSettingsClass>(string nameMarker = null)
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