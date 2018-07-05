namespace Bouncer.Model.Manager
{
    using System;

    public interface IPrincipal
    {
        Guid Id { get; set; }

        bool IsDeletable { get; set; }
    }
}
