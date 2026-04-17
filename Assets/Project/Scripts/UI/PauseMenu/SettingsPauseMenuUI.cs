using BigProject.Managers;
using BigProject.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace BigProject.UI
{
    public class SettingsPauseMenuUI : SettingsBaseUI
    {
         private PauseMenuManager _pauseMenuManager;

        public void Init(SettingsManager settingsManager, PauseMenuManager pauseMenuManager)
        {
            _pauseMenuManager = pauseMenuManager;
            _settingsManager = settingsManager;

            ExceptionUtilities.ThrowIfNull(_settingsManager, gameObject.name, "Settings Manager is null!");
            ExceptionUtilities.ThrowIfNull(_pauseMenuManager, gameObject.name, "Pause Menu Manager is null!");

            SetResolutionDropdown();
            SetScreenModeDropdown();
            SetSoundVolumeSlider();
            SetMusicVolumeSlider();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _backButton.onClick.AddListener(_pauseMenuManager.GoToPausePanel);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            _backButton.onClick.RemoveAllListeners();
        }
    }
}