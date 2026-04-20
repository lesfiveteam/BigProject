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
        private RunesConfig _runesConfig;

        public event Action<int> OnRuneAdded;
        public event Action<int> OnQuestChanged;
        public event Action OnCleared;
        public List<int> _unlockedSegments = new();

        public RunesSystem(RuneShardsSystem runeShardsSystem, RunesConfig runesConfig)
        {
            _runeShardsSystem = runeShardsSystem;
            _runesConfig = runesConfig;
            ExceptionUtilities.ThrowIfNull(_runeShardsSystem, "RunesSystem", "RuneShardsSystem");
            ExceptionUtilities.ThrowIfNull(_runesConfig, "RunesSystem", "RunesConfig");
            _runeShardsSystem.OnSegmentFilled += OnSegmentFilled;
            _runeShardsSystem.OnUpdated += OnUpdated;
        }

        public void Dispose()
        {
            _runeShardsSystem.OnSegmentFilled -= OnSegmentFilled;
            _runeShardsSystem.OnUpdated -= OnUpdated;
        }

        private void CheckQuestRunesAssemble()
        {
            List<int> rewardedQuests = _runesConfig.GetRewardedQuests();
            rewardedQuests.Sort((a, b) => b.CompareTo(a));

            foreach (int questId in rewardedQuests)
            {
                bool isPassed = true;

                foreach (int runesSegmentId in _runesConfig.GetQuestRewardRunes(questId))
                {
                    if (!_unlockedSegments.Contains(runesSegmentId))
                    {
                        isPassed = false;
                        break;
                    }
                }

                if (isPassed)
                {
                    OnQuestChanged?.Invoke(questId);
                    break;
                }
            }
        }

        private void OnSegmentFilled(int segmentId)
        {
            _unlockedSegments.Add(segmentId);
            OnRuneAdded?.Invoke(segmentId);
            GameLogManager.Info(string.Format(LogStr.INFO_SYSTEM, "RunesSystem", $"add runes segment {segmentId}"));
            CheckQuestRunesAssemble();
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