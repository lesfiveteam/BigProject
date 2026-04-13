using UnityEngine;

namespace BigProject.Intercatable 
{
    public interface IInteractable
    {
        public bool NeedComeUp => true;
        public bool NeedLookAt => false;
        public void Interact();

        public Vector3 TargetPosition
        {
            get
            {
                if (this is MonoBehaviour obj)
                {
                    return obj.transform.position;
                }

                return default;
            }
        }

        public Vector3 TargetLookAt => TargetPosition;

        public float MaxDistance => 2f;
    }
}
