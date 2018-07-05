namespace GeekLearning.Bouncer.EntityFrameworkCore.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public abstract class Audit
    {
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;

        [Required]
        public Guid? CreationBy { get; set; }

        public DateTime ModificationDate { get; set; } = DateTime.UtcNow;

        [Required]
        public Guid? ModificationBy { get; set; }
    }
}
