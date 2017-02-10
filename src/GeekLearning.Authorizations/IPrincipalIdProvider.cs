namespace GeekLearning.Authorizations
{
    using System;

    public interface IPrincipalIdProvider
    {
        Guid PrincipalId { get; }
    }
}
