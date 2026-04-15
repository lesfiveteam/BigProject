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

        public void Init(SoundsManager soundsManager)
        {
            _soundsManager = soundsManager;
        }

        public void Ring()
        {
            _soundsManager.PlaySound(_audioClip);
            Debug.Log("Колокльчик #" + Id + " звенит!!!");
        }
    }
}

