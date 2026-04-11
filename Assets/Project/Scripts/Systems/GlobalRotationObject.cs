using UnityEngine;

namespace BigProject.Systems
{
    /// <summary>
    /// Is required on objects that need to be rotated regardless of their parent
    /// </summary>
    public class GlobalRotationObject : MonoBehaviour, ILateTickable
    {
        [SerializeField] private Vector3 _desiredGlobalRotation;

        public void LateTick()
        {
            transform.rotation = Quaternion.Euler(_desiredGlobalRotation);
        }
    }
}