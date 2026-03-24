using UnityEngine;

namespace BigProject.NPC
{
    public class NPCChatPanel : MonoBehaviour
    {
        private void OnEnable()
        {
            transform.LookAt(transform.position + Camera.main.transform.forward);
        }
    }
}