using UnityEngine;
using BigProject.Systems;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace BigProject.Managers
{
    public enum GameplayState
    {
        Play,
        Dialogue,
        Map,
        Inventory,
        Pause
    }

    /// <summary>
    /// Переводит игру в различные геймплейные состояния, переключая очереди обновления.
    /// </summary>
    public class GameplayManager
    {
        private GameplayState _state;
        private readonly ManualLoop _manualLoop;
        private readonly Dictionary<GameplayState, List<int>> _tickQueueIds = new();
        private readonly List<int> _activeQueueIds = new();

        public GameplayManager(ManualLoop manualLoop)
        {
            Assert.IsNotNull(manualLoop, "Gameplay Manager: manual loop is null.");
            _state = GameplayState.Play;
            _manualLoop = manualLoop;
        }

        /// <summary>
        /// Добавляет очередь обновления с привязкой к состоянию игры.
        /// </summary>
        /// <param name="state">Состояние игры</param>
        /// <param name="id">Идентификатор очереди</param>
        public void AddQueueToState(GameplayState state, int id)
        {
            if (_tickQueueIds.TryGetValue(state, out var stateIds))
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
        /// Меняет состояние игры.
        /// </summary>
        /// <param name="state">Новое состояние</param>
        public void ChangeState(GameplayState state)
        {
            if (_state == state)
            {
                return;
            }

            if (!_tickQueueIds.TryGetValue(state, out var nextIds))
            {
                Debug.LogWarning($"Gameplay Manager: can't find tickable queue for state {_state}");
                return;
            }

            _state = state;

            foreach (int id in _activeQueueIds)
            {
                _manualLoop.SetTickableQueueActive(id, false);
            }

            _activeQueueIds.Clear();

            foreach (int id in nextIds)
            {
                _manualLoop.SetTickableQueueActive(id, true);
                _activeQueueIds.Add(id);
            }
        }
    }
}