using Assets.Project.Scripts.NPC.NPCWalkSystem;
using BigProject.Systems;
using BigProject.Utilities;
using UnityEngine;

namespace BigProject.NPC.States
{
    public class NPCStateWandering : INPCState
    {
        private NPCStatePatrolling _statePatrolling;
        private NPCWalkController _walkController;

        public NPCState State => NPCState.Wandering;

        public NPCStateWandering(NPCStatePatrolling statePatrolling, NPCWalkController behaviourController)
        {
            _statePatrolling = statePatrolling;
            _walkController = behaviourController;
            ExceptionUtilities.ThrowIfNull(_statePatrolling, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"NPCStateWandering", "NPCStatePatrolling"));
            ExceptionUtilities.ThrowIfNull(_walkController, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"NPCStateWandering", "NPCWalkController"));
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