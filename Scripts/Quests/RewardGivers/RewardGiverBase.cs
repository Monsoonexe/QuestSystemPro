using Devdog.QuestSystemPro.UI;

namespace Devdog.QuestSystemPro
{
    public abstract class RewardGiverBase : INamedRewardGiver
    {
        public string taskName;

        public virtual RewardRowUI rewardUIPrefab
        {
            get { return QuestManager.instance.settingsDatabase.defaultRewardRowUI; }
        }

        public abstract string name { get; }

        public virtual ConditionInfo CanGiveRewards(Quest quest)
        {
            return ConditionInfo.success;
        }

        public abstract void GiveRewards(Quest quest);

        public override string ToString() => name;
    }
}
