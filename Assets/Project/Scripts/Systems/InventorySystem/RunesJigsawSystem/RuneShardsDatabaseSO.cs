using System;
using System.Collections.Generic;
using UnityEngine;

namespace BigProject.Systems.Inventory
{
    [Serializable]
    public struct RuneShard
    {
        public int Id;
        public int SegmentId;
        public Sprite Sprite;
    }

    [CreateAssetMenu(menuName = "Inventory/RuneShardsDatabase")]
    public class RuneShardsDatabaseSO : ScriptableObject
    {
        public List<RuneShard> Shards;
    }
}