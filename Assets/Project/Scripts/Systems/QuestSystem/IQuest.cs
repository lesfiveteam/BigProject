using System;
using System.Collections.Generic;

namespace BigProject.Systems.QuestSystem
{
    /// <summary>
    /// Task status (there will probably be two of them - active/completed).
    /// </summary>
    public enum QuestState
    {
        Inactive,
        Active,
        Completed,
        Failed
    }

    /// <summary>
    /// The state of any activity in a quest, such as picking up a gear, talking to an NPC, etc.
    /// Arranged in order of the activity's life cycle, which is taken into account when resolving state conflicts
    /// (for example, when, according to quest logic, an object can be either Active or Completed).
    /// </summary>
    public enum QuestActionState
    {
        Undefined, // для указания любого состояния в условиях квеста
        Inactive,
        Active,
        Completed,
        Failed,
        Released
    }

    public enum QuestActionType
    {
        /// <summary>
        /// Upon reaching the Completed/Failed state, the only possible transition is to Released.
        /// </summary>
        FireproofResult,

        /// <summary>
        /// It is possible to switch from Completed/Failed back to Active/Inactive.
        /// </summary>
        MaxMet
    }

    /// <summary>
    /// quest is a set of activities (activity ID + state) and the conditions that link them.
    /// External code executes valid transitions (that do not disrupt the quest logic), which automatically changes the states of the activities linked by the conditions.
    /// </summary>
    public interface IQuest
    {
        public int ID { get; }
        public string Name { get; }
        public QuestState CurrentState { get; }
        public bool IsSavingAllowed { get; }

        /// <summary>
        /// To track the progress of any activity in the quest.
        /// </summary>
        public event Action<IQuest> Progressed;

        /// <summary>
        /// To track the change in the state of the entire quest(completed, failed, etc.)
        /// </summary>
        public event Action<IQuest> StateChanged;

        /// <summary>
        /// Performs a manual transition of the activity to a new state.
        /// </summary>
        /// <param name="newState">New state</param>
        /// <param name="forced">When true,transition will ignore quest logic. Not recomended.</param>
        /// <returns>True when success.</returns>
        public bool ManualTransition(int actionId, QuestActionState newState, bool forced = false);

        /// <returns>True when success.</returns>
        public bool TryGetActionState(int id, out QuestActionState state);

        /// <summary>
        /// Returns the last changed actions.
        /// Can be used after calling MakeTransition to track quest progress.
        /// </summary>
        public IReadOnlyDictionary<int, QuestActionState> GetLastChangedActions();

        public IReadOnlyDictionary<int, QuestActionState> GetAllActions();

        /// <returns>True when success.</returns>
        public bool TryGetActionHandler(int actionId, out IQuestActionHandler actionHandler);
    }
}