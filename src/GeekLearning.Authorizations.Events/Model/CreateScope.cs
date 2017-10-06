namespace GeekLearning.Authorizations.Events.Model
{
    public class CreateScope : EventBase
    {
        public CreateScope()
        {
        }

        public CreateScope(string scopeName) : base($"{scopeName}")
        {
        }
    }
}
