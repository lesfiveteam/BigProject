using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace BigProject.Systems.Inventory
{
    public class RuneShardsSystem
    {
        [SerializeField] private RuneShardsDatabaseSO _runeShardsDatabase;
        [SerializeField] private RuneSegmentsDatabaseSO _runeSegmentsDatabase;
        
        private List<int> _foundShardsIDs;
        private List<int> _unlockedSegmentsIDs;
        private List<int> _placedShardsIDs;
        private List<int> _filledSegmentsIDs;

        public event Action<RuneShard> OnShardAdded;
        public event Action<int> OnSegmentUnlocked;

        public void AddRuneShard(int id)
        {
            _foundShardsIDs.Add(id);
            foreach (var shard in _runeShardsDatabase._shards)
            {
                if (shard._id == id)
                {
                    OnShardAdded?.Invoke(shard);
                    return;
                }
            }
        }

        public void UnlockSegment(int id)
        {
            _unlockedSegmentsIDs.Add(id);
            OnSegmentUnlocked?.Invoke(_unlockedSegmentsIDs.Count);
        }

        public int GetUnlockedSegmentsNum() => _foundShardsIDs.Count;

        public List<int> GetFoundShardsIDs() => _foundShardsIDs;

        public List<int> GetPlacedShardsISs() => _placedShardsIDs;

        public List<int> GetFilledSegmentsIDs() => _filledSegmentsIDs;

        public List<int> GetUnlockedSegmentsIDs() => _unlockedSegmentsIDs;

        public List<int> GetShardsLeftToFinishSegments()
        {
            List<int> shardsLeftToFinishSegments = new List<int>();
            foreach (var segment in _runeSegmentsDatabase._segments)
            {
                shardsLeftToFinishSegments.Add(0);
            }

            foreach (var shard in _runeShardsDatabase._shards)
            {
                shardsLeftToFinishSegments[shard._segmentId]++;
            }

            foreach (var shardID in _placedShardsIDs)
            {
                RuneShard shard = _runeShardsDatabase._shards.FirstOrDefault(x => x._id == shardID);
                shardsLeftToFinishSegments[shard._segmentId]--;
            }

            foreach (var segmentID in _filledSegmentsIDs)
            {
                shardsLeftToFinishSegments[segmentID] = 0;
            }

            return shardsLeftToFinishSegments;
        }
    }
}
