namespace GeekLearning.Authorizations.EntityFrameworkCore.Data
{
    using GeekLearning.Authorizations.Model.Manager;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Group : Audit, IGroup
    {
        public Group()
        {
        }

        public Group(Principal principal)
        {
            this.Id = principal.Id;
            this.CreationBy = principal.CreationBy;
            this.ModificationBy = principal.ModificationBy;

            this.Principal = principal;
        }

        [Key]
        public Guid Id { get; set; }

        public bool IsDeletable { get; set; } = true;

        [Required]
        [StringLength(Constants.ColumnNameLength)]
        public string Name { get; set; }
       
        public Principal Principal { get; set; }
    }
}
