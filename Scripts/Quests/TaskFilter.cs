namespace Devdog.QuestSystemPro
{
    // [System.Flags] // TODO - make flags
    public enum TaskFilter
    {
        InActive = 1,
        Active = 2,
        ActiveAndCompleted = 4,
        Failed = 8,
        All = -1
    }
}
