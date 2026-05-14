using Assets.Project.Scripts.Managers.SceneLoader;
using BigProject.Initializers;
using BigProject.Managers;
using BigProject.Managers.SoundsMusicManagers;
using BigProject.Settings;
using BigProject.Systems;
using BigProject.Systems.QuestSystem;
using BigProject.Utilities;
using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace BigProject.UI
{
    public class MenuPanelUI : MonoBehaviour
    {
        [SerializeField] protected MainMenuPanelManager _mainMenuPanelManager;
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _quitButton;
        [SerializeField] private AudioClip _clickSound;
        [SerializeField] private GlobalConfig _globalConfig;
        [SerializeField] private Animator _boyAnimator;
        [SerializeField] private Animator _backgroundAnimator;
        [SerializeField] private AudioListener _audioListener;

        private const string ANIM_START_TRIGGER = "Start";

        private ProgressManager _progressManager;
        private SceneLoadManager _sceneLoader;
        private SavesManager _savesManager;
        private SoundsManager _soundsManager;
        private SettingsManager _settingsManager;

        public void Init(ProgressManager progressManager, SceneLoadManager sceneLoader, SavesManager savesManager, SoundsManager soundsManager, SettingsManager settingsManager)
        {
            _progressManager = progressManager;
            _sceneLoader = sceneLoader;
            _savesManager = savesManager;
            _soundsManager = soundsManager;
            _settingsManager = settingsManager;
            ExceptionUtilities.ThrowIfNull(_progressManager, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "ProgressManager"));
            ExceptionUtilities.ThrowIfNull(_sceneLoader, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "SceneLoadManager"));
            ExceptionUtilities.ThrowIfNull(_savesManager, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "SavesManager"));
            ExceptionUtilities.ThrowIfNull(_soundsManager, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "SoundsManager"));
            ExceptionUtilities.ThrowIfNull(_settingsManager, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "SettingsManager"));

            _mainMenuPanelManager.GetSettingsPanel().Init(_settingsManager);
        }

        private void Awake()
        {
            Assert.IsNotNull(_newGameButton, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "New Game Button"));
            Assert.IsNotNull(_continueButton, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Continue Button"));
            Assert.IsNotNull(_settingsButton, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Settings Button"));
            Assert.IsNotNull(_quitButton, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Exit Button"));
            Assert.IsNotNull(_globalConfig, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "GlobalConfig"));
            Assert.IsNotNull(_audioListener, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "AudioListener"));
        }

        private void Start()
        {
            _continueButton.interactable = _progressManager.HasSavedProgress();
        }

        private void OnEnable()
        {
            _newGameButton.onClick.AddListener(() =>
            {
                _soundsManager.PlaySound(_clickSound, is2D: true);

                if (_sceneLoader.IsLoading)
                {
                    return;
                }

                _savesManager.DeleteSave(_globalConfig.PlayerProfileName);
                _savesManager.DeleteSave($"{_globalConfig.PlayerProfileName}_{ProgressManager.ADDITIONAL_DATA_NAME}");

                if (!Bootstrapper.IsFirstPlay)
                {
                    _progressManager.Reload(new QuestJsonLoader(_globalConfig.QuestsFolder));
                }

                PlayAnimations();
                _sceneLoader.LoadScene(Scenes.Intro);
            });

            _continueButton.onClick.AddListener(() =>
            {
                _soundsManager.PlaySound(_clickSound, is2D: true);

                if (_sceneLoader.IsLoading)
                {
                    return;
                }

                _progressManager.LoadProgress();
                PlayAnimations();
                _sceneLoader.SceneLoadingStarted += OnGameLoadingStarted;
                _sceneLoader.LoadScene(Scenes.Village);
            });

            _settingsButton.onClick.AddListener(() =>
            {
                _soundsManager.PlaySound(_clickSound, is2D: true);
                _mainMenuPanelManager.GetSettingsPanel().gameObject.SetActive(true);
                _mainMenuPanelManager.GetStudioLogo().SetActive(false);
                _mainMenuPanelManager.ToggleBlur(true);
                gameObject.SetActive(false);
            });

            _quitButton.onClick.AddListener(() =>
            {
                _soundsManager.PlaySound(_clickSound, is2D: true);
                Debug.Log(String.Format(LogStr.INFO_SYSTEM, "MainMenu", "clicked Quit Button"));
                Application.Quit();
            });
        }

        private void OnGameLoadingStarted()
        {
            _sceneLoader.SceneLoadingStarted -= OnGameLoadingStarted;
            _audioListener.enabled = false;
            Bootstrapper.SetStage(GameExecutionStage.Gameplay);
        }

        private void OnDisable()
        {
            _newGameButton.onClick.RemoveAllListeners();
            _continueButton.onClick.RemoveAllListeners();
            _settingsButton.onClick.RemoveAllListeners();
            _quitButton.onClick.RemoveAllListeners();
        }

        private void PlayAnimations()
        {
            _boyAnimator.SetTrigger(ANIM_START_TRIGGER);
            _backgroundAnimator.SetTrigger(ANIM_START_TRIGGER);
        }
    }
}