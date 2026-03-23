using UnityEngine;
using UnityEngine.UI;
using BigProject.Managers;

namespace BigProject.UI
{
    public class PauseMenuUI : MonoBehaviour
    {
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _backToMenuButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _quitButton;

        private PauseMenuManager _pauseMenuManager;

        public void Init(PauseMenuManager pauseMenuManager)
        {
            _pauseMenuManager = pauseMenuManager;
        }

        private void OnEnable()
        {
            _resumeButton.onClick.AddListener(_pauseMenuManager.UnpauseGame);
            _backToMenuButton.onClick.AddListener(GoToMainMenu);
            _settingsButton.onClick.AddListener(_pauseMenuManager.GoToSettingsPanel);
            _quitButton.onClick.AddListener(QuitGame);
        }

        private void OnDisable()
        {
            _resumeButton.onClick.RemoveListener(_pauseMenuManager.UnpauseGame);
            _backToMenuButton.onClick.RemoveListener(GoToMainMenu);
            _settingsButton.onClick.RemoveListener(_pauseMenuManager.GoToSettingsPanel);
            _quitButton.onClick.RemoveListener(QuitGame);
        }

        private void QuitGame()
        {
            Application.Quit();
        }

        private void GoToMainMenu()
        {
            if (ServiceLocator.TryGetService(out SceneLoadManager sceneLoader))
            {
                sceneLoader.LoadScene(Scenes.MainMenu);
                _pauseMenuManager.UnpauseGame();
                //Destroy(_pauseMenuManager.gameObject);
            }
        }
    }
}
