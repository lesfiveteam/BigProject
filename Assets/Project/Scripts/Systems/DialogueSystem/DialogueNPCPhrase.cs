using UnityEngine;

namespace BigProject.Systems.DialogueSystem
{
    [CreateAssetMenu(fileName = "DialogueNPCPhrase", menuName = "Scriptable Objects/DialogueNPCPhrase")]
    public class DialogueNPCPhrase : ScriptableObject
    {
        // Идентификатор фразы. Если указан, то будет вызываться событие OnDialogueStep
        public int Id = 0; 
        public string Text;
        public Sprite CharacterSprite;
    }
}
