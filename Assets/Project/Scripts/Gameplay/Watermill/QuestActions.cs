using BigProject.Managers;
using BigProject.Systems.Inventory;
using UnityEngine;
using UnityEngine.Localization;

namespace BigProject.Gameplay.Watermill
{
    public class QuestActions : MonoBehaviour
    {
        [Header("Player remarks")]
        [SerializeField]
        private LocalizedString _firstEnterRemark;
        [SerializeField]
        private LocalizedString _panelIncompletedRemark;
        [SerializeField]
        private LocalizedString _panelCompletedRemark;

        private void Awake()
        {
            ServiceLocator.GetService<InventorySystem>().AddItemByName("repaired_lever");
            //ServiceLocator.GetService<RunesSystem>().AddRune(0);
            //ServiceLocator.GetService<RunesSystem>().AddRune(4);
        }
        public void FirstWatermillMeeting()
        {
            ReplicaManager.ShowReplica(_firstEnterRemark);
        }

        public void PanelIncompletedClue()
        {
            ReplicaManager.ShowReplica(_panelIncompletedRemark);
        }

        public void PanelCompletedClue()
        {
            ReplicaManager.ShowReplica(_panelCompletedRemark);
        }
    }
}
