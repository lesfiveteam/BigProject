using UnityEngine;

namespace Assets.Project.Scripts.NPC.Animals
{
    public class NPCDog : MonoBehaviour, IScared, IUnscared
    {
        private readonly int ActionBool = Animator.StringToHash("isActive");

        [SerializeField] private Animator _animator;

        public void Scare(Vector3 dangerPosition) => _animator.SetBool(ActionBool, true);

        public void Unscare() => _animator.SetBool(ActionBool, false);
    }
}