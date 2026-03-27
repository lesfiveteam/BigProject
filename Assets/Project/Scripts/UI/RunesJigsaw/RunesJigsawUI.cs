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
        private List<ShardUI> _freeShards;
        private List<ShardUI> _placedShards;
        private List<int> _shardsLeftToFinishSegments;

        public void Init(RuneShardsSystem runeShardsSystem)
        {
            _runeShardsSystem = runeShardsSystem;
            _runeShardsSystem.OnShardAdded += this.AddShard;
            _runeShardsSystem.OnSegmentUnlocked += this.UpdateBackingVisual;

            UpdateBackingVisual(_runeShardsSystem.GetUnlockedSegmentsNum());

            _shardsLeftToFinishSegments = _runeShardsSystem.GetShardsLeftToFinishSegments();

            foreach (var segmentID in _runeShardsSystem.GetFilledSegmentsIDs())
            {
                ShowSegmentFilled(segmentID);
            }
        }

        private void AddShard(RuneShard shard)
        {
            int id = shard._id;
            GameObject startShard = _startShards[id];
            GameObject spawnedShard = Instantiate(_shardPrefab, startShard.transform.position, Quaternion.identity);
            ShardUI shardUI = spawnedShard.GetComponent<ShardUI>();
            GameObject goalShard = _goalShards[id];
            shardUI.Init(startShard.transform, goalShard.transform, shard._sprite, _boardBorders, id, shard._segmentId);
            _freeShards.Add(shardUI);
            shardUI.OnShardPlacedCorrectly += this.HandleShardPlacedCorrectly;
        }

        private void HandleShardPlacedCorrectly(int id)
        {
            foreach (var shard in _freeShards)
            {
                if (shard.GetID() == id)
                {
                    _placedShards.Add(shard);
                    _freeShards.Remove(shard);
                    var segmentID = shard.GetSegmentID();
                    _shardsLeftToFinishSegments[segmentID]--;
                    if (_shardsLeftToFinishSegments[segmentID] == 0)
                    {
                        ShowSegmentFilled(segmentID);
                    }
                    break;
                }
            }
        }

        private void UpdateBackingVisual(int unlockedSegmentsNum)
        {
            foreach (var backingImage in  _backingImages)
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
            _segmentImages[segmentID].gameObject.SetActive(true);
        }
    }
}