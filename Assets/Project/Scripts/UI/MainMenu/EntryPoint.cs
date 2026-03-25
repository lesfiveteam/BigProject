using Assets.Project.Scripts.Managers.SceneLoader;
using BigProject.Managers;
using BigProject.Systems;
using UnityEngine;
using UnityEngine.Assertions;

namespace BigProject.UI.MainMenu
{
    public class EntryPoint : MonoBehaviour
    {
        [SerializeField]
        private MenuPanelUI _menuPanel;

        private void Awake()
        {
            Assert.IsNotNull(_menuPanel, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, "Menu EntryPoint", "MenuPanelUI"));
            _menuPanel.Init(ServiceLocator.GetService<ProgressManager>(), ServiceLocator.GetService<SceneLoadManager>(),
                ServiceLocator.GetService<SavesManager>());
        }
    }
}