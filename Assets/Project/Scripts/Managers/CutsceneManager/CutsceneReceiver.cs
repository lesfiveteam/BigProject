using BigProject.Systems;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace BigProject.Managers.CutsceneManager
{
    public class CutsceneReceiver : MonoBehaviour, INotificationReceiver
    {
        [Serializable]
        private class MarkerLogic
        {
            public UnityEvent logic;
            public CutsceneEvent cutsceneEvent;
        }

        [SerializeField]
        private List<MarkerLogic> _markersLogic;

        public void OnNotify(Playable _, INotification notification, object context)
        {
            if (notification is CutsceneMarker marker)
            {

                MarkerLogic markerLogic = _markersLogic.Find(x => x.cutsceneEvent == marker.EventType);

                if (markerLogic != null)
                {
                    GameLogManager.Info(string.Format(LogStr.INFO_SYSTEM, $"CutsceneReceiver {gameObject.name}", $"receive message {markerLogic.cutsceneEvent}"));
                    markerLogic.logic?.Invoke();
                }
            }
        }
    }
}