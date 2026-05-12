using BigProject.Managers;
using BigProject.Player;
using BigProject.Settings;
using BigProject.Systems.HUD;
using System;

namespace BigProject.Utilities
{
    public class GameplayStatesHandler : IDisposable
    {
        private HUDConfig _hudConfig;
        private PlayerInputHandler _input;
        private GameplayManager _gameplayManager;
        private HUD _hud;

        public GameplayStatesHandler(HUDConfig hudConfig, GameplayManager gameplayManager, PlayerInputHandler input, HUD hud)
        {
            _gameplayManager = gameplayManager;
            _hudConfig = hudConfig;
            _input = input;
            _hud =  hud;
            ExceptionUtilities.ThrowIfNull(_gameplayManager, "Gameplay states handler get null Gameplay Manager.");
            ExceptionUtilities.ThrowIfNull(_input, "Gameplay states handler get null Player Input Handler.");
            ExceptionUtilities.ThrowIfNull(_hudConfig, "Gameplay states handler get null HUD config.");
            ExceptionUtilities.ThrowIfNull(_hud, "Gameplay states handler get null HUD.");
            _gameplayManager.StateChanged += OnGameStateChanged;
        }

        public void Dispose()
        {
            _gameplayManager.StateChanged -= OnGameStateChanged;
        }

        private void OnGameStateChanged(GameplayState state)
        {
            switch (state)
            {
                case GameplayState.Play:
                    _hud.ShowWidget(_hudConfig.HUDJournalWidgetId, 0.1f);
                    _hud.ShowWidget(_hudConfig.HUDInventoryWidgetId, 0.1f);
                    _hud.ShowWidget(_hudConfig.HUDRunesWidgetId, 0.1f);
                    _hud.HideWidget(_hudConfig.HUDCancelWidgetId);
                    _hud.HideWidget(_hudConfig.HUDResetWidgetId);
                    _hud.HideWidget(_hudConfig.HUDRunesJigsawWidgetId);
                    _input.SwitchToPlayerActionMap();
                    break;
                case GameplayState.MiniGame:
                    _hud.HideWidget(_hudConfig.HUDJournalWidgetId);
                    _hud.HideWidget(_hudConfig.HUDRunesWidgetId);
                    _hud.ShowWidget(_hudConfig.HUDCancelWidgetId);
                    ReplicaManager.HideReplica();
                    _input.SwitchToMiniGameActionMap();
                    break;
                case GameplayState.RunesJagsaw:
                    _hud.HideWidget(_hudConfig.HUDJournalWidgetId);
                    _hud.HideWidget(_hudConfig.HUDRunesWidgetId);
                    _hud.ShowWidget(_hudConfig.HUDRunesJigsawWidgetId);
                    ReplicaManager.HideReplica();
                    _input.SwitchToMiniGameActionMap();
                    break;
                case GameplayState.Dialogue:
                case GameplayState.Cutscene:
                    _hud.HideWidget(_hudConfig.HUDJournalWidgetId);
                    _hud.HideWidget(_hudConfig.HUDRunesWidgetId);
                    _hud.HideWidget(_hudConfig.HUDInventoryWidgetId);
                    _hud.HideWidget(_hudConfig.HUDCancelWidgetId);
                    ReplicaManager.HideReplica();
                    _input.SwitchToMiniGameActionMap();
                    break;
                default:
                    break;
            }
        }
    }
}
