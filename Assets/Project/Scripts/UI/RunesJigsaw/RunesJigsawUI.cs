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
        [SerializeField] private Transform _shardHolder;
        [SerializeField] private Button _exitButton;
        [SerializeField] private Image _finalImage;

        private RuneShardsSystem _runeShardsSystem;
        private List<ShardUI> _freeShards = new List<ShardUI>();
        private List<ShardUI> _placedShards = new List<ShardUI>();
        private List<int> _shardsLeftToFinishSegments;
        private int _filledSegmentsNum = 0;

        public void Init(RuneShardsSystem runeShardsSystem)
        {
            if (runeShardsSystem == null)
            {
                Debug.LogError("RuneShardsSystem is null.");
                return;
            }

            _runeShardsSystem = runeShardsSystem;

            _runeShardsSystem.OnShardAdded += AddNewShard;
            _runeShardsSystem.OnSegmentUnlocked += UpdateBackingVisual;

            UpdateBackingVisual(_runeShardsSystem.GetUnlockedSegmentsNum());

            _shardsLeftToFinishSegments = _runeShardsSystem.GetShardsLeftToFinishSegments();

            if (_shardsLeftToFinishSegments == null)
            {
                Debug.LogError("ShardsLeftToFinishSegments is null.");
                return;
            }

            foreach (var segmentID in _runeShardsSystem.GetFilledSegmentsIDs())
            {
                ShowSegmentFilled(segmentID);
                _filledSegmentsNum++;
            }

            TryShowFinalImage();

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

            _exitButton.onClick.AddListener(OnExitButtonClicked);
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

            if (startShard == null || goalShard == null)
            {
                Debug.LogError($"SpawnShard: Missing start/goal shard for ID {id}");
                return;
            }

            Vector3 spawnPos = isPlaced ? goalShard.transform.position : startShard.transform.position;
            GameObject spawnedShard = Instantiate(_shardPrefab, spawnPos, Quaternion.identity, _shardHolder);

            ShardUI shardUI = spawnedShard.GetComponent<ShardUI>();

            if (shardUI == null)
            {
                Debug.LogError("Spawned shard prefab has no ShardUI component!");
                return;
            }

            RectTransform rect = goalShard.GetComponent<RectTransform>();
            if (rect == null)
            {
                Debug.LogError($"Goal shard {id} has no RectTransform.");
                return;
            }

            Vector2 imgSize = new Vector2(rect.sizeDelta.x, rect.sizeDelta.y);

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

            if (placedShard == null)
            {
                Debug.LogError($"HandleShardPlacedCorrectly: shard {id} not found in free list.");
                return;
            }

            int segmentID = placedShard.SegmentID;

            if (segmentID < 0 || segmentID >= _shardsLeftToFinishSegments.Count)
            {
                Debug.LogError($"Invalid segmentID {segmentID} for shard {id}.");
                return;
            }

            _placedShards.Add(placedShard);
            _freeShards.Remove(placedShard);

            _shardsLeftToFinishSegments[segmentID]--;

            if (_shardsLeftToFinishSegments[segmentID] < 0)
            {
                Debug.LogError($"Segment {segmentID} went below zero.");
            }

            _runeShardsSystem.AddPlacedShardID(id);

            if (_shardsLeftToFinishSegments[segmentID] == 0)
            {
                ShowSegmentFilled(segmentID);
                _runeShardsSystem.AddFilledSegmentID(segmentID);
            }
        }

        private void UpdateBackingVisual(int unlockedSegmentsNum)
        {
            if (_backingImages == null || _backingImages.Count == 0)
            {
                Debug.LogError("BackingImages not set.");
                return;
            }

            foreach (var backingImage in _backingImages.OrderByDescending(b => b.unlockedSegmentsThreshold))
            {
                if (unlockedSegmentsNum >= backingImage.unlockedSegmentsThreshold)
                {
                    _backingImage.sprite = backingImage.sprite;
                    return;
                }
            }

            Debug.LogWarning("No suitable backing image found.");
        }

        public void Hide() => gameObject.SetActive(false);
        public void Show() => gameObject.SetActive(true);

        private void ShowSegmentFilled(int segmentID)
        {
            if (segmentID < 0 || segmentID >= _segmentImages.Count)
            {
                Debug.LogError($"Invalid segmentID {segmentID} in ShowSegmentFilled");
                return;
            }

            _segmentImages[segmentID].gameObject.SetActive(true);

            var shardsToRemove = _placedShards.Where(s => s.SegmentID == segmentID).ToList();

            foreach (var shard in shardsToRemove)
            {
                _placedShards.Remove(shard);
                Destroy(shard.gameObject);
            }

            _filledSegmentsNum++;
            TryShowFinalImage();            
        }

        private void OnExitButtonClicked()
        {
            foreach (var freeShard in _freeShards)
            {
                freeShard.ResetPosition();
            }
            Hide();
        }

        private void TryShowFinalImage()
        {
            if (_filledSegmentsNum == _segmentImages.Count)
            {
                _finalImage.gameObject.SetActive(true);
            }
        }
    }
}