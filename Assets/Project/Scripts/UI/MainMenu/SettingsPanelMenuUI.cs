using BigProject.Managers;
using BigProject.Utilities;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BigProject.UI
{
    public class SettingsPanelMenuUI : SettingsBaseUI
    {
        [SerializeField] private MainMenuPanelManager _mainMenuPanelManager;

        public void Init(SettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
            ExceptionUtilities.ThrowIfNull(_settingsManager, gameObject.name, "Settings Manager is null!");

            SetResolutionDropdown();
            SetScreenModeDropdown();
            SetSoundVolumeSlider();
            SetMusicVolumeSlider();
        }

        protected override void OnEnable()
        { 
            base.OnEnable();
            
            _backButton.onClick.AddListener(() =>
            {
                _mainMenuPanelManager.GetMenuPanel().gameObject.SetActive(true);
                _mainMenuPanelManager.GetStudioLogo().SetActive(true);
                _mainMenuPanelManager.ToggleBlur(false);
                gameObject.SetActive(false);
            });
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            _backButton.onClick.RemoveAllListeners();
        }
    }


    /// <summary>
    /// Base class for settings UI
    /// </summary>
    [HideInInspector]
    public class SettingsBaseUI : MonoBehaviour
    {
        [SerializeField] protected Button _backButton;
        [SerializeField] private TMP_Dropdown _resolutionDropdown;
        [SerializeField] private TMP_Dropdown _screenModeDropdown;
        [SerializeField] private Slider _soundVolumeSlider;
        [SerializeField] private Slider _musicVolumeSlider;

        protected SettingsManager _settingsManager;

        protected virtual void OnEnable()
        {
            _resolutionDropdown.onValueChanged.AddListener(SetResolution);
            _screenModeDropdown.onValueChanged.AddListener(SetScreenMode);
            _soundVolumeSlider.onValueChanged.AddListener(SoundVolumeChanged);
            _musicVolumeSlider.onValueChanged.AddListener(MusicVolumeChanged);
        }

        protected virtual void OnDisable()
        {
            _backButton.onClick.RemoveAllListeners();
            _soundVolumeSlider.onValueChanged.RemoveListener(SoundVolumeChanged);
            _resolutionDropdown.onValueChanged.RemoveListener(SetResolution);
        }

        protected void SetResolutionDropdown()
        {
            _resolutionDropdown.ClearOptions();
            List<Resolution> filteredResolutions = _settingsManager.GetPossibleResolutions();

            filteredResolutions.Sort((a, b) =>
            {
                if (a.width != b.width)
                    return b.width.CompareTo(a.width);
                else
                    return b.height.CompareTo(a.height);
            });

            int currentResolutionIndex = 0;

            List<string> options = new List<string>();
            for (int i = 0; i < filteredResolutions.Count; i++)
            {
                string resolutionOption = filteredResolutions[i].width + ":" + filteredResolutions[i].height;
                options.Add(resolutionOption);
                if (filteredResolutions[i].width == Screen.width && filteredResolutions[i].height == Screen.height
                    && (float)filteredResolutions[i].refreshRateRatio.value == (float)Screen.currentResolution.refreshRateRatio.value)
                {
                    currentResolutionIndex = i;
                    _settingsManager.SetCurrentResolutionIndex(i);
                }
            }
            _resolutionDropdown.AddOptions(options);
            _resolutionDropdown.value = currentResolutionIndex;
            _resolutionDropdown.RefreshShownValue();
            SetResolution(currentResolutionIndex);
        }

        protected void SetScreenModeDropdown()
        {
            _screenModeDropdown.ClearOptions();
            _screenModeDropdown.AddOptions(new List<string> { "Полный экран", "Окно" });
            _screenModeDropdown.value = Screen.fullScreen ? 0 : 1;
            _screenModeDropdown.RefreshShownValue();
            //SetScreenMode(0);
        }

        protected void SetSoundVolumeSlider()
        {
            _soundVolumeSlider.value = _settingsManager.GetSoundVolume();
        }

        protected void SetMusicVolumeSlider()
        {
            _musicVolumeSlider.value = _settingsManager.GetMusicVolume();
        }


        private void SetResolution(int resolutionIndex)
        {
            Resolution resolution = _settingsManager.GetPossibleResolutions()[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, _settingsManager.IsFullscreen());
        }

        private void SetScreenMode(int id)
        {
            Resolution resolution = _settingsManager.GetPossibleResolutions()[_settingsManager.GetChosenResolutionIndex()];
            _settingsManager.SetIsFullscreen(id == 0);
            Screen.SetResolution(resolution.width, resolution.height, _settingsManager.IsFullscreen());
        }

        private void SoundVolumeChanged(float val)
        {
            _settingsManager.SetSoundVolume(val);    
        }
        private void MusicVolumeChanged(float val)
        {
            _settingsManager.SetMusicVolume(val);
        }
    }
}
