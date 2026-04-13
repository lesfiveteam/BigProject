using BigProject.Systems.Inventory;
using System;
using UnityEngine;

namespace BigProject.Gameplay.Watermill
{
    public interface IControlPanelState : IDisposable
    {
        /// <summary>
        /// Is ready to work.
        /// </summary>
        public bool IsReady => true;
        public void Start() { }
        public void Stop() { }
        public void OnClicked() { }
        public void OnSwiped(Vector2 delta) { }
        public void OnUnclicked() { }
        public void ApplyItem(Item item) { }
        public void OnActivated() { }
        public void Reset() { }
    }
}