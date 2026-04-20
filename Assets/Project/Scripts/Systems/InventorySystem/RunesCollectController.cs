using BigProject.Systems.QuestSystem;
using BigProject.Utilities;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace BigProject.Systems.Inventory
{
    public class RunesCollectController : IQuestBoundariesController
    {
        private List<int> _segmentsToAdd;
        private RunesSystem _runesSystem;

        public int QuestId {  get; private set; }

        public RunesCollectController(RunesSystem runesSystem, int questId, List<int> segmentsToAdd)
        {
            _runesSystem = runesSystem;
            ExceptionUtilities.ThrowIfNullFormat(_runesSystem, "RunesSystem");
            ExceptionUtilities.ThrowIfNullFormat(segmentsToAdd, "Segments to add");
            QuestId = questId;
        }

        public void End()
        {
            foreach (int segment in _segmentsToAdd)
            {
               // _runesSystem.AddRune(segment);
            }
        }
    }
}