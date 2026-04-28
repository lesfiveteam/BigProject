using BigProject.Managers;
using BigProject.Settings;
using BigProject.Utilities;
using System;
using System.Collections.Generic;

namespace BigProject.Systems.Inventory
{
    public class RunesSystem : IDisposable
    {
        private RuneShardsSystem _runeShardsSystem;

        public event Action<int> OnRuneAdded;
        public event Action<int> OnSegmentUnlocked;
        public event Action OnCleared;
        public List<int> _unlockedSegments = new();

        public RunesSystem(RuneShardsSystem runeShardsSystem)
        {
            _runeShardsSystem = runeShardsSystem;
            ExceptionUtilities.ThrowIfNull(_runeShardsSystem, "RunesSystem", "RuneShardsSystem");
            _runeShardsSystem.OnSegmentFilled += OnSegmentFilled;
            _runeShardsSystem.OnSegmentUnlocked += OnShardsSegmentUnlocked;
            _runeShardsSystem.OnUpdated += OnUpdated;
        }

        public void Dispose()
        {
            _runeShardsSystem.OnSegmentFilled -= OnSegmentFilled;
            _runeShardsSystem.OnSegmentUnlocked -= OnShardsSegmentUnlocked;
            _runeShardsSystem.OnUpdated -= OnUpdated;
        }

        private void OnSegmentFilled(int segmentId)
        {
            _unlockedSegments.Add(segmentId);
            OnRuneAdded?.Invoke(segmentId);
            GameLogManager.Info(string.Format(LogStr.INFO_SYSTEM, "RunesSystem", $"add runes segment {segmentId}"));
        }

        private void OnShardsSegmentUnlocked(int segmentsCount)
        {
            OnSegmentUnlocked?.Invoke(segmentsCount);
        }

        private void OnUpdated()
        {
            _unlockedSegments.Clear();
            OnCleared?.Invoke();

            foreach (int segmentId in _runeShardsSystem.GetFilledSegmentsIDs())
            {
                OnSegmentFilled(segmentId);
            }
        }
    }
}