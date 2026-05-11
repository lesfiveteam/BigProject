using UnityEngine;

namespace Assets.Project.Scripts.NPC.Animals.Chicken
{
    public class NPCPeckPoint : MonoBehaviour
    {
        [field: SerializeField] public bool IsSpawnPoin { get; private set; } = false;
        [field: SerializeField, Min(0)] public int ChickenCount {  get; private set; } = 0;

        private bool _isOccupied;

        public bool IsOccupied
        {
            get => _isOccupied;
            set
            {
                if (value == _isOccupied)
                    Debug.LogError($"PeckPoint {gameObject.name} allready occupied: {value}");

                _isOccupied = value;
            }
        }
    }
}
