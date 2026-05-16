using BigProject.Player;
using BigProject.Settings;
using BigProject.Systems.HUD;
using BigProject.UI.Map;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BigProject.Managers
{
    public class MapManager : IDisposable
    {
        private const float MAP_OPENING_AND_CLOSING_TIME = 0.4f;
        private PlayerInputHandler _playerInputHandler;
        private MapUI _mapUI;
        private GameplayManager _gameplayManager;
        private HUD _hud;
        private HUDConfig _hudConfig;
        private bool _isAvailable;

        public MapManager(MapUI mapUI, PlayerInputHandler playerInputHandler, GameplayManager gameplayManager, HUD hud, HUDConfig hudConfig)
        {
            _mapUI = mapUI;
            _playerInputHandler = playerInputHandler;
            _gameplayManager = gameplayManager;
            _hud = hud;
            _hudConfig = hudConfig;
            _isAvailable = true;
            _playerInputHandler.OpenMap += ToggleMap;
            _gameplayManager.StateChanged += OnGameStateChanged;
        }

        public void Init()
        {
            _mapUI.Init();
            _mapUI.gameObject.SetActive(false);
        }

        private void ToggleMap()
        {
            if (!_isAvailable)
            {
                return;
            }

            if (_gameplayManager.State == GameplayState.Map)
            {
                _mapUI.StartCoroutine(WaitAndCloseMap(true));
            }
            else if (_gameplayManager.State == GameplayState.Play)
            {
                _gameplayManager.ChangeState(GameplayState.Map);
            }
        }

        private void OpenMap()
        {
            _hud.HideWidget(_hudConfig.HUDInventoryWidgetId);
            _hud.HideWidget(_hudConfig.HUDJournalWidgetId);
            _hud.HideWidget(_hudConfig.HUDRunesWidgetId);
            _mapUI.gameObject.SetActive(true);
            _mapUI.OpenMap();
        }

        private IEnumerator WaitAndCloseMap(bool goToPlay)
        {
            _isAvailable = false;
            _mapUI.CloseMap();
            yield return new WaitForSeconds(MAP_OPENING_AND_CLOSING_TIME);
            _mapUI.gameObject.SetActive(false);
            _hud.ShowWidget(_hudConfig.HUDInventoryWidgetId);
            _hud.ShowWidget(_hudConfig.HUDJournalWidgetId);
            _hud.ShowWidget(_hudConfig.HUDRunesWidgetId);
            _isAvailable = true;

            if (goToPlay)
            {
                _gameplayManager.ChangeState(GameplayState.Play);
            }
        }

        private void OnGameStateChanged(GameplayState state)
        {
            if (_gameplayManager.State == GameplayState.Map)
            {
                OpenMap();
            }
            else if (_mapUI.gameObject.activeSelf && _isAvailable)
            {
                _mapUI.StartCoroutine(WaitAndCloseMap(false));
            }
        }

        public void Dispose()
        {
            _playerInputHandler.OpenMap -= ToggleMap;
            _gameplayManager.StateChanged -= OnGameStateChanged;
        }
    }
}

