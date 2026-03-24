using BigProject.Systems;
using BigProject.Utilities;
using UnityEngine;

namespace BigProject.NPC.States
{
    public class NPCStatePatrolling : INPCState
    {
        private NPCController _controller;
        private NPCBehaviourController _behaviourController;

        public NPCState State => NPCState.Patrolling;

        public NPCStatePatrolling(NPCController controller, NPCBehaviourController behaviourController)
        {
            _controller = controller;
            _behaviourController = behaviourController;
            ExceptionUtilities.ThrowIfNull(_controller, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"NPCStatePatrolling", "NPCController"));
            ExceptionUtilities.ThrowIfNull(_behaviourController, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"NPCStatePatrolling", "NPCBehaviourController"));
        }

        public void Start()
        {
            _controller.AgentOn();
            _behaviourController.enabled = true;
        }

        public void Stop()
        {
            _behaviourController.enabled = false;
        }

        public void OnTriggerEnter(Collider other)
        {
            NPCController companion = other.GetComponentInParent<NPCController>();

            if (companion != null && IsSuitableStateForChat(companion.State) && WantToChat())
            {
                NPCStateChase chaser = new(_controller, companion, other.transform);
                companion.ChangeState(new NPCStateWait(companion, chaser, _controller.transform));
                _controller.ChangeState(chaser);
            }
        }

        private bool WantToChat() => Random.Range(0f, 1f) <= _controller.Talkativeness;

        private bool IsSuitableStateForChat(NPCState state) => state < NPCState.Chase;

        public void Dispose() { }
    }
}