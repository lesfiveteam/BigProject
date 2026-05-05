using Assets.Project.Scripts.NPC;
using BigProject.Systems;
using BigProject.Utilities;
using UnityEngine;
using UnityEngine.Assertions;

namespace BigProject.NPC
{
    public class NPCCutsceneController : MonoBehaviour
    {
        [SerializeField]
        private Animator _animator;

        private void Awake()
        {
            Assert.IsNotNull(_animator, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, $"{name}", "Animator"));
        }

        public void SetAnimation(string animation)
        {
            _animator.SetTrigger(animation);
        }
    }
}