using Devdog.General;

namespace Devdog.QuestSystemPro
{
    public class RequiresCompletedQuestCondition : IQuestCondition
    {
        public Asset<Quest> requiredQuest;

        public bool toActivate;
        public bool toCancel;
        public bool toComplete;
        public bool toDecline;
        public bool toDiscover;

        public ConditionInfo Check()
        {
            if (!QuestManager.instance.HasCompletedQuest(requiredQuest.val))
            {
                return new ConditionInfo(false, QuestManager.instance.languageDatabase.canNotAcceptQuestRequiresCompletedQuest);
            }

            return ConditionInfo.success;
        }

        public ConditionInfo CanActivateQuest(Quest quest)
        {
            return toActivate ? Check() : ConditionInfo.success;
        }

        public ConditionInfo CanCancelQuest(Quest quest)
        {
            return toCancel ? Check() : ConditionInfo.success;
        }

        public ConditionInfo CanCompleteQuest(Quest quest)
        {
            return toComplete ? Check() : ConditionInfo.success;
        }

        public ConditionInfo CanDeclineQuest(Quest quest)
        {
            return toDecline ? Check() : ConditionInfo.success;
        }

        public ConditionInfo CanDiscoverQuest(Quest quest)
        {
            return toDiscover ? Check() : ConditionInfo.success;
        }
    }
}
