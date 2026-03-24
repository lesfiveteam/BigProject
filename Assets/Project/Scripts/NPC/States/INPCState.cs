using System;
using UnityEngine;

namespace BigProject.NPC.States
{
    public enum NPCState
    {
        Idle,
        Patrolling,
        Wandering, // Random patrolling
        Chase, // To chat
        Wait, // Wait for chat
        Chat
    }

    public interface INPCState : IDisposable
    {
        public void Start() { }
        public void Stop() { }
        public void OnTriggerEnter(Collider other) { }

        public NPCState State { get; }
    }
}