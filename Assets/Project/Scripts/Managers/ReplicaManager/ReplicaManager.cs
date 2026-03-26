using BigProject.Systems;
using BigProject.UI.Chat;
using BigProject.Utilities;
using System.Collections;
using UnityEngine;

namespace BigProject.Managers
{
    public class ReplicaManager
    {
        private const float REPLICA_LIFE_TIME = 3f;
        private static PlayerChatController _chatController;
        private static Coroutine _currentCoroutine;
        private static ManualLoop _manualLoop;

        public ReplicaManager(PlayerChatController chatController, ManualLoop manualLoop)
        {
            _chatController = chatController;
            _manualLoop = manualLoop;
            ExceptionUtilities.ThrowIfNull(_chatController, string.Format(LogStr.CRITICAL_NULL_REFERENCE, "ReplicaManager", "PlayerChatController"));
            ExceptionUtilities.ThrowIfNull(_manualLoop, string.Format(LogStr.CRITICAL_NULL_REFERENCE, "ReplicaManager", "ManualLoop"));
            _chatController.HideChat();
        }

        public static void ShowReplica(string text)
        {
            _chatController.SetText(text);
            _chatController.ShowChat();

            if (_currentCoroutine != null)
            {
                _manualLoop.StopCoroutine(_currentCoroutine);
            }

            _currentCoroutine = _manualLoop.StartCoroutine(WaitAndCloseReplicaWindow());
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

        private static IEnumerator WaitAndCloseReplicaWindow()
        {
            yield return new WaitForSeconds(REPLICA_LIFE_TIME);
            _chatController.HideChat();
        }
    }
}
