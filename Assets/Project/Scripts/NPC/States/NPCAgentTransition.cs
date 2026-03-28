using BigProject.Systems;
using BigProject.Utilities;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;

namespace BigProject.NPC.States
{
    public class NPCAgentTransition : IDisposable
    {
        private NavMeshAgent _agent;
        private Transform _target;
        private CancellationTokenSource _ctSource;

        private const float CHAT_DISTANCE = 1f;
        private const float MAX_DISTANCE_TO_NAV_MESH = 3f;

        public NPCAgentTransition(NavMeshAgent agent, Transform target)
        {
            _agent = agent;
            _target = target;
            ExceptionUtilities.ThrowIfNull(_agent, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"NPCAgentTransition", "NavMeshAgent"));
            ExceptionUtilities.ThrowIfNull(_target, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"NPCAgentTransition", "Target Transform"));
        }

        public void GoTo(Action onComplete = null)
        {
            PrepareToken();
            _ = GoToRoutine(onComplete, _ctSource.Token);
        }

        public void LookAt(Action onComplete = null)
        {
            PrepareToken();
            _ = LookAtRoutine(onComplete, _ctSource.Token);
        }

        public void GoToAndLookAt(Action onComplete = null) => GoTo(() => LookAt(onComplete));

        private void PrepareToken()
        {
            if (_ctSource != null && !_ctSource.IsCancellationRequested)
            {
                _ctSource.Cancel();
            }

            _ctSource?.Dispose();
            _ctSource = new();
        }

        private async Awaitable GoToRoutine(Action onComplete, CancellationToken ct)
        {
            try
            {
                Vector3 offset = _target.transform.forward * CHAT_DISTANCE;
                Vector3 target = _target.transform.position + offset;

                if (NavMesh.SamplePosition(target, out NavMeshHit hit, MAX_DISTANCE_TO_NAV_MESH, NavMesh.AllAreas))
                {
                    target = hit.position;
                }

                _agent.SetDestination(target);

                while (_agent.pathPending || _agent.remainingDistance > _agent.stoppingDistance)
                {
                    await Awaitable.NextFrameAsync(ct);
                }

                onComplete?.Invoke();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Debug.LogError(string.Format(LogStr.ERROR_SYSTEM, $"{_agent.name} NPCAgentTransition", $"go to routine error.\n{ex.Message}"));
            }
        }

        private async Awaitable LookAtRoutine(Action onComplete, CancellationToken ct)
        {
            try
            {
                Vector3 lookDir = (_target.position - _agent.transform.position);
                lookDir.y = 0f;

                if (lookDir != Vector3.zero)
                {
                    Quaternion targetRot = Quaternion.LookRotation(lookDir);

                    while (Quaternion.Angle(_agent.transform.rotation, targetRot) > 1f)
                    {
                        _agent.transform.rotation = Quaternion.Slerp(_agent.transform.rotation, targetRot, Time.deltaTime * _agent.angularSpeed);
                        await Awaitable.NextFrameAsync(ct);
                    }
                }

                onComplete?.Invoke();
            }
            catch (Exception ex) when(ex is not OperationCanceledException)
            {
                Debug.LogError(string.Format(LogStr.ERROR_SYSTEM, $"{_agent.name} NPCAgentTransition", $"look at routine error.\n{ex.Message}"));
            }
        }

        public void Dispose()
        {
            _ctSource?.Cancel();
            _ctSource?.Dispose();
            _ctSource = null;
        }
    }
}