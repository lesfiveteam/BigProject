using UnityEngine;
using BigProject.Managers;
using System.Collections.Generic;
using UnityEngine.Assertions;
//using UnityEngine.Localization.Settings;
using System;


namespace BigProject.Systems.HUD
{
    /// <summary>
    /// Логика журнала.
    /// </summary>
    public class QuestJournal : IDisposable
    {
        private QuestJournalConfig _config;
        private GameLogManager _logger;
        private ProgressManager _pm;
        private List<(IQuestActionHandler, Action)> _journalWriters = new();


        private string _taskNote = "";
        private int _currentQuestId;
        private bool _hasActiveQuest;

        public event Action<string> TaskChanged;
        public event Action<string> QuestChanged;

        public QuestJournal(ProgressManager pm, QuestJournalConfig config)
        {
            _config = config;
            _logger = GameLogManager.Instance;
            _pm= pm;
            Assert.IsNotNull(_config, "Config of journal is null.");
            Assert.IsNotNull(pm, "Can't create journal: progress manager is null.");
            Assert.IsNotNull(_logger, "Journal unable to get logger.");
            //Init();
        }

        public void Dispose()
        {
            FinishCurrentQuest();

            foreach (QuestJournalTriggers questTriggers in _config)
            {
                _pm.RemoveQuestListener(questTriggers.QuestId, OnQuestStateChanged);
            }
        }

        public void Init()
        {
            ReleaseWriters();
            _hasActiveQuest = false;

            foreach (QuestJournalTriggers questTriggers in _config)
            {
                if (!_pm.AddQuestListener(questTriggers.QuestId, OnQuestStateChanged))
                {
                    Debug.LogWarning($"Journal unable to subscribe on quest {questTriggers.QuestId}.");
                    continue;
                }

                if (!_hasActiveQuest && _pm.GetQuestState(questTriggers.QuestId) == QuestState.Active)
                {
                    StartQuestRecord(questTriggers);
                }
            }
        }

        private void StartQuestRecord(QuestJournalTriggers questTriggers)
        {
            if (questTriggers == null)
            {
                Debug.LogError("Journal unable to get triggers: quest triggers config is null.");
                return;
            }

            _hasActiveQuest = true;
            _currentQuestId = questTriggers.QuestId;
            _logger.Info($"Start record quest {_currentQuestId} tasks to journal.");

            foreach (var questTrigger in questTriggers.Triggers)
            {
                if (_pm.TryGetQuestActionHandler(questTriggers.QuestId, questTrigger.ActionId, out IQuestActionHandler actionHandler))
                {
                    Action writer = () =>
                    {
                        if (actionHandler.CurrentState == questTrigger.StateWhenWrite)
                        {
                            WriteToJournal(questTrigger.TableEntryKey);
                        }
                    };

                    actionHandler.StateChanged += writer;
                    _journalWriters.Add((actionHandler, writer));
                }
                else
                {
                    Debug.LogWarning($"Journal unable to get action {questTrigger.ActionId} of quest {_currentQuestId}.");
                }
            }

            //string questName = LocalizationSettings.StringDatabase.GetLocalizedString(_config.LocalizationTableName, questTriggers.NameTableEntryKey);
            //QuestChanged?.Invoke(questName);
            TaskChanged?.Invoke(_taskNote);
        }

        private void ReleaseWriters()
        {
            if (_journalWriters.Count > 0)
            {
                foreach (var writerRecord in _journalWriters)
                {
                    writerRecord.Item1.StateChanged -= writerRecord.Item2;
                }

                _journalWriters.Clear();
            }
        }

        private void WriteToJournal(string tableEntry)
        {
            //_taskNote = LocalizationSettings.StringDatabase.GetLocalizedString(_config.LocalizationTableName, tableEntry);
            //TaskChanged?.Invoke(_taskNote);
        }

        private void OnQuestStateChanged(IQuest quest)
        {
            if (_hasActiveQuest)
            {
                if (_currentQuestId == quest.ID && quest.CurrentState > QuestState.Active)
                {
                    FinishCurrentQuest();
                }
            }
            else if (quest.CurrentState == QuestState.Active)
            {
                StartQuestRecord(_config.GetQuestJournalTriggers(_currentQuestId));
            }    
        }

        private void FinishCurrentQuest()
        {
            if (_hasActiveQuest)
            {
                _taskNote = "";
                QuestChanged?.Invoke("");
                TaskChanged?.Invoke(_taskNote);
                ReleaseWriters();
                _hasActiveQuest = false;
            }
        }
    }
}