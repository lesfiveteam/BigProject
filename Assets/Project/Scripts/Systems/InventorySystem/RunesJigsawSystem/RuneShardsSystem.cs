using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using BigProject.UI;
using Unity.VisualScripting;

namespace BigProject.Systems.Inventory
{
    public class RuneShardsSystem : MonoBehaviour
    {
        [SerializeField] private RuneShardsDatabaseSO _runeShardsDatabase;
        [SerializeField] private RuneSegmentsDatabaseSO _runeSegmentsDatabase;
        [SerializeField] private RunesJigsawUI _runesJigsawUI;

        private List<int> _foundShardsIDs = new List<int>();
        private List<int> _unlockedSegmentsIDs = new List<int>();
        private List<int> _placedShardsIDs = new List<int>();
        private List<int> _filledSegmentsIDs = new List<int>();

        public event Action<RuneShard> OnShardAdded;
        public event Action<int> OnSegmentUnlocked;

        public void AddRuneShard(int id)
        {
            if (!_foundShardsIDs.Contains(id))
            {
                _foundShardsIDs.Add(id);
            }
            else
            {
                Debug.LogWarning($"Shard with ID {id} already added.");
            }

            var shard = _runeShardsDatabase.Shards.FirstOrDefault(s => s.Id == id);

            OnShardAdded?.Invoke(shard);
        }

        public void UnlockSegment(int id)
        {
            if (_unlockedSegmentsIDs.Contains(id))
            {
                Debug.LogWarning($"Segment {id} already unlocked.");
                return;
            }

            if (!_runeSegmentsDatabase.Segments.Any(s => s.Id == id))
            {
                Debug.LogError($"Segment ID {id} not found in database.");
                return;
            }

            _unlockedSegmentsIDs.Add(id);
            OnSegmentUnlocked?.Invoke(_unlockedSegmentsIDs.Count);
        }

        public int GetUnlockedSegmentsNum() => _unlockedSegmentsIDs.Count;

        public List<int> GetFoundShardsIDs() => _foundShardsIDs;
        public List<int> GetPlacedShardsIDs() => _placedShardsIDs;
        public List<int> GetFilledSegmentsIDs() => _filledSegmentsIDs;
        public List<int> GetUnlockedSegmentsIDs() => _unlockedSegmentsIDs;

        public List<int> GetFreeShardsIDs() => _foundShardsIDs.Except(_placedShardsIDs).ToList();

        public List<int> GetShardsLeftToFinishSegments()
        {
            List<int> shardsLeftToFinishSegments = new List<int>();

            foreach (var segment in _runeSegmentsDatabase.Segments)
            {
                shardsLeftToFinishSegments.Add(0);
            }

            foreach (var shard in _runeShardsDatabase.Shards)
            {
                if (shard.SegmentId >= 0 && shard.SegmentId < shardsLeftToFinishSegments.Count)
                {
                    shardsLeftToFinishSegments[shard.SegmentId]++;
                }
            }

            foreach (var shardID in _placedShardsIDs)
            {
                RuneShard shard = _runeShardsDatabase.Shards.FirstOrDefault(x => x.Id == shardID);
                if (shard.SegmentId >= 0 && shard.SegmentId < shardsLeftToFinishSegments.Count)
                {
                    shardsLeftToFinishSegments[shard.SegmentId]--;
                }
            }

            foreach (var segmentID in _filledSegmentsIDs)
            {
                if (segmentID >= 0 && segmentID < shardsLeftToFinishSegments.Count)
                {
                    shardsLeftToFinishSegments[segmentID] = 0;
                }
            }

            return shardsLeftToFinishSegments;
        }

        public RuneShard GetShardByID(int id)
        {
            return _runeShardsDatabase.Shards.FirstOrDefault(shard => shard.Id == id);
        }

        public void AddPlacedShardID(int id)
        {
            _placedShardsIDs.Add(id);
        }

        public void AddFilledSegmentID(int id)
        {
            _filledSegmentsIDs.Add(id);
        }

        private void Awake()
        {
            for (int i = 0; i < 4; ++i)
                _unlockedSegmentsIDs.Add(i);

            for (int i = 0; i < 22; ++i)
                _foundShardsIDs.Add(i);

            _runesJigsawUI.Init(this);
        }
    }
}