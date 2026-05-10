using BigProject.Utilities;
using UnityEngine;

namespace Assets.Project.Scripts.NPC.Animals.Dog
{
    public class NPCDog : MonoBehaviour, IScared, IUnscared
    {
        private readonly int _actionBool = Animator.StringToHash("isActive");

        [SerializeField] private Animator _animator;

        private void Start()
        {
            ExceptionUtilities.ThrowIfNullFormat(_animator);
        }

        public void Scare(Transform danger) => _animator.SetBool(_actionBool, true);

        public void Unscare() => _animator.SetBool(_actionBool, false);
    }
}