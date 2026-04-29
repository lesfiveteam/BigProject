using BigProject.Settings;
using BigProject.Systems;
using BigProject.Systems.Inventory;
using BigProject.UI;
using BigProject.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BigProject.Gameplay
{
    public class RunesDriver
    {
        private RuneShardsSystem _runesSystem;
        private RunesConfig _runesConfig;
        private RunePanelUI _runesPanel;
        private Transform _initialPoint;
        private int _questId;

        public RunesDriver(RuneShardsSystem runesSystem, RunesConfig runesConfig, RunePanelUI runesPanel, int questId, Transform initialPoint)
        {
            _runesSystem = runesSystem;
            _runesConfig = runesConfig;
            _runesPanel = runesPanel;
            _initialPoint = initialPoint;
            _questId = questId;
            ExceptionUtilities.ThrowIfNull(_runesSystem, String.Format(LogStr.CRITICAL_NULL_REFERENCE, "RunesDriver", "RuneShardsSystem"));
            ExceptionUtilities.ThrowIfNull(_runesConfig, String.Format(LogStr.CRITICAL_NULL_REFERENCE, "RunesDriver", "RunesConfig"));
            ExceptionUtilities.ThrowIfNull(_runesPanel, String.Format(LogStr.CRITICAL_NULL_REFERENCE, "RunesDriver", "RunesConfig"));
            ExceptionUtilities.ThrowIfNull(_initialPoint, String.Format(LogStr.CRITICAL_NULL_REFERENCE, "RunesDriver", "Initial Point Transform"));
        }

        public void Deliver(Camera camera = null)
        {
            if (camera == null)
            {
                camera = Camera.main;
            }

            IReadOnlyList<int> rewardRunes = _runesConfig.GetQuestRewardRunes(_questId);
            ExceptionUtilities.ThrowIfNullFormat(rewardRunes, "unable to get reward runes");

            Vector3 screenPos = camera.WorldToScreenPoint(_initialPoint.position);

            if (screenPos.z > 0)
            {
                _runesPanel.SetRunesOnScreenPosition(new(screenPos.x, screenPos.y));
            }

            foreach (int rewardRuneId in rewardRunes)
            {
                _runesSystem.AddRunesSegment(rewardRuneId);
            }
        }
    }
}