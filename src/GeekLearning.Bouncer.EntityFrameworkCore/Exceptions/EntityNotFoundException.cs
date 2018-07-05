namespace GeekLearning.Bouncer.EntityFrameworkCore.Exceptions
{
    using System;

    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(string entityName) : base($"Entity {entityName} not found.")
        {
        }
    }
}
