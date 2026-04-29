using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Project.Scripts.NPC.Animals.Chicken
{
    public class NPCCock : NPCFowl
    {
        private const float MIN_TIME_TO_CHANGE_PECK_POINT = 10f;
        private const float MAX_TIME_TO_CHANGE_PECK_POINT = 15f;

        public Action<NPCPeckPoint> changePeckPoint;

        private NPCChickenSpawner _peckManager;

        private Coroutine _changePeckPointCoroutine = null;

        private float _timeToChangePeckPoint;
        private float _peckPointChangeTimer = 0f;

        public void Init(NPCPeckPoint currentPeckPoint, NPCChickenSpawner peckManager)
        {
            _currentPeckPoint = currentPeckPoint;
            _peckManager = peckManager;

            _timeToChangePeckPoint = Random.Range(MIN_TIME_TO_CHANGE_PECK_POINT, MAX_TIME_TO_CHANGE_PECK_POINT);
            _isAlive = true;

            _peckCoroutine = StartCoroutine(PeckRoutine());
        }

        protected override void PeckTimer()
        {
            if (_changePeckPointCoroutine != null)
            {
                StopCoroutine(_changePeckPointCoroutine);
                _changePeckPointCoroutine = null;
            }

            _changePeckPointCoroutine = StartCoroutine(ChangePeckPointRoutine());
        }

        protected IEnumerator ChangePeckPointRoutine()
        {
            bool waitToNewPeckPoint = true;

            while (waitToNewPeckPoint)
            {
                if (_isScared)
                {
                    yield return new WaitWhile(() => Time.timeScale == 0);
                    continue;
                }

                _peckPointChangeTimer += Time.deltaTime;

                if (_peckPointChangeTimer > _timeToChangePeckPoint)
                {
                    _peckPointChangeTimer = 0f;
                    _timeToChangePeckPoint = Random.Range(MIN_TIME_TO_CHANGE_PECK_POINT, MAX_TIME_TO_CHANGE_PECK_POINT);

                    _newPeckPoint = _peckManager.GetNewPeckPoint(_currentPeckPoint);

                    if (_newPeckPoint != null)
                    {
                        waitToNewPeckPoint = false;

                        _goToNewPeckPointCoroutine = StartCoroutine(GoToNewPeckPointRoutine());
                        changePeckPoint?.Invoke(_newPeckPoint);
                    }
                }

                yield return new WaitWhile(() => Time.timeScale == 0);
            }
        }

        private void OnDisable()
        {
            if (_changePeckPointCoroutine != null)
            {
                StopCoroutine(_changePeckPointCoroutine);
                _changePeckPointCoroutine = null;
            }
        }
    }
}