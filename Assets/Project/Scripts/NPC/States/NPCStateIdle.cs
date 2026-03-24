using BigProject.Systems;
using BigProject.Utilities;

namespace BigProject.NPC.States
{
    public class NPCStateIdle : INPCState
    {
        private NPCController _controller;
        public NPCState State => NPCState.Idle;

        public NPCStateIdle(NPCController controller)
        {
            _controller = controller;
            ExceptionUtilities.ThrowIfNull(_controller, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"NPCStateIdle", "NPCController"));
        }

        public void Start()
        {
            _controller.ObstacleOn();
        }

        public void Dispose() { }
    }
}