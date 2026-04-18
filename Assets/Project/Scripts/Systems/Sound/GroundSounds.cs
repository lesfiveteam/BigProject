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
        private float _stepVolume = 1f;

        public AudioClip GetStepSound() => _stepSounds[Random.Range(0, _stepSounds.Length)];
        public float StepVolume => _stepVolume;

        private void Awake()
        {
            foreach(AudioClip stepSound in _stepSounds)
            {
                if(stepSound == null)
                {
                    Debug.LogError("На " + gameObject.name + " звук == null");
                }
            }
        }
    }
}