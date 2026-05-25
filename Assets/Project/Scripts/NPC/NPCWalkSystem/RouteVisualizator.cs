using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Project.Scripts.NPC.NPCWalkSystem
{
    [ExecuteAlways]
    public class RouteVisualizator : MonoBehaviour
    {
#if UNITY_EDITOR
        private const float UPDATE_TRANSFORMS_DELAY = 1.0f;

        private List<NPCWay> _activeWays;

        [SerializeField] private NPCRouteService _routeService;

        [SerializeField] private ChildCountChangeNotifier _attractionPointsParent;
        [SerializeField] private ChildCountChangeNotifier _routePointsParent;

        [SerializeField] private bool _isAttractionPointsVisible;
        private bool _cashedIsAttractionPointsVisible;

        [SerializeField] private bool _isRootPointsVisible;
        private bool _cashedIsRootPointsVisible;

        [SerializeField] private bool _isWaysVisible;
        private bool _cashedIsWaysVisible;

        private List<NPCRoutePoint> _attractionPoints = new();
        private List<NPCRoutePoint> _routePoints = new();

        private float _updateTimer = 0f;
        private Coroutine _updateCoroutine;

        private void OnEnable()
        {
            _activeWays = GetComponent<NPCRouteService>().ActiveWays;
            _attractionPointsParent.ChildCountChanged += OnAttractionPointChanged;
            _routePointsParent.ChildCountChanged += OnRoutePointChanged;

            UpdatePointsList(_attractionPointsParent.transform, _attractionPoints, _attractionPointsParent.transform.childCount);
            UpdatePointsList(_routePointsParent.transform, _routePoints, _routePointsParent.transform.childCount);

            _updateCoroutine = StartCoroutine(UpdateTransforms());
        }

        private void OnDisable()
        {
            _attractionPointsParent.ChildCountChanged -= OnAttractionPointChanged;
            _routePointsParent.ChildCountChanged -= OnRoutePointChanged;

            if (_updateCoroutine != null)
            {
                StopCoroutine(_updateCoroutine);
                _updateCoroutine = null;
            }
        }

        private void OnAttractionPointChanged(int childCount)
            => UpdatePointsList(_attractionPointsParent.transform, _attractionPoints, childCount);

        private void OnRoutePointChanged(int childCount)
            => UpdatePointsList(_routePointsParent.transform, _routePoints, childCount);

        private void UpdatePointsList(Transform parent, List<NPCRoutePoint> cachedPoints, int childCount)
        {
            cachedPoints.Clear();

            for (int i = 0; i < childCount; i++)
            {
                NPCRoutePoint child = parent.GetChild(i).GetComponentInChildren<NPCRoutePoint>();

                if (child != null)
                    cachedPoints.Add(child);
            }
        }

        private void OnDrawGizmos()
        {
            CheckboxChecker();

            if (_isWaysVisible)
                DrawWays();
        }

        private void CheckboxChecker()
        {
            if (_isAttractionPointsVisible != _cashedIsAttractionPointsVisible)
            {
                _cashedIsAttractionPointsVisible = _isAttractionPointsVisible;

                foreach (NPCRoutePoint point in _attractionPoints)
                    point.IsVisible = _cashedIsAttractionPointsVisible;
            }

            if (_isRootPointsVisible != _cashedIsRootPointsVisible)
            {
                _cashedIsRootPointsVisible = _isRootPointsVisible;

                foreach (NPCRoutePoint point in _routePoints)
                    point.IsVisible = _cashedIsRootPointsVisible;
            }

            if (_isWaysVisible != _cashedIsWaysVisible)
            {
                _cashedIsWaysVisible = _isWaysVisible;

                foreach (NPCWay way in _activeWays)
                    if (way != null)
                        way.IsVisible = _cashedIsWaysVisible;
            }
        }

        private IEnumerator UpdateTransforms()
        {
            while (isActiveAndEnabled)
            {
                if(!_isWaysVisible && !_isAttractionPointsVisible && !_isRootPointsVisible)
                {
                    yield return null;
                    continue;
                }

                if (Time.realtimeSinceStartup - _updateTimer > UPDATE_TRANSFORMS_DELAY)
                {
                    _updateTimer = Time.realtimeSinceStartup;

                    foreach (NPCWay way in _activeWays)
                    {
                        way.UpdateCaches();
                    }
                }

                yield return null;
            }
        }

        private void DrawWays()
        {
            foreach (NPCWay way in _activeWays)
            {
                way.DrawGizmos();
            }
        }
#endif
    }
}