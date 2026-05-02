using Assets.Project.Scripts.Managers.SceneLoader;
using BigProject.Managers.CursorManager;
using BigProject.Player;
using BigProject.Systems;
using BigProject.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace BigProject.Intercatable.HighlightedObjects
{
    public class InteractableObjectsHighlighter : MonoBehaviour
    {
        [SerializeField]
        private float _objectCheckDelay = 0.1f;

        private Camera _camera;
        private SceneLoadManager _sceneLoader;
        private WaitForSeconds _objectCheckWait;
        private HighlightedObject _currentObject;       
        private CursorManager _cursorManager;
        private PlayerInputHandler _inputHandler;

        public void Init(SceneLoadManager sceneLoader, CursorManager cursorManager, PlayerInputHandler inputHandler)
        {
            _sceneLoader = sceneLoader;
            _cursorManager = cursorManager;
            _inputHandler = inputHandler;
            ExceptionUtilities.ThrowIfNull(_sceneLoader, string.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "SceneLoadManager"));
            ExceptionUtilities.ThrowIfNull(_cursorManager, string.Format(LogStr.CRITICAL_NULL_REFERENCE, "InteractableObjectsHighlighter", "CursorManager"));
            ExceptionUtilities.ThrowIfNull(_inputHandler, string.Format(LogStr.CRITICAL_NULL_REFERENCE, "InteractableObjectsHighlighter", "InputHandler"));
        }

        private void Awake()
        {
            _objectCheckWait = new(_objectCheckDelay);
        }

        //Coroutine checks ui highlighted objects and if none are found, checks for 3d objects on scene
        private IEnumerator ObjectCheckRoutine()
        {
            while (true)
            {
                bool uiHit = false;
                PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
                {
                    position = Mouse.current.position.ReadValue()
                };
                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerEventData, results);
                HighlightedObject newUIObject = null;

                foreach (RaycastResult result in results)
                {
                    newUIObject = result.gameObject.GetComponent<HighlightedObject>();

                    if (newUIObject != null)
                    {
                        break;
                    }
                }

                if (newUIObject != null)
                {
                    uiHit = true;

                    if (newUIObject != _currentObject)
                    {
                        SetNewObject(newUIObject);
                    }
                }

                if (!uiHit)
                {
                    Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        HighlightedObject newObject = hit.transform.GetComponent<HighlightedObject>();

                        if (newObject != null)
                        {
                            if (newObject != _currentObject)
                            {
                                SetNewObject(newObject);
                            }
                        }
                        else if (_currentObject != null)
                        {
                            _currentObject.Unhighlight();
                            _currentObject = null;
                        }
                    }
                    else if (_currentObject != null)
                    {
                        _currentObject.Unhighlight();
                        _currentObject = null;
                    }
                }

                yield return _objectCheckWait;
            }
        }

        public void RestartChecking()
        {
            _camera = Camera.main;
            StopAllCoroutines();
            _cursorManager.ResetToDefault();
            StartCoroutine(ObjectCheckRoutine());
        }

        private void SetNewObject(HighlightedObject highlightedObject)
        {
            if (_currentObject != null)
            {
                _currentObject.Unhighlight();
                _currentObject.OnDeactivate -= OnObjectDeactivate;
            }

            highlightedObject.Highlight();
            _currentObject = highlightedObject;
            _currentObject.OnDeactivate += OnObjectDeactivate;
        }

        private void OnObjectDeactivate(HighlightedObject highlightedObject)
        {
            if(highlightedObject != _currentObject)
            {
                return;
            }

            _currentObject.Unhighlight();
            _currentObject = null;
        }

        private void OnClick()
        {
            if (_currentObject != null)
            {
                _currentObject.PressHighlight();
            }
        }

        private void OnClickRelease()
        {
            if (_currentObject != null)
            {
                _currentObject.Unhighlight();
                _currentObject.Highlight();
            }
        }

        private void OnEnable()
        {
            _sceneLoader.SceneLoadingStarted += StopAllCoroutines;
            _sceneLoader.SceneLoadingCompleted += RestartChecking;
            _inputHandler.Click += OnClick;
            _inputHandler.ClickRelease += OnClickRelease;
        }

        private void OnDisable()
        {
            _sceneLoader.SceneLoadingStarted -= StopAllCoroutines;
            _sceneLoader.SceneLoadingCompleted -= RestartChecking;
            _inputHandler.Click -= OnClick;
            _inputHandler.ClickRelease -= OnClickRelease;
        }
    }
}