using UnityEngine;

namespace BigProject.UI.Map
{
    public class MapUI : MonoBehaviour
    {
        private const string OPEN_MAP_TRIGGER = "OpenMapTrigger";
        private const string CLOSE_MAP_TRIGGER = "CloseMapTrigger";

        private Animator _animator;
        public void Init()
        {
            _animator = GetComponent<Animator>();
        }

        public void OpenMap()
        {
            _animator.SetTrigger(OPEN_MAP_TRIGGER);
        }

        public void CloseMap()
        {
            _animator.SetTrigger(CLOSE_MAP_TRIGGER);
        }
    }
}
