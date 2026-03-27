using UnityEngine;

namespace Assets.Project.Scripts.NPC.NPCWalkSystem
{
    public class NPCRootPoint : MonoBehaviour
    {
        [SerializeField] public bool IsVisible;

        public Vector3 Position => transform.position;

#if UNITY_EDITOR
        private readonly Color SPHERE_COLOR = Color.blueViolet;
        private const float SPHERE_RADIUS = 0.5f;

        private const float SNAP_OFFSET = 0.1f;
        private const int TERRAIN_LAYER = 1 << 0;

        protected virtual void OnDrawGizmos()
        {
            if (!IsVisible)
                return;

            // Auto-snap when moving in the editor
            if (UnityEditor.Selection.Contains(gameObject))
                SnapToTerrain();

            DrawSphere();
        }

        private void SnapToTerrain()
        {
            RaycastHit hit;
            Vector3 origin = transform.position + Vector3.up * 100f;

            if (Physics.Raycast(origin, Vector3.down, out hit, 200f, TERRAIN_LAYER))
            {
                Vector3 pos = transform.position;
                pos.y = hit.point.y + SNAP_OFFSET;
                transform.position = pos;
            }
        }

        protected virtual void DrawSphere()
        {
            Gizmos.color = SPHERE_COLOR;
            Gizmos.DrawSphere(transform.position, SPHERE_RADIUS);
        }
#endif
    }
}