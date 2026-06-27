using System;
using System.Collections.Generic;

namespace Assets.Project.Scripts.NPC.NPCWalkSystem.Algorithms
{
    public class PriorityQueue<T>
    {
        private List<(T item, float priority)> _heap;

        public int Count => _heap.Count;

        public PriorityQueue(int capacity = 0)
        {
            _heap = new(capacity);
        }

        public void Enqueue(T item, float priority)
        {
            _heap.Add((item, priority));
            BubbleUp(_heap.Count - 1);
        }

        public T Dequeue()
        {
            if (_heap.Count == 0)
                throw new InvalidOperationException("Queue is empty");

            T item = _heap[0].item;
            _heap[0] = _heap[^1];
            _heap.RemoveAt(_heap.Count - 1);

            if (_heap.Count > 0)
                BubbleDown(0);

            return item;
        }

        public bool TryDequeue(out T item)
        {
            if (_heap.Count == 0)
            {
                item = default;

                return false;
            }

            item = Dequeue();

            return true;
        }

        public bool TryDequeue(out T item, out float priority)
        {
            if (_heap.Count == 0)
            {
                item = default;
                priority = 0f;

                return false;
            }

            (item, priority) = _heap[0];
            _heap[0] = _heap[^1];
            _heap.RemoveAt(_heap.Count - 1);

            if (_heap.Count > 0)
                BubbleDown(0);

            return true;
        }

        public T Peek()
        {
            if (_heap.Count == 0)
                throw new InvalidOperationException("Queue is empty");

            return _heap[0].item;
        }

        public float PeekPriority()
        {
            if (_heap.Count == 0)
                throw new InvalidOperationException("Queue is empty");

            return _heap[0].priority;
        }

        public void Clear()
        {
            _heap.Clear();
        }

        private void BubbleUp(int index)
        {
            List<(T item, float priority)> heap = _heap;

            while (index > 0)
            {
                int parent = (index - 1) / 2;

                if (heap[index].priority >= heap[parent].priority)
                    break;

                (heap[parent], heap[index]) = (heap[index], heap[parent]);
                index = parent;
            }
        }

        private void BubbleDown(int index)
        {
            int last = _heap.Count - 1;
            List<(T item, float priority)> heap = _heap;

            while (true)
            {
                int left = (index * 2) + 1;

                if (left > last)
                    break;

                int right = left + 1;
                int smallest = left;

                if (right <= last && heap[right].priority < heap[left].priority)
                    smallest = right;

                if (heap[smallest].priority >= heap[index].priority)
                    break;

                (heap[smallest], heap[index]) = (heap[index], heap[smallest]);
                index = smallest;
            }
        }
    }
}