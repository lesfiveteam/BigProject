using BigProject.Systems;
using UnityEngine;
using UnityEngine.Assertions;

namespace BigProject.UI.Common
{ 
    public class PolygonRaycast : MonoBehaviour, ICanvasRaycastFilter
    {
        [SerializeField]
        private PolygonCollider2D _collider;

        private void Awake()
        {
            Assert.IsNotNull(_collider, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "PolygonCollider2D"));
        }

        public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(transform as RectTransform, screenPoint, eventCamera, out Vector3 worldPoint))
            {
                return _collider.OverlapPoint(worldPoint);
            }

            return false;
        }
    }
}