namespace Devdog.QuestSystemPro
{
    public abstract class BasicQuestCondition : IQuestCondition
    {
        public bool toActivate;
        public bool toCancel;
        public bool toComplete;
        public bool toDecline;
        public bool toDiscover;

        public abstract ConditionInfo Check();

        private ConditionInfo Test(bool flag)
        {
            return flag ? Check() : ConditionInfo.success;
        }

        public ConditionInfo CanActivateQuest(Quest quest)
        {
            return Test(toActivate);
        }

        public ConditionInfo CanCancelQuest(Quest quest)
        {
            return Test(toCancel);
        }

        public ConditionInfo CanCompleteQuest(Quest quest)
        {
            return Test(toComplete);
        }

        public ConditionInfo CanDeclineQuest(Quest quest)
        {
            return Test(toDecline);
        }

        public ConditionInfo CanDiscoverQuest(Quest quest)
        {
            return Test(toDiscover);
        }
    }
}
