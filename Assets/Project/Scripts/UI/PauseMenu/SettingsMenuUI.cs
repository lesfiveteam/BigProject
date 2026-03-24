using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BigProject.UI
{
    public class SettingsMenuUI : MonoBehaviour
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private TMP_Dropdown _resolutionDropdown;
        [SerializeField] private TMP_Dropdown _screenModeDropdown;
        [SerializeField] private Slider _soundVolumeSlider;
        [SerializeField] private Slider _musicVolumeSlider;


        private PauseMenuManager _pauseMenuManager;

        public void Init(PauseMenuManager pauseMenuManager)
        {
            _pauseMenuManager = pauseMenuManager;
        }

        private void OnEnable()
        {
            _backButton.onClick.AddListener(_pauseMenuManager.GoToPausePanel);
        }

        private void OnDisable()
        {
            _backButton.onClick.RemoveAllListeners();
        }
    }
}