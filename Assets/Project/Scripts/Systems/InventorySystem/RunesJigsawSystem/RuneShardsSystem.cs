using UnityEngine;
using System.Collections.Generic;

namespace BigProject.Systems.Inventory
{
    public class RuneShardsSystem
    {
        [SerializeField]
        private RuneShardsDatabaseSO _runeShardsDatabase;
        [SerializeField]
        private RuneSegmentsDatabaseSO _runeSegmentsDatabase;
        
        private List<int> _foundShardsIDs;
        private List<int> _placedShardsIDs;
        private List<int> _filledSegmentsIDs;

        public void AddRuneShard(int id)
        {
            _foundShardsIDs.Add(id);
        }
    }
}
