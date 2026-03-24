using BigProject.Managers;
using BigProject.Systems.QuestSystem;
using UnityEngine;
using UnityEngine.Events;
using BigProject.Systems;
using BigProject.NPC;
using BigProject.Gameplay.Common;
using BigProject.Player;
using BigProject.Intercatable.HighlightedObjects;
using BigProject.Managers.CursorManager;

namespace BigProject.Initializers
{
    /// <summary>
    /// Scene dependencies.
    /// </summary>
    public class GameplaySceneEntryPoint : MonoBehaviour
    {
        [SerializeField, Tooltip("Actions to execute for early initialize.")]
        private UnityEvent _initActions;

        private void Awake()
        {
#if UNITY_EDITOR
            if (Bootstrapper.Stage != GameExecutionStage.Gameplay)
            {
                Bootstrapper.SetStage(GameExecutionStage.Gameplay);
            }
#endif
            GameLogManager.Info(LogStr.INFO_INITIALIZING_SCENE_SERVICES);
            ProgressManager pm = ServiceLocator.GetService<ProgressManager>();
            InitQuestHandlers(pm);
            InitInteractable(pm);
            InitDoors();
            InitDialogueNPCs();
            InitCursorChangingEffects();
            InitNPCControllers(pm);
            GameLogManager.Info(LogStr.INFO_INITIALIZING_SCENE_SERVICES_COMPLETED);
            _initActions?.Invoke();
        }

        private void InitQuestHandlers(ProgressManager progressManager)
        {
            QuestActionHandlerMono[] actionsHandlers = FindObjectsByType<QuestActionHandlerMono>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (QuestActionHandlerMono actionHandler in actionsHandlers)
            {
                actionHandler.Init(progressManager);
            }

            QuestActionHandlersContainer[] actionHandlersContainers = FindObjectsByType<QuestActionHandlersContainer>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (QuestActionHandlersContainer container in actionHandlersContainers)
            {
                container.Init(progressManager);
            }

            QuestTriggerHandler[] triggersHandlers = FindObjectsByType<QuestTriggerHandler>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (QuestTriggerHandler triggerHandler in triggersHandlers)
            {
                triggerHandler.Init(progressManager);
            }
        }

        private void InitInteractable(ProgressManager progressManager)
        {
            QuestInteractableHandler[] interactableHandlers = FindObjectsByType<QuestInteractableHandler>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (QuestInteractableHandler interactableHandler in interactableHandlers)
            {
                interactableHandler.Init(progressManager);
            }
        }

        private void InitDoors()
        {
            MovingNextSceneHandler[] movingHandlers = FindObjectsByType<MovingNextSceneHandler>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            SceneLoadManager sceneLoader = ServiceLocator.GetService<SceneLoadManager>();
            PlayerSpawner playerSpawner = ServiceLocator.GetService<PlayerSpawner>();

            foreach (MovingNextSceneHandler movingHandler in movingHandlers)
            {
                movingHandler.Init(sceneLoader, playerSpawner);
            }
        }

        private void InitDialogueNPCs()
        {
            DialogNPC[] dialogueNPCs = FindObjectsByType<DialogNPC>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            DialogueManager dialogueManager = ServiceLocator.GetService<DialogueManager>();

            foreach (DialogNPC dialogueNPC in dialogueNPCs)
            {
                dialogueNPC.Init(dialogueManager);
            }
        }

        private void InitCursorChangingEffects()
        {
            CursorChangingEffect[] cursorChangingEffects = FindObjectsByType<CursorChangingEffect>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            CursorManager cursorManager = ServiceLocator.GetService<CursorManager>();

            foreach (CursorChangingEffect cursorChangingEffect in cursorChangingEffects)
            {
                cursorChangingEffect.Init(cursorManager);
            }
        }

        private void InitNPCControllers(ProgressManager progressManager)
        {
            NPCController[] controllers = FindObjectsByType<NPCController>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (NPCController controller in controllers)
            {
                controller.Init(progressManager);
            }
        }
    }
}