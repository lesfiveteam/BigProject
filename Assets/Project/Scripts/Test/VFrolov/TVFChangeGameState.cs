using BigProject.Managers;
using UnityEngine;

namespace BigProject.Test.VFrolov
{
    public class TVFChangeGameState : MonoBehaviour
    {
        private GameplayManager _gameplayManager;

        public void Init(GameplayManager gameplayManager)
        {
            _gameplayManager = gameplayManager;
        }

        public void ChangeState(int state)
        {
            _gameplayManager.ChangeState((GameplayState)state);
        }
    }
}