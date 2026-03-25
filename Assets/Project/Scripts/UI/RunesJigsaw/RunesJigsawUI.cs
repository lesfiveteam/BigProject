using BigProject.Systems.HUD;
using BigProject.Systems.Inventory;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

namespace BigProject.UI
{
    public class RunesJigsawUI : MonoBehaviour, IHUDWidget
    {
        [SerializeField] private GameObject _shardPrefab;
        [SerializeField] private Image _boardBorders;
        [SerializeField] private List<GameObject> _startShards;
        [SerializeField] private List<GameObject> _goalShards;

        private RuneShardsSystem _runeShardsSystem;
        private List<ShardUI> _shards;
        private List<ShardUI> _placedShards;

        public void Init(RuneShardsSystem runeShardsSystem)
        {
            _runeShardsSystem = runeShardsSystem;
            _runeShardsSystem.OnShardAdded += this.OnShardAdded;
        }

        private void OnShardAdded(RuneShard shard)
        {
            int id = shard._id;
            GameObject startShard = _startShards[id];
            GameObject spawnedShard = Instantiate(_shardPrefab, startShard.transform.position, Quaternion.identity);
            ShardUI shardUI = spawnedShard.GetComponent<ShardUI>();
            GameObject goalShard = _goalShards[id];
            Image image = shard._image;
            shardUI.Init(startShard.transform, goalShard.transform, image, _boardBorders, id);
            _shards.Add(shardUI);
            shardUI.OnShardPlacedCorrectly += this.OnShardPlacedCorrectly;
        }

        private void OnShardPlacedCorrectly(int id)
        {
            foreach (var shard in _shards)
            {
                if (shard.GetID() == id)
                {
                    _placedShards.Add(shard);
                    _shards.Remove(shard);
                    break;
                }
            }
        }

        public void Hide() => gameObject.SetActive(false);
        public void Show() => gameObject.SetActive(true);
    }
}