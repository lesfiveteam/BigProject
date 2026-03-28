using Assets.Project.Scripts.NPC;
using BigProject.Systems;
using BigProject.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace BigProject.NPC.States
{
    public class NPCStateView : MonoBehaviour
    {
        [SerializeField]
        private Animator _animator;
        [SerializeField]
        private List<StateAnimations> _statesAnimations;
        [SerializeField]
        private NPCController _controller;

        private INPCState _currentState;

        [Serializable]
        private class StateAnimations
        {
            public NPCState state;
            public List<NPCAnimation> triggers;
        }

        private void Awake()
        {
            Assert.IsNotNull(_animator, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"{name}", "Animator"));
            Assert.IsNotNull(_controller, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"{name}", "NPCController"));
            OnAwake();
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

                ClearOldState(_currentState);
            }

            _currentState = state;
            StateAnimations stateAnimations = _statesAnimations.FirstOrDefault(x => x.state == state.State);

            if (stateAnimations != null && stateAnimations.triggers.Count > 0)
            {
                _animator.SetTrigger(stateAnimations.triggers[UnityEngine.Random.Range(0, stateAnimations.triggers.Count)].ToString());
            }

            PrepareNewState(_currentState);
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
            if (_currentState != null)
            {
                ClearOldState(_currentState);
            }

            _controller.StateChanged -= OnStateChanged;
        }

        protected virtual void PrepareNewState(INPCState _)
        {
        }

        protected virtual void ClearOldState(INPCState _)
        {
        }

        protected virtual void OnAwake() 
        {
        }
    }
}