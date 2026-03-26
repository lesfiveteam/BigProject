using UnityEngine;
using TMPro;
using BigProject.Systems.HUD;

namespace BigProject.UI.Chat
{
    public class PlayerChatUI : MonoBehaviour, IHUDWidget
    {
        [SerializeField]
        private GameObject _panel;
        [SerializeField]
        private TMP_Text _text;

        public void Show() => _panel.SetActive(true);

        public void Hide() => _panel.SetActive(false);

        public void SetText(string text) => _text.text = text;
    }
}

