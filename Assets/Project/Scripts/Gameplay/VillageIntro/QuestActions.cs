using Assets.Project.Scripts.Managers.SceneLoader;
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
using UnityEngine.SceneManagement;


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
        private SceneLoadManager _sceneLoader;
        private bool _isSceneLoaded;

        private void Awake()
        {
            Assert.IsNotNull(_actionHandler, String.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "QuestActionHandlerMono"));
            Assert.IsNotNull(_edler, String.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Elder DialogNPC"));
        }

        public void Init(PlayerController player, GameplayManager gameplayManager, SceneLoadManager sceneLoader)
        {
            _player = player;
            _gameplayManager = gameplayManager;
            _sceneLoader = sceneLoader;
            ExceptionUtilities.ThrowIfNull(_player, String.Format(gameObject.name, "PlayerController"));
            ExceptionUtilities.ThrowIfNull(_gameplayManager, String.Format(gameObject.name, "GameplayManager"));
            ExceptionUtilities.ThrowIfNull(_sceneLoader, String.Format(gameObject.name, "SceneLoadManager"));
        }

        public void SpeakWithElder()
        {
            StopAllCoroutines();
            StartCoroutine(SpeakWithElderRoutine());
        }

        private IEnumerator SpeakWithElderRoutine()
        {
            yield return new WaitUntil(() => _isSceneLoaded);
            _player.AutoTarget(_edler);
            _actionHandler.MakeTransition(0);
        }

        private void OnSceneLoadingCompleted() => _isSceneLoaded = true;

        private void OnEnable()
        {
            if (_sceneLoader.IsLoading)
            {
                _sceneLoader.SceneLoadingCompleted += OnSceneLoadingCompleted;
            }
            else
            {
                _isSceneLoaded = true;
            }
        }

        private void OnDisable()
        {
            _sceneLoader.SceneLoadingCompleted -= OnSceneLoadingCompleted;
        }
    }
}