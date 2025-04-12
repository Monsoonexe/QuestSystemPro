/* TODO - fix error messages
 * 
 */

using Devdog.General;

namespace Devdog.QuestSystemPro
{
    /// <summary>
    /// Requires a quest to be active in order to activate, cancel, complete, decline or discover this quest.
    /// </summary>
    public class RequiresOnQuest : BasicQuestCondition, IQuestCondition
    {
        public Asset<Quest> requiredQuest;

        public override ConditionInfo Check()
        {
            if (!QuestManager.instance.HasActiveQuest(requiredQuest.val))
            {
                return new ConditionInfo(false, QuestManager.instance.languageDatabase.canNotAcceptQuestRequiresCompletedQuest); // also, [sic]
            }

            return ConditionInfo.success;
        }
    }
}
