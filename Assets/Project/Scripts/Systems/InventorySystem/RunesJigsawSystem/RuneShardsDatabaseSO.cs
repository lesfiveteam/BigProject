using System;
using System.Collections.Generic;
using UnityEngine;

namespace BigProject.Systems.Inventory
{
    [Serializable]
    public struct RuneShard
    {
        public int _id;
        public int _segmentId;
        public Sprite _sprite;
    }

    [CreateAssetMenu(menuName = "Inventory/RuneShardsDatabase")]
    public class RuneShardsDatabaseSO : ScriptableObject
    {
        public List<RuneShard> _shards;
    }
}