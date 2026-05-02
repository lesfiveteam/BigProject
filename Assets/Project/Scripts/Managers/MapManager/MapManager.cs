using BigProject.Player;
using BigProject.Settings;
using BigProject.Systems.HUD;
using BigProject.UI.Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BigProject.Managers
{
    public class MapManager
    {
        private const float MAP_OPENING_AND_CLOSING_TIME = 0.4f;
        private PlayerInputHandler _playerInputHandler;
        private GameplayState _previousState;
        private bool _isOpenMap;
        private MapUI _mapUI;
        private GameplayManager _gameplayManager;
        private HUD _hud;
        private HUDConfig _hudConfig;

        public MapManager(MapUI mapUI, PlayerInputHandler playerInputHandler, GameplayManager gameplayManager, HUD hud, HUDConfig hudConfig)
        {
            _mapUI = mapUI;
            _playerInputHandler = playerInputHandler;
            _gameplayManager = gameplayManager;
            _hud = hud;
            _hudConfig = hudConfig;
            _playerInputHandler.OpenMap += ToggleMap;
        }

        public void Init()
        {
            _mapUI.Init();
            _mapUI.gameObject.SetActive(false);
            _isOpenMap = false;
        }

        private void ToggleMap()
        {
            if (!_isOpenMap)
            {
                if (_gameplayManager.State == GameplayState.Play)
                {
                    // Can open only in Play State
                    OpenMap();
                }
            }
            else
            {
                _mapUI.StartCoroutine(WaitAndCloseMap());
            }
        }

        private List<int> GetWidgetIds()
        {
            return new List<int> { 
                _hudConfig.HUDInventoryWidgetId, 
                _hudConfig.HUDJournalWidgetId, 
                _hudConfig.HUDRunesWidgetId 
            };
        }

        private void OpenMap()
        {
            _gameplayManager.ChangeState(GameplayState.Map);
            List<int> ids = new List<int>();
            ids.Add(_hudConfig.HUDInventoryWidgetId);
            _hud.HideWidgets(GetWidgetIds());
            _mapUI.gameObject.SetActive(true);
            _mapUI.OpenMap();
            _isOpenMap = true;
        }

        private IEnumerator WaitAndCloseMap()
        {
            _mapUI.CloseMap();
            yield return new WaitForSeconds(MAP_OPENING_AND_CLOSING_TIME);
            _gameplayManager.ChangeState(GameplayState.Play);
            _mapUI.gameObject.SetActive(false);
            _isOpenMap = false;
            _hud.ShowWidgets(GetWidgetIds());
        }
    }
}

