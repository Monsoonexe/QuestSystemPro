/* TODO - fix error messages
 * 
 */

using Devdog.General;

namespace Devdog.QuestSystemPro
{
    public class RequiresOnQuest : IQuestCondition
    {
        public Asset<Quest> requiredQuest;

        public bool toActivate;
        public bool toCancel;
        public bool toComplete;
        public bool toDecline;
        public bool toDiscover;

        public ConditionInfo Check()
        {
            if (!QuestManager.instance.HasActiveQuest(requiredQuest.val))
            {
                return new ConditionInfo(false, QuestManager.instance.languageDatabase.canNotAcceptQuestRequiresCompletedQuest); // also, [sic]
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
