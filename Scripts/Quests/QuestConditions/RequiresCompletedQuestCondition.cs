using Devdog.General;

namespace Devdog.QuestSystemPro
{
    /// <summary>
    /// Requires a quest to be completed in order to activate, cancel, complete, decline or discover this quest.
    /// </summary>
    public class RequiresCompletedQuestCondition : BasicQuestCondition, IQuestCondition
    {
        public Asset<Quest> requiredQuest;

        public override ConditionInfo Check()
        {
            if (!QuestManager.instance.HasCompletedQuest(requiredQuest.val))
            {
                return new ConditionInfo(false, QuestManager.instance.languageDatabase.canNotAcceptQuestRequiresCompletedQuest);
            }

            return ConditionInfo.success;
        }
    }
}
