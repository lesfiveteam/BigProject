using System;
using UnityEngine;

namespace Assets.Project.Scripts.NPC.NPCWalkSystem
{
    [ExecuteAlways]
    public class ChildCountChangeNotifier : MonoBehaviour
    {
        public Action<int> ChildCountChanged;
        private void OnTransformChildrenChanged()
        {
            ChildCountChanged?.Invoke(transform.childCount);
        }
    }
}