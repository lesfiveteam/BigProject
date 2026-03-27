using System;
using System.Collections.Generic;
using UnityEngine;

namespace BigProject.Systems.Inventory
{
    [Serializable]
    public struct RuneSegment
    {
        public int Id;
        public Sprite Sprite;
    }

    [CreateAssetMenu(menuName = "Inventory/RuneSegmentsDatabase")]
    public class RuneSegmentsDatabaseSO : ScriptableObject
    {
        public List<RuneSegment> Segments;
    }
}