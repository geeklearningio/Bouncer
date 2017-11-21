namespace GeekLearning.Authorizations.Model.Manager
{
    using System;

    public interface IGroup
    {
        Guid Id { get; set; }

        bool IsDeletable { get; set; }

        string Name { get; set; }
    }
}
