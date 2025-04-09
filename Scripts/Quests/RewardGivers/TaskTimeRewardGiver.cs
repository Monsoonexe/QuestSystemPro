using Devdog.General;
using System;

namespace Devdog.QuestSystemPro
{
    public class TaskTimeRewardGiver : RewardGiverBase, INamedRewardGiver
    {
        public float addTimeInSeconds;

        public override string name
        {
            get { return "Task '" + taskName + "' time"; }
        }

        public override void GiveRewards(Quest quest)
        {
            Task task = quest.GetTask(taskName);
            if (task == null)
            {
                DevdogLogger.LogWarning("Task " + taskName + " not found on quest " + quest);
                return;
            }

            task.timeLimitInSeconds += addTimeInSeconds;
            DevdogLogger.LogVerbose("Gave task " + taskName + " " + addTimeInSeconds + " extra seconds (rewardGiver)");
        }

        public override string ToString()
        {
            return TimeSpan.FromSeconds(addTimeInSeconds).ToString();
        }
    }
}
