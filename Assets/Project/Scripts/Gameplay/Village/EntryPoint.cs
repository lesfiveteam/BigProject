using BigProject.Managers;
using BigProject.Managers.SoundsMusicManagers;
using BigProject.NPC;
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
        [SerializeField]
        private NPCChatsDatabasesController _chatsDatabasesController;
        [SerializeField]
        private WatermillHandler _watermillHandler;

        private QuestsBoundariesTracker _questsTracker;

        public void Init()
        {
            _questsControllers.RemoveAll(x => x is not IQuestBoundariesController);
            _questsTracker = ServiceLocator.GetService<QuestsBoundariesTracker>();
            ServiceLocator.GetService<MusicManager>().PlayMusic(_music, 0.1f, 0.1f);
            PlayerController player = ServiceLocator.GetService<PlayerController>();
            _camera.Follow = player.transform;

            foreach (IQuestBoundariesController questController in _questsControllers)
            {
                _questsTracker.AddQuestController(questController);
            }

            _questsTracker.OnSceneEntry();

            //_chestSound.Init(ServiceLocator.GetService<SoundsManager>());
            ProgressManager progressManager = ServiceLocator.GetService<ProgressManager>();
            _chatsDatabasesController.Init(progressManager);
            _watermillHandler.Init(progressManager);
            SwithOnOutline(player);
        }

        private void SwithOnOutline(PlayerController player)
        {
            Outline outline = player.GetComponentInChildren<Outline>();
            if (outline != null)
                outline.enabled = true;
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