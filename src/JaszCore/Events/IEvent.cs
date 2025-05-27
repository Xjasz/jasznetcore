namespace JaszCore.Events
{
    public interface IEvent
    {
        public enum EVENT_STATE
        {
            EVENT_STARTED,
            EVENT_STOPPED,
            EVENT_RUNNING,
            EVENT_WAITING
        }

        public void StartEvent(string[] args = null);
    }
}
