namespace GeekLearning.Authorizations.Model.Manager
{
    using System;

    public interface IPrincipal
    {
        Guid Id { get; set; }

        bool IsDeletable { get; set; }
    }
}
