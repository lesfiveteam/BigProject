using UnityEngine;

namespace Assets.Project.Scripts.NPC.NPCWalkSystem
{
    public class NPCAttractionPoint : NPCRootPoint
    {
        [field: SerializeField] public float DelayModificator { get; private set; } = 1f;
        [field: SerializeField, Range(0, 10)] public int Weight { get; private set; } = 5;
        [field: SerializeField] public NPCAnimationAction TargetAnimation { get; private set; } = NPCAnimationAction.Idle;

        public string Id => $"{transform.position.x:F0}_{transform.position.y:F0}_{transform.position.z:F0}";

#if UNITY_EDITOR
        private readonly Color ATTRACTION_COLOR = Color.blue;
        private readonly Color CROSSROAD_COLOR = Color.blueViolet;

        private const float SPHERE_RADIUS = 1.0f;

        private const float LABEL_OFFSET_Y = 3f;
        private const int LABEL_FONT_SIZE = 12;

        protected override void DrawSphere()
        {
            Gizmos.color = Weight > 0 ? ATTRACTION_COLOR : CROSSROAD_COLOR;
            Gizmos.DrawSphere(transform.position, SPHERE_RADIUS);
        }

        protected override void AdditionalDraw() => DrawLabel();

        private void DrawLabel()
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.black;
            style.fontSize = LABEL_FONT_SIZE;
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.MiddleCenter;

            Vector3 labelPos = transform.position + Vector3.up * LABEL_OFFSET_Y;
            UnityEditor.Handles.Label(labelPos, Id, style);
        }
#endif
    }
}