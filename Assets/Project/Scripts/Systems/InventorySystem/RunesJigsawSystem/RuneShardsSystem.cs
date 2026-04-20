using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using BigProject.Settings;
using BigProject.Utilities;
using BigProject.Managers;

namespace BigProject.Systems.Inventory
{
    public class RuneShardsSystem : ISavable
    {
        private RuneShardsDatabaseSO _runeShardsDatabase;
        private RuneSegmentsDatabaseSO _runeSegmentsDatabase;
        private List<int> _foundShardsIDs = new List<int>();
        private List<int> _unlockedSegmentsIDs = new List<int>();
        private List<int> _placedShardsIDs = new List<int>();
        private List<int> _filledSegmentsIDs = new List<int>();
        private RunesConfig _runesConfig;
        private DataToSave _dataToSave;
        private ProgressManager _progressManager;
        private bool _isLoading = false;

        public event Action<RuneShard> OnShardAdded;
        public event Action<int> OnSegmentUnlocked;
        public event Action<int> OnSegmentFilled;
        public event Action OnUpdated;

        [Serializable]
        private class DataToSave
        {
            public List<int> unlockedSegmentsIds;
            public List<int> filledSegmentsIds;
            public List<int> placedShardsIDs;
        }

        public string Key => "RuneShardsSystem";

        public object SavingData
        {
            get
            {
                CreateDTO();
                return _dataToSave;
            }
        }

        public void OnSaved(bool _) => _dataToSave = null;

        public void OnLoad()
        {
            if (_dataToSave == null)
            {
                return;
            }

            _isLoading = true;
            _foundShardsIDs.Clear();
            _unlockedSegmentsIDs.Clear();

            foreach (int segmentId in _dataToSave.unlockedSegmentsIds)
            {
                AddRunesSegment(segmentId);
            }

            _dataToSave = null;
            OnUpdated?.Invoke();
            _isLoading = false;
        }

        public RuneShardsSystem(RunesConfig runesConfig, RuneShardsDatabaseSO runeShardsDatabase, RuneSegmentsDatabaseSO runeSegmentsDatabase,
            ProgressManager progressManager)
        {
            _runesConfig = runesConfig;
            _runeShardsDatabase = runeShardsDatabase;
            _runeSegmentsDatabase = runeSegmentsDatabase;
            _progressManager = progressManager;
            ExceptionUtilities.ThrowIfNull(_runesConfig, "RuneShardsSystem", "RunesConfig");
            ExceptionUtilities.ThrowIfNull(_runeShardsDatabase, "RuneShardsSystem", "RuneShardsDatabaseSO");
            ExceptionUtilities.ThrowIfNull(_runeSegmentsDatabase, "RuneShardsSystem", "RuneSegmentsDatabaseSO");
            ExceptionUtilities.ThrowIfNull(_progressManager, "RuneShardsSystem", "ProgressManager");
        }

        /// <summary>
        /// Adds unlocked runes when invoked after each quest.
        /// </summary>
        public void AddRunesSegment(int segmentId)
        {
            UnlockSegmentByID(segmentId);

            foreach (RuneShard shard in _runeShardsDatabase.Shards)
            {
                if (shard.SegmentId == segmentId)
                {
                    _foundShardsIDs.Add(shard.Id);
                    OnShardAdded?.Invoke(shard);
                }
            }
        }

        private void UnlockSegmentByID(int id)
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

            foreach (RuneSegment segment in _runeSegmentsDatabase.Segments)
            {
                shardsLeftToFinishSegments.Add(0);
            }

            foreach (RuneShard shard in _runeShardsDatabase.Shards)
            {
                if (shard.SegmentId >= 0 && shard.SegmentId < shardsLeftToFinishSegments.Count)
                {
                    shardsLeftToFinishSegments[shard.SegmentId]++;
                }
            }

            foreach (int shardID in _placedShardsIDs)
            {
                RuneShard shard = _runeShardsDatabase.Shards.FirstOrDefault(x => x.Id == shardID);
                if (shard.SegmentId >= 0 && shard.SegmentId < shardsLeftToFinishSegments.Count)
                {
                    shardsLeftToFinishSegments[shard.SegmentId]--;
                }
            }

            foreach (int segmentID in _filledSegmentsIDs)
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
            OnSegmentFilled?.Invoke(id);

            if (!_isLoading)
            {
                _progressManager.SaveAdditionalData(this);
            }
        }

        private void CreateDTO()
        {
            if (_dataToSave == null)
            {
                _dataToSave = new();
            }

            _dataToSave.unlockedSegmentsIds = new(_unlockedSegmentsIDs);
            _dataToSave.filledSegmentsIds = _filledSegmentsIDs;
            _dataToSave.placedShardsIDs = _placedShardsIDs;
        }
    }
}