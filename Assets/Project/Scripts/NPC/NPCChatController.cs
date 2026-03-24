//using BigProject.Systems;
//using System;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Assertions;

//namespace BigProject.NPC
//{
//    public class NPCChatController : MonoBehaviour
//    {
//        [SerializeField]
//        private NPCChatsDatabase _chatsDB;
//        [SerializeField]
//        private NPCController _controller;
//        [SerializeField]
//        private Animator _animator;
//        [SerializeField]
//        private string _animWaitTrigger;
//        [SerializeField]
//        private List<string> _animTalkTriggers;

//        private bool _isChating;
//        private NPCChatController _companion;
//        private NPCChat _chat;

//        private void Awake()
//        {
//            Assert.IsNotNull(_chatsDB, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, $"{name}", "NPCChatsDatabase"));
//            Assert.IsNotNull(_controller, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, $"{name}", "NPCController"));
//            Assert.IsNotNull(_animator, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, $"{name}", "Animator"));
//            Assert.IsTrue(_animTalkTriggers.Count > 0, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, $"{name}", "Talk animations"));
//        }

//        public void Speak(string text)
//        {
//            Debug.Log(text);
//        }

//        public void ShutUp()
//        {
//           // _controller.ContinueRoute();
//        }

//        public void Listen(Vector3 speaker)
//        {
//            _controller.LookAt(speaker);
//            _animator.SetTrigger(_animWaitTrigger);
//            _isChating = true;
//        }

//        public void StartChat()
//        {
//            _animator.SetTrigger(_animTalkTriggers[Random.Range(0, _animTalkTriggers.Count)]);
//        }

//        public void EndChat()
//        {
//            _isChating = false;
//            _controller.ContinueRoute();
//        }

//        private void OnTriggerEnter(Collider other)
//        {
//            // For case when two enter calls
//            if (_isChating || (_companion = other.GetComponentInChildren<NPCChatController>()) == null)// && GetInstanceID() < other.GetInstanceID())
//            {
//                return;
//            }

//            _companion.Listen(_controller.transform.position);
//            Vector3 target = _companion.transform.position;

//            _controller.GoToAndLookAt(target, target, () =>
//            {
//                _chat = new(this, _companion, _chatsDB.GetRandomChat());
//                _chat.ChatCompleted += OnChatCompleted;
//                StartChat();
//                _companion.StartChat();
//            });

//            _isChating = true;
//        }

//        private void OnChatCompleted()
//        {
//            _chat.ChatCompleted -= OnChatCompleted;
//            _chat.Dispose();
//            EndChat();
//            _companion.EndChat();
//        }

//        private void OnDestroy()
//        {
//            if (_chat != null)
//            {
//                _chat.ChatCompleted -= OnChatCompleted;
//                _chat.Interrupt();
//            }
//        }
//    }
//}