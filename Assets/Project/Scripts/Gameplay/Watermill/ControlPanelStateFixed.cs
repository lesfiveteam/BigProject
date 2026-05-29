using BigProject.Managers;
using BigProject.Managers.SoundsMusicManagers;
using BigProject.Player;
using BigProject.Systems;
using BigProject.Systems.HUD;
using BigProject.Systems.Inventory;
using BigProject.Systems.QuestSystem;
using BigProject.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace BigProject.Gameplay.Watermill
{
    public class ControlPanelStateFixed : IControlPanelState
    {
        private ControlPanel _controlPanel;
        private PlayerInputHandler _input;
        private IQuestActionHandler _activateMechAction;
        private CancellationTokenSource _ctSource;
        private float _leverMoveTime;
        private float _leverStaggerTime;
        private float _staggerDistance;
        private int _noteItemId;
        private bool _isMoving;
        private GearsHandler _gearsHandler;
        private InventorySystem _inventory;
        private Lever _chosenLever;
        private List<Lever> _levers;
        private List<LeverPoint> _leversPoints;
        private List<Transform> _pointsTransforms;
        private Vector2 _delta = Vector2.zero;
        private HUD _hud;
        private int _resetWidgetId;
        private Dictionary<Lever, (Vector3, int)> _leversInitPositions = new();
        private const float MIN_SWIPE_DELTA = 80f;
        private const float MAX_SWIPE_ANGLE_DELTA = 30f;
        private SoundsManager _soundsManager;
        private AudioClip _leverMoveSound;
        private float _leverMoveVolume = 0.1f;

        // Key points of lever route.
        private class LeverPoint
        {
            public Transform transform;
            public int id;
            public LeverPoint next, prev;
            // Routes to move on.
            public Vector2 nextRail, prevRail;
            public bool isFree;
        }

        public ControlPanelStateFixed(ControlPanel controlPanel, PlayerInputHandler input, List<Transform> pointsTransforms,
            List<Lever> levers, float leverMoveTime, float leverStaggerTime, float staggerDistance, int noteItemId, 
            GearsHandler gearsHandler, IQuestActionHandler activateMechAction, InventorySystem inventory, HUD hud,
            int resetWidgetId, bool isActivated, SoundsManager soundsManager, AudioClip leverMoveSound)
        {
            _controlPanel = controlPanel;
            _input = input;
            _activateMechAction = activateMechAction;
            _activateMechAction.StateChanged += OnStateChanged;
            _levers = levers;
            _leverMoveTime = leverMoveTime;
            _leverStaggerTime = leverStaggerTime;
            _staggerDistance = staggerDistance;
            _noteItemId = noteItemId;
            _isMoving = false;
            _chosenLever = null;
            _gearsHandler = gearsHandler;
            _pointsTransforms = pointsTransforms;
            SetLeversPoints(_pointsTransforms);
            _inventory = inventory;
            _hud = hud;
            _resetWidgetId = resetWidgetId;
            _leverMoveSound = leverMoveSound;
            _soundsManager = soundsManager;

            foreach (Lever lever in _levers)
            {
                _leversInitPositions.TryAdd(lever, (lever.Transform.position, lever.PointId));
            }

            if (isActivated)
            {
                OnActivated();
            }
        }

        public bool IsReady => _activateMechAction.CurrentState == QuestActionState.Active;

        public void Start()
        {
            OnStateChanged();
        }
        public void Stop()
        {
            _chosenLever = null;
        }

        public void OnClicked()
        {
            if (GameplayUtilities.TryGetClickedObject(_input.GetMousePosition(), out GameObject go))
            {
                Lever lever = _levers.FirstOrDefault(x => x.Transform == go.transform);

                if (lever != null)
                {
                    _chosenLever = lever;
                }
            }
        }

        public void OnSwiped(Vector2 delta)
        {
            if (_isMoving || _chosenLever == null)
            {
                return;
            }

            _delta += delta;

            // Move only if has enough swipe delta.
            if (_delta.magnitude < MIN_SWIPE_DELTA)
            {
                return;
            }

            _delta = Vector2.zero;
            LeverPoint currentPoint = _leversPoints[_chosenLever.PointId];

            (float, LeverPoint)[] routes =
            {
                (Vector2.Angle(delta, currentPoint.nextRail), currentPoint.next),
                (Vector2.Angle(delta, currentPoint.prevRail), currentPoint.prev)
            };

            int i = routes[0].Item1 < routes[1].Item1 ? 0 : 1;

            if (routes[i].Item1 < MAX_SWIPE_ANGLE_DELTA)
            {
                _ctSource?.Dispose();
                _ctSource = new();
                _ = MoveLever(_chosenLever, currentPoint, routes[i].Item2, _ctSource.Token);
            }
        }

        public void OnUnclicked()
        {
            _chosenLever = null;
            _delta = Vector2.zero;
        }

        public void Dispose()
        {
            if (_gearsHandler != null)
            {
                _gearsHandler.enabled = true;
            }

            _activateMechAction.StateChanged -= OnStateChanged;
            _ctSource?.Cancel();
            _ctSource?.Dispose();
        }

        public void OnActivated()
        {
            _hud.ShowWidget(_resetWidgetId);
        }

        public void Reset()
        {
            _ctSource?.Cancel();
            _ctSource?.Dispose();
            _ctSource = null;

            foreach (Lever lever in _levers)
            {
                (lever.Transform.position, lever.PointId) = _leversInitPositions.GetValueOrDefault(lever);
            }

            SetLeversPoints(_pointsTransforms);
        }

        private void SetLeversPoints(List<Transform> pointsTransforms)
        {
            int pointsNumber = pointsTransforms.Count;
            int i = 0;
            _leversPoints = pointsTransforms.ConvertAll(x => new LeverPoint() { transform = x, id = i++ });

            for (i = 0; i < pointsNumber; i++)
            {
                LeverPoint point = _leversPoints[i];

                // Levers points is dll.
                point.next = _leversPoints[(i + 1) % pointsNumber];
                point.prev = _leversPoints[(i - 1 + pointsNumber) % pointsNumber];

                // Calculate routes for nodes.
                point.nextRail = point.next.transform.localPosition - point.transform.localPosition;
                point.prevRail = point.prev.transform.localPosition - point.transform.localPosition;

                point.isFree = true;
            }

            // Take initial points.
            foreach (Lever lever in _levers)
            {
                _leversPoints.ElementAt(lever.PointId).isFree = false;
            }    
        }

        private bool IsLeversInTargetPosition()
        {
            foreach (Lever lever in _levers)
            {
                if (!lever.InTargetPoint())
                {
                    return false;
                }
            }

            return true;
        }

        private async Awaitable MoveLever(Lever lever, LeverPoint currentPoint, LeverPoint target, CancellationToken ct)
        {
            _soundsManager.PlaySound(_leverMoveSound, is2D:true, volume: _leverMoveVolume);
            _isMoving = true;
            Vector3 startPosition = lever.Transform.localPosition;
            Vector3 newLeverPosition = startPosition;
            newLeverPosition.x = target.transform.localPosition.x;
            newLeverPosition.y = target.transform.localPosition.y;

            if (target.isFree)
            {
                currentPoint.isFree = true;
                lever.PointId = target.id;
                target.isFree = false;

                try
                {
                    await _controlPanel.MoveLever(lever.Transform, newLeverPosition, _leverMoveTime, ct);
                }
                finally
                {
                    _chosenLever = null;

                    if (IsLeversInTargetPosition())
                    {
                        MakeTransition();
                    }

                    _isMoving = false;
                }
            }
            else
            {
                Vector3 endPosition = startPosition + (newLeverPosition - startPosition).normalized * _staggerDistance;

                try
                {
                    await _controlPanel.MoveLever(lever.Transform, endPosition, _leverStaggerTime + 0.1f, ct);
                    await _controlPanel.MoveLever(lever.Transform, startPosition, _leverStaggerTime + 0.1f, ct);
                }
                finally
                {
                    lever.Transform.localPosition = startPosition;
                    _isMoving = false;
                }
            }
        }

        private void MakeTransition()
        {
            try
            {
                _activateMechAction.MakeTransition(0);
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format(LogStr.ERROR_QUEST, $"unable to make action transition from fixed control panel. {ex.Message}"));
                _controlPanel.Deactivate();
            }
        }

        private void OnStateChanged()
        {
            if (_activateMechAction.CurrentState >= QuestActionState.Completed)
            {
                _hud.HideWidget(_resetWidgetId);
                _controlPanel.ChangeState(ControlPanelState.Completed);
                _controlPanel.PlayFixedMusic();
                _controlPanel.DeactivateOnExit();
                _controlPanel.Deactivate();
            }
        }
    }
}