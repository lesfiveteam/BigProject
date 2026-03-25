using BigProject.Systems;
using BigProject.Utilities;
using System;

namespace BigProject.NPC.States
{
    // For single use, recreate.
    public class NPCStateChat : INPCState
    {
        private NPCController _controller;
        private NPCChat _chat;

        public NPCState State => NPCState.Chat;
        public event Action<string> Speak;

        public NPCStateChat(NPCController controller, NPCChat chat)
        {
            _controller = controller;
            _chat = chat;
            ExceptionUtilities.ThrowIfNull(_controller, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"NPCStateChat", "NPCController"));
            ExceptionUtilities.ThrowIfNull(_chat, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"NPCStateChat", "NPCChat"));
            _chat.ChatCompleted += OnChatCompleted;
        }

        public void Start()
        {
            _controller.ObstacleOn();
        }

        public void Say(string text) => Speak?.Invoke(text);

        public void ShutUp() => Speak?.Invoke("");

        public void Dispose()
        {
            if (_chat != null)
            {
                _chat.ChatCompleted -= OnChatCompleted;
                _chat.Dispose();
            }
        }

        private void OnChatCompleted()
        {
            _chat.ChatCompleted -= OnChatCompleted;
            _controller.ChangeState(_controller.StateBeforeDistracted);
        }
    }
}