using UnityEngine;

namespace Assets.Project.Scripts.NPC.NPCWalkSystem
{
    public class NPCRoutePoint : MonoBehaviour
    {
        private const int GROUND_LAYER = 0;
        public Vector3 Position { get; private set; }

        private void OnEnable()
        {
            Position = transform.position;
        }

#if UNITY_EDITOR
        private const float ROUTE_POINT_SPHERE_RADIUS = 0.2f;
        protected readonly Color ROUTE_POINT_COLOR = Color.blueViolet;

        private const float SNAP_OFFSET = 0.1f;
        private const int TERRAIN_LAYER = 1 << GROUND_LAYER;

        [field: SerializeField] public bool IsVisible { get; set; }
        [SerializeField] public bool IsSnap = true;

        private void OnDrawGizmos()
        {
            if (!IsVisible)
                return;

            DrawSphere();
            AdditionalDraw();
            PositionChangeChecker();
        }

        private void OnDrawGizmosSelected()
        {
            SnapToTerrain();
        }

        private void PositionChangeChecker()
        {
            if (transform.hasChanged)
            {
                Position = transform.position;
                transform.hasChanged = false;
            }
        }

        protected virtual void DrawSphere()
        {
            Gizmos.color = ROUTE_POINT_COLOR;
            Gizmos.DrawSphere(transform.position, ROUTE_POINT_SPHERE_RADIUS);
        }

        protected virtual void AdditionalDraw() { }

        private void SnapToTerrain()
        {
            if (!IsSnap)
                return;

            Vector3 origin = transform.position + (Vector3.up * 100f);

            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 200f, TERRAIN_LAYER))
            {
                Vector3 pos = transform.position;
                pos.y = hit.point.y + SNAP_OFFSET;
                transform.position = pos;
            }
        }
#endif
    }
}