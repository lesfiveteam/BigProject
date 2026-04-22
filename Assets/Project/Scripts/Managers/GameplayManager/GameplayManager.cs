using UnityEngine;
using BigProject.Systems;
using System.Collections.Generic;
using UnityEngine.Assertions;
using System;
using BigProject.Utilities;

namespace BigProject.Managers
{
    public enum GameplayState
    {
        Play,
        Dialogue,
        MiniGame,
        RunesJagsaw,
        Map,
        Inventory,
        Pause,
        Cutscene,
    }

    /// <summary>
    /// Set different game states and switch manual update queues.
    /// </summary>
    public class GameplayManager
    {
        private GameplayState _state;
        private readonly ManualLoop _manualLoop;
        private readonly Dictionary<GameplayState, List<int>> _tickQueueIds = new();
        private readonly List<int> _activeQueueIds = new();

        public event Action<GameplayState> StateChanged;
        public GameplayState State => _state;

        public GameplayManager(ManualLoop manualLoop)
        {
            _manualLoop = manualLoop;
            ExceptionUtilities.ThrowIfNull(_manualLoop, String.Format(LogStr.CRITICAL_NULL_REFERENCE, "Gameplay Manager", "manual loop"));
            _state = GameplayState.Play;
        }

        /// <summary>
        /// Add update queue and bind it to game state.
        /// </summary>
        /// <param name="state">Game state</param>
        /// <param name="id">Queue id</param>
        public void AddQueueToState(GameplayState state, int id)
        {
            if (_tickQueueIds.TryGetValue(state, out List<int> stateIds))
            {
                stateIds.Add(id);
            }
            else
            {
                _tickQueueIds.Add(state, new() { id });
            }

            if (_manualLoop.IsTickableQueueActive(id))
            {
                _activeQueueIds.Add(id);
            }
        }

        /// <summary>
        /// Change game state.
        /// </summary>
        /// <param name="state">New state</param>
        public void ChangeState(GameplayState state)
        {
            if (_state == state)
            {
                return;
            }

            // Turn off all active.
            foreach (int id in _activeQueueIds)
            {
                _manualLoop.SetTickableQueueActive(id, false);
            }

            _activeQueueIds.Clear();
            _state = state;
            StateChanged?.Invoke(_state);

            if (_tickQueueIds.TryGetValue(_state, out List<int> nextIds))
            {
                foreach (int id in nextIds)
                {
                    _manualLoop.SetTickableQueueActive(id, true);
                    _activeQueueIds.Add(id);
                }
            }
        }
    }
}