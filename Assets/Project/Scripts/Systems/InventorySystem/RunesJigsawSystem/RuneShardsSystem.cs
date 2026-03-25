using UnityEngine;
using System.Collections.Generic;
using System;

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
    }
}
