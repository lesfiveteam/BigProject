using BigProject.Managers.SoundsMusicManagers;
using BigProject.Systems;
using BigProject.Utilities;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace BigProject.UI
{
    public class MainMenuPanelManager : MonoBehaviour
    {
        [SerializeField] private GameObject _menuPanel;
        [SerializeField] private SettingsPanelMenuUI _settingsPanel;
        [SerializeField] private GameObject _studioLogo;
        [SerializeField] private GameObject _blurScreen;
        [SerializeField] private Camera _sceneCamera;
        [SerializeField] private Volume _blurVolume;
        [SerializeField] private AudioClip _clickSound;
        [SerializeField] private AudioClip _focusedSound;
        private DepthOfField _blurDepthOfField;
        private UniversalAdditionalCameraData _cameraData;
        private SoundsManager _soundsManager;

        private void Awake()
        {
            if (!_blurVolume.profile.TryGet(out _blurDepthOfField))
            {
                Debug.LogError("Global Volume for blur wasn't set in MainMenuPanelManager");
            }
            if (_menuPanel == null)
            {
                Debug.LogError("menuPanel wasn't set in MainMenyPanelManager");
            }
            if (_settingsPanel == null)
            {
                Debug.LogError("settingsPanel wasn't set in MainMenyPanelManager");
            }
            if (_sceneCamera == null)
            {
                Debug.LogError("Camera wasn't set in MainMenyPanelManager");
            }

            _cameraData = _sceneCamera.GetComponent<UniversalAdditionalCameraData>();
        }

        public void Init(SoundsManager soundsManager)
        {
            _soundsManager = soundsManager;
            ExceptionUtilities.ThrowIfNull(_soundsManager, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "SoundsManager"));
        }

        public GameObject GetMenuPanel() { return _menuPanel; }
        public SettingsPanelMenuUI GetSettingsPanel() { return _settingsPanel; }
        public GameObject GetStudioLogo() { return _studioLogo; }
        public GameObject GetBlurScreen() { return _blurScreen; }

        public void OnButtonClickSound(Button button)
        {
            if (button.interactable)
                _soundsManager.PlaySound(_clickSound, is2D: true);
        }

        public void OnButtonFocusedSound(Button button)
        {
            if (button.interactable)
                _soundsManager.PlaySound(_focusedSound, is2D: true);
        }

        public void ToggleBlur(bool isBlurOn)
        {
            _cameraData.renderPostProcessing = isBlurOn;
            _blurDepthOfField.focusDistance.value = isBlurOn ? 0.8f : 10f;
            _blurScreen.SetActive(isBlurOn);
        }
    }
}