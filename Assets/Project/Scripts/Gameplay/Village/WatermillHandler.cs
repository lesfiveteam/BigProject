using BigProject.Gameplay.Watermill;
using BigProject.Managers;
using BigProject.Systems;
using BigProject.Systems.QuestSystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace BigProject.Gameplay.Village
{
    public class WatermillHandler : MonoBehaviour
    {
        [SerializeField]
        private int _repairQuestId;
        [SerializeField]
        private int _repairActionId;
        [SerializeField]
        private GearsHandler _wheelHandler;
        [SerializeField]
        private Transform _barrier;
        [SerializeField]
        private List<RiverPart> _riverParts;
        [SerializeField]
        private RiverSettings _beforeRepairSettings;
        [SerializeField]
        private RiverSettings _afterRepairSettings;
        [SerializeField]
        private Transform _adapterPart;

        [Serializable]
        private struct RiverPart
        {
            public Transform transform;
            public MeshRenderer mesh;
            public bool isDynamicHeight;
            public bool isDynamicMaterial;
        }

        [Serializable]
        private struct RiverSettings
        {
            public float barrierHeight;
            public float waterHeight;
            public float adapterHeight;
            public float adapterAngle;
            public float surfaceNoiseScroll;
            [Range(0f, 1f)]
            public float surfaceNoiseCutoff;
            public float foamMaxDistance;
        }

        private void Awake()
        {
            Assert.IsNotNull(_wheelHandler, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, $"{name}", "Wheel Handler"));
            Assert.IsNotNull(_barrier, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, $"{name}", "Barrier Transform"));
            Assert.IsNotNull(_adapterPart, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, $"{name}", "Adapter Transform"));
        }

        public void Init(ProgressManager progressManager)
        {
            if (!progressManager.TryGetQuestActionHandler(_repairQuestId, _repairActionId, out IQuestActionHandler actionHandler))
            {
                Debug.LogError(string.Format(LogStr.ERROR_SYSTEM, "WatermillHandler", "unable to get ActionHandler"));
                return;
            }
            
            SetState(progressManager.GetQuestState(_repairQuestId) == QuestState.Completed || actionHandler.CurrentState == QuestActionState.Completed);
        }

        private void SetState(bool isRepaired)
        {
            RiverSettings setings = isRepaired ? _afterRepairSettings : _beforeRepairSettings;
            _wheelHandler.enabled = isRepaired;
            Vector3 position = _barrier.localPosition;
            position.y = setings.barrierHeight;
            _barrier.localPosition = position;

            position = _adapterPart.localPosition;
            position.y = setings.adapterHeight;
            _adapterPart.localPosition = position;

            Vector3 angles = _adapterPart.localEulerAngles;
            angles.x = setings.adapterAngle;
            _adapterPart.localEulerAngles = angles;

            foreach (RiverPart riverPart in _riverParts)
            {
                if (riverPart.isDynamicHeight)
                {
                    position = riverPart.transform.localPosition;
                    position.y = setings.waterHeight;
                    riverPart.transform.localPosition = position;
                }

                if (riverPart.isDynamicMaterial)
                {
                    MaterialPropertyBlock propBlock = new();
                    Renderer renderer = riverPart.mesh;
                    renderer.GetPropertyBlock(propBlock);
                    propBlock.SetFloat("_MyParam", 0.5f);
                    propBlock.SetVector("_SurfaceNoiseScroll", new(0f, setings.surfaceNoiseScroll, 0f, 0f));
                    propBlock.SetFloat("_SurfaceNoiseCutoff", setings.surfaceNoiseCutoff);
                    propBlock.SetFloat("_FoamMaxDistance", setings.foamMaxDistance);
                    renderer.SetPropertyBlock(propBlock);
                }
            }
        }
    }
}