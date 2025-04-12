namespace Devdog.QuestSystemPro
{
    /// <remarks>Example.</remarks>
    public interface INamedRewardGiver : IRewardGiver
    {
        /// <summary>
        /// User-facing name.
        /// </summary>
        string name { get; }
    }
}