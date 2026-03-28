using BigProject.Systems;
using BigProject.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BigProject.Managers
{
    /// <summary>
    /// Saves and loads ISavable collection data.
    /// </summary>
    public class SavesManager
    {
        /// <summary>
        /// Stores data from an ISavable collection.
        /// </summary>
        /// <returns>True when success.</returns>
        public bool SaveGame(string saveName, IEnumerable<ISavable> data)
        {
            ExceptionUtilities.ThrowIfNull(data, String.Format(LogStr.CRITICAL_NULL_REFERENCE, "Saves manager", "Savables collection"));
            List<string> jsonRecs = new();
            List<ISavable> successSaved = new();

            foreach (ISavable savable in data)
            {
                string jsonData = JsonUtility.ToJson(savable.SavingData);

                if (String.IsNullOrEmpty(jsonData))
                {
                    Debug.LogWarning(String.Format(LogStr.WARNING_SYSTEM, "SavesManager",
                        $"data with key {savable.Key} in save {saveName} is empty. It will be ignored"));
                    continue;
                }

                // Collect a string with the object's id and data and add it to the list.
                jsonData = $"[{savable.Key}]{jsonData}";
                jsonRecs.Add(jsonData);
                successSaved.Add(savable);
            }

            if (jsonRecs.Count == 0)
            {
                Debug.LogWarning(String.Format(LogStr.WARNING_SYSTEM, "SavesManager", "try to save empty data"));
                return false;
            }

            // Collect all records to one and save with one key.
            SaveStringsList(saveName, jsonRecs);

            foreach (ISavable savable in data)
            {
                savable.OnSaved(successSaved.Contains(savable));
            }

            return true;
        }

        /// <summary>
        /// Load data to ISavable collection.
        /// </summary>
        /// <returns>True when success.</returns>
        public bool LoadGame(string saveName, IEnumerable<ISavable> data)
        {
            ExceptionUtilities.ThrowIfNull(data, String.Format(LogStr.CRITICAL_NULL_REFERENCE, "Saves manager", "Savables collection"));
            string summaryData = PlayerPrefs.GetString(saveName);

            if (String.IsNullOrEmpty(summaryData))
            {
                Debug.LogWarning(String.Format(LogStr.WARNING_SYSTEM, "SavesManager", $"try to load non-existent save {saveName}"));
                return false;
            }

            if (GetJsonRecords(out Dictionary<string, string> jsonRecs, summaryData, saveName))
            {
                foreach (ISavable savable in data)
                {
                    if (!jsonRecs.ContainsKey(savable.Key))
                    {
                        Debug.LogWarning(String.Format(LogStr.WARNING_SYSTEM, "SavesManager",
                            $"try to load non-existent data with key {savable.Key} in save {saveName}. It will be ignored"));
                        continue;
                    }

                    // Write object's fields with data from the corresponding row.
                    JsonUtility.FromJsonOverwrite(jsonRecs[savable.Key], savable.SavingData);
                    savable.OnLoad();
                }

                Debug.Log(String.Format(LogStr.INFO_SYSTEM, "SavesManager", "Game progress loaded"));
                return true;
            }


            Debug.LogError(String.Format(LogStr.ERROR_SYSTEM, "SavesManager", $"Unable to load save {saveName}"));
            return false;
        }

        public bool HasSave(string saveName)
        {
            string summaryData = PlayerPrefs.GetString(saveName);

            if (String.IsNullOrEmpty(summaryData))
            {
                Debug.LogWarning(String.Format(LogStr.WARNING_SYSTEM, "SavesManager", $"checking save data for non-existent save {saveName}"));
                return false;
            }

            Debug.Log(String.Format(LogStr.INFO_SYSTEM, "SavesManager", $"found saves for {saveName}"));
            return true;
        }

        /// <summary>
        /// Add savable record to existent save or create new one.
        /// </summary>
        /// <returns>True when success.</returns>
        public bool AddToSave(string saveName, ISavable savable)
        {
            ExceptionUtilities.ThrowIfNull(saveName, String.Format(LogStr.CRITICAL_NULL_REFERENCE, "Saves manager", "Savable record"));
            string summaryData = PlayerPrefs.GetString(saveName);

            if (String.IsNullOrEmpty(summaryData))
            {
                return SaveGame(saveName, new[] { savable });
            }

            Dictionary<string, string> jsonRecs;

            if (GetJsonRecords(out jsonRecs, summaryData, saveName))
            {
                string record = JsonUtility.ToJson(savable.SavingData);

                if (jsonRecs.ContainsKey(savable.Key))
                {
                    jsonRecs[savable.Key] = record;
                }
                else
                {
                    jsonRecs.Add(savable.Key, record);
                }
            }
            else
            {
                Debug.LogWarning(String.Format(LogStr.WARNING_SYSTEM, "SavesManager", "unable to add record, save is broken"));
                savable.OnSaved(false);
                return false;
            }

            SaveStringsList(saveName, jsonRecs.Select(x => $"[{x.Key}]{x.Value}").ToList());
            savable.OnSaved(true);
            return true;
        }

        /// <summary>
        /// Load savable record data from existent save.
        /// </summary>
        /// <param name="removeAfterLoad">When true record will be removed after loading.</param>
        /// <param name="silent">When true errors and warning won't be trigger (usefull when not sure about data availability).</param>
        /// <returns>True when success.</returns>
        public bool LoadFromSave(string saveName, ISavable savable, bool removeAfterLoad, bool silent)
        {
            ExceptionUtilities.ThrowIfNull(saveName, String.Format(LogStr.CRITICAL_NULL_REFERENCE, "Saves manager", "Savable record"));
            string summaryData = PlayerPrefs.GetString(saveName);

            if (String.IsNullOrEmpty(summaryData))
            {
                Log(Debug.LogWarning, $"Try to load record from non-existent save {saveName}.", silent);
                return false;
            }

            if (GetJsonRecords(out Dictionary<string, string> jsonRecs, summaryData, saveName) &&
                jsonRecs.TryGetValue(savable.Key, out string data))
            {
                JsonUtility.FromJsonOverwrite(data, savable.SavingData);

                if (removeAfterLoad)
                {
                    jsonRecs.Remove(savable.Key);
                    SaveStringsList(saveName, jsonRecs.Select(x => $"[{x.Key}]{x.Value}").ToList());
                }

                Log(Debug.Log, String.Format(LogStr.INFO_SYSTEM, "SavesManager", $"{savable.Key} loaded from {saveName}"), silent);
                savable.OnLoad();
                return true;
            }

            Log(Debug.LogError, $"Unable to load record {savable.Key} from save {saveName}.", silent);
            return false;
        }

        public void DeleteSave(string saveName)
        {
            PlayerPrefs.DeleteKey(saveName);
            Debug.Log(String.Format(LogStr.INFO_SYSTEM, "SavesManager", $"save {saveName} deleted"));
        }

        /// <summary>
        /// Delete one record from save if exist.
        /// </summary>
        public void DeleteFromSave(string saveName, string key)
        {
            string summaryData = PlayerPrefs.GetString(saveName);

            if (!String.IsNullOrEmpty(summaryData) &&
                GetJsonRecords(out Dictionary<string, string> jsonRecs, summaryData, saveName) && jsonRecs.ContainsKey(key))
            {
                jsonRecs.Remove(key);
                SaveStringsList(saveName, jsonRecs.Select(x => $"[{x.Key}]{x.Value}").ToList());
            }
        }

        /// <summary>
        /// Creates a json dictionary of data entries.
        /// </summary>
        /// <param name="jsonRecs">Records dictionary</param>
        /// <param name="summaryData">Initial data</param>
        /// <returns>True when success.</returns>
        private bool GetJsonRecords(out Dictionary<string, string> jsonRecs, string summaryData, string saveName)
        {
            jsonRecs = new();

            // Go through all the rows with data.
            foreach (string jsonRec in summaryData.Split('\n'))
            {
                // Find the id (key) of the record.
                int keyStart = jsonRec.IndexOf('[') + 1;
                int keyEnd = jsonRec.IndexOf(']');

                if (keyStart == -1 || keyEnd == -1 || keyEnd < keyStart)
                {
                    Debug.LogWarning(String.Format(LogStr.WARNING_SYSTEM, "SavesManager",
                        $"incorrect record format in save {saveName}: {jsonRec}.\nIt will be ignored"));
                    continue;
                }

                string key = jsonRec[keyStart..keyEnd];

                // Check for duplicate.
                if (jsonRecs.ContainsKey(key))
                {
                    Debug.LogWarning(String.Format(LogStr.WARNING_SYSTEM, "SavesManager", $"duplicate key {key} in save {saveName}. It will be ignored"));
                    continue;
                }

                // Json substring.
                string jsonData = jsonRec[(keyEnd + 1)..];

                if (String.IsNullOrEmpty(jsonData))
                {
                    Debug.LogWarning(String.Format(LogStr.WARNING_SYSTEM, "SavesManager", $"empty entry with key {key} in save {saveName}. It will be ignored"));
                    continue;
                }

                jsonRecs.Add(key, jsonData);
            }

            return jsonRecs.Count > 0;
        }

        private void SaveStringsList(string saveName, List<string> data)
        {
            string summaryData = String.Join('\n', data);
            PlayerPrefs.SetString(saveName, summaryData);
            PlayerPrefs.Save();
            Log(Debug.Log, "Game progress saved.");
        }

        private void Log(Action<object> logger, string msg, bool silent = false)
        {
            if (silent)
            {
                logger = x => GameLogManager.Info(x.ToString());
            }
           
            logger(string.Format(LogStr.INFO_SYSTEM, "SavesManager", msg));
        }
    }
}