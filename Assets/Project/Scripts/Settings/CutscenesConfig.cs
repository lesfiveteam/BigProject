using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
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
            public AssetReferenceT<TimelineAsset> timelineRef;
            public List<GameObject> prefabs;
        }

        [SerializeField]
        private List<CutsceneSettings> _cutscenesSettings;

        public bool TryGetCutscenePrefabs(object runtimeKey, out List<GameObject> prefabs)
        {
            CutsceneSettings cutsceneSettings = _cutscenesSettings.FirstOrDefault(x => x.timelineRef.RuntimeKey.Equals(runtimeKey));
            prefabs = cutsceneSettings?.prefabs;
            return prefabs != null;
        }
    }
}