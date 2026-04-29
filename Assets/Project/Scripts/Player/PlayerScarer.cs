using Assets.Project.Scripts.NPC.Animals;
using UnityEngine;

namespace Assets.Project.Scripts.Player
{
    public  class PlayerScarer : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out IScared victim))
            {
                victim.Scare(transform);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out IUnscared victim))
            {
                victim.Unscare();
            }
        }
    }
}