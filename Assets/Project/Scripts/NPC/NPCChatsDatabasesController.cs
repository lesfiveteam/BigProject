using BigProject.Managers;
using BigProject.Systems;
using BigProject.Systems.QuestSystem;
using BigProject.Utilities;
using UnityEngine;
using UnityEngine.Assertions;

namespace BigProject.NPC
{
    public class NPCChatsDatabasesController : MonoBehaviour
    {
        [SerializeField]
        private NPCChatsDatabase _databaseBA;
        [SerializeField]
        private NPCChatsDatabase _databaseAA;
        [SerializeField]
        private int _triggerQuestId;
        [SerializeField]
        private int _triggerId;

        private IQuestActionHandler _actionHandler;

        public static NPCChatsDatabase ActualChatsDatabase { get; private set; }

        private void Awake()
        {
            Assert.IsNotNull(_databaseBA, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"{name}", "NPCChatsDatabase before ambassador"));
            Assert.IsNotNull(_databaseAA, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"{name}", "NPCChatsDatabase after ambassador"));
            ActualChatsDatabase = _databaseBA;
        }

        public void Init(ProgressManager progressManager)
        {
            ExceptionUtilities.ThrowIfNull(progressManager, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"{name}", "ProgressManager"));
            
            if (!progressManager.TryGetQuestActionHandler(_triggerQuestId, _triggerId, out _actionHandler))
            {
                Debug.LogError(string.Format(LogStr.ERROR_SYSTEM, "NPCChatsDatabasesController", "unable to get Action Handler"));
            }
        }

        private void OnStateChanged()
        {
            if (_actionHandler.Quest.CurrentState == QuestState.Completed || _actionHandler.CurrentState == QuestActionState.Completed)
            {
                GameLogManager.Info(string.Format(LogStr.INFO_SYSTEM, "NPCChatsDatabasesController", "NPC Chats switch to database after ambassador"));
                ActualChatsDatabase = _databaseAA;
            }
        }

        private void OnEnable()
        {
            _actionHandler.StateChanged += OnStateChanged;
            OnStateChanged();
        }

        private void OnDisable()
        {
            _actionHandler.StateChanged -= OnStateChanged;
        }
    }
}
