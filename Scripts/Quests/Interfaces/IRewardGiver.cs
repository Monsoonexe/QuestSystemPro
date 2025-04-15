using Devdog.QuestSystemPro.UI;

namespace Devdog.QuestSystemPro
{
    public interface IRewardGiver
    {
        RewardRowUI rewardUIPrefab { get; }

        /// <remarks>i.e. will it fit?</remarks>
        ConditionInfo CanGiveRewards(Quest quest);

        /// <param name="quest">The soure of the rewards.</param>
        void GiveRewards(Quest quest);
    }
}
