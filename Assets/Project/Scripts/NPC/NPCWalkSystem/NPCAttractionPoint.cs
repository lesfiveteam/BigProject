using UnityEngine;

namespace Assets.Project.Scripts.NPC.NPCWalkSystem
{
    public class NPCAttractionPoint : NPCRoutePoint
    {
        private const int MINIMAL_POINT_WEIGHT = 0;

        private int _graphWeithWithoutThis;
        private int _cachedId;

        [field: SerializeField] public NPCAnimationAction TargetAnimation { get; private set; } = NPCAnimationAction.Downtime;
        [field: SerializeField] public float Modificator { get; private set; } = 1f;
        [field: SerializeField, Range(MINIMAL_POINT_WEIGHT, 10)] public int Weight { get; private set; } = 5;

        public int Id
        {
            get
            {
                if (_cachedId != 0)
                    return _cachedId;

                int x = (int)transform.position.x;
                int z = (int)transform.position.z;
                _cachedId = (x << 16) | (z & 0xFFFF);

                return _cachedId;
            }
        }

        public int GraphWeightWithoutThis
        {
            get
            {
                if (_graphWeithWithoutThis > 0)
                    return _graphWeithWithoutThis;

                Debug.LogError($"{gameObject.name}: Total weight without this point is not inited");
                return 0;
            }
            private set
            {
                if (value < 0)
                {
                    Debug.LogError($"{gameObject.name}: Total weight without this point can't be: {value}");
                    return;
                }

                _graphWeithWithoutThis = value;
            }
        }

        public void Init(int totalWeightWithoutThis)
        {
            _ = Id;
            GraphWeightWithoutThis = totalWeightWithoutThis;
        }

#if UNITY_EDITOR
        private const float ATTRACTION_POINT_SPHERE_RADIUS = 0.3f;
        private readonly Color ATTRACTION_POINT_COLOR = Color.blue;
       
        private const float LABEL_OFFSET_Y = 3f;
        private const int LABEL_FONT_SIZE = 12;


        private static GUIStyle _labelStyle;

        protected override void DrawSphere()
        {
            Gizmos.color = Weight > MINIMAL_POINT_WEIGHT ? ATTRACTION_POINT_COLOR : ROUTE_POINT_COLOR;
            Gizmos.DrawSphere(transform.position, ATTRACTION_POINT_SPHERE_RADIUS);
        }

        protected override void AdditionalDraw() => DrawLabel();

        private void DrawLabel()
        {
            if (_labelStyle == null)
            {
                _labelStyle = new GUIStyle();
                _labelStyle.normal.textColor = ATTRACTION_POINT_COLOR;
                _labelStyle.fontSize = LABEL_FONT_SIZE;
                _labelStyle.fontStyle = FontStyle.Bold;
                _labelStyle.alignment = TextAnchor.MiddleCenter;
            }

            Vector3 labelPos = Position + (Vector3.up * LABEL_OFFSET_Y);
            UnityEditor.Handles.Label(labelPos, gameObject.name, _labelStyle);
        }
#endif
    }
}