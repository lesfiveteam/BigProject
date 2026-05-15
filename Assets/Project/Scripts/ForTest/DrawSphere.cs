using UnityEngine;

namespace Assets.Project.Scripts.ForTest
{
    public class DrawSphere : MonoBehaviour
    {
        [SerializeField, Range(0, 200)] private float _radius = 1f;
        [SerializeField] private Color _color = Color.yellow;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = _color;
            Gizmos.DrawWireSphere(transform.position, _radius);
        }
    }
}