using UnityEngine;

namespace BigProject.Systems
{
    public interface IUsesItem
    {
        public bool DoesUseItem(Item item)
        {
            Debug.Log("Нужно переопределить метод DoesUseItem!");
            return false;
        }

        public void UseItem(Item item)
        {
            Debug.Log("Нужно переопределить метод UseItem!");   
        }
    }
}