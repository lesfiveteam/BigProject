using BigProject.Systems.HUD;
using BigProject.Systems.Inventory;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BigProject.UI
{
    public class RunesJigsawUI : MonoBehaviour, IHUDWidget
    {
        [Serializable]
        public struct BackingImage
        {
            public int unlockedSegmentsThreshold;
            public Sprite sprite;
        }

        [SerializeField] private GameObject _shardPrefab;
        [SerializeField] private Image _boardBorders;
        [SerializeField] private List<GameObject> _startShards;
        [SerializeField] private List<GameObject> _goalShards;
        [SerializeField] private List<BackingImage> _backingImages;
        [SerializeField] private Image _backingImage;
        [SerializeField] private List<Image> _segmentImages;

        private RuneShardsSystem _runeShardsSystem;
        private List<ShardUI> _freeShards = new List<ShardUI>();
        private List<ShardUI> _placedShards = new List<ShardUI>();
        private List<int> _shardsLeftToFinishSegments;

        public void Init(RuneShardsSystem runeShardsSystem)
        {
            _runeShardsSystem = runeShardsSystem;

            _runeShardsSystem.OnShardAdded += AddShard;
            _runeShardsSystem.OnSegmentUnlocked += UpdateBackingVisual;

            UpdateBackingVisual(_runeShardsSystem.GetUnlockedSegmentsNum());

            _shardsLeftToFinishSegments = _runeShardsSystem.GetShardsLeftToFinishSegments();

            foreach (var segmentID in _runeShardsSystem.GetFilledSegmentsIDs())
            {
                ShowSegmentFilled(segmentID);
            }
        }

        private void OnDestroy()
        {
            if (_runeShardsSystem != null)
            {
                _runeShardsSystem.OnShardAdded -= AddShard;
                _runeShardsSystem.OnSegmentUnlocked -= UpdateBackingVisual;
            }
        }

        private void AddShard(RuneShard shard)
        {
            int id = shard.Id;
            GameObject startShard = _startShards[id];
            GameObject goalShard = _goalShards[id];

            GameObject spawnedShard = Instantiate(_shardPrefab, startShard.transform.position, Quaternion.identity);
            ShardUI shardUI = spawnedShard.GetComponent<ShardUI>();

            shardUI.Init(startShard.transform, goalShard.transform, shard.Sprite, _boardBorders, id, shard.SegmentId);
            shardUI.OnShardPlacedCorrectly += HandleShardPlacedCorrectly;

            _freeShards.Add(shardUI);
        }

        private void HandleShardPlacedCorrectly(int id)
        {
            ShardUI placedShard = _freeShards.Find(shard => shard.ID == id);
            if (placedShard == null) return;

            _placedShards.Add(placedShard);
            _freeShards.Remove(placedShard);

            int segmentID = placedShard.SegmentID;
            _shardsLeftToFinishSegments[segmentID]--;

            if (_shardsLeftToFinishSegments[segmentID] == 0)
            {
                ShowSegmentFilled(segmentID);
            }
        }

        private void UpdateBackingVisual(int unlockedSegmentsNum)
        {
            foreach (var backingImage in _backingImages)
            {
                if (unlockedSegmentsNum >= backingImage.unlockedSegmentsThreshold)
                {
                    _backingImage.sprite = backingImage.sprite;
                    break;
                }
            }
        }

        public void Hide() => gameObject.SetActive(false);
        public void Show() => gameObject.SetActive(true);

        private void ShowSegmentFilled(int segmentID)
        {
            if (segmentID >= 0 && segmentID < _segmentImages.Count)
            {
                _segmentImages[segmentID].gameObject.SetActive(true);
            }
        }
    }
}