﻿using Devdog.General;
using Devdog.General.Localization;
using Devdog.General.ThirdParty.UniLinq;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Devdog.QuestSystemPro
{
    [System.Serializable]
    public partial class Quest : BetterScriptableObject
    {
        public delegate void StatusChanged(QuestStatus before, Quest quest);
        public delegate void TaskReachedTimeLimit(Task task, Quest quest);
        public delegate void TaskStatusChanged(TaskStatus before, Task task, Quest quest);
        public delegate void TaskProgressChanged(float taskProgressBefore, Task task, Quest quest);

        public event StatusChanged OnStatusChanged;
        public event TaskReachedTimeLimit OnTaskReachedTimeLimit;
        public event TaskProgressChanged OnTaskProgressChanged;
        public event TaskStatusChanged OnTaskStatusChanged;

        [InspectorReadOnly]
        public int ID;

        [Header("General info")]
        public new LocalizedString name = new LocalizedString();

        [TextArea]
        public LocalizedString description = new LocalizedString();
        public Sprite icon;

        [Header("Tasks")]
        public bool autoCompleteWhenTasksAreDone = false;
        public TaskOrder taskOrder = TaskOrder.Parallel;

        // Don't let Unity serialize, serialize using fullSerializer (handles polymorphic)
        [CustomSerialization]
        private Task[] _tasks = Array.Empty<Task>();
        public Task[] tasks
        {
            get { return _tasks; }
            set
            {
                UnRegisterEventsOnTasks();
                _tasks = value;
                RegisterEventsOnTasks();

                foreach (Task task in _tasks)
                {
                    task.owner = this;
                }

#if UNITY_EDITOR
                var prevKeys = new List<string>();
                foreach (Task task in tasks)
                {
                    if (prevKeys.Contains(task.key))
                    {
                        throw new InvalidOperationException("A quest can't contain tasks with the same key.");
                    }

                    prevKeys.Add(task.key);
                }
#endif
            }
        }

        [Header("Rewards")]
        public IRewardGiver[] onActivationRewardGivers = Array.Empty<IRewardGiver>();
        public IRewardGiver[] rewardGivers = Array.Empty<IRewardGiver>();

        //        [MinValue(1)]
        public int maxRepeatTimes = 1;

        [NonSerialized]
        private int _repeatedTimes;
        public int repeatedTimes
        {
            get { return _repeatedTimes; }
            protected set { _repeatedTimes = value; }
        }

        [NonSerialized]
        private QuestStatus _status = QuestStatus.InActive;
        public QuestStatus status
        {
            get { return _status; }
            protected set
            {
                QuestStatus before = _status;
                _status = value;

                if (before != _status)
                {
                    NotifyStatusChanged(before);
                }
            }
        }

        [Header("Conditions")]
        public IQuestCondition[] conditions = Array.Empty<IQuestCondition>();

        [Required]
        public IQuestTimeHandler timeHandler = new QuestTimeHandler();

        /// <summary>
        /// The local identifier this quest belongs to (set at run-time).
        /// </summary>
        [IgnoreCustomSerialization]
        public ILocalIdentifier localIdentifier { get; set; }

        protected Quest()
            : base()
        { }

        public static Quest Create(int id = 0)
        {
            return Create(QuestManager.instance.localIdentifier, id);
        }

        public static Quest Create(ILocalIdentifier localIdentifier, int id = 0)
        {
            Quest q = CreateInstance<Quest>();
            q.ID = id;
            q.localIdentifier = localIdentifier;

            return q;
        }

        public override void Load()
        {
            base.Load();

            RegisterEventsOnTasks();

            foreach (Task task in _tasks)
            {
                task.owner = this;
            }
        }

        #region Notifies

        public void NotifyReachedTimeLimit(Task task)
        {
            DoNotifyReachedTimeLimit(task);
        }

        protected virtual void DoNotifyReachedTimeLimit(Task task)
        {
            QuestManager.instance.NotifyQuestTaskReachedTimeLimit(task, this);
            OnTaskReachedTimeLimit?.Invoke(task, this);
        }

        public void NotifyTaskProgressChanged(float before, Task task)
        {
            DoNotifyTaskProgressChanged(before, task);
            if (autoCompleteWhenTasksAreDone && AreRequiredTasksCompletable())
            {
                CompleteAndGiveRewards();
            }
        }

        protected virtual void DoNotifyTaskProgressChanged(float before, Task task)
        {
            QuestManager.instance.NotifyQuestTaskProgressChanged(before, task, this);
            OnTaskProgressChanged?.Invoke(before, task, this);
        }

        public void NotifyTaskStatusChanged(TaskStatus before, TaskStatus after, Task task)
        {
            DoNotifyTaskStatusChanged(before, after, task);

            // Activate the next task (if one is found)
            if (after == TaskStatus.Completed)
            {
                Task t = tasks.FirstOrDefault(o => o.status == TaskStatus.InActive);
                if (t != null)
                {
                    t.Activate();
                }
            }
        }

        protected virtual void DoNotifyTaskStatusChanged(TaskStatus before, TaskStatus after, Task task)
        {
            QuestManager.instance.NotifyQuestTaskStatusChanged(before, after, task, this);
            OnTaskStatusChanged?.Invoke(before, task, this);
        }

        public void NotifyStatusChanged(QuestStatus before)
        {
            DoNotifyQuestStatusChanged(before);
        }

        protected virtual void DoNotifyQuestStatusChanged(QuestStatus before)
        {
            QuestManager.instance.NotifyQuestStatusChanged(before, this);
            OnStatusChanged?.Invoke(before, this);
        }

        #endregion

        #region Tasks

        private void UnRegisterEventsOnTasks()
        {
            foreach (Task task in tasks)
            {
                task.OnReachedTimeLimit -= NotifyReachedTimeLimit;
                task.OnProgressChanged -= NotifyTaskProgressChanged;
                task.OnStatusChanged -= NotifyTaskStatusChanged;
            }
        }

        private void RegisterEventsOnTasks()
        {
            foreach (Task task in tasks)
            {
                task.OnReachedTimeLimit += NotifyReachedTimeLimit;
                task.OnProgressChanged += NotifyTaskProgressChanged;
                task.OnStatusChanged += NotifyTaskStatusChanged;
            }
        }

        public virtual bool CanSetTaskProgress(string key, float value)
        {
            if (status != QuestStatus.Active)
            {
                DevdogLogger.LogVerbose("Trying to set progress on quest task but quest is not active. Setting progress ignored status is: " + status);
                return false;
            }

            if (taskOrder == TaskOrder.Single)
            {
                Task setProgressTask = GetTask(key);
                foreach (Task task in tasks)
                {
                    if (task == setProgressTask)
                    {
                        break;
                    }

                    if (task.isCompleted == false)
                    {
                        DevdogLogger.LogVerbose("Task is not completed: " + task.key);
                        return false;
                    }
                }
            }

            return true;
        }

        public virtual bool SetTaskProgress(string key, float value)
        {
            if (CanSetTaskProgress(key, value) == false)
            {
                return false;
            }

            Task task = GetTask(key);
            if (task != null)
            {
                return task.SetProgress(value);
            }

            return false;
        }

        public bool ChangeTaskProgress(string key, float value)
        {
            Task task = GetTask(key);
            if (task != null)
            {
                return SetTaskProgress(key, task.progress + value);
            }

            return false;
        }

        [Obsolete("Use AreRequiredTasksCompletable instead.")]
        public bool AreTasksProgressedSufficientlyToCompleteQuest()
        {
            return AreRequiredTasksCompletable();
        }

        public bool AreRequiredTasksCompletable()
        {
            foreach (Task task in tasks)
            {
                if (task.IsProgressSufficientToComplete() == false && task.requirement == TaskRequirement.Required)
                {
                    return false;
                }
            }

            return true;
        }

        public Task GetTask(string key)
        {
            return tasks.FirstOrDefault(task => task.key == key);

            //            DevdogLogger.LogVerbose("Couldn't find quest task with name " + key + " on quest with ID #" + ID);
        }

        public IEnumerable<Task> GetFailedTasks()
        {
            return tasks.Where(task => task.status == TaskStatus.Failed);
        }

        public IEnumerable<Task> GetInActiveTasks()
        {
            return tasks.Where(task => task.status == TaskStatus.InActive);
        }

        public IEnumerable<Task> GetActiveTasks()
        {
            return tasks.Where(task => task.status == TaskStatus.Active);
        }

        public IEnumerable<Task> GetActiveAndCompletedTasks()
        {
            return tasks.Where(task => task.status == TaskStatus.Active || task.status == TaskStatus.Completed);
        }

        public IEnumerable<Task> GetCompletedTasks()
        {
            return tasks.Where(task => task.status == TaskStatus.Completed);
        }

        public IEnumerable<Task> GetTasks(TaskFilter filter)
        {
            switch (filter)
            {
                case TaskFilter.InActive:
                    return GetInActiveTasks();
                case TaskFilter.Active:
                    return GetActiveTasks();
                case TaskFilter.ActiveAndCompleted:
                    return GetActiveAndCompletedTasks();
                case TaskFilter.Failed:
                    return GetFailedTasks();
                case TaskFilter.All:
                    return tasks.ToList();
                default:
                    throw new ArgumentOutOfRangeException("filter", filter, null);
            }
        }

        #endregion

        #region Quest conditions

        //        /// <summary>
        //        /// Can this quest be discovered? If true the quest will be showable to the player. If it's false quest givers won't show it.
        //        /// </summary>
        //        /// <returns></returns>
        //        public bool CanDiscover()
        //        {
        //            return true;
        //        }

        public ConditionInfo CanActivate()
        {
            if (!CanRepeat())
            {
                return new ConditionInfo(false, QuestManager.instance.languageDatabase.canNotAcceptQuestReachedMaxRepeatTimes);
            }

            foreach (IQuestCondition condition in conditions)
            {
                ConditionInfo s = condition.CanActivateQuest(this);
                if (s == false)
                {
                    return s;
                }
            }

            foreach (IRewardGiver rewardGiver in onActivationRewardGivers)
            {
                ConditionInfo s = rewardGiver.CanGiveRewards(this);
                if (s == false)
                {
                    return s;
                }
            }

            if (QuestManager.instance.GetQuestStates(localIdentifier).activeQuests.Count + 1 > QuestManager.instance.settingsDatabase.playerMaxActiveQuests)
            {
                return new ConditionInfo(false, QuestManager.instance.languageDatabase.canNotAcceptQuestMaxActiveQuests);
            }

            return ConditionInfo.success;
        }

        public bool CanRepeat()
        {
            return repeatedTimes < maxRepeatTimes;
        }

        public ConditionInfo CanComplete()
        {
            if (status != QuestStatus.Active)
            {
                return new ConditionInfo(false);
            }

            foreach (IQuestCondition condition in conditions)
            {
                ConditionInfo c = condition.CanCompleteQuest(this);
                if (c == false)
                {
                    return c;
                }
            }

            foreach (Task task in tasks)
            {
                ConditionInfo c = task.CanComplete();
                if (c == false)
                {
                    return c;
                }
            }

            ConditionInfo canGiveRewards = CanGiveRewards();
            if (canGiveRewards == false)
            {
                return canGiveRewards;
            }

            return ConditionInfo.success;
        }

        public ConditionInfo CanCancel()
        {
            if (status != QuestStatus.Active)
            {
                //                DevdogLogger.Log("Tried to cancel an inactive quest. Quest ID: " + ID, DevdogLogger.LogType.Minimal);
                return new ConditionInfo(false);
            }

            foreach (IQuestCondition condition in conditions)
            {
                ConditionInfo c = condition.CanCancelQuest(this);
                if (c == false)
                {
                    return c;
                }
            }

            return ConditionInfo.success;
        }

        public ConditionInfo CanDecline()
        {
            if (status != QuestStatus.InActive)
            {
                return new ConditionInfo(false);
            }

            foreach (IQuestCondition condition in conditions)
            {
                ConditionInfo c = condition.CanDeclineQuest(this);
                if (c == false)
                {
                    return c;
                }
            }

            return ConditionInfo.success;
        }

        #endregion

        #region Quest actions

        public bool Activate(bool resetTaskProgress = true)
        {
            if (CanActivate() == false)
            {
                return false;
            }

            if (resetTaskProgress)
            {
                ResetProgress();
            }

            status = QuestStatus.Active;
            GiveActivationRewards();
            ActivateTasks();

            return true;
        }

        private void ActivateTasks()
        {
            switch (taskOrder)
            {
                case TaskOrder.Parallel:
                    foreach (Task task in tasks)
                    {
                        task.Activate();
                    }

                    break;
                case TaskOrder.Single:
                    if (tasks.Length > 0)
                    {
                        tasks[0].Activate();
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void ResetProgress()
        {
            foreach (Task task in tasks)
            {
                task.ResetProgress();
            }

            // status = QuestStatus.InActive;
        }

        public bool CompleteAndGiveRewards(bool forceComplete = false)
        {
            if (CanComplete() == false && forceComplete == false)
            {
                return false;
            }

            // Even when forcing the quest CAN NOT be completed if the user can't get his/her rewards.
            if (CanGiveRewards() == false)
            {
                return false;
            }

            repeatedTimes++;

            CompleteCompletableTasks(forceComplete);
            GiveRewards();

            status = repeatedTimes < maxRepeatTimes ? QuestStatus.InActive : QuestStatus.Completed;
            DevdogLogger.Log("Completed quest/achievement with ID: " + ID + " and gave rewards. Repeated quest #" + repeatedTimes + " times. Quest status is: " + status);

            NotifyTasksQuestCompleted();

            return true;
        }

        private void NotifyTasksQuestCompleted()
        {
            foreach (Task task in tasks)
            {
                task.NotifyQuestCompleted();
            }
        }

        /// <summary>
        /// Quests cannot be completed if their rewards cannot be given.
        /// </summary>
        public ConditionInfo CanGiveRewards()
        {
            foreach (IRewardGiver rewardGiver in rewardGivers)
            {
                ConditionInfo s = rewardGiver.CanGiveRewards(this);
                if (s == false)
                {
                    return s;
                }
            }

            foreach (Task task in tasks)
            {
                ConditionInfo s = task.CanGiveRewards();
                if (s == false && task.gaveRewards == false)
                {
                    return s;
                }
            }

            return ConditionInfo.success;
        }

        private void GiveActivationRewards()
        {
            foreach (IRewardGiver rewardGiver in onActivationRewardGivers)
            {
                rewardGiver.GiveRewards(this);
            }
        }

        private void GiveRewards()
        {
            foreach (IRewardGiver rewardGiver in rewardGivers)
            {
                rewardGiver.GiveRewards(this);
            }

            foreach (Task task in tasks)
            {
                if (task.isCompleted)
                {
                    task.GiveRewards();
                }
            }
        }

        private void CompleteCompletableTasks(bool forceComplete)
        {
            foreach (Task task in tasks)
            {
                if ((task.isCompleted == false && task.IsProgressSufficientToComplete()) || forceComplete)
                {
                    task.Complete(forceComplete);
                    //                    task.GiveRewards(); // Rewards only given when entire quest is completed.
                }
            }
        }

        public virtual bool Cancel()
        {
            if (CanCancel() == false)
            {
                return false;
            }

            status = QuestStatus.Cancelled;
            CancelAllTasks();

            return true;
        }

        private void CancelAllTasks()
        {
            foreach (Task task in tasks)
            {
                task.Cancel();
            }
        }

        public virtual bool Decline()
        {
            if (CanDecline() == false)
            {
                return false;
            }

            status = QuestStatus.InActive;
            //            ResetProgress();

            return true;
        }

        public void DoAction(QuestStatusAction action, bool forceActions = false)
        {
            switch (action)
            {
                case QuestStatusAction.Activate:
                    Activate(forceActions);
                    break;
                case QuestStatusAction.Complete:
                    CompleteAndGiveRewards(forceActions);
                    break;
                case QuestStatusAction.Cancel:
                    Cancel();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("action", action, null);
            }
        }

        #endregion

        /// <summary>
        /// Force reset this quest to it's initial state.
        /// </summary>
        public virtual void ForceReset()
        {
            ResetProgress();
            repeatedTimes = 0;
            status = QuestStatus.InActive;
        }

        public override string ToString()
        {
            return name.message;
        }
    }
}