using BigProject.Systems;
using BigProject.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BigProject.NPC
{
    [CreateAssetMenu(fileName = "NPCChatConfig", menuName = "Scriptable Objects/Configs/NPC/ChatConfig")]
    public class NPCChatConfig : ScriptableObject, IEnumerable<NPCChatConfig.Entry>
    {
        [field: SerializeField]
        public string LocalizationTableName { get; private set; }

        [SerializeField]
        private List<Entry> _entries;
        [SerializeField]
        private float _minSpeachTime = 1f;
        [SerializeField]
        private int _speechLengthForMinTime = 100;
        [SerializeField]
        private float _timeCorrectionPerLetter = 0.01f;

        [Serializable]
        public class Entry
        {
            [field: SerializeField, Range(0, 1)]
            public int SpeakerID { get; private set; }
            [field: SerializeField]
            public string TableEntryKey { get; private set; }
        }

        public float GetSpeachTime(string text)
        {
            ExceptionUtilities.ThrowIfNull(text, string.Format(LogStr.CRITICAL_NULL_REFERENCE, "NPCChatConfig", "speach text"));
            return _minSpeachTime + _timeCorrectionPerLetter * Mathf.Max(0, text.Length - _speechLengthForMinTime);
        }

        public IEnumerator<Entry> GetEnumerator()
        {
            foreach (Entry entry in _entries)
            {
                yield return entry;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}