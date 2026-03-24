using BigProject.Systems;
using BigProject.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace BigProject.NPC.States
{
    public class NPCStateView : MonoBehaviour
    {
        [SerializeField]
        private Animator _animator;
        [SerializeField]
        private List<StateAnimations> _statesAnimations;
        [SerializeField]
        private Canvas _chatPanel;
        [SerializeField]
        private Image _chatBoard;
        [SerializeField]
        private TMP_Text _chatText;
        [SerializeField]
        private NPCController _controller;

        private INPCState _currentState;

        [Serializable]
        private class StateAnimations
        {
            public NPCState state;
            public List<string> triggers;
        }

        private void Awake()
        {
            Assert.IsNotNull(_animator, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"{name}", "Animator"));
            Assert.IsNotNull(_chatPanel, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"{name}", "Chat Canvas"));
            Assert.IsNotNull(_chatBoard, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"{name}", "Chat Image"));
            Assert.IsNotNull(_chatText, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"{name}", "Chat Text"));
            Assert.IsNotNull(_controller, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"{name}", "NPCController"));
        }

        private void OnStateChanged(INPCState state)
        {
            ExceptionUtilities.ThrowIfNull(state, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"{name}", "INPCState"));

            if (_currentState != null)
            {
                if (_currentState.State == state.State)
                {
                    return;
                }

                ClearOldState();
            }

            _currentState = state;
            StateAnimations stateAnimations = _statesAnimations.FirstOrDefault(x => x.state == state.State);

            if (stateAnimations != null && stateAnimations.triggers.Count > 0)
            {
                _animator.SetTrigger(stateAnimations.triggers[UnityEngine.Random.Range(0, stateAnimations.triggers.Count)]);
            }

            PrepareNewState();
        }

        private void PrepareNewState()
        {
            if (_currentState == null)
            {
                return;
            }

            switch (_currentState.State)
            {
                case NPCState.Chat:
                    if (_currentState is NPCStateChat stateChat)
                    {
                        stateChat.Speak += OnSpeak;
                    }

                    _chatPanel.gameObject.SetActive(true);
                    break;
                default:
                    break;
            }
        }

        private void ClearOldState()
        {
            if (_currentState == null)
            {
                return;
            }

            switch (_currentState.State)
            {
                case NPCState.Chat :
                    if (_currentState is NPCStateChat stateChat)
                    {
                        OnSpeak("");
                        stateChat.Speak -= OnSpeak;
                    }

                    _chatPanel.gameObject.SetActive(false);
                    break;
                default:
                    break;

            }

            _currentState = null;
        }

        private void OnSpeak(string text)
        {
            _chatText.text = text;
            _chatBoard.enabled = !string.IsNullOrEmpty(_chatText.text);
        }

        private void OnEnable()
        {
            INPCState state = _controller.GetCurrentState();

            if (state != null)
            {
                OnStateChanged(state);
            }

            _controller.StateChanged += OnStateChanged;
        }

        private void OnDisable()
        {
            ClearOldState();
            _controller.StateChanged -= OnStateChanged;
        }
    }
}