using UnityEngine;
using UnityEngine.UI;

namespace Devdog.QuestSystemPro.UI
{
    /// <summary>
    /// Example
    /// </summary>
    public class DefaultRewardRowUI : RewardRowUI
    {
        [Header("UI References")]
        public Text key;
        public Text val;

        public override void Repaint(IRewardGiver rewardGiver, Quest quest)
        {
            string named = rewardGiver is INamedRewardGiver nReward ? nReward.name : rewardGiver.ToString();

            key.text = named;
            val.text = named.ToString();
        }
    }
}
