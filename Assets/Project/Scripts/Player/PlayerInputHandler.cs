using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BigProject.Player
{
    public class PlayerInputHandler : IDisposable
    {
        private InputSystemActions _inputActions;
        
        //Player Actions
        public event Action Click;
        public event Action OpenMap;
        public event Action PressPause;
        public event Action Cancel;
        public event Action Reset;

        //Mini-game Actions
        public event Action MiniGameClick;
        public event Action MiniGameRightClick;
        public event Action<Vector2> MiniGameSwipe;
        public event Action MiniGameUnclick;

        public PlayerInputHandler()
        {
            _inputActions = new InputSystemActions();
            _inputActions.Enable();
            _inputActions.Player.Enable();
            _inputActions.UI.Enable();
            _inputActions.MiniGame.Disable();

            _inputActions.UI.Cancel.performed += OnCancel;
            _inputActions.UI.Reset.performed += OnReset;

            _inputActions.Player.Click.performed += OnClick;
            _inputActions.Player.OpenMap.performed += OnOpenedMap;
            _inputActions.Player.OpenMenu.performed += OnPressedPause;

            _inputActions.MiniGame.Click.performed += OnMiniGameClick;
            _inputActions.MiniGame.RightClick.performed += OnMiniGameRightClick;
            _inputActions.MiniGame.Swipe.performed += OnMiniGameSwipe;
            _inputActions.MiniGame.Click.canceled += OnMiniGameUnclick;
        }

        private void OnPressedPause(InputAction.CallbackContext obj)
        {
            PressPause?.Invoke();
        }

        private void OnClick(InputAction.CallbackContext obj)
        {
            Click?.Invoke();
        }

        private void OnOpenedMap(InputAction.CallbackContext obj)
        { 
            OpenMap?.Invoke();
        }
        private void OnMiniGameClick(InputAction.CallbackContext obj)
        {
            MiniGameClick?.Invoke();
        }

        private void OnMiniGameRightClick(InputAction.CallbackContext obj)
        {
            MiniGameRightClick?.Invoke();
        }

        private void OnMiniGameSwipe(InputAction.CallbackContext obj)
        {
            MiniGameSwipe?.Invoke(obj.ReadValue<Vector2>());
        }

        private void OnMiniGameUnclick(InputAction.CallbackContext _)
        {
            MiniGameUnclick?.Invoke();
        }

        private void OnCancel(InputAction.CallbackContext _)
        {
            Cancel?.Invoke();
        }

        private void OnReset(InputAction.CallbackContext _)
        {
            Reset?.Invoke();
        }

        public void SwitchToPlayerActionMap()
        {
            _inputActions.Player.Enable();
            _inputActions.MiniGame.Disable();
        }

        public void SwitchToMiniGameActionMap()
        {
            _inputActions.MiniGame.Enable();
            _inputActions.Player.Disable();
        }

        public Vector2 GetMousePosition()
        {
            return _inputActions.UI.Point.ReadValue<Vector2>();
        }

        public void Dispose()
        {
            _inputActions.Disable();

            _inputActions.UI.Cancel.performed -= OnCancel;
            _inputActions.UI.Reset.performed -= OnReset;

            _inputActions.Player.Click.performed -= OnClick;
            _inputActions.Player.OpenMap.performed -= OnOpenedMap;
            _inputActions.Player.OpenMenu.performed -= OnPressedPause;

            _inputActions.MiniGame.Click.performed -= OnMiniGameClick;
            _inputActions.MiniGame.RightClick.performed -= OnMiniGameRightClick;
            _inputActions.MiniGame.Swipe.performed -= OnMiniGameSwipe;
            _inputActions.MiniGame.Click.canceled -= OnMiniGameUnclick;
        }
    }
}

