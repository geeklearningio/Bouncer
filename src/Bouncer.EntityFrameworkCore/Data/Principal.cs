namespace Bouncer.EntityFrameworkCore.Data
{
    using Bouncer.Model.Manager;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Principal : Audit, IPrincipal
    {
        [Key]
        public Guid Id { get; set; }

        public bool IsDeletable { get; set; } = true;
        
        public Group Group { get; set; }
    }
}
