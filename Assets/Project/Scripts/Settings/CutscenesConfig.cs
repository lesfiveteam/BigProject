using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;

namespace BigProject.Settings
{
    [CreateAssetMenu(fileName = "CutscenesConfig", menuName = "Scriptable Objects/Configs/CutscenesConfig")]
    public class CutscenesConfig : ScriptableObject
    {
        [field: SerializeField]
        public string CameraActorPrefix { get; private set; }

        [Serializable]
        private class CutsceneSettings
        {
            public TimelineAsset timeline;
            public List<GameObject> prefabs;
        }

        [SerializeField]
        private List<CutsceneSettings> _cutscenesSettings;

        public bool TryGetCutscenePrefabs(TimelineAsset timeline, out List<GameObject> prefabs)
        {
            CutsceneSettings cutsceneSettings = _cutscenesSettings.FirstOrDefault(x =>  x.timeline == timeline);
            prefabs = cutsceneSettings?.prefabs;
            return prefabs != null;
        }
    }
}