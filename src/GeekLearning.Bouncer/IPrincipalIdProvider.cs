namespace GeekLearning.Bouncer
{
    using System;

    public interface IPrincipalIdProvider
    {
        Guid PrincipalId { get; }
    }
}
