using BigProject.Systems;
using BigProject.Utilities;
using System;
using System.Threading;
using UnityEngine;

namespace BigProject.NPC.States
{
    // For single use, recreate.
    public class NPCStateChase : INPCState
    {
        private NPCController _controller;
        private NPCController _target;
        private NPCAgentTransition _transition;
        private CancellationTokenSource _ctSource;

        public Action<NPCStateChat> CameUp;

        public NPCState State => NPCState.Chase;

        public NPCStateChase(NPCController controller, NPCController target, Transform targetTransform)
        {
            _controller = controller;
            _target = target;
            ExceptionUtilities.ThrowIfNull(_controller, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"NPCStateChase", "NPCController"));
            ExceptionUtilities.ThrowIfNull(_target, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"NPCStateChase", "Target NPCController"));
            _transition = new(_controller.Agent, targetTransform);
            _ctSource = new();
        }

        public void Start()
        {
            _controller.AgentOn();
            NPCChat chat = new(_controller, out NPCStateChat speaker1, _target, out NPCStateChat speaker2, _controller.ChatsDatabase.GetRandomChat());
            _transition.GoToAndLookAt(() => _ = StartChat(chat, speaker1, speaker2));
        }

        private async Awaitable StartChat(NPCChat chat, NPCStateChat speaker1, NPCStateChat speaker2)
        {
            try
            {
                CameUp?.Invoke(speaker2);

                while (!chat.IsPrepared)
                {
                    await Awaitable.NextFrameAsync(_ctSource.Token);
                }

                chat.Start();
                _controller.ChangeState(speaker1);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Debug.LogError(string.Format(LogStr.ERROR_SYSTEM, $"{_controller.name} NPCStateChase", $"start chating error.\n{ex.Message}"));
            }
        }

        public void Dispose()
        {
            _transition.Dispose();
            _ctSource?.Cancel();
            _ctSource?.Dispose();
            _ctSource = null;
        }
    }
}