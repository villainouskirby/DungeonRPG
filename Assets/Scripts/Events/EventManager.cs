using Core;


namespace Events
{
    public class EventManager : Singleton<EventManager>, IManager
    {
        public PriorityEvent<MonsterKilledEventArgs> MonsterKilledEvent = new();
        public PriorityEvent<InventoryChangedEventArgs> InventoryChangedEvent = new();
        public PriorityEvent<InventoryBehaviorEventArgs> InventoryBehaviorEvent = new();
        public PriorityEvent<QuestAllocateEventArgs> QuestAllocateEvent = new();
        public PriorityEvent<QuestClearEventArgs> QuestClearEvent = new();
        public PriorityEvent<QuestCompleteEventArgs> QuestCompleteEvent = new();
        public PriorityEvent<QuestUnlockedEventArgs> QuestUnlockedEvent = new();
        public PriorityEvent<NPCConversationEventArgs> NPCConversationEvent = new();

        public void Initialize()
        {

        }

        protected override void AfterAwake()
        {
            base.AfterAwake();
        }
    }
}