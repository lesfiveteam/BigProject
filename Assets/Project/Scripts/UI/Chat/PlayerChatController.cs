using BigProject.Managers;
using BigProject.Systems;
using BigProject.Systems.HUD;
using BigProject.Utilities;
using TMPro;
using UnityEngine;

namespace BigProject.UI.Chat
{
    public class PlayerChatController
    {
        private GameObject _worldChat;
        private TMP_Text _worldChatText;
        private PlayerChatUI _chatWidget;
        private GameplayManager _gameplayManager;

        public PlayerChatController(GameObject worldChat, TMP_Text worldChatText, PlayerChatUI chatWidget, GameplayManager gameplayManager)
        {
            _worldChat = worldChat;
            _worldChatText = worldChatText;
            _chatWidget = chatWidget;
            _gameplayManager = gameplayManager;
            ExceptionUtilities.ThrowIfNull(_chatWidget, string.Format(LogStr.CRITICAL_NULL_REFERENCE, "PlayerChatController", "PlayerChatUI"));
            ExceptionUtilities.ThrowIfNull(_worldChat, string.Format(LogStr.CRITICAL_NULL_REFERENCE, "PlayerChatController", "World chat GameObject"));
            ExceptionUtilities.ThrowIfNull(_worldChatText, string.Format(LogStr.CRITICAL_NULL_REFERENCE, "PlayerChatController", "World chat TMP_Text"));
            ExceptionUtilities.ThrowIfNull(_gameplayManager, string.Format(LogStr.CRITICAL_NULL_REFERENCE, "PlayerChatController", "GameplayManager"));
        }
        public void SetText(string text)
        {
            if (_gameplayManager.State == GameplayState.MiniGame)
            {
                _chatWidget.SetText(text);
            }
            else
            {
                _worldChatText.text = text;
            }
        }

        public void ShowChat()
        {
            if (_gameplayManager.State == GameplayState.MiniGame)
            {
                _chatWidget.Show();
            }
            else
            {
                _worldChat.SetActive(true);
            }
        }

        public void HideChat()
        {
            _chatWidget.Hide();
            _worldChat.SetActive(false);
        }
    }
}