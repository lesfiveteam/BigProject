using BigProject.Managers;
using BigProject.Utilities;
using System;
using UnityEngine;
using UnityEngine.Localization;

namespace BigProject.Intercatable
{
    public class Signpost : MonoBehaviour, IInteractable
    {
        [SerializeField]
        private LocalizedString _locationName;

        private void Awake()
        {
            ExceptionUtilities.ThrowIfNull(_locationName, String.Format(gameObject.name, "LocalizedString"));
        }

        public void Interact()
        {
            ReplicaManager.ShowReplica(_locationName);
        }
    }
}