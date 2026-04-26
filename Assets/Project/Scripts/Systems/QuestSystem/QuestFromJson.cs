using BigProject.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace BigProject.Systems.QuestSystem
{
    /// <summary>
    /// Quest that retrieves data from a Json string.
    /// </summary>
    internal class QuestFromJson : IQuest, ISavable
    {
        //  The names of the serialized variables are given taking into account that they are displayed in the same form in the json file, therefore there are no underscores, etc.
        
        [SerializeField]
        private int id;
        [SerializeField]
        private string name;
        [SerializeField]
        private QuestState currentState;
        [SerializeField]
        private bool isSavingAllowed = true;
        [SerializeField]
        private List<Action> actions;
        [SerializeField]
        private List<QuestCondition> questStates;

        private Dictionary<int, Action> _actionsDict;
        private Dictionary<int, QuestActionState> _lastChangedActions = new();
        private Dictionary<int, QuestActionHandler> _actionHandlers;

        /// <summary>
        /// An activity stores its state and the conditions for transitions to other states.
        /// </summary>
        [Serializable]
        private class Action
        {
            public int id;
            public string name = "action";
            public QuestActionType type = QuestActionType.FireproofResult;
            public QuestActionState currentState = QuestActionState.Inactive;
            public List<ActionCondition> conditions;
            public List<ManualActionTransition> manualTransitions;
        }

        /// <summary>
        /// Condition for transitioning an activity to a given state.
        /// Stores dependencies on other states; when their conditions are met, the transition occurs.
        /// </summary>
        [Serializable]
        private class ActionCondition
        {
            // Once the condition is met, it can be removed.
            // Useful for one-time conditions (e.g., after a button is activated, the activation conditions are no longer required)
            public bool isOneShot;

            // From which state are we transitioning.
            public QuestActionState fromState;

            // What state does the activity go to when the conditions are met.
            public QuestActionState toState;

            // Dependency includes the id of the influencing activity and its state at which its requirements are considered fulfilled.
            [Serializable]
            public class Dependency
            {
                public int id;
                public QuestActionState state;
            }

            // A wrapper class for a list of dependencies. The built-in JsonUtility cannot work with nested lists.
            [Serializable]
            public class DependencyPack
            {
                // The conditions are met when the conditions of all dependencies from the list are met (essentially the AND operator).
                public List<Dependency> dependencies;
            }

            // List of dependency sets. Needed to be able to link conditions with the OR operator.
            // Conditions are met when any of the condition sets are met.
            public List<DependencyPack> dependencyPacks;
        }

        /// <summary>
        /// Transitions that are allowed for manual control (by external code).
        /// </summary>
        [Serializable]
        private class ManualActionTransition
        {
            public int id;
            public QuestActionState fromState;
            public QuestActionState toState;
            public bool isOneShot;
        }

        /// <summary>
        /// Condition for transitioning the quest to a given state.
        /// The quest state is linked to a specific activity.
        /// Complex conditions can be set in the linked activity itself.
        /// </summary>
        [Serializable]
        private class QuestCondition
        {
            public QuestState state;

            // Влияющая активность.
            public int actionId;
            public QuestActionState actionState;
        }

        public int ID => id;
        public string Name => name;
        public QuestState CurrentState
        {
            get => currentState;
            private set => currentState = value;
        }

        public bool IsSavingAllowed => isSavingAllowed;
        public event Action<IQuest> Progressed;
        public event Action<IQuest> StateChanged;

        // Field of ISavable.
        public string Key => $"Quest_{Name}";
        // Save all data.
        public object SavingData => this;


        /// <param name="jsonData">Quest data in json format</param>
        public QuestFromJson(string jsonData)
        {
            JsonUtility.FromJsonOverwrite(jsonData, this);
            _actionHandlers = new();
            Init();
        }

        // Check IQuest
        public bool ManualTransition(int actionId, QuestActionState newState, bool forced = false)
        {
            // Can transit only in active quest.
            if (CurrentState != QuestState.Active && !forced)
            {
                Debug.LogWarning(String.Format(LogStr.WARNING_SYSTEM, "Quest", $"[{Name}] in state [{CurrentState}], but you try to access it."));
                return false;
            }
            
            if (!_actionsDict.TryGetValue(actionId, out Action targetAction))
            {
                Debug.LogError(String.Format(LogStr.ERROR_SYSTEM, "Quest", $"action [{actionId}] not found in quest [{name}]."));
                return false;
            }

            if (targetAction.currentState == newState)
            {
                Debug.LogWarning(String.Format(LogStr.WARNING_SYSTEM, "Quest", $"action [{actionId}] in quest [{name}] already in state [{newState}]"));
                return false;
            }

            if (newState == QuestActionState.Undefined)
            {
                Debug.LogWarning(String.Format(LogStr.WARNING_SYSTEM, "Quest", $"action [{actionId}] in quest [{name}] can't be in undefined state. Transition will be ignored"));
                return false;
            }

            if (forced)
            {
                Debug.LogWarning(String.Format(LogStr.WARNING_SYSTEM, "Quest", $"[{name}], make forced transition of Action [{actionId}], new state [{newState}]"));
                MakeTransition(targetAction, new() { toState = newState });
                return true;
            }

            foreach (ManualActionTransition transition in targetAction.manualTransitions)
            {
                // If a manual transition is allowed.
                // A possible transition from Undefined is taken into account (in this case, any current state of the activity is considered suitable).
                if (IsEqualStates(transition.fromState, targetAction.currentState) && transition.toState ==  newState)
                {
                    MakeTransition(targetAction, transition);
                    return true;
                }
            }

            Debug.LogError(String.Format(LogStr.ERROR_SYSTEM, "Quest", $"[{Name}] has no manual transitions to Action [{actionId}] state [{newState}]"));
            return false;
        }

        // Check IQuest
        public bool TryGetActionState(int id, out QuestActionState state)
        {
            if (_actionsDict.TryGetValue(id, out Action action))
            {
                state = currentState == QuestState.Inactive ? QuestActionState.Inactive : action.currentState;
                return true;
            }

            state = QuestActionState.Undefined;
            return false;
        }

        // Check IQuest
        public IReadOnlyDictionary<int, QuestActionState> GetLastChangedActions() => _lastChangedActions;

        // Check IQuest
        public IReadOnlyDictionary<int, QuestActionState> GetAllActions() => _actionsDict.ToDictionary(x => x.Key, x => x.Value.currentState);

        // Check IQuest
        public bool TryGetActionHandler(int actionId, out IQuestActionHandler actionHandler)
        {
            if (_actionHandlers.ContainsKey(actionId))
            {
                actionHandler = _actionHandlers[actionId];
                return true;
            }

            if (!_actionsDict.ContainsKey(actionId))
            {
                Debug.LogError(String.Format(LogStr.ERROR_SYSTEM, "Quest", $"[{Name}] unable to set action [{actionId}] handler, id not found"));
                actionHandler = null;
                return false;
            }

            Action targetAction = _actionsDict[actionId];
            // Get all manual transitions.
            var transitions = targetAction.manualTransitions.ToDictionary(x => x.id, x => (x.fromState, x.toState));
            actionHandler = new QuestActionHandler(this, actionId, targetAction.name, targetAction.currentState, transitions);
            _actionHandlers.Add(actionId, actionHandler as QuestActionHandler);
            return true;
        }

        // ISavable
        public void OnLoad()
        {
            Init();
            ProgressNotify();
        }

        private void Init()
        {
            ActionsToDictionary();
            InitialActionsCheck();

            // Available quest states are sorted in descending order in case of conflicts
            // (if the current quest state satisfies several states at once).
            questStates.Sort((a, b) => b.state.CompareTo(a.state));

            // Update all states (possibly there are conditions that are met immediately).
            ResetActions();
            questStates.Sort((x, y) => y.state.CompareTo(x.state));
            ResetQuestState();

            // After loading, all activities represent the latest changes.
            _lastChangedActions = _actionsDict.ToDictionary(x => x.Key, x => x.Value.currentState);
        }

        /// <summary>
        /// Performs an activity transition according to the transaction.
        /// </summary>
        private void MakeTransition(Action action, ManualActionTransition transition)
        {
            _lastChangedActions.Clear(); // Сброс последних изменений перед новыми.
            action.currentState = transition.toState;

            // Manual transitions can be one-time.
            if (transition.isOneShot)
            {
                action.manualTransitions.Remove(transition);

                if (_actionHandlers.ContainsKey(action.id))
                {
                    _actionHandlers[action.id].RemoveTransition(transition.id);
                }
            }

            CommitActionChange(action);
            ResetActions();
            ResetQuestState();
            ProgressNotify();
        }

        // All progress notifications.
        private void ProgressNotify()
        {
            SendToActionHandlers();
            Progressed?.Invoke(this);
        }

        // Sends notifications about changed status.
        private void SendToActionHandlers()
        {
            foreach (KeyValuePair<int, QuestActionHandler> actionHandler in _actionHandlers)
            {
                // Notify only changed activities.
                if (_lastChangedActions.TryGetValue(actionHandler.Key, out QuestActionState newState))
                {
                    actionHandler.Value.OnStateChanged(newState);
                }
            }
        }


        // Make dictionary from list to optimize work.
        private void ActionsToDictionary()
        {
            _actionsDict = actions.ToDictionary(x => x.id, x => x);
            //actions.Clear(); - можно стереть список, если не надо будет сохранять все активности в квесте.
        }

        // Check initial actions states.
        private void InitialActionsCheck()
        {
            foreach (Action action in _actionsDict.Values)
            {
                if (action.currentState == QuestActionState.Undefined)
                {
                    Debug.LogError(String.Format(LogStr.ERROR_SYSTEM, "Quest", $"[{Name}] has action [{action.name}] in undefined state"));
                    return;
                }
                else if (action.type == QuestActionType.FireproofResult)
                {
                    TryCleanUpForRelease(action);
                }
            }
        }

        /// <summary>
        /// Rechecks activities and changes their states when conditions are met.
        /// </summary>
        private void ResetActions()
        {
            // For cross-dependencies, we'll iterate through the states,
            // until we're sure all states have reached their final values.
            bool actionsChanged = false;

            do
            {
                foreach (Action action in _actionsDict.Values)
                {
                    QuestActionState startState = action.currentState;
                    ResetAction(action);
                    actionsChanged = (startState != action.currentState);

                    if (actionsChanged)
                    {
                        CommitActionChange(action);
                        break;
                    }
                }
            } while (actionsChanged);
        }

        /// <summary>
        /// Records activity in the change list.
        /// </summary>
        private void CommitActionChange(Action action)
        {
            if (_lastChangedActions.ContainsKey(action.id))
            {
                _lastChangedActions[action.id] = action.currentState;
            }
            else
            {
                _lastChangedActions.Add(action.id, action.currentState);
            }

            GameLogManager.Info(String.Format(LogStr.INFO_SYSTEM, "Quest", $"action \"{action.name}\" change state to {action.currentState}"));
        }

        /// <summary>
        /// Checks the activity state and changes it when conditions are met.
        /// </summary>
        private void ResetAction(Action action)
        {
            // For conflicts, when the activity satisfies several states, we take the largest one.
            QuestActionState maxMetState = QuestActionState.Inactive;

            // List of conditions met with the isOneShot flag (see ActionCondition)
            List<ActionCondition> conditionsToRemove = new();

            foreach (ActionCondition condition in action.conditions)
            {
                // If the transition conditions do not satisfy the current state or the transition is not relevant from the point of view of the largest feasible state.
                if (!IsEqualStates(condition.fromState, action.currentState) || condition.toState <= maxMetState)
                {
                    continue;
                }

                if (IsConditionMet(condition))
                {
                    // When the conditions are met, we move to a new state only if the current one is lower.
                    if (action.currentState < condition.toState)
                    {
                        action.currentState = condition.toState;

                        if (action.currentState == QuestActionState.Released)
                        {
                            action.conditions.Clear();
                            action.manualTransitions.Clear();
                            return;
                        }
                        else if (TryCleanUpForRelease(action))
                        {
                            return;
                        }
                        else if (condition.isOneShot)
                        {
                            conditionsToRemove.Add(condition);
                        }
                    }

                    maxMetState = action.currentState;
                }
                // If we jumped over an invalid state, we fall back to the nearest feasible one.
                // This scenario works for Undefined->Specific transitions. If the transition is not met, it will automatically roll back to a valid state.
                else if (action.currentState >= condition.toState)
                {
                    action.currentState = maxMetState;
                }
            }

            // Remove the conditions met by OneShot.
            action.conditions.RemoveAll(x => conditionsToRemove.Contains(x));
        }

        private bool TryCleanUpForRelease(Action action)
        {
            if (action.currentState < QuestActionState.Completed || action.type == QuestActionType.MaxMet)
                return false;

            ActionCondition targetCondition = null;

            foreach (ActionCondition condition in action.conditions)
            {
                if (IsEqualStates(condition.fromState, action.currentState) && condition.toState == QuestActionState.Released)
                {
                    targetCondition = condition;
                    break;
                }
            }

            action.conditions.Clear();

            if (targetCondition != null)
            {
                action.conditions.Add(targetCondition);
            }

            ManualActionTransition targetTransition = null;

            foreach (ManualActionTransition transition in action.manualTransitions)
            {
                if (IsEqualStates(transition.fromState, action.currentState) && transition.toState == QuestActionState.Released)
                {
                    targetTransition = transition;
                    break;
                }
            }

            action.manualTransitions.Clear();

            if (targetTransition != null)
            {
                action.manualTransitions.Add(targetTransition);
            }

            return true;
        }

        private bool IsConditionMet(ActionCondition condition)
        {
            // We go through all the sets of dependencies - there is actually an OR connection between them
            foreach (ActionCondition.DependencyPack dependencyPack in condition.dependencyPacks)
            {
                // If at least one of the sets is fulfilled, the conditions are met.
                if (IsDependenciesSatisfied(dependencyPack.dependencies))
                {
                    return true;
                }
            }

            return false;
        }

        /// <returns>True if the conditions of all dependencies are met.</returns>
        private bool IsDependenciesSatisfied(List<ActionCondition.Dependency> dependencies)
        {
            foreach (ActionCondition.Dependency dependency in dependencies)
            {
                // Find the influencing action.
                if (!_actionsDict.TryGetValue(dependency.id, out Action influenceAction))
                {
                    Debug.LogWarning(String.Format(LogStr.WARNING_SYSTEM, "Quest", $"[{name}] unable to find influence Action with id [{dependency.id}]. Skip condition"));
                    continue;
                }

                // If it is in the wrong state - all condition is not met. Further can not be checked.
                // Given the Undefined state in the condition - then the activity can be in any state, which is equivalent to its not being in the condition.
                if (!IsEqualStates(influenceAction.currentState, dependency.state))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks the global state of the quest and changes it when conditions are met.
        /// </summary>
        private void ResetQuestState()
        {
            foreach (QuestCondition questState in questStates)
            {
                if (CurrentState >= questState.state)
                {
                    continue;
                }

                if (!_actionsDict.TryGetValue(questState.actionId, out Action influenceAction))
                {
                    Debug.LogWarning(String.Format(LogStr.WARNING_SYSTEM, "Quest", $"[{name}] unable to find Action [{questState.actionId}] while changing quest global state"));
                    continue;
                }

                if (influenceAction.currentState == questState.actionState)
                {
                    CurrentState = questState.state;
                    Debug.Log(String.Format(LogStr.INFO_SYSTEM, "Quest", $"change global state to [{questState.state}]"));
                    StateChanged?.Invoke(this);

                    // States are sorted in descending order, when executing the largest further conditions cannot be checked.
                    break;
                }
            }
        }

        /// <summary>
        /// Compares the states of activensotes considering uncertainty.
        /// </summary>
        private bool IsEqualStates(QuestActionState state1, QuestActionState state2) =>
            state1 == state2 || state1 == QuestActionState.Undefined || state2 == QuestActionState.Undefined;
    }
}