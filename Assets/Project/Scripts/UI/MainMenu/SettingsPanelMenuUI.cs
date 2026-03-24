using UnityEngine;
using UnityEngine.UI;

namespace BigProject.UI
{
    public class SettingsPanelMenuUI : MainMenuPanel
    {
        [SerializeField] private Button _backButton;

        private void OnEnable()
        {
            _backButton.onClick.AddListener(() =>
            {
                _mainMenuPanelManager.GetMenuPanel().gameObject.SetActive(true);
                _mainMenuPanelManager.GetStudioLogo().SetActive(true);
                _mainMenuPanelManager.ToggleBlur(false);
                gameObject.SetActive(false);
            });
        }

        private void OnDisable()
        {
            _backButton.onClick.RemoveAllListeners();
        }
    }
}
