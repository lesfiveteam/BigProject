using BigProject.Intercatable;
using BigProject.Systems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

namespace BigProject.NPC
{
    public class NPCCommentator : MonoBehaviour, IInteractable
    {
        [SerializeField]
        private NPCChatView _chatView;
        [SerializeField]
        private float _commentDuration = 2f;
        [SerializeField]
        private List<LocalizedString> _comments;

        [field: SerializeField]
        public bool NeedComeUp { get; private set; }

        private static AsyncOperationHandle<string> _currentHandle;
        private WaitForSeconds _commentDelay;
        private bool _isCommenting;

        private void Awake()
        {
            Assert.IsNotNull(_chatView, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, $"{name}", "NPCChatView"));
            _commentDelay = new WaitForSeconds(_commentDuration);
        }

        public void Interact()
        {
            if (_isCommenting)
            {
                return;
            }

            if (_comments.Count == 0)
            {
                Debug.LogWarning(string.Format(LogStr.WARNING_SYSTEM, gameObject.name, "no comments to say"));
                return;
            }

            _isCommenting = true;

            if (_currentHandle.IsValid())
            {
                _currentHandle.Completed -= ShowComment;
            }

            _currentHandle = _comments.ElementAt(Random.Range(0, _comments.Count)).GetLocalizedStringAsync();
            _currentHandle.Completed += ShowComment;

        }

        private void ShowComment(AsyncOperationHandle<string> handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                StartCoroutine(ShowCommentRoutine(handle.Result));
            }
            else
            {
                Debug.LogWarning(string.Format(LogStr.WARNING_SYSTEM, gameObject.name, $"unable to get comment by operation ${handle.ToString()}"));
                _isCommenting = false;
            }    
        }

        private IEnumerator ShowCommentRoutine(string comment)
        {
            _chatView.Show();
            _chatView.Speak(comment);
            yield return _commentDelay;
            _chatView.ShutUp();
            _chatView.Hide();
            _isCommenting = false;
        }
    }

}