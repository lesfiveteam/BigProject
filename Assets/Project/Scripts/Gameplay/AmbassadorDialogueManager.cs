using BigProject.Managers;
using BigProject.NPC;
using BigProject.Player;
using BigProject.Systems;
using BigProject.Systems.QuestSystem;
using BigProject.Utilities;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

namespace Managers.Gameplay
{
    public class AmbassadorDialogueManager : MonoBehaviour
    {
        [SerializeField] private string _playerTag = "Player";
        [SerializeField] private DialogNPC _ambassador;
        [SerializeField] private DialogNPC _nextDialogueNPC;
        [SerializeField] private Fader _faderPrefab;
        [SerializeField] private int _spawnPointId;
        [SerializeField] private Collider _trigger;
        [SerializeField] private QuestActionHandlerMono _takeToElderActionHandler;
        [SerializeField] private Collider _dialogueCollider;
        [SerializeField] private Collider _commentHandlerCollider;

        private PlayerController _player;
        private PlayerSpawner _playerSpawner;
        private DialogueManager _dialogueManager;
        private GameplayManager _gameplayManager;

        public void Init(PlayerController player, PlayerSpawner playerSpawner, DialogueManager dialogueManager, GameplayManager gameplayManager)
        {
            _player = player;
            _playerSpawner = playerSpawner;
            _dialogueManager = dialogueManager;
            _gameplayManager = gameplayManager;
            ExceptionUtilities.ThrowIfNull(_player, String.Format(LogStr.CRITICAL_NULL_REFERENCE, "AmbassadorDialogueManager", "PlayerController"));
            ExceptionUtilities.ThrowIfNull(_playerSpawner, String.Format(LogStr.CRITICAL_NULL_REFERENCE, "AmbassadorDialogueManager", "PlayerSpawner"));
            ExceptionUtilities.ThrowIfNull(_dialogueManager, String.Format(LogStr.CRITICAL_NULL_REFERENCE, "AmbassadorDialogueManager", "DialogueManager"));
            ExceptionUtilities.ThrowIfNull(_gameplayManager, String.Format(LogStr.CRITICAL_NULL_REFERENCE, "AmbassadorDialogueManager", "GameplayManager"));
        }

        public void MoveToElder()
        {
            Destroy(_trigger);
            _dialogueCollider.enabled = false;
            _commentHandlerCollider.enabled = true;
        }

        private void Awake()
        {
            if (_takeToElderActionHandler.CurrentState == QuestActionState.Released)
            {
                Destroy(gameObject);
                return;
            }

            Assert.IsNotNull(_ambassador, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Ambassador Dialogue"));
            Assert.IsNotNull(_nextDialogueNPC, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Next Dialogue"));
            Assert.IsNotNull(_faderPrefab, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Fader"));
            Assert.IsNotNull(_trigger, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Trigger Collider"));
            Assert.IsNotNull(_takeToElderActionHandler, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Trigger Collider"));
            Assert.IsNotNull(_dialogueCollider, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Dialogue Collider"));
            Assert.IsNotNull(_commentHandlerCollider, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Comment Handler Collider"));
        }

        /// <summary>
        /// Forces player to get to ambassador, then teleports to a specific location and forces to get to the next dialogue
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(_playerTag) || _player.IsAutopilot)
            {
                return;
            }

            _gameplayManager.ChangeState(GameplayState.Dialogue);
            StartCoroutine(AmbassadorDialogueRoutine());
        }

        private IEnumerator AmbassadorDialogueRoutine()
        {
            _player.AutoTarget(_ambassador);

            while (_dialogueManager.IsDialogue || _player.IsAutopilot)
            {
                yield return null;
            }
            
            _gameplayManager.ChangeState(GameplayState.Dialogue);
            Fader fader = Instantiate(_faderPrefab);
            bool isWaiting = true;
            fader.FadeIn(() => isWaiting = false);
            yield return new WaitUntil(() => !isWaiting);
            _playerSpawner.PositionPlayer(_spawnPointId);
            _player.AutoTarget(_nextDialogueNPC);
            _takeToElderActionHandler.MakeTransition(0);
            isWaiting = true;
            fader.FadeOut(() => isWaiting = false);
            yield return new WaitUntil(() => !isWaiting);
            DestroyImmediate(fader.gameObject);
            Destroy(this);
        }
    }
}