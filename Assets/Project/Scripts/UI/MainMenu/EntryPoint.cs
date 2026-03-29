using Assets.Project.Scripts.Managers.SceneLoader;
using BigProject.Managers;
using BigProject.Managers.SoundsMusicManagers;
using BigProject.Systems;
using UnityEngine;
using UnityEngine.Assertions;

namespace BigProject.UI.MainMenu
{
    public class EntryPoint : MonoBehaviour
    {
        [SerializeField]
        private MenuPanelUI _menuPanel;
        [SerializeField]
        private Texture2D _cursorTexture;

        private void Awake()
        {
            // Хак - после предзащиты исправить
            Cursor.SetCursor(_cursorTexture, new Vector2(0, 0), CursorMode.ForceSoftware);
            Assert.IsNotNull(_menuPanel, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, "Menu EntryPoint", "MenuPanelUI"));
            _menuPanel.Init(ServiceLocator.GetService<ProgressManager>(), ServiceLocator.GetService<SceneLoadManager>(),
                ServiceLocator.GetService<SavesManager>(), ServiceLocator.GetService<SoundsManager>());
        }
    }
}