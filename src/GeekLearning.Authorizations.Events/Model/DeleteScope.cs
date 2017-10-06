namespace GeekLearning.Authorizations.Events.Model
{
    public class DeleteScope : EventBase
    {
        public DeleteScope()
        {
        }

        public DeleteScope(string scopeName) : base($"{scopeName}")
        {
        }
    }
}
