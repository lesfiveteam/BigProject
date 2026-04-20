using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BigProject.Settings
{
    [CreateAssetMenu(fileName = "RunesConfig", menuName = "Scriptable Objects/Configs/RunesConfig")]
    public class RunesConfig : ScriptableObject
    {
        [Serializable]
        private struct QuestRewardRunes
        {
            public int questId;
            public List<int> runes;
        }

        [SerializeField]
        private List<QuestRewardRunes> _questsRewardRunes;

        public List<int> GetRewardedQuests() => _questsRewardRunes.Select(x => x.questId).ToList();

        public IReadOnlyList<int> GetQuestRewardRunes(int questId) => _questsRewardRunes.FirstOrDefault(x => x.questId == questId).runes;
    }
}