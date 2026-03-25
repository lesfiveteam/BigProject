using Assets.Project.Scripts.Managers.SceneLoader;
using BigProject.Gameplay.Common;
using BigProject.Systems;
using System;
using UnityEngine;
using UnityEngine.AI;

namespace BigProject.Player
{
    public class PlayerSpawner : IDisposable
    {
        private SceneLoadManager _sceneLoader;
        private NavMeshAgent _agent;
        private int _spawnPointId;

        private const float MAX_DISTANCE_TO_NAV_MESH = 100f;
        private readonly Vector3 DEFAULT_POSITION = Vector3.zero;
        private readonly Vector3 DEFAULT_ANGLES = Vector3.zero;

        public PlayerSpawner(SceneLoadManager sceneLoader, NavMeshAgent agent)
        {
            _sceneLoader = sceneLoader;
            _agent = agent;
            _spawnPointId = 0;
            _sceneLoader.SceneLoadingCompleted += OnSceneLoadingCompleted;
        }

        public void SetSpawnPoint(int pointId)
        {
            _spawnPointId = pointId;
        }

        public void Dispose()
        {
            _sceneLoader.SceneLoadingCompleted -= OnSceneLoadingCompleted;
        }

        public void PositionPlayer(int spawnPointId)
        {
            SpawnPointsHandler spawnPointsHandler = GameObject.FindFirstObjectByType<SpawnPointsHandler>();
            Transform spawnTransform;
            Vector3 spawnPosition;
            Vector3 spawnAngles = Vector3.zero;

            if (spawnPointsHandler != null && spawnPointsHandler.TryGetSpawnTransform(spawnPointId, out spawnTransform))
            {
                spawnPosition = spawnTransform.position;
                spawnAngles = spawnTransform.localEulerAngles;
            }
            else
            {
                Debug.LogWarning(String.Format(LogStr.WARNING_SYSTEM, "PlayerSpawner", $"unable to get spawn transform {spawnPointId}, move Player to default position"));
                spawnPosition = DEFAULT_POSITION;
                spawnAngles = DEFAULT_ANGLES;
            }

            if (NavMesh.SamplePosition(spawnPosition, out NavMeshHit hit, MAX_DISTANCE_TO_NAV_MESH, NavMesh.AllAreas))
            {
                Debug.Log(String.Format(LogStr.INFO_SYSTEM, "PlayerSpawner", $"move player to spawn position {hit.position},"));
                _agent.Warp(hit.position);
                _agent.transform.localEulerAngles = spawnAngles;
            }
            else
            {
                Debug.LogError(String.Format(LogStr.ERROR_SYSTEM, "PlayerSpawner", "unable to find NavMesh point for agent"));
            }
        }

        private void OnSceneLoadingCompleted()
        {
            PositionPlayer(_spawnPointId);
            _spawnPointId = 0;
        }
    }
}