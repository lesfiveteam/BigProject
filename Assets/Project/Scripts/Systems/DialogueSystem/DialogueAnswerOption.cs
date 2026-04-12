using UnityEngine;

namespace BigProject.Systems.DialogueSystem
{
    [CreateAssetMenu(fileName = "DialogueAnswerOption", menuName = "Scriptable Objects/DialogueAnswerOption")]
    public class DialogueAnswerOption : ScriptableObject
    {
        // Ñonsider the hash string and the name Dialogue Line to be unique values
        public string HashId { 
            get 
            { 
                return this.Text.GetHashCode() 
                        + ":" 
                        + (this.DialogueLine ? this.DialogueLine.name.GetHashCode() : "-")
                        + ":"
                        + this.name; 
            } 
        }
        public string Text;
        public DialogueLine DialogueLine;
        public bool IsChosenByDefault = false;
    }
}
