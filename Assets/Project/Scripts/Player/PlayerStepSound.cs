using UnityEngine;

namespace BigProject.Player
{
    public class PlayerStepSound : MonoBehaviour
    {
        [SerializeField] private PlayerController _playerController;

        public void PlayGroundSound() => _playerController.PlayGroundSound();
    }
}