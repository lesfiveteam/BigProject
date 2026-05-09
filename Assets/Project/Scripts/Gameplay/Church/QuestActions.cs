using Assets.Project.Scripts.Managers.SceneLoader;
using BigProject.Managers;
using BigProject.Settings;
using BigProject.Systems;
using BigProject.Systems.HUD;
using BigProject.Systems.Inventory;
using BigProject.UI;
using BigProject.Utilities;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Localization;

namespace BigProject.Gameplay.Church
{
    public class QuestActions : MonoBehaviour
    {
        private InventorySystem _inventory;
        private RuneShardsSystem _runesSystem;
        private RunesConfig _runesConfig;
        private SkinnedMeshRenderer _playerRenderer;
        private Collider _playerCollider;
        private RunesDriver _runesDriver;
        private GameplayManager _gameplayManager;
        private HUD _hud;
        private InventoryUI _inventoryUI;

        [SerializeField]
        private string _noteOne, _noteThree, _noteFour;

        [SerializeField]
        private int _questId;


        [SerializeField]
        private GameObject _priest;
        [SerializeField]
        private GameObject _priestFinishPosition;
        [SerializeField]
        private CinemachineCamera _puzzleCamera;
        [SerializeField]
        private HUDConfig _hudConfig;

        [Header("Player remarks")]
        [SerializeField]
        private LocalizedString _firstEnterRemark;
        [SerializeField]
        private LocalizedString _touchStillageRemark;
        [SerializeField]
        private LocalizedString _findNoteRemark;

        public void Init(InventorySystem inventory, RuneShardsSystem runesSystem, RunesConfig runesConfig, RunePanelUI runePanel,
            Collider playerCollider, SkinnedMeshRenderer playerRenderer, GameplayManager gameplayManager, HUD hud, InventoryUI inventoryUI)
        {
            _inventory = inventory;
            _playerRenderer = playerRenderer;
            _playerCollider = playerCollider;
            _gameplayManager = gameplayManager;
            _hud = hud;
            _inventoryUI = inventoryUI;
            ExceptionUtilities.ThrowIfNull(_inventory, string.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Inventory System"));
            ExceptionUtilities.ThrowIfNull(_playerRenderer, string.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Player Collider"));
            ExceptionUtilities.ThrowIfNull(_playerCollider, string.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Player Renderer"));
            ExceptionUtilities.ThrowIfNull(_hud, string.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "HUD"));
            ExceptionUtilities.ThrowIfNull(_inventoryUI, string.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "InventoryUI"));
            _runesDriver = new(runesSystem, runesConfig, runePanel, _questId, _priest.transform);
        }

        public void AddNoteOne()
        {
            _inventory.AddItemByName(_noteOne);
            ReplicaManager.ShowReplica(_findNoteRemark);
        }

        public void AddNoteFour()
        {
            _inventory.RemoveItemByName(_noteThree);
            _inventory.AddItemByName(_noteFour);
        }

        // For test build
        public void FinishGame()
        {
            Invoke("StartOutro", 2f);
        }

        private void StartOutro()
        {
            ServiceLocator.GetService<SceneLoadManager>().LoadScene(Scenes.Outro);
        }
        public void TouchStillageAction()
        {
            ReplicaManager.ShowReplica(_touchStillageRemark); // При начале обыске стиллажа
        }
        public void MovePriestToFinishPosition()
        {
            _priest.transform.position = _priestFinishPosition.transform.position;
        }

        public void DeactivatePuzzleCamera()
        {
            _puzzleCamera.enabled = false;
            _inventoryUI.SetNoteVisibility(false);
        }

        public void ActivatePlayer()
        {
            _playerRenderer.enabled = true;
            _playerCollider.enabled = true;
        }

        public void GetRunes()
        {
            StartCoroutine(GetRunesRoutine());
        }

        private IEnumerator GetRunesRoutine()
        {
            yield return new WaitUntil(() => _gameplayManager.State == GameplayState.Play);
            _hud.ShowWidget(_hudConfig.HUDRunesWidgetId);
            _runesDriver.Deliver();
        }
    }
}
