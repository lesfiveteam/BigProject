using BigProject.Systems.HUD;
using BigProject.Systems.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] private Transform shardHolder;

        private RuneShardsSystem _runeShardsSystem;
        private List<ShardUI> _freeShards = new List<ShardUI>();
        private List<ShardUI> _placedShards = new List<ShardUI>();
        private List<int> _shardsLeftToFinishSegments;

        public void Init(RuneShardsSystem runeShardsSystem)
        {
            _runeShardsSystem = runeShardsSystem;

            _runeShardsSystem.OnShardAdded += AddNewShard;
            _runeShardsSystem.OnSegmentUnlocked += UpdateBackingVisual;

            UpdateBackingVisual(_runeShardsSystem.GetUnlockedSegmentsNum());

            _shardsLeftToFinishSegments = _runeShardsSystem.GetShardsLeftToFinishSegments();

            foreach (var segmentID in _runeShardsSystem.GetFilledSegmentsIDs())
            {
                ShowSegmentFilled(segmentID);
            }

            var placedShardsIDs = _runeShardsSystem.GetPlacedShardsIDs();
            var freeShardsIDs = _runeShardsSystem.GetFreeShardsIDs();

            foreach (var shardID in placedShardsIDs)
            {
                SpawnShard(_runeShardsSystem.GetShardByID(shardID), isPlaced: true);
            }

            foreach (var shardID in freeShardsIDs)
            {
                SpawnShard(_runeShardsSystem.GetShardByID(shardID), isPlaced: false);
            }

            foreach (var segmentID in _runeShardsSystem.GetFilledSegmentsIDs())
            {
                ShowSegmentFilled(segmentID);
            }

            for (int i = 0; i < _shardsLeftToFinishSegments.Count; i++)
            {
                if (_shardsLeftToFinishSegments[i] == 0) ShowSegmentFilled(i);
            }
        }

        private void OnDestroy()
        {
            if (_runeShardsSystem != null)
            {
                _runeShardsSystem.OnShardAdded -= AddNewShard;
                _runeShardsSystem.OnSegmentUnlocked -= UpdateBackingVisual;
            }
        }
        private void AddNewShard(RuneShard shard)
        {
            SpawnShard(shard, isPlaced: false);
        }

        private void SpawnShard(RuneShard shard, bool isPlaced)
        {
            int id = shard.Id;
            GameObject startShard = _startShards[id];
            GameObject goalShard = _goalShards[id];

            Vector3 spawnPos = isPlaced ? goalShard.transform.position : startShard.transform.position;
            GameObject spawnedShard = Instantiate(_shardPrefab, spawnPos, Quaternion.identity, shardHolder);

            ShardUI shardUI = spawnedShard.GetComponent<ShardUI>();

            RectTransform transform = goalShard.GetComponent<RectTransform>();
            Vector2 imgSize = new Vector2(transform.sizeDelta.x, transform.sizeDelta.y);

            shardUI.Init(startShard.transform, goalShard.transform, shard.Sprite, _boardBorders, id, shard.SegmentId, imgSize, isPlaced);
            shardUI.OnShardPlacedCorrectly += HandleShardPlacedCorrectly;

            if (isPlaced)
            {
                _placedShards.Add(shardUI);
            }
            else
            {
                _freeShards.Add(shardUI);
            }
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
                foreach (var shard in _placedShards)
                {
                    if (shard.SegmentID == segmentID) Destroy(shard.gameObject);
                }
            }
        }
    }
}