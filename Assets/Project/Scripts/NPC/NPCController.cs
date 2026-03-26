using BigProject.Managers;
using BigProject.NPC.States;
using BigProject.Systems;
using BigProject.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

namespace BigProject.NPC
{
    public class NPCController : MonoBehaviour, ISavable
    {
        [SerializeField]
        private NPCBehaviourController _behaviourController;
        [SerializeField]
        private NavMeshObstacle _obstacle;
        [SerializeField]
        private NPCState _initialState = NPCState.Idle;
        [HideInInspector, SerializeField]
        private AgentDTO _agentDTO;

        private ProgressManager _progressManager;
        private INPCState _state;
        private Dictionary<NPCState, INPCState> _hashedStates = new();

        [field: SerializeField, Range(0f, 1f), Tooltip("Level of talkativeness. 0 - silent.")]
        public float Talkativeness { get; private set; } = 0.8f;
        [field: SerializeField]
        public NavMeshAgent Agent { get; private set; }
        [field: SerializeField]
        public NPCChatsDatabase ChatsDatabase { get; private set; }

        public event Action<INPCState> StateChanged;
        public string Key => $"{gameObject.name}_controller";
        public NPCState State => _state != null ? _state.State : NPCState.Idle;
        public NPCState StateBeforeDistracted { get; private set; }

        [Serializable]
        private class AgentDTO
        {
            public Vector3 position;
        }

        public object SavingData
        {
            get
            {
                _agentDTO.position = Agent.nextPosition;
                return _agentDTO;
            }
        }

        private void Awake()
        {
            Assert.IsNotNull(_behaviourController, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, $"{name}", "NPCBehaviourController"));
            Assert.IsNotNull(_obstacle, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, $"{name}", "NavMeshObstacle"));
            CreateHashedStates();
            StateBeforeDistracted = _initialState;
            ChangeState(_initialState);
        }

        private void Start() => _progressManager.LoadAdditionalData(this, silent: true);

        private void OnDestroy()
        {
            _progressManager.SaveAdditionalData(this);
            _state?.Dispose();

            foreach (INPCState state in _hashedStates.Values)
            {
                state.Dispose();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            _state?.OnTriggerEnter(other);
        }

        public void Init(ProgressManager progressManager)
        {
            _progressManager = progressManager;
            ExceptionUtilities.ThrowIfNull(_progressManager, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"{name}", "ProgressManager"));
        }

        public void ChangeState(NPCState state)
        {
            if (_state != null && _state.State == state)
            {
                return;
            }

            if (_hashedStates.TryGetValue(state, out INPCState newState))
            {
                ChangeState(newState);
            }
            else
            {
                Debug.LogWarning(string.Format(LogStr.WARNING_SYSTEM, $"NPCController {name}", $"unable create new state {state}"));
            }
        }

        public INPCState GetCurrentState() => _state;

        public void ChangeState(INPCState state)
        {
            ExceptionUtilities.ThrowIfNull(state, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"{name}", "INPCState"));
            _state?.Stop();

            if (_state != null)
            {
                if (!IsDistractedState(_state.State))
                {
                    StateBeforeDistracted = _state.State;
                }

                if (!_hashedStates.ContainsKey(_state.State))
                {
                    _state.Dispose();
                }
            }

            _state = state;
            StateChanged?.Invoke(_state);
            _state.Start();
            GameLogManager.Info(string.Format(LogStr.INFO_SYSTEM, $"NPCController {name}", $"change state to {_state.State}"));
        }

        public void OnLoad()
        {
            Agent.Warp(_agentDTO.position);
        }

        public void AgentOn()
        {
            _obstacle.enabled = false;
            Agent.enabled = true;
        }

        public void ObstacleOn()
        {
            if (Agent.enabled)
            {
                Agent.ResetPath();
                Agent.enabled = false;
            }

            _obstacle.enabled = true;
        }

        private void CreateHashedStates()
        {
            _hashedStates.Add(NPCState.Idle, new NPCStateIdle(this));
            NPCStatePatrolling statePatrolling = new(this, _behaviourController);
            _hashedStates.Add(NPCState.Patrolling, statePatrolling);
            _hashedStates.Add(NPCState.Wandering, new NPCStateWandering(statePatrolling, _behaviourController));
        }

        private bool IsDistractedState(NPCState state) => state > NPCState.Wandering;
    }
}