using Assets.Project.Scripts.Managers.SceneLoader;
using BigProject.Managers;
using BigProject.Systems.Inventory;
using BigProject.Utilities;
using UnityEngine;

namespace BigProject.Gameplay.Church
{
    public class QuestActions : MonoBehaviour
    {
        private InventorySystem _inventory;

        [SerializeField]
        private string _noteOne, _noteThree, _noteFour;

        public void Init(InventorySystem inventory)
        {
            _inventory = inventory;
            ExceptionUtilities.ThrowIfNull(_inventory, string.Format(gameObject.name, "Inventory System"));
        }

        public void AddNoteOne()
        {
            _inventory.AddItemByName(_noteOne);
        }

        public void AddNoteFour()
        {
            _inventory.RemoveItemByName(_noteThree);
            _inventory.AddItemByName(_noteFour);
        }

        // For test build
        public void FinishGame()
        {
            Invoke("StartOutro", 2f);
        }

        private void StartOutro()
        {
            ServiceLocator.GetService<SceneLoadManager>().LoadScene(Scenes.Outro);
        }    
    }
}
