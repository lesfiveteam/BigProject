using BigProject.Systems;
using BigProject.Systems.QuestSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BigProject.Managers
{
    /// <summary>
    /// Progress manager, manages game quests and progress recording.
    /// </summary>
    public class ProgressManager : ISavable, IDisposable
    {
        // Objects whose state needs to be fixed when saving progress.
        [SerializeField]
        private List<ISavable> _savable;

        private string _profileName; // To separate player profiles.
        private SavesManager _savesManager;
        private Dictionary<int, IQuest> _quests;

        public const string ADDITIONAL_DATA_NAME = "additional_data";
        private readonly string _additionalDataFullName;
        private Dictionary<string, bool> _additionalRelevance = new();

        // ISavable
        public string Key => "GeneralProgress";
        public object SavingData => this;

        /// <summary>
        /// When True, saves progress when any of the tracked quests changes status.
        /// </summary>
        public bool AutoSave { get; set; } = true;


        /// <param name="profileName">Player profile name</param>
        /// <param name="questLoader">Quest loader to use</param>
        public ProgressManager(string profileName, IQuestLoader questLoader, SavesManager savesManager)
        {
            _profileName = profileName;
            _additionalDataFullName = $"{_profileName}_{ADDITIONAL_DATA_NAME}";

            // Record general data from Progress Manager.
            _savable = new() { this }; 

            _savesManager = savesManager;

            try
            {
                _quests = questLoader.GetAllQuests().ToDictionary(x => x.ID, x => x);
            }
            catch (ArgumentException ex)
            {
                Debug.LogError(String.Format(LogStr.ERROR_SYSTEM, "ProgressManager", $"can't add quest. {ex.Message}"));
                _quests = new();
            }
            catch (Exception ex)
            {
                Debug.LogError(String.Format(LogStr.ERROR_SYSTEM, "ProgressManager", $"try to add quests with the same key! {ex.Message}"));
                _quests = new();
            }

            AddQuestsToSavable();

            foreach (IQuest quest in _quests.Values)
            {
                quest.StateChanged += OnQuestProgressed;
            }
        }

        /// <summary>
        /// Add quests to save data.
        /// </summary>
        private void AddQuestsToSavable()
        {
            foreach (IQuest quest in _quests.Values)
            {
                if (quest is ISavable savable)
                {
                    AddSavable(savable);
                }
            }
        }

        /// <summary>
        /// Subscribe to quest.
        /// </summary>
        public bool AddQuestListener(int quiestId, Action<IQuest> callback)
        {
            if (!_quests.TryGetValue(quiestId, out IQuest quest))
            {
                Debug.LogError(String.Format(LogStr.ERROR_SYSTEM, "ProgressManager", $"unable to add listener, has no quest [{quiestId}]"));
                return false;
            }

            quest.Progressed += callback;
            return true;
        }

        /// <summary>
        /// Unsubscribe from quest.
        /// </summary>
        public void RemoveQuestListener(int quiestId, Action<IQuest> callback)
        {
            if (_quests.TryGetValue(quiestId, out IQuest quest))
            {
                quest.Progressed -= callback;
            }
            else
            {
                Debug.LogError(String.Format(LogStr.ERROR_SYSTEM, "ProgressManager", $"unable to remove listener, has no quest [{quiestId}]"));
            }
        }

        /// <summary>
        /// Subscribe to quest.
        /// </summary>
        public bool AddQuestStateListener(int questId, Action<IQuest> callback)
        {
            if (!_quests.TryGetValue(questId, out IQuest quest))
            {
                Debug.LogError(String.Format(LogStr.ERROR_SYSTEM, "ProgressManager", $"unable to add state listener, has no quest [{questId}]"));
                return false;
            }

            quest.StateChanged += callback;
            return true;
        }

        /// <summary>
        /// Unsubscribe from quest.
        /// </summary>
        public void RemoveQuestStateListener(int questId, Action<IQuest> callback)
        {
            if (_quests.TryGetValue(questId, out IQuest quest))
            {
                quest.StateChanged -= callback;
            }
            else
            {
                Debug.LogError(String.Format(LogStr.ERROR_SYSTEM, "ProgressManager", $"unable to remove state listener, has no quest [{questId}]"));
            }
        }

        /// <summary>
        /// Adds a saved object (e.g. inventory, characters, etc.).
        /// </summary>
        public void AddSavable(ISavable savable)
        {
            if (_savable.Contains(savable))
            {
                Debug.LogWarning(String.Format(LogStr.ERROR_SYSTEM, "ProgressManager", $"already tracking savable data [{savable.Key}]"));
                return;
            }

            _savable.Add(savable);
        }

        /// <summary>
        /// Remove object from saving data.
        /// </summary>
        public void RemoveSavable(ISavable savable)
        {
            if (!_savable.Remove(savable))
            {
                Debug.LogWarning(String.Format(LogStr.ERROR_SYSTEM, "ProgressManager", $"try to remove not tracking savable data [{savable.Key}]"));
            }
        }

        public void SaveProgress()
        {
            try
            {
                _savesManager.SaveGame(_profileName, _savable);
            }
            catch
            {
                Debug.LogError(String.Format(LogStr.ERROR_SYSTEM, "ProgressManager", "unable to save progress"));
                return;
            }

            foreach (string key in _additionalRelevance.Keys.ToList())
            {
                _additionalRelevance[key] = true;
            }
        }

        public void LoadProgress()
        {
            try
            {
                _savesManager.LoadGame(_profileName, _savable);
            }
            catch
            {
                Debug.LogError(String.Format(LogStr.ERROR_SYSTEM, "ProgressManager", "unable to load progress"));
                return;
            }

            List<string> _additionalToDelete = _additionalRelevance.Where(x => !x.Value).Select(x => x.Key).ToList();

            foreach (string key in _additionalToDelete)
            {
                    DeleteAdditionalData(key);
            }
        }

        public bool HasSavedProgress()
        {
            return _savesManager.HasSave(_profileName);
        }

        /// <summary>
        /// Manual quest transition.
        /// </summary>
        /// <param name="newState">New action state</param>
        /// <returns></returns>
        public bool ManualProgress(int questId, int actionId, QuestActionState newState)
        {
            if (!_quests.TryGetValue(questId, out IQuest quest))
            {
                Debug.LogError(String.Format(LogStr.ERROR_SYSTEM, "ProgressManager", $"has no quest [{questId}], but trigger try to access it"));
                return false;
            }

            QuestState prevState = quest.CurrentState;
            return quest.ManualTransition(actionId, newState);
        }

        /// <returns>Quest's action state.</returns>
        public QuestActionState GetActionState(int questId, int actionId)
        {
            if (!_quests.TryGetValue(questId, out IQuest quest))
            {
                Debug.LogError(String.Format(LogStr.ERROR_SYSTEM, "ProgressManager", $"has no quest [{questId}], but you try to get Action state from it"));
                return QuestActionState.Undefined;
            }

            if (!quest.TryGetActionState(actionId, out QuestActionState actionState))
            {
                Debug.LogError(String.Format(LogStr.ERROR_SYSTEM, "ProgressManager", $"quest [{questId}] has no Action [{actionId}], but you try to get it"));
                return QuestActionState.Undefined;
            }

            return actionState;
        }

        /// <returns>All quest's actions.</returns>
        public IReadOnlyDictionary<int, QuestActionState> GetAllActions(int questId)
        {
            if (!_quests.TryGetValue(questId, out IQuest quest))
            {
                Debug.LogError(String.Format(LogStr.ERROR_SYSTEM, "ProgressManager", $"has no quest [{questId}], but you try to get Action state from it"));
                return new Dictionary<int, QuestActionState>();
            }

            return quest.GetAllActions();
        }

        /// <param name="actionHandler">Quest action handler</param>
        /// <returns>True when success.</returns>
        public bool TryGetQuestActionHandler(int questId, int actionId, out IQuestActionHandler actionHandler)
        {
            if (!_quests.TryGetValue(questId, out IQuest quest))
            {
                Debug.LogError(String.Format(LogStr.ERROR_SYSTEM, "ProgressManager", $"has no quest [{questId}], but you try to get action handler from it"));
                actionHandler = null;
                return false;
            }

            return quest.TryGetActionHandler(actionId, out actionHandler);
        }

        /// <returns>Actual state of quest.</returns>
        public QuestState GetQuestState(int questId)
        {
            if (!_quests.TryGetValue(questId, out IQuest quest))
            {
                Debug.LogWarning(String.Format(LogStr.WARNING_SYSTEM, "ProgressManager", $" has no quest [{questId}], but you try to get quest state"));
                return QuestState.Inactive;
            }

            return quest.CurrentState;            
        }

        /// <summary>
        /// Add additional data.
        /// </summary>
        /// <returns>True when success.</returns>
        public bool SaveAdditionalData(ISavable savable, bool trackRelevance = true)
        {
            bool result;

            try
            {
                result = _savesManager.AddToSave(_additionalDataFullName, savable);
            }
            catch
            {
                Debug.LogError(String.Format(LogStr.ERROR_SYSTEM, "ProgressManager", "unable to add additional data to save"));
                return false;
            }

            if (result)
            {
                if (_additionalRelevance.ContainsKey(savable.Key))
                {
                    if (trackRelevance)
                    {
                        _additionalRelevance[savable.Key] = false;
                    }
                    else
                    {
                        _additionalRelevance.Remove(savable.Key);
                    }
                }
                else if (trackRelevance)
                {
                    _additionalRelevance.Add(savable.Key, false);
                }
            }

            return result;
        }

        /// <summary>
        /// Load additional data.
        /// </summary>
        /// <param name="removeAfterLoad">When true data will be removed after loading.</param>
        /// <param name="silent">When true errors and warning won't be trigger (usefull when not sure about data availability).</param>
        /// <returns>True when success.</returns>
        public bool LoadAdditionalData(ISavable savable, bool removeAfterLoad = false, bool silent = true)
        {
            bool result;

            try
            {
                result = _savesManager.LoadFromSave(_additionalDataFullName, savable, removeAfterLoad, silent);
            }
            catch
            {
                Debug.LogError(String.Format(LogStr.ERROR_SYSTEM, "ProgressManager", "unable to load additional data from save"));
                return false;
            }

            if (result && removeAfterLoad)
            {
                _additionalRelevance.Remove(savable.Key);
            }

            return result;
        }


        /// <summary>
        /// Delete one record from additional data.
        /// </summary>
        public void DeleteAdditionalData(string key)
        {
            _savesManager.DeleteFromSave(_additionalDataFullName, key);
            _additionalRelevance.Remove(key);
        }

        private void OnQuestProgressed(IQuest quest)
        {
            if (!_quests.ContainsKey(quest.ID))
            {
                Debug.LogWarning(String.Format(LogStr.WARNING_SYSTEM, "ProgressManager", $"get callback from untracked quest [{quest.Name}]"));
                return;
            }

            if (AutoSave)
            {
                Debug.Log(String.Format(LogStr.INFO_SYSTEM, "ProgressManager", "Autosaving..."));
                SaveProgress();
            }
        }

        public void Dispose()
        {
            foreach (IQuest quest in _quests.Values)
            {
                quest.Progressed -= OnQuestProgressed;
            }
        }
    }
}