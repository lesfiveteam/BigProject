using BigProject.Managers;
using BigProject.Systems;
using BigProject.Systems.QuestSystem;
using BigProject.UI;
using BigProject.Utilities;
using UnityEngine;

namespace BigProject.Gameplay.Final
{
    public class QuestActions : MonoBehaviour
    {
        [SerializeField]
        private int _questId;
        [SerializeField]
        private int _runesAssembleActionId;

        private RunePanelUI _runePanel;
        private IQuestActionHandler _runesAssembleHandler;

        public void Init(ProgressManager progressManager, RunePanelUI runePanel)
        {
            _runePanel = runePanel;
            ExceptionUtilities.ThrowIfNull(progressManager, string.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "ProgressManager"));
            ExceptionUtilities.ThrowIfNull(_runePanel, string.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "RunePanelUI"));
            progressManager.TryGetQuestActionHandler(_questId, _runesAssembleActionId, out _runesAssembleHandler);
        }

        public void OnRunesAssembled()
        {
            if (_runePanel.IsCompleted)
            {
                _runesAssembleHandler.MakeTransition(0);
            }
        }

        private void OnEnable()
        {
            _runePanel.Completed += OnRunesAssembled;
        }

        private void OnDisable()
        {
            _runePanel.Completed -= OnRunesAssembled;
        }
    }
}