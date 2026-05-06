using BigProject.Utilities;
using UnityEngine;

namespace Assets.Project.Scripts.NPC.Animals.Dog
{
    public class NPCDogPlaySound : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;

        private void Start()
        {
            ExceptionUtilities.ThrowIfNullFormat(_audioSource);
        }

        private void PlaySound()
        {
            _audioSource.Play();
        }
    }
}