using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace BigProject.Managers.CutsceneManager
{
    public enum CutsceneEvent
    {
        Disable,
        Enable,
        Action
    }

    public class CutsceneMarker : Marker, INotification
    {
        public PropertyName id => new("CutsceneMarker");

        [field: SerializeField]
        public CutsceneEvent EventType {  get; private set; }
    }
}