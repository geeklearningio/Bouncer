namespace GeekLearning.Authorizations.Model.Manager
{
    public interface IGroup : IPrincipal
    {
        string Name { get; set; }
    }
}
