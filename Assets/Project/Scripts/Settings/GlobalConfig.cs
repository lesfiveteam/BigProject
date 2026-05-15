using UnityEngine;

namespace BigProject.Settings
{
    /// <summary>
    /// Base build settings.
    /// </summary>
    [CreateAssetMenu(fileName = "GlobalConfig", menuName = "Scriptable Objects/Configs/GlobalConfig")]
    public class GlobalConfig : ScriptableObject
    {
        [field:SerializeField]
        public string PlayerProfileName { get; private set; }
        [field: SerializeField]
        public string QuestsFolder { get; private set; }
        [field: SerializeField]
        public string GameSettingsName { get; private set; }
    }
}