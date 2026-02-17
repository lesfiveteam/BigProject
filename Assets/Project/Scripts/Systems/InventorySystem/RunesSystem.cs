using System;
using UnityEngine;

namespace BigProject.Systems
{
    public class RunesSystem
    {
        private int _numberOfRunes;
        public event Action<int> OnRuneAdded;

        public void AddRune()
        {
            if (_numberOfRunes >= 3)
            {
                Debug.LogError("Rune bar is already full, new rune wasn't added");
                return;
            }

            OnRuneAdded?.Invoke(_numberOfRunes);
            _numberOfRunes++;
        }

        public int GetNumberOfRunes()
        {
            return _numberOfRunes;
        }
    }
}