using System;
using System.Collections.Generic;
using UnityEngine;

namespace BigProject.Systems.Inventory
{
    [Serializable]
    public struct RuneSegment
    {
        public int _id;
        public Sprite _sprite;
    }

    [CreateAssetMenu(menuName = "Inventory/RuneSegmentsDatabase")]
    public class RuneSegmentsDatabaseSO : ScriptableObject
    {
        public List<RuneSegment> _segments;
    }
}