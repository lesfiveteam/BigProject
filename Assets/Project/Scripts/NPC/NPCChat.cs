using BigProject.Managers;
using BigProject.NPC.States;
using BigProject.Systems;
using BigProject.Utilities;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace BigProject.NPC
{
    public class NPCChat : IDisposable
    {
        private NPCStateChat _speaker1;
        private NPCStateChat _speaker2;
        private NPCChatConfig _chatConfig;
        private StringTable _localTable;
        private CancellationTokenSource _ctSource;
        private bool _isDisposed = false;

        public event Action ChatCompleted;
        public bool IsPrepared { get; private set; }

        public NPCChat(NPCController controller1, out NPCStateChat speaker1, NPCController controller2, out NPCStateChat speaker2, NPCChatConfig chatConfig)
        {
            speaker1 = new(controller1, this);
            speaker2 = new(controller2, this);
            _speaker1 = speaker1;
            _speaker2 = speaker2;
            _chatConfig = chatConfig;
            ExceptionUtilities.ThrowIfNull(_chatConfig, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"NPCChat", "NPCChatConfig"));
            _ctSource = new();
            AsyncOperationHandle<StringTable> getTable = LocalizationSettings.StringDatabase.GetTableAsync(_chatConfig.LocalizationTableName);
            getTable.Completed += OnTableLoaded; // one shot, no need unsubscribe.
        }

        public void Start()
        {
            if (!IsPrepared)
            {
                Debug.LogWarning(string.Format(LogStr.INFO_SYSTEM, "NPCChat", "not ready for start"));
                return;
            }

            _ = ChatRoutine(_ctSource.Token);
        }

        private void OnTableLoaded(AsyncOperationHandle<StringTable> handle)
        {
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogWarning(string.Format(LogStr.WARNING_SYSTEM, "NPCChat", $"unable to get table for chat by operation ${handle.ToString()}"));
                return;
            }

            _localTable = handle.Result;
            IsPrepared = true;
        }

        private async Awaitable ChatRoutine(CancellationToken ct)
        {
            GameLogManager.Info(string.Format(LogStr.INFO_SYSTEM, "NPCChat", $"start chat"));
            NPCStateChat speaker;
            await Awaitable.NextFrameAsync(ct); // For prepare listeners.

            foreach (NPCChatConfig.Entry entry in _chatConfig)
            {
                StringTableEntry tableEntry = _localTable.GetEntry(entry.TableEntryKey);

                if (tableEntry == null)
                {
                    Debug.LogWarning(string.Format(LogStr.WARNING_SYSTEM, "NPCChat", $"unable to get table entry {entry.TableEntryKey} for chat"));
                    continue;
                }

                speaker = entry.SpeakerID == 0 ? _speaker1 : _speaker2;
                speaker.Say(tableEntry.LocalizedValue);

                try
                {
                    await Awaitable.WaitForSecondsAsync(_chatConfig.GetSpeachTime(tableEntry.LocalizedValue), ct);
                }
                catch (OperationCanceledException)
                {
                    ChatCompleted?.Invoke();
                    throw;
                }
                finally
                {
                    speaker.ShutUp();
                }
            }

            ChatCompleted?.Invoke();
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _ctSource?.Cancel();
                _ctSource?.Dispose();
                _ctSource = null;
                _isDisposed = true;
            }
        }
    }
}