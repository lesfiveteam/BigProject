using UnityEngine;

namespace Assets.Project.Scripts.NPC.NPCWalkSystem
{
    public class NPCAttractionPoint : NPCRootPoint
    {
        [field: SerializeField] public float DelayModificator { get; private set; } = 1f;
        [field: SerializeField] public NPCAnimation TargetAnimation { get; private set; } = NPCAnimation.Idle;

        public string Id => $"{transform.position.x:F1}_{transform.position.y:F1}_{transform.position.z:F1}";

#if UNITY_EDITOR
        private readonly Color SPHERE_COLOR = Color.blue;
        private const float SPHERE_RADIUS = 1.0f;

        private const float LABEL_OFFSET_Y = 3f;
        private const int LABEL_FONT_SIZE = 12;

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            DrawLabel();
        }

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

        protected override void DrawSphere()
        {
            Gizmos.color = SPHERE_COLOR;
            Gizmos.DrawSphere(transform.position, SPHERE_RADIUS);
        }
#endif
    }
}