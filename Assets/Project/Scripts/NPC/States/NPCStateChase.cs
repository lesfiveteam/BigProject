using Assets.Project.Scripts.NPC.NPCWalkSystem;
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
            NPCWalkController walkController = controller.GetComponent<NPCWalkController>();

            ExceptionUtilities.ThrowIfNull(_controller, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"NPCStateChase", "NPCController"));
            ExceptionUtilities.ThrowIfNull(_target, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"NPCStateChase", "Target NPCController"));
            ExceptionUtilities.ThrowIfNull(walkController, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"NPCStateWait", "NPCWalkController"));

            _transition = new(targetTransform, walkController);
            _ctSource = new();
        }

        public async void Start()
        {
            await _controller.AgentOn(_ctSource.Token);

            if (_controller == null || _controller.gameObject == null)
                return;

            NPCChatsDatabase chatsDb = NPCChatsDatabasesController.ActualChatsDatabase;
            ExceptionUtilities.ThrowIfNull(chatsDb, string.Format(LogStr.CRITICAL_NULL_REFERENCE, "NPCStateChase", "NPCChatsDatabase"));
            NPCChat chat = new(_controller, out NPCStateChat speaker1, _target, out NPCStateChat speaker2, chatsDb.GetRandomChat());

            _transition.GoToAndLookAt(() => _ = StartChat(chat, speaker1, speaker2, _ctSource.Token));
        }

        private async Awaitable StartChat(NPCChat chat, NPCStateChat speaker1, NPCStateChat speaker2, CancellationToken ct)
        {
            try
            {
                CameUp?.Invoke(speaker2);

                while (!chat.IsPrepared)
                {
                    await Awaitable.NextFrameAsync(ct);
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