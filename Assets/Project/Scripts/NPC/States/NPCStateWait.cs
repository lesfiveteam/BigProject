using Assets.Project.Scripts.NPC.NPCWalkSystem;
using BigProject.Systems;
using BigProject.Utilities;
using UnityEngine;

namespace BigProject.NPC.States
{
    // For single use, recreate.
    public class NPCStateWait : INPCState
    {
        private NPCController _controller;
        private NPCStateChase _chaser;
        private NPCAgentTransition _transition;

        public NPCState State => NPCState.Wait;

        public NPCStateWait(NPCController controller, NPCStateChase chaser, Transform chaserTransform)
        {
            _controller = controller;
            _chaser = chaser;
            NPCWalkController walkController = controller.GetComponent<NPCWalkController>();

            ExceptionUtilities.ThrowIfNull(_controller, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"NPCStateWait", "NPCController"));
            ExceptionUtilities.ThrowIfNull(_chaser, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"NPCStateWait", "NPCStateChase"));
            ExceptionUtilities.ThrowIfNull(walkController, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"NPCStateWait", "NPCWalkController"));

            _chaser.CameUp += OnCameUp;
            _transition = new(chaserTransform, walkController);
        }

        public void Start()
        {
            _controller.ObstacleOn();
            _transition.LookAt();
        }

        private void OnCameUp(NPCStateChat newState)
        {
            _chaser.CameUp -= OnCameUp;
            _controller.ChangeState(newState);
        }

        public void Dispose()
        {
            _transition.Dispose();

            if (_chaser != null)
            {
                _chaser.CameUp -= OnCameUp;
            }
        }
    }
}