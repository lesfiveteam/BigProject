using System.Collections.Generic;
using UnityEngine;

namespace BigProject.Systems
{
    /// <summary>
    /// Manual update loop based on MonoBehaviour.Update.
    /// </summary>
    public class ManualLoop : MonoBehaviour
    {       
        private class TickQueue
        {
            public bool isActive = true;
            public List<ITickable> tickables = new();
            public List<IFixedTickable> fixedTickables = new();
            public List<ILateTickable> lateTickables = new();
            public bool IsEmpty => tickables.Count == 0 && fixedTickables.Count == 0 && lateTickables.Count == 0;
        }

        private readonly Dictionary<int, TickQueue> _tickQueues = new();
        private readonly List<TickQueue> _activeQueues = new();

        public static float DeltaTime => Time.deltaTime;
        public static float FixedDeltaTime => Time.fixedDeltaTime;

        private void Update()
        {
            foreach (TickQueue queue in _activeQueues)
            {
                foreach (ITickable tickable in queue.tickables)
                {
                    tickable.Tick();
                }
            }
        }

        private void FixedUpdate()
        {
            foreach (TickQueue queue in _activeQueues)
            {
                foreach (IFixedTickable tickable in queue.fixedTickables)
                {
                    tickable.FixedTick();
                }
            }
        }

        private void LateUpdate()
        {
            foreach (TickQueue queue in _activeQueues)
            {
                foreach (ILateTickable tickable in queue.lateTickables)
                {
                    tickable.LateTick();
                }
            }
        }

        /// <summary>
        /// Add updatable object.
        /// </summary>
        /// <param name="queueId">Id of update queue</param>
        public void AddTickable(object tickable, int queueId = 0)
        {
            if (_tickQueues.TryGetValue(queueId, out TickQueue queue))
            {
                AddTickableToQueue(queue, tickable);
            }
            else
            {
                TickQueue newQueue = new();
                AddTickableToQueue(newQueue, tickable);

                if (!newQueue.IsEmpty)
                {
                    _tickQueues.Add(queueId, newQueue);
                    _activeQueues.Add(newQueue);
                }           
            }
        }

        public void RemoveTickable(object tickable, int queueId = 0)
        {
            if (_tickQueues.TryGetValue(queueId, out TickQueue queue))
            {
                RemoveTickableFromQueue(queue, tickable);   
            }
            else
            {
                Debug.LogWarning(string.Format(LogStr.WARNING_SYSTEM, "ManualLoop",
                    $"unable to remove tickable {tickable.GetType()}, has no queue {queueId}"));
            }

        }

        /// <summary>
        /// Add updatable object list.
        /// </summary>
        /// <param name="queueId">Id of update queue</param>
        public void AddTickables(List<object> tickables, int queueId = 0)
        {
            foreach (object tickable in tickables)
            {
                AddTickable(tickable, queueId);
            }
        }

        /// <summary>
        /// Set queue activity.
        /// </summary>
        public void SetTickableQueueActive(int queueId, bool isActive)
        {
            if (!_tickQueues.TryGetValue(queueId, out TickQueue queue))
            {
                Debug.LogError($"Manual Loop: can't find queue {queueId}");
                return;
            }

            queue.isActive = isActive;
            bool wasActive = _activeQueues.Contains(queue);

            if (isActive && !wasActive)
            {
                _activeQueues.Add(queue);
            }
            else if (!isActive && wasActive)
            {
                _activeQueues.Remove(queue);
            }
        }

        public bool IsTickableQueueActive(int queueId)
        {
            if (!_tickQueues.TryGetValue(queueId, out TickQueue queue))
            {
                return false;
            }

            return queue.isActive;
        }

        private void AddTickableToQueue(TickQueue queue, object tickObj)
        {
            if (tickObj is ITickable tickable)
            {
                queue.tickables.Add(tickable);
            }

            if (tickObj is IFixedTickable fixedTickable)
            {
                queue.fixedTickables.Add(fixedTickable);
            }

            if (tickObj is ILateTickable lateTickable)
            {
                queue.lateTickables.Add(lateTickable);
            }
        }

        private void RemoveTickableFromQueue(TickQueue queue, object tickObj)
        {
            if (tickObj is ITickable tickable)
            {
                queue.tickables.Remove(tickable);
            }

            if (tickObj is IFixedTickable fixedTickable)
            {
                queue.fixedTickables.Remove(fixedTickable);
            }

            if (tickObj is ILateTickable lateTickable)
            {
                queue.lateTickables.Remove(lateTickable);
            }
        }
    }
}