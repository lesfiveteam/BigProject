using BigProject.Managers;
using BigProject.Player;
using BigProject.Utilities;
using UnityEngine;

namespace BigProject.UI
{
    public class PauseMenuManager : MonoBehaviour
    {
        [SerializeField] private PauseMenuUI _pausePanel;
        [SerializeField] private SettingsMenuUI _settingsPanel;
        private PlayerInputHandler _playerInputHandler;
        private GameplayState _previousState;
        private bool _isPaused = false;

        private void Awake()
        {
            this.gameObject.SetActive(false);
            _settingsPanel.Init(this);
            _pausePanel.Init(this);
        }

        private void OnDestroy()
        {
            if (_playerInputHandler != null)
                _playerInputHandler.PressPause -= PressPause;
        }

        public void Init(PlayerInputHandler playerInputHandler)
        {
            _playerInputHandler = playerInputHandler;
            ExceptionUtilities.ThrowIfNull(_playerInputHandler, gameObject.name, "Player input handler is null!");

            if (_playerInputHandler != null)
                _playerInputHandler.PressPause += PressPause;
        }

        private void PressPause()
        {
            if (!_isPaused)
            {
                PauseGame();
            }
            else
            {
                UnpauseGame();
            }
        }

        public void PauseGame()
        {
            _previousState = ServiceLocator.GetService<GameplayManager>().State;
            if (ServiceLocator.TryGetService(out GameplayManager gameplayManager))
            {
                gameplayManager.ChangeState(GameplayState.Pause);
            }
            this.gameObject.SetActive(true);

            _isPaused = true;
        }

        public void UnpauseGame()
        {
            if (ServiceLocator.TryGetService(out GameplayManager gameplayManager))
            {
                gameplayManager.ChangeState(_previousState);
            }
            GoToPausePanel();
            this.gameObject.SetActive(false);

            _isPaused = false;
        }

        public void GoToSettingsPanel()
        {
            _settingsPanel.gameObject.SetActive(true);
            _pausePanel.gameObject.SetActive(false);
        }

        public void GoToPausePanel()
        {
            _settingsPanel.gameObject.SetActive(false);
            _pausePanel.gameObject.SetActive(true);
        }
    }
}
