using Assets.Project.Scripts.Managers.SceneLoader;
using BigProject.Managers;
using BigProject.Managers.SoundsMusicManagers;
using BigProject.Player;
using BigProject.Systems;
using UnityEngine;
using UnityEngine.Assertions;

namespace BigProject.UI.MainMenu
{
    public class EntryPoint : MonoBehaviour
    {
        [SerializeField]
        private MainMenuPanelManager _mainMenuPanelManager;
        [SerializeField]
        private MenuPanelUI _menuPanel;
        [SerializeField]
        private AudioClip _audioClip;

        private void Awake()
        {
            // Хак - после предзащиты исправить
            MusicManager musicManager = ServiceLocator.GetService<MusicManager>();
            musicManager.PlayMusic(_audioClip);
            Assert.IsNotNull(_menuPanel, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, "Menu EntryPoint", "MenuPanelUI"));
            _mainMenuPanelManager.Init(ServiceLocator.GetService<SoundsManager>());
            _menuPanel.Init(ServiceLocator.GetService<ProgressManager>(), ServiceLocator.GetService<SceneLoadManager>(),
                ServiceLocator.GetService<SavesManager>(),
                ServiceLocator.GetService<SettingsManager>(), ServiceLocator.GetService<PlayerLocation>());
        }
    }
}