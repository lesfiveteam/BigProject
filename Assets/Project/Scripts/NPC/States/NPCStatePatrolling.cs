using Assets.Project.Scripts.NPC.NPCWalkSystem;
using BigProject.Systems;
using BigProject.Utilities;
using UnityEngine;

namespace BigProject.NPC.States
{
    public class NPCStatePatrolling : INPCState
    {
        private NPCController _controller;
        private NPCWalkController _walkController;

        public NPCState State => NPCState.Patrolling;

        public NPCStatePatrolling(NPCController controller, NPCWalkController walkController)
        {
            _controller = controller;
            _walkController = walkController;
            ExceptionUtilities.ThrowIfNull(_controller, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"NPCStatePatrolling", "NPCController"));
            ExceptionUtilities.ThrowIfNull(_walkController, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"NPCStatePatrolling", "NPCWalkController"));
        }

        public void Start()
        {
            _controller.AgentOn();
            _walkController.StartWalk();
        }

        public void Stop()
        {
            _walkController.StopWalk();
        }

        public void OnTriggerEnter(Collider other)
        {
            NPCController companion = other.GetComponentInParent<NPCController>();

            if (companion != null && IsSuitableStateForChat(companion.State) && WantToChat() && ComanionWantToChat(companion))
            {
                NPCStateChase chaser = new(_controller, companion, other.transform, _walkController);
                companion.ChangeState(new NPCStateWait(companion, chaser, _controller.transform, _walkController));
                _controller.ChangeState(chaser);
            }
        }

        private bool WantToChat() => Random.Range(0f, 1f) < _controller.Talkativeness;

        private bool ComanionWantToChat(NPCController companion) => !Mathf.Approximately(companion.Talkativeness, 0f);

        private bool IsSuitableStateForChat(NPCState state) => state < NPCState.Chase;

        public void Dispose() { }
    }
}