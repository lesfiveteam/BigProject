using Assets.Project.Scripts.Managers.SceneLoader;
using BigProject.Managers;
using BigProject.Settings;
using BigProject.Systems;
using BigProject.Systems.Inventory;
using BigProject.Utilities;
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

        [Header("Player remarks")]
        [SerializeField]
        private LocalizedString _firstEnterRemark;
        [SerializeField]
        private LocalizedString _touchStillageRemark;
        [SerializeField]
        private LocalizedString _findNoteRemark;

        public void Init(InventorySystem inventory, RuneShardsSystem runesSystem, RunesConfig runesConfig,
            Collider playerCollider, SkinnedMeshRenderer playerRenderer)
        {
            _inventory = inventory;
            _runesSystem = runesSystem;
            _runesConfig = runesConfig;
            _playerRenderer = playerRenderer;
            _playerCollider = playerCollider;
            ExceptionUtilities.ThrowIfNull(_inventory, string.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Inventory System"));
            ExceptionUtilities.ThrowIfNull(_runesSystem, string.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Rune Shards System"));
            ExceptionUtilities.ThrowIfNull(_runesConfig, string.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Runes Config"));
            ExceptionUtilities.ThrowIfNull(_playerRenderer, string.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Player Collider"));
            ExceptionUtilities.ThrowIfNull(_playerCollider, string.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Player Renderer"));
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

            IReadOnlyList<int> rewardRunes = _runesConfig.GetQuestRewardRunes(_questId);
            ExceptionUtilities.ThrowIfNullFormat(rewardRunes, "unable to get reward runes");

            foreach (int rewardRuneId in rewardRunes)
            {
                _runesSystem.AddRunesSegment(rewardRuneId);
            }
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
        }

        public void ActivatePlayer()
        {
            _playerRenderer.enabled = true;
            _playerCollider.enabled = true;
        }
    }
}
