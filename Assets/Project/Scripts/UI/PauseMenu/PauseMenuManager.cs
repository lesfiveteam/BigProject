using Assets.Project.Scripts.Managers.SceneLoader;
using BigProject.Managers;
using BigProject.Player;
using BigProject.Systems;
using BigProject.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BigProject.UI
{
    public class PauseMenuManager : MonoBehaviour
    {
        [SerializeField] private PauseMenuUI _pausePanel;
        [SerializeField] private SettingsPauseMenuUI _settingsPanel;
        private PlayerInputHandler _playerInputHandler;
        private GameplayState _previousState;
        private bool _isPaused = false;
        private SceneLoadManager _sceneLoader;

        private void Awake()
        {
            this.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_playerInputHandler != null)
            {
                _playerInputHandler.PressPause -= PressPause;
            }

            _sceneLoader.SceneLoaded -= OnSceneLoaded;
        }

        public void Init(PlayerInputHandler playerInputHandler, SettingsManager settingsManager, SceneLoadManager sceneLoader)
        {
            _playerInputHandler = playerInputHandler;
            _sceneLoader = sceneLoader;
            ExceptionUtilities.ThrowIfNull(_playerInputHandler, string.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "PlayerInputHandler"));
            ExceptionUtilities.ThrowIfNull(_sceneLoader, string.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "SceneLoadManager"));

            _settingsPanel.Init(settingsManager, this);
            _pausePanel.Init(this, sceneLoader);



            if (SceneManager.GetActiveScene().name.Equals(Scenes.Intro.ToString()))
            {
                _sceneLoader.SceneLoaded += OnSceneLoaded;
            }
            else
            {
                _playerInputHandler.PressPause += PressPause;
            }

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

        private void OnSceneLoaded(Scenes _)
        {
            _playerInputHandler.PressPause += PressPause;
            _sceneLoader.SceneLoaded -= OnSceneLoaded;
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
