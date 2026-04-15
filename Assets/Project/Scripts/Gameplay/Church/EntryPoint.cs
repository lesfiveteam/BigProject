using BigProject.Gameplay.Common;
using BigProject.Systems;
using UnityEngine;
using UnityEngine.Assertions;
using BigProject.Player;
using BigProject.Managers;
using BigProject.UI;
using BigProject.Managers.SoundsMusicManagers;

namespace BigProject.Gameplay.Church
{
    public class EntryPoint : MonoBehaviour
    {
        [SerializeField]
        private MiniGameActivator _miniGameActivator;
        [SerializeField]
        private BellsPuzzle _bellsPuzzle;

        private void Awake()
        {
            Assert.IsNotNull(_miniGameActivator, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Mini Game Activator"));
            Assert.IsNotNull(_bellsPuzzle, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Bells Puzzle"));
        }

        public void Init()
        {
            PlayerInputHandler inputHandler = ServiceLocator.GetService<PlayerInputHandler>();
            GameplayManager gameplayManager = ServiceLocator.GetService<GameplayManager>();
            InventoryUI inventoryUI = ServiceLocator.GetService<InventoryUI>();
            PlayerController player = ServiceLocator.GetService<PlayerController>();
            SoundsManager soundsManager = ServiceLocator.GetService<SoundsManager>();

            _miniGameActivator.Init(gameplayManager, inputHandler, inventoryUI, player.GetComponent<Collider>(), 
                player.GetComponentInChildren<SkinnedMeshRenderer>());
            _bellsPuzzle.Init(inputHandler, _miniGameActivator, soundsManager);
        }
    }
}