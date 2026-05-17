using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace BigProject.Settings
{

    [CreateAssetMenu(fileName = "ScreenFreqConfig", menuName = "Scriptable Objects/Configs/ScreenFreqConfig")]
    public class ScreenFreqConfig : ScriptableObject, IEnumerable<ScreenFreqConfig.Mode>
    {
        [Serializable]
        public class Mode
        {
            [field: SerializeField]
            public string Name { get; private set; }

            [field: SerializeField]
            public int VSync { get; private set; }

            [field: SerializeField]
            public int Freq { get; private set; }
        }

        [SerializeField]
        private List<Mode> _modes;

        public Mode GetMode(string name) => _modes.FirstOrDefault(x => x.Name.Equals(name));

        public IEnumerator<Mode> GetEnumerator()
        {
            foreach (Mode mode in _modes)
            {
                yield return mode;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}