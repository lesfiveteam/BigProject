using System.Collections.Generic;
using UnityEngine;

namespace Assets.Project.Scripts.NPC.Animals.Fish
{
    public class NPCFishPool : MonoBehaviour
    {
        [SerializeField] private NPCFish _prefab;
        [SerializeField] private int _poolSize = 10;
        [SerializeField] private Transform _parent;

        private Vector3[] _cachedPositions;
        private float[] _cachedDistances;

        private Queue<NPCFish> _pool = new();

        public void Init(Vector3[] positions, float[] distances)
        {
            _cachedPositions = positions;
            _cachedDistances = distances;

            CreatePool();
        }

        private void CreatePool()
        {
            for (int i = 0; i < _poolSize; i++)
            {
                NPCFish fish = Instantiate(_prefab, _parent);
                fish.Init(this, _cachedPositions, _cachedDistances);
                fish.gameObject.SetActive(false);
                _pool.Enqueue(fish);
            }
        }

        public NPCFish Get()
        {
            if (_pool.Count > 0)
            {
                NPCFish fish = _pool.Dequeue();
                fish.gameObject.SetActive(true);

                return fish;
            }

            NPCFish newFish = Instantiate(_prefab, _parent);
            newFish.Init(this, _cachedPositions, _cachedDistances);

            return newFish;
        }

        public void Return(NPCFish fish)
        {
            fish.gameObject.SetActive(false);
            _pool.Enqueue(fish);
        }
    }
}