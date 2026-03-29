using UnityEngine;

namespace BigProject.Systems.Sound
{
    //Is required on all surfaces player can walk on. Also requires a Collider to be registered.
    //Make sure that collider of the object is lower, than the boy transform point
    
    public class GroundSounds : MonoBehaviour
    {
        [SerializeField] 
        private AudioClip[] _stepSounds;

        [SerializeField]
        private float _volume = 0.2f;
        public AudioClip GetStepSound()
        {
            return _stepSounds[Random.Range(0, _stepSounds.Length)];
        }

        public float GetSoundVolume()
        {
            return _volume;
        }
    }
}