using UnityEngine;

namespace BigProject.Managers.CursorManager
{
    [CreateAssetMenu(fileName = "CursorsConfig", menuName = "Scriptable Objects/Configs/CursorsConfig")]
    public class CursorsConfig : ScriptableObject
    {
        [SerializeField] private CursorData[] _cursorDatas;

        public CursorData[] CursorDatas => _cursorDatas;
    }
}