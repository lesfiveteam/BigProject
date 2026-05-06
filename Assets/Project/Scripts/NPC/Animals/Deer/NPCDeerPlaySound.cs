using BigProject.Utilities;
using UnityEngine;

namespace Assets.Project.Scripts.NPC.Animals.Deer
{
    public class NPCDeerPlaySound : MonoBehaviour
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