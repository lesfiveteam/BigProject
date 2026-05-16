using BigProject.Gameplay.Common;
using BigProject.Managers;
using BigProject.Managers.SoundsMusicManagers;
using BigProject.Player;
using BigProject.Systems;
using BigProject.Systems.HUD;
using BigProject.Systems.Inventory;
using BigProject.UI;
using UnityEngine;
using UnityEngine.Assertions;

namespace BigProject.Gameplay.Watermill
{
    public class EntryPoint : MonoBehaviour
    {
        private const float FADE_OUT_MUSIC_DURATION = 0.1f;
        private const float FADE_IN_MUSIC_DURATION = 0.1f;
        private const float IN_BUILD_MUSIC_VOLUME = 0.1f;
        private const float MILL_VOLUME = 0.1f;

        [SerializeField]
        private ControlPanel _controlPanel;
        [SerializeField]
        private MiniGameActivator _miniGameActivator;
        [SerializeField]
        private AudioClip _villageMusic;
        [SerializeField]
        private AudioClip _millSound;

        private void Awake()
        {
            Assert.IsNotNull(_controlPanel, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "_controlPanel"));
            Assert.IsNotNull(_miniGameActivator, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "_miniGameActivator"));
            Assert.IsNotNull(_villageMusic, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "_villageMusic"));
            Assert.IsNotNull(_millSound, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "_millSound"));
        }

        public void Init()
        {
            MusicManager musicManager = ServiceLocator.GetService<MusicManager>();
            SoundsManager soundsManager = ServiceLocator.GetService<SoundsManager>();
            GameplayManager gameplayManager = ServiceLocator.GetService<GameplayManager>();
            PlayerInputHandler inputHandler = ServiceLocator.GetService<PlayerInputHandler>();
            PlayerController player = ServiceLocator.GetService<PlayerController>();

            musicManager.PlayMusic(_villageMusic, FADE_OUT_MUSIC_DURATION, FADE_IN_MUSIC_DURATION, IN_BUILD_MUSIC_VOLUME);

            if (_controlPanel.CurrentPanelState == ControlPanelState.Completed)
            {
                soundsManager.PlaySound(_millSound, volume: MILL_VOLUME, isLooped: true);
            }

            _controlPanel.Init(
                gameplayManager, 
                inputHandler, 
                ServiceLocator.GetService<InventorySystem>(), 
                ServiceLocator.GetService<HUD>(), soundsManager);

            _miniGameActivator.Init(
                gameplayManager, 
                inputHandler, 
                ServiceLocator.GetService<InventoryUI>(), 
                player.GetComponent<Collider>(),
                player.GetComponentInChildren<SkinnedMeshRenderer>());

            SwithOffOutline(player);
        }

        private void SwithOffOutline(PlayerController player)
        {
            Outline outline = player.GetComponentInChildren<Outline>();

            if (outline != null)
                outline.enabled = false;
        }
    }
}