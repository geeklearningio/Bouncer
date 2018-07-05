namespace Bouncer.EntityFrameworkCore.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class ModelModificationDate
    {
        [Key]
        public byte Id { get; set; }

        [Required]
        public DateTime Rights { get; set; }

        [Required]
        public DateTime Roles { get; set; }

        [Required]
        public DateTime Scopes { get; set; }
    }
}
