using Assets.Project.Scripts.Managers.SceneLoader;
using BigProject.Gameplay.Common;
using BigProject.Managers;
using BigProject.Systems;
using BigProject.Utilities;
using System;
using UnityEngine;

namespace BigProject.Player
{
    [Serializable]
    public class PlayerLocation : IDisposable, ISavable
    {
        [SerializeField]
        private Scenes scene;
        [SerializeField]
        private int spawnPointId;

        private SceneLoadManager _sceneLoader;
        private Transform _player;
        private ProgressManager _progressManager;

        public Scenes Scene => scene;

        public int SpawnPointId => spawnPointId;

        public string Key => "PlayerLocation";

        public object SavingData => this;

        public PlayerLocation()
        {
            Reset();
        }

        public void Init(Transform player, SceneLoadManager sceneLoader, ProgressManager progressManager)
        {
            _player = player;
            _sceneLoader = sceneLoader;
            _progressManager = progressManager;
            ExceptionUtilities.ThrowIfNull(_player, string.Format(LogStr.CRITICAL_NULL_REFERENCE, "PlayerLocation", "Player Transform"));
            ExceptionUtilities.ThrowIfNull(_sceneLoader, string.Format(LogStr.CRITICAL_NULL_REFERENCE, "PlayerLocation", "SceneLoadManager"));
            ExceptionUtilities.ThrowIfNull(_progressManager, string.Format(LogStr.CRITICAL_NULL_REFERENCE, "PlayerLocation", "ProgressManager"));
            _sceneLoader.SceneLoaded += OnSceneLoaded;
            _progressManager.ProgressSaved += OnProgressSaved;
        }

        public void Dispose()
        {
            Reset();
            _sceneLoader.SceneLoaded -= OnSceneLoaded;
            _progressManager.ProgressSaved -= OnProgressSaved;
        }

        private void Reset()
        {
            scene = Scenes.Village;
            spawnPointId = 0;
        }

        private void CalculateSpawnPoint()
        {
            SpawnPointsHandler spawner = GameObject.FindFirstObjectByType<SpawnPointsHandler>();
            spawnPointId = spawner != null ? spawner.GetNearestPointId(_player != null ? _player.position : Vector3.zero) : 0;
        }

        private void OnSceneLoaded(Scenes scene) => this.scene = scene;

        private void OnProgressSaved()
        {
            CalculateSpawnPoint();
            _progressManager.SaveAdditionalData(this, false);
        }
    }
}