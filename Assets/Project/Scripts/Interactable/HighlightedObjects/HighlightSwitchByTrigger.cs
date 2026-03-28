using BigProject.Managers;
using BigProject.Systems;
using BigProject.Systems.QuestSystem;
using BigProject.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace BigProject.Intercatable.HighlightedObjects
{
    public class HighlightSwitchByTrigger : MonoBehaviour
    {
        [SerializeField]
        private bool _isActiveByDefault;
        [SerializeField]
        private Collider _collider;
        [SerializeField]
        private List<SwitchCondition> _switchConditions;

        private Dictionary<IQuestActionHandler, SwitchCondition> _switchConditionsDict = new();

        [Serializable]
        private class SwitchCondition
        {
            public int questId;
            public int actionId;
            public QuestActionState stateToSwitch;

            public IQuestActionHandler actionHandler;
            public Action handler;
        }

        private void Awake()
        {
            Assert.IsNotNull(_collider, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, $"{name} HighlightSwitchByTrigger", "Collider"));
        }

        public void Init(ProgressManager progressManager)
        {
            ExceptionUtilities.ThrowIfNull(progressManager, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"{name} HighlightSwitchByTrigger", "ProgressManager"));
            _collider.enabled = _isActiveByDefault;

            foreach (SwitchCondition condition in _switchConditions)
            {
                if (!progressManager.TryGetQuestActionHandler(condition.questId, condition.actionId, out condition.actionHandler))
                {
                    Debug.LogWarning(string.Format(LogStr.WARNING_SYSTEM, $"{name} HighlightSwitchByTrigger", $"unable get quest {condition.questId} action {condition.actionId}"));
                    continue;
                }

                if (!_switchConditionsDict.TryAdd(condition.actionHandler, condition))
                {
                    Debug.LogWarning(string.Format(LogStr.WARNING_SYSTEM, $"{name} HighlightSwitchByTrigger", $"unable to add quest {condition.questId} action {condition.actionId}"));
                    continue;
                }

                condition.handler = () => _collider.enabled = (condition.actionHandler.CurrentState == condition.stateToSwitch) != _isActiveByDefault;

                if (progressManager.GetQuestState(condition.questId) == QuestState.Active)
                {
                    condition.handler.Invoke();
                }
            }
        }

        private void OnEnable()
        {
            foreach (SwitchCondition condition in _switchConditionsDict.Values)
            {
                condition.actionHandler.StateChanged += condition.handler;
            }
        }

        private void OnDisable()
        {
            foreach (SwitchCondition condition in _switchConditionsDict.Values)
            {
                condition.actionHandler.StateChanged -= condition.handler;
            }
        }
    }
}