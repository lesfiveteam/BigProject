using BigProject.Settings;
using BigProject.Systems;
using BigProject.UI.Chat;
using BigProject.Utilities;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace BigProject.Managers
{
    public class ReplicaManager
    {
        private static PlayerChatController _chatController;
        private static Coroutine _currentCoroutine;
        private static ManualLoop _manualLoop;
        private static AsyncOperationHandle<string> _currentHandle;
        private static Action<AsyncOperationHandle<string>> _entryLoadedHandler;
        private static PlayerConfig _config;

        public ReplicaManager(PlayerChatController chatController, ManualLoop manualLoop, PlayerConfig config)
        {
            _chatController = chatController;
            _manualLoop = manualLoop;
            _config = config;
            ExceptionUtilities.ThrowIfNull(_chatController, string.Format(LogStr.CRITICAL_NULL_REFERENCE, "ReplicaManager", "PlayerChatController"));
            ExceptionUtilities.ThrowIfNull(_manualLoop, string.Format(LogStr.CRITICAL_NULL_REFERENCE, "ReplicaManager", "ManualLoop"));
            ExceptionUtilities.ThrowIfNull(_config, string.Format(LogStr.CRITICAL_NULL_REFERENCE, "ReplicaManager", "PlayerConfig"));
            _chatController.HideChat();
        }

        public static void ShowReplica(LocalizedString localizedString, float delay = 0f)
        {
            if (_currentHandle.IsValid())
            {
                _currentHandle.Completed -= _entryLoadedHandler;
            }
            else
            {
                HideReplica();
            }

            _currentHandle = localizedString.GetLocalizedStringAsync();
            _entryLoadedHandler = (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    ShowReplica(handle.Result, delay);
                }
            };

            _currentHandle.Completed += _entryLoadedHandler;
        }

        public static void HideReplica()
        {
            if (_currentCoroutine != null)
            {
                _manualLoop.StopCoroutine(_currentCoroutine);
                _currentCoroutine = null;
            }

            _chatController.HideChat();
        }

        private static void ShowReplica(string text, float delay)
        {
            _chatController.SetText(text);
            float lifeTime = _config.MinSpeachTime + _config.TimeCorrectionPerLetter * Mathf.Max(0, text.Length - _config.SpeachLengthForMinTime);
            _currentCoroutine = _manualLoop.StartCoroutine(ShowReplicaRoutine(delay, lifeTime));
        }

        private static IEnumerator ShowReplicaRoutine(float delay, float lifeTime)
        {
            yield return new WaitForSeconds(delay);
            _chatController.ShowChat();
            yield return new WaitForSeconds(lifeTime);
            _chatController.HideChat();
        }
    }
}
