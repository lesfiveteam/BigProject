using BigProject.Managers;
using BigProject.NPC;
using BigProject.Player;
using BigProject.Systems;
using BigProject.Utilities;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Managers.Gameplay
{
    public class AmbassadorDialogueManager : MonoBehaviour
    {
        [SerializeField] private string _playerTag = "Player";
        [SerializeField] private DialogNPC _ambassador;
        [SerializeField] private Transform _villageCentre;
        [SerializeField] private DialogNPC _nextDialogueNPC;
        [SerializeField] private Fader _faderPrefab;

        private PlayerController _player;
        private DialogueManager _dialogueManager;
        private GameplayManager _gameplayManager;

        public void Init(PlayerController player, DialogueManager dialogueManager, GameplayManager gameplayManager)
        {
            _player = player;
            _dialogueManager = dialogueManager;
            _gameplayManager = gameplayManager;
            ExceptionUtilities.ThrowIfNull(_player, String.Format(LogStr.CRITICAL_NULL_REFERENCE, "AmbassadorDialogueManager", "PlayerController"));
            ExceptionUtilities.ThrowIfNull(_dialogueManager, String.Format(LogStr.CRITICAL_NULL_REFERENCE, "AmbassadorDialogueManager", "DialogueManager"));
            ExceptionUtilities.ThrowIfNull(_gameplayManager, String.Format(LogStr.CRITICAL_NULL_REFERENCE, "AmbassadorDialogueManager", "GameplayManager"));
        }

        private void Awake()
        {
            Assert.IsNotNull(_ambassador, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Ambassador Dialogue"));
            Assert.IsNotNull(_villageCentre, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Village Centre"));
            Assert.IsNotNull(_nextDialogueNPC, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Next Dialogue"));
        }

        /// <summary>
        /// Forces player to get to ambassador, then teleports to a specific location and forces to get to the next dialogue
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(_playerTag))
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

            _player.transform.position = _villageCentre.position;
            _player.AutoTarget(_nextDialogueNPC);
            isWaiting = true;
            fader.FadeOut(() => isWaiting = false);
            yield return new WaitUntil(() => !isWaiting);
            DestroyImmediate(fader.gameObject);
            Destroy(gameObject);
        }
    }
}