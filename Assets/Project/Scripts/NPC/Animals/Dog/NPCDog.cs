using UnityEngine;

namespace Assets.Project.Scripts.NPC.Animals.Dog
{
    public class NPCDog : MonoBehaviour, IScared, IUnscared
    {
        private readonly int ActionBool = Animator.StringToHash("isActive");

        [SerializeField] private Animator _animator;

        public void Scare(Transform danger) => _animator.SetBool(ActionBool, true);

        public void Unscare() => _animator.SetBool(ActionBool, false);
    }
}