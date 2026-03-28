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
        private const float REPLICA_LIFE_TIME = 3f;
        private static PlayerChatController _chatController;
        private static Coroutine _currentCoroutine;
        private static ManualLoop _manualLoop;
        private static AsyncOperationHandle<string> _currentHandle;
        private static Action<AsyncOperationHandle<string>> _entryLoadedHandler;

        private static bool _isInitialized = false;

        public ReplicaManager(PlayerChatController chatController, ManualLoop manualLoop)
        {
            if (_isInitialized)
            {
                throw new InvalidOperationException(string.Format(LogStr.CRITICAL_SYSTEM, "ReplicaManager", "try duplicate instance"));
            }

            _isInitialized = true;
            _chatController = chatController;
            _manualLoop = manualLoop;
            ExceptionUtilities.ThrowIfNull(_chatController, string.Format(LogStr.CRITICAL_NULL_REFERENCE, "ReplicaManager", "PlayerChatController"));
            ExceptionUtilities.ThrowIfNull(_manualLoop, string.Format(LogStr.CRITICAL_NULL_REFERENCE, "ReplicaManager", "ManualLoop"));
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
            _currentCoroutine = _manualLoop.StartCoroutine(ShowReplicaRoutine(delay));
        }

        private static IEnumerator ShowReplicaRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            _chatController.ShowChat();
            yield return new WaitForSeconds(REPLICA_LIFE_TIME);
            _chatController.HideChat();
        }
    }
}
