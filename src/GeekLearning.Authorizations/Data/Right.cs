namespace GeekLearning.Authorizations.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class Right : Audit
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(Constants.ColumnNameLength)]
        public string Name { get; set; }

        public bool IsDeletable { get; set; }
    }
}
