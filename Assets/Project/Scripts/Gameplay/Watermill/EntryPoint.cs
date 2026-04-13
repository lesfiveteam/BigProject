using BigProject.Gameplay.Common;
using BigProject.Managers;
using BigProject.Player;
using BigProject.Managers.SoundsMusicManagers;
using BigProject.Systems.Inventory;
using BigProject.UI;
using UnityEngine;
using BigProject.Systems.HUD;

namespace BigProject.Gameplay.Watermill
{
    public class EntryPoint : MonoBehaviour
    {
        [SerializeField]
        private ControlPanel _controlPanel;
        [SerializeField]
        private MiniGameActivator _miniGameActivator;
        [SerializeField]
        private AudioClip _millFixedMusic;
        [SerializeField]
        private AudioClip _millBrokenMusic;

        public void Init()
        {
            MusicManager musicManager = ServiceLocator.GetService<MusicManager>();
            musicManager.PlayMusic(_controlPanel.CurrentPanelState == ControlPanelState.Completed ? _millFixedMusic : _millBrokenMusic, 0.1f, 0.1f);
            GameplayManager gameplayManager = ServiceLocator.GetService<GameplayManager>();
            PlayerInputHandler inputHandler = ServiceLocator.GetService<PlayerInputHandler>();
            _controlPanel.Init(gameplayManager, inputHandler, ServiceLocator.GetService<InventorySystem>(), musicManager, ServiceLocator.GetService<HUD>());
            PlayerController player = ServiceLocator.GetService<PlayerController>();
            _miniGameActivator.Init(gameplayManager, inputHandler, ServiceLocator.GetService<InventoryUI>(), player.GetComponent<Collider>(),
                player.GetComponentInChildren<SkinnedMeshRenderer>());
        }
    }
}