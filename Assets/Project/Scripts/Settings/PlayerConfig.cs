using UnityEngine;

namespace BigProject.Settings
{
    /// <summary>
    /// Base build settings.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "Scriptable Objects/Configs/PlayerConfig")]
    public class PlayerConfig : ScriptableObject
    {
        [field: SerializeField]
        public float MinSpeachTime { get; private set; }
        [field: SerializeField]
        public int SpeachLengthForMinTime { get; private set; }
        [field: SerializeField]
        public float TimeCorrectionPerLetter { get; private set; }
    }
}