using BigProject.Managers;
using BigProject.NPC;
using BigProject.Player;
using BigProject.Systems;
using BigProject.Systems.QuestSystem;
using BigProject.Utilities;
using System;
using UnityEngine;
using UnityEngine.Assertions;


namespace BigProject.Gameplay.VillageIntro
{
    public class QuestActions : MonoBehaviour
    {
        [SerializeField]
        private QuestActionHandlerMono _actionHandler;
        [SerializeField]
        private DialogNPC _edler;

        private PlayerController _player;
        private GameplayManager _gameplayManager;

        private void Awake()
        {
            Assert.IsNotNull(_actionHandler, String.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "QuestActionHandlerMono"));
            Assert.IsNotNull(_edler, String.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Elder DialogNPC"));
        }

        public void Init(PlayerController player, GameplayManager gameplayManager)
        {
            _player = player;
            _gameplayManager = gameplayManager;
            ExceptionUtilities.ThrowIfNull(_player, String.Format(gameObject.name, "PlayerController"));
            ExceptionUtilities.ThrowIfNull(_gameplayManager, String.Format(gameObject.name, "GameplayManager"));
        }

        public void SpeakWithElder()
        {
            _player.AutoTarget(_edler);
            _gameplayManager.ChangeState(GameplayState.Cutscene);
            _actionHandler.MakeTransition(0);
        }
    }
}