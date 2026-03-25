using System.Collections.Generic;
using UnityEngine;

namespace BigProject.NPC
{
    [CreateAssetMenu(fileName = "NPCChatsDatabase", menuName = "Scriptable Objects/Configs/NPC/ChatsDatabase")]
    public class NPCChatsDatabase : ScriptableObject
    {
        [SerializeField]
        private List<NPCChatConfig> _chats;

        public NPCChatConfig GetRandomChat() => _chats.Count > 0 ? _chats[Random.Range(0, _chats.Count)] : null;
    }
}