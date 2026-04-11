using BigProject.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BigProject.Systems.Sound
{
    public class RiverSoundMover : MonoBehaviour, ITickable
    {
        [SerializeField] private List<Transform> _waypointTransforms = new();
        [SerializeField] private Transform _soundPoint;
        [SerializeField] private float _moveSpeed = 10f;

        [Header("Gizmos only")]
        [SerializeField] private Color _lineColor = Color.cyan;
        [SerializeField] private Color _soundPointColor = Color.red;
        [SerializeField] private float _soundPointRadius = 0.5f;

        private Vector3[] _cachedPoints;
        private Vector3 _targetPoint;
        private float _updateInterval = 0.1f;
        private float _lastUpdateTime;
        private Transform _playerTransform;

        private void Awake()
        {
            if (_soundPoint == null)
            {
                Debug.LogError("Точка со звуком не назначена");
                enabled = false;
                return;
            }

            if (_waypointTransforms.Count < 2)
            {
                Debug.LogError("Недостаточно опорных точек (минимум 2)");
                enabled = false;
                return;
            }

            CachePoints();
            _soundPoint.position = _cachedPoints[0];
            _targetPoint = _cachedPoints[0];
            _lastUpdateTime = Time.time;
        }

        public void Tick()
        {
            if (Time.time - _lastUpdateTime >= _updateInterval)
            {
                _lastUpdateTime = Time.time;
                FindNearestPointOnLine();
            }

            MoveMarker();
        }

        public void Init(Transform playerTransform)
        {
            ExceptionUtilities.ThrowIfNull(playerTransform, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "PlayerTransform"));
            _playerTransform = playerTransform;
        }

        private void FindNearestPointOnLine()
        {
            _targetPoint = GetNearestPointOnLine(_playerTransform.position);
        }

        private void CachePoints()
        {
            _cachedPoints = new Vector3[_waypointTransforms.Count];

            for (int i = 0; i < _waypointTransforms.Count; i++)
            {
                _cachedPoints[i] = _waypointTransforms[i].position;
            }
        }

        private Vector3 GetNearestPointOnLine(Vector3 point)
        {
            if (_cachedPoints == null || _cachedPoints.Length < 2)
            {
                return Vector3.zero;
            }

            int nearestIdx = 0;
            float minDistSqr = float.MaxValue;
            
            for (int i = 0; i < _cachedPoints.Length; i++)
            {
                float d2 = (_cachedPoints[i] - point).sqrMagnitude;

                if (d2 < minDistSqr)
                {
                    minDistSqr = d2;
                    nearestIdx = i;
                }
            }

            Vector3 bestPoint = _cachedPoints[nearestIdx];
            float bestDistSqr = minDistSqr;

            if (nearestIdx > 0)
            {
                Vector3 a = _cachedPoints[nearestIdx - 1];
                Vector3 b = _cachedPoints[nearestIdx];
                Vector3 closest = ClosestPointOnSegment(a, b, point);
                float d2 = (closest - point).sqrMagnitude;

                if (d2 < bestDistSqr)
                {
                    bestDistSqr = d2;
                    bestPoint = closest;
                }
            }

            if (nearestIdx < _cachedPoints.Length - 1)
            {
                Vector3 a = _cachedPoints[nearestIdx];
                Vector3 b = _cachedPoints[nearestIdx + 1];
                Vector3 closest = ClosestPointOnSegment(a, b, point);
                float d2 = (closest - point).sqrMagnitude;

                if (d2 < bestDistSqr)
                {
                    bestPoint = closest;
                }
            }

            return bestPoint;
        }

        private Vector3 ClosestPointOnSegment(Vector3 a, Vector3 b, Vector3 point)
        {
            Vector3 ab = b - a;
            float t = Vector3.Dot(point - a, ab) / ab.sqrMagnitude;
            t = Mathf.Clamp01(t);
            return a + t * ab;
        }

        private void MoveMarker()
        {
            _soundPoint.position = Vector3.MoveTowards(_soundPoint.position, _targetPoint, _moveSpeed * Time.deltaTime);
        }

        private void OnDrawGizmos()
        {
            if (_waypointTransforms == null || _waypointTransforms.Count < 2)
            {
                return;
            }

            List<Vector3> currentPoints = new List<Vector3>();

            foreach (Transform point in _waypointTransforms)
            {
                if (point != null)
                {
                    currentPoints.Add(point.position);
                }
            }

            Gizmos.color = _lineColor;

            for (int i = 1; i < currentPoints.Count; i++)
            {
                Gizmos.DrawLine(currentPoints[i - 1], currentPoints[i]);
            }

            if (_soundPoint != null)
            {
                Gizmos.color = _soundPointColor;
                Gizmos.DrawSphere(_soundPoint.position, _soundPointRadius);
            }
        }
    }
}