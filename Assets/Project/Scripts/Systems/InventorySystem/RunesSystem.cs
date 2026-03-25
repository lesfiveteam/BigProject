using BigProject.Managers;
using System;
using UnityEngine;

namespace BigProject.Systems.Inventory
{
    public class RunesSystem
    {
        private int _numberOfRunes;
        public event Action<int> OnRuneAdded;
        public event Action<int> OnQuestChanged;

        /// <summary>
        /// <para>IDs for the first quest: 0, 4</para>
        /// <para>IDs for the second quest: 1, 3</para>
        /// <para>IDs for the third quest: 2, 5</para>
        /// </summary>
        /// <param name="runeId"></param>
        public void AddRune(int runeID)
        {
            if (_numberOfRunes >= 6)
            {
                Debug.LogError("Rune bar is already full, new rune wasn't added");
                return;
            }

            OnRuneAdded?.Invoke(runeID);
            _numberOfRunes++;
            GameLogManager.Info("Added rune");
        }

        /// <summary>
        /// Used to change Runebar background between quests (see Figma for details). 
        /// QuestID starts from zero!
        /// </summary>
        /// <param name="questID">Starts from zero</param>
        public void ChangeRunebarBackgroundBasedOnQuest(int questID)
        {
            OnQuestChanged?.Invoke(questID);
        }

        public int GetNumberOfRunes()
        {
            return _numberOfRunes;
        }
    }
}