namespace GeekLearning.Authorizations.EntityFrameworkCore.Data
{
    using GeekLearning.Authorizations.Model.Manager;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Group : Audit, IGroup
    {
        [Key]
        public Guid Id { get; set; }

        public bool IsDeletable { get; set; } = true;

        [Required]
        [StringLength(Constants.ColumnNameLength)]
        public string Name { get; set; }
    }
}
