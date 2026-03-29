using BigProject.Intercatable;
using BigProject.Managers.SoundsMusicManagers;
using UnityEngine;

namespace BigProject.Gameplay.Village
{
    public class ChestSound : MonoBehaviour, IInteractable
    {
        [SerializeField]
        private AudioClip _sound;
        private SoundsManager _soundsManager;

        public void Init(SoundsManager soundsManager)
        {
            _soundsManager = soundsManager;
        }
        public void Interact()
        {
            _soundsManager.PlaySound(_sound);
        }
    }
}

