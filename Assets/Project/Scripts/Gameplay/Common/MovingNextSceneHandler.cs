using Assets.Project.Scripts.Managers.SceneLoader;
using BigProject.Managers.SoundsMusicManagers;
using BigProject.Player;
using BigProject.Systems;
using BigProject.Utilities;
using System;
using UnityEngine;

namespace BigProject.Gameplay.Common
{
    public abstract class MovingNextSceneHandler : MonoBehaviour
    {
        [SerializeField]
        private Scenes _sceneToLoad;
        [SerializeField]
        private int _spawnPointId = 0;
        [SerializeField]
        private AudioClip _changeSceneSound;
        [SerializeField]
        private float _changeSceneSoundVolume = 1f;

        private SceneLoadManager _sceneLoader;
        private PlayerSpawner _playerSpawner;
        private SoundsManager _soundsManager;

        public void Init(SceneLoadManager sceneLoader, PlayerSpawner playerSpawner, SoundsManager soundsManager)
        {
            _sceneLoader = sceneLoader;
            _playerSpawner = playerSpawner;
            _soundsManager = soundsManager;
            ExceptionUtilities.ThrowIfNull(_sceneLoader, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "SceneLoadManager"));
            ExceptionUtilities.ThrowIfNull(_playerSpawner, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "PlayerSpawner"));
            ExceptionUtilities.ThrowIfNull(_playerSpawner, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "SoundsManager"));
        }

        protected void Move()
        {
            if(_changeSceneSound !=null)
            {
                _soundsManager.PlaySound(_changeSceneSound, volume: _changeSceneSoundVolume);
            }

            _playerSpawner.SetSpawnPoint(_spawnPointId);
            _sceneLoader.LoadScene(_sceneToLoad);
        }
    }
}
