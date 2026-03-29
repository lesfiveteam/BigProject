using BigProject.Managers;
using BigProject.Managers.SoundsMusicManagers;
using BigProject.Player;
using BigProject.Systems.QuestSystem;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

namespace BigProject.Gameplay.Village
{
    public class EntryPoint : MonoBehaviour
    {
        [SerializeField]
        private CinemachineCamera _camera;
        [SerializeField]
        private AudioClip _music;
        [SerializeField]
        private List<MonoBehaviour> _questsControllers;

        private QuestsBoundariesTracker _questsTracker;

        public void Init()
        {
            _questsControllers.RemoveAll(x => x is not IQuestBoundariesController);
            _questsTracker = ServiceLocator.GetService<QuestsBoundariesTracker>();
            ServiceLocator.GetService<MusicManager>().PlayMusic(_music, 0.1f, 0.1f);
            _camera.Follow = ServiceLocator.GetService<PlayerController>().transform;

            foreach (IQuestBoundariesController questController in _questsControllers)
            {
                _questsTracker.AddQuestController(questController);
            }

            _questsTracker.OnSceneEntry();
        }

        private void OnDestroy()
        {
            foreach (IQuestBoundariesController questController in _questsControllers)
            {
                _questsTracker.RemoveQuestController(questController);
            }
        }
    }
}