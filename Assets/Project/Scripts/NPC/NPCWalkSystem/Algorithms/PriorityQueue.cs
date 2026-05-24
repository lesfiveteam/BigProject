using System;
using System.Collections.Generic;

namespace Assets.Project.Scripts.NPC.NPCWalkSystem.Algorithms
{
    public class PriorityQueue<T>
    {
        private List<(T item, float priority)> _heap = new();

        public int Count => _heap.Count;

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
            _heap[0] = _heap[_heap.Count - 1];
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

        private void BubbleUp(int index)
        {
            while (index > 0)
            {
                int parent = (index - 1) / 2;

                if (_heap[index].priority >= _heap[parent].priority)
                    break;

                (_heap[parent], _heap[index]) = (_heap[index], _heap[parent]);
                index = parent;
            }
        }

        private void BubbleDown(int index)
        {
            int last = _heap.Count - 1;

            while (true)
            {
                int left = index * 2 + 1;
                int right = index * 2 + 2;
                int smallest = index;

                if (left <= last && _heap[left].priority < _heap[smallest].priority)
                    smallest = left;

                if (right <= last && _heap[right].priority < _heap[smallest].priority)
                    smallest = right;

                if (smallest == index)
                    break;

                (_heap[smallest], _heap[index]) = (_heap[index], _heap[smallest]);
                index = smallest;
            }
        }
    }
}