using Core;


namespace Events
{
    public class EventManager : Singleton<EventManager>, IManager
    {
        

        public void Initialize()
        {

        }

        protected override void AfterAwake()
        {
            base.AfterAwake();
        }
    }
}