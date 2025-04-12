using Devdog.QuestSystemPro.UI;

namespace Devdog.QuestSystemPro
{
    public abstract class RewardGiverBase : IRewardGiver
    {
        public static RewardRowUI defaultRewardUIPrefab
        {
            get { return QuestManager.instance.settingsDatabase.defaultRewardRowUI; }
        }

        public virtual RewardRowUI rewardUIPrefab => defaultRewardUIPrefab;

        public virtual ConditionInfo CanGiveRewards(Quest quest)
        {
            return ConditionInfo.success;
        }

        public abstract void GiveRewards(Quest quest);
    }
}
