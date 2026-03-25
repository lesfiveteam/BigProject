using Assets.Project.Scripts.Managers.SceneLoader;
using BigProject.Player;
using BigProject.Systems;
using BigProject.Utilities;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace BigProject.Gameplay.Common
{
    public abstract class MovingNextSceneHandler : MonoBehaviour
    {
        [SerializeField]
        private Scenes _sceneToLoad;
        [SerializeField]
        private int _spawnPointId = 0;

        private SceneLoadManager _sceneLoader;
        private PlayerSpawner _playerSpawner;

        public void Init(SceneLoadManager sceneLoader, PlayerSpawner playerSpawner)
        {
            _sceneLoader = sceneLoader;
            _playerSpawner = playerSpawner;
            ExceptionUtilities.ThrowIfNull(_sceneLoader, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "SceneLoadManager"));
            ExceptionUtilities.ThrowIfNull(_playerSpawner, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "PlayerSpawner"));
        }

        protected void Move()
        {
            _playerSpawner.SetSpawnPoint(_spawnPointId);
            _sceneLoader.LoadScene(_sceneToLoad);
        }
    }
}
