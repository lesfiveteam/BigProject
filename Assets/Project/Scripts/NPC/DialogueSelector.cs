using System;
using UnityEngine;
using BigProject.Systems.DialogueSystem;
using BigProject.Systems.QuestSystem;
using System.Collections.Generic;
using UnityEngine.Assertions;
using BigProject.Managers;
using BigProject.Systems;
using BigProject.Utilities;

namespace BigProject.NPC
{
    /// <summary>
    /// Switch dialogue by quest action state.
    /// </summary>
    public class DialogueSelector : MonoBehaviour
    {
        [SerializeField]
        private DialogNPC _dialogue;
        [SerializeField]
        private int _questId;
        [SerializeField]
        private List<DialogueCondition> _conditions;

        private ProgressManager _progressManager;

        [Serializable]
        private class DialogueCondition
        {
            public int id;
            public QuestActionState state;
            public DialogueLine dialogue;
            public bool hasTransition;
            public int phraseIdToTransit;
            public int transitionId;
            // line after completing the quest
            public DialogueLine defaultDialogue;

            [HideInInspector]
            public IQuestActionHandler actionHandler;
        }
        
        private void Awake()
        {
            // TODO: remove ServiceLocator from here
            _progressManager = ServiceLocator.GetService<ProgressManager>();
            Assert.IsNotNull(_dialogue, String.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "NPC dialogue"));
            ExceptionUtilities.ThrowIfNull(_progressManager, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Progress manager"));

            List<DialogueCondition> conditionsToRemove = new();

            foreach (DialogueCondition condition in _conditions)
            {
                if (_progressManager.TryGetQuestActionHandler(_questId, condition.id, out condition.actionHandler))
                {
                    continue;
                }

                Debug.LogWarning(String.Format(LogStr.WARNING_QUEST, $"{gameObject.name} unable to get action {condition.id}. It will be ignored"));
                conditionsToRemove.Add(condition);
            }

            _conditions.RemoveAll(x => conditionsToRemove.Contains(x));

            if (_progressManager.GetQuestState(_questId) > QuestState.Active)
            {
                Destroy(this);
            }
        }

        private void Start()
        {
            StateChanged();
        }

        private void OnEnable()
        {
            foreach (DialogueCondition condition in _conditions)
            {
                condition.actionHandler.StateChanged += StateChanged;
            }

            DialogueManager.OnDialoguePhrase += OnDialoguePhrase;
        }

        private void OnDisable()
        {
            foreach (DialogueCondition condition in _conditions)
            {
                condition.actionHandler.StateChanged -= StateChanged;
            }

            DialogueManager.OnDialoguePhrase -= OnDialoguePhrase;
        }

        private void StateChanged()
        {
            if (_progressManager.GetQuestState(_questId) != QuestState.Active)
            {
                return;
            }

            foreach (DialogueCondition condition in _conditions)
            {
                IQuestActionHandler handler = condition.actionHandler;

                if (handler.CurrentState == condition.state)
                {
                    _dialogue.StartDialogLine = condition.dialogue;
                    return;
                }
                else if (handler.CurrentState >= QuestActionState.Active)
                {
                    _dialogue.StartDialogLine = condition.defaultDialogue;
                }
            }
        }

        private void OnDialoguePhrase(int phraseId)
        {
            DialogueCondition condition = _conditions.Find(x => x.hasTransition && x.phraseIdToTransit == phraseId);
            condition?.actionHandler.MakeTransition(condition.transitionId);

        }
    }
}
