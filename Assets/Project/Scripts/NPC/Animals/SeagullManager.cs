using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Project.Scripts.NPC.Animals
{
    public class SeagullManager : MonoBehaviour
    {
        [SerializeField] private Transform[] _waypoints;
        [SerializeField] private NPCSeagull[] _seagulls;

        [SerializeField, Min(0)] private int _maxSeagullsInAir;
        [SerializeField] private float _patrolDelay = 5f;

        private List<NPCSeagull> _seagullReadyToFly = new();
        private Coroutine _sendCoroutine;
        private WaitForSeconds _patrolWait;

        private bool _isRunning = false;

        private void Start()
        {
            _patrolWait = new WaitForSeconds(_patrolDelay);

            InitSeagulls();

            _isRunning = true;

            if (_sendCoroutine != null)
                StopCoroutine(_sendCoroutine);

            _sendCoroutine = StartCoroutine(SendSeagull());
        }

        private void InitSeagulls()
        {
            if (_seagulls == null || _seagulls.Length == 0)
                return;

            foreach (NPCSeagull seagull in _seagulls)
            {
                seagull.ReadyToFly += OnReadyToFly;
                seagull.SetWaypoints(_waypoints);
            }
        }

        private void OnReadyToFly(NPCSeagull seagull, bool isReady)
        {
            if (isReady)
                _seagullReadyToFly.Add(seagull);
            else
                _seagullReadyToFly.Remove(seagull);
        }

        private void OnValidate()
        {
            if (_seagulls != null && _maxSeagullsInAir > _seagulls.Length)
                _maxSeagullsInAir = _seagulls.Length;
        }

        private IEnumerator SendSeagull()
        {
            while (_isRunning)
            {
                yield return _patrolWait;

                if (_seagullReadyToFly.Count == 0)
                    continue;

                if (_seagulls.Length - _seagullReadyToFly.Count >= _maxSeagullsInAir)
                    continue;

                NPCSeagull randomSeagull = _seagullReadyToFly[Random.Range(0, _seagullReadyToFly.Count)];
                randomSeagull.StartFlight(randomSeagull.transform.forward);
            }
        }

        private void OnDestroy()
        {
            foreach (NPCSeagull seagull in _seagulls)
            {
                seagull.ReadyToFly -= OnReadyToFly;
            }

            if (_sendCoroutine != null)
                StopCoroutine(_sendCoroutine);
        }

        // FOR DEBUG
        //[SerializeField] private bool TestFly = false;
        //private void Update()
        //{
        //    if (TestFly)
        //    {
        //        foreach (NPCSeagull seagull in _seagulls)
        //        {
        //            if (!seagull.InFly)
        //                seagull.StartFlight(seagull.transform.forward);
        //        }
        //    }
        //}
    }
}