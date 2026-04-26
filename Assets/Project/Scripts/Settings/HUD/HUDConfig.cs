using UnityEngine;

namespace BigProject.Settings
{
    /// <summary>
    /// HUD settings.
    /// </summary>
    [CreateAssetMenu(fileName = "HUDConfig", menuName = "Scriptable Objects/Configs/HUDConfig")]
    public class HUDConfig : ScriptableObject
    {
        [field: SerializeField]
        public int HUDInventoryWidgetId { get; private set; }
        [field: SerializeField]
        public int HUDJournalWidgetId { get; private set; }
        [field: SerializeField]
        public int HUDRunesWidgetId { get; private set; }
        [field: SerializeField]
        public int HUDRunesJigsawWidgetId { get; private set; }
        [field: SerializeField]
        public int HUDCancelWidgetId { get; private set; }
        [field: SerializeField]
        public int HUDResetWidgetId { get; private set; }
    }
}