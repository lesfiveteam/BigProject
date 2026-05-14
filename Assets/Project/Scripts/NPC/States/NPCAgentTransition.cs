using Assets.Project.Scripts.NPC.NPCWalkSystem;
using BigProject.Systems;
using BigProject.Utilities;
using System;
using UnityEngine;

namespace BigProject.NPC.States
{
    public class NPCAgentTransition : IDisposable
    {
        private Transform _target;
        private NPCWalkController _walkController;

        private const float CHAT_DISTANCE = 1f;

        public NPCAgentTransition(Transform target, NPCWalkController walkController)
        {
            _target = target;
            _walkController = walkController;
            ExceptionUtilities.ThrowIfNull(_target, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"NPCAgentTransition", "Target Transform"));
            ExceptionUtilities.ThrowIfNull(_walkController, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"NPCStateWait", "NPCWalkController"));
        }

        public void GoTo(Action onComplete = null)
        {
            Vector3 offset = (_walkController.transform.position - _target.transform.position).normalized * CHAT_DISTANCE;
            Vector3 target = _target.transform.position + offset;

            _walkController.GoTo(target, onComplete);
        }

        public void LookAt(Action onComplete = null)
        {
            _walkController.LookAt(_target.position, onComplete);
        }

        public void GoToAndLookAt(Action onComplete = null) => GoTo(() => LookAt(onComplete));

        public void Dispose() { }
    }
}