using BigProject.Systems;
using BigProject.Utilities;
using UnityEngine;

namespace BigProject.NPC.States
{
    public class NPCStateWandering : INPCState
    {
        private NPCStatePatrolling _statePatrolling;
        private NPCBehaviourController _behaviourController;

        public NPCState State => NPCState.Wandering;

        public NPCStateWandering(NPCStatePatrolling statePatrolling, NPCBehaviourController behaviourController)
        {
            _statePatrolling = statePatrolling;
            _behaviourController = behaviourController;
            ExceptionUtilities.ThrowIfNull(_statePatrolling, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"NPCStateWandering", "NPCStatePatrolling"));
            ExceptionUtilities.ThrowIfNull(_behaviourController, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"NPCStateWandering", "NPCBehaviourController"));
        }

        public void Start()
        {
            //_behaviourController.random = true
            _statePatrolling.Start();
        }

        public void Stop()
        {
            //_behaviourController.random = false
            _statePatrolling.Stop();
        }

        public void OnTriggerEnter(Collider other)
        {
            _statePatrolling.OnTriggerEnter(other);
        }

        public void Dispose()
        {
            _statePatrolling.Dispose();
        }
    }
}