namespace GeekLearning.Authorizations.EntityFrameworkCore.Data
{
    using GeekLearning.Authorizations.Model.Manager;
    using System.ComponentModel.DataAnnotations;

    public class Group : Principal, IGroup
    {
        [Required]
        [StringLength(Constants.ColumnNameLength)]
        public string Name { get; set; }
    }
}
