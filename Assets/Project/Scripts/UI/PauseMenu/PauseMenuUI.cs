using UnityEngine;
using UnityEngine.UI;
using BigProject.Managers;
using Assets.Project.Scripts.Managers.SceneLoader;
using BigProject.Initializers;
using System.Collections;
using BigProject.Systems;

namespace BigProject.UI
{
    public class PauseMenuUI : MonoBehaviour
    {
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _backToMenuButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _quitButton;

        private PauseMenuManager _pauseMenuManager;
        private SceneLoadManager _sceneLoader;

        public void Init(PauseMenuManager pauseMenuManager, SceneLoadManager sceneLoader)
        {
            _pauseMenuManager = pauseMenuManager;
            _sceneLoader = sceneLoader;
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
            _sceneLoader.SceneLoadingStarted += OnMainMenuLoadingStarted;
            _sceneLoader.LoadScene(Scenes.MainMenu);
            _pauseMenuManager.UnpauseGame();
            Destroy(_pauseMenuManager);
        }

        private void OnMainMenuLoadingStarted()
        {
            _sceneLoader.SceneLoadingStarted -= OnMainMenuLoadingStarted;
            GameplaySceneEntryPoint sceneEntryPoint = FindFirstObjectByType<GameplaySceneEntryPoint>();

            if (sceneEntryPoint != null)
            {
                Destroy(sceneEntryPoint);
            }

            Bootstrapper.SetStage(GameExecutionStage.Launch);
        }
    }
}
