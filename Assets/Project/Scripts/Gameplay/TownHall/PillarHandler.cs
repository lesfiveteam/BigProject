using BigProject.Intercatable;
using BigProject.Systems;
using BigProject.Systems.QuestSystem;
using UnityEngine;
using UnityEngine.Assertions;

namespace BigProject.Gameplay.TownHall
{
    public class PillarHandler : MonoBehaviour, IInteractable
    {
        [SerializeField]
        private QuestActionHandlerMono _action;

        private void Awake()
        {
            Assert.IsNotNull(_action, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Quest Action Handler"));
        }

        public void Interact()
        {
            if (_action.CurrentState == QuestActionState.Active)
            {
                _action.MakeTransition(0);
            }
        }
    }
}