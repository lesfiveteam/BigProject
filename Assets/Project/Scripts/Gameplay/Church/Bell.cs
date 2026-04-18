using BigProject.Managers.SoundsMusicManagers;
using UnityEngine;

namespace BigProject.Gameplay.Church
{
    public class Bell : MonoBehaviour
    {
        public int Id;
        [SerializeField]
        private AudioClip _audioClip;
        
        private SoundsManager _soundsManager;
        private Animator _animator;

        public void Init(SoundsManager soundsManager)
        {
            _soundsManager = soundsManager;
            _animator = GetComponent<Animator>();
        }

        public void Ring()
        {
            //_soundsManager.PlaySound(_audioClip);
            Debug.Log("Колокльчик #" + Id + " звенит!!!");
            //_animator.SetTrigger("Ring");
            _animator.Play("Scene", 0, 0f);
        }
    }
}

