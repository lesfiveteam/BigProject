using BigProject.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace BigProject.NPC
{
    public class NPCChatView : MonoBehaviour
    {
        [SerializeField]
        private Image _chatBoard;
        [SerializeField]
        private TMP_Text _speach;

        private void LateUpdate()
        {
            transform.LookAt(transform.position + Camera.main.transform.forward);
        }

        private void Awake()
        {
            Assert.IsNotNull(_chatBoard, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, $"{name}", "Ñhat board image"));
            Assert.IsNotNull(_speach, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, $"{name}", "TMP_Text"));
        }

        public void Speak(string text)
        {
            _chatBoard.enabled = true;
            _speach.text = text;
        }

        public void ShutUp()
        {
            _speach.text = "";
            _chatBoard.enabled = false;
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            ShutUp();
            gameObject.SetActive(false);
        }
    }
}