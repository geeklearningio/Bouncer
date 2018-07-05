namespace Bouncer.EntityFrameworkCore.Data
{
    using Microsoft.EntityFrameworkCore.ChangeTracking;

    public class AuthorizationTrackedEntity<TEntity> where TEntity : class
    {
        public AuthorizationTrackedEntity()
        {
        }

        public AuthorizationTrackedEntity(TEntity entity, EntityEntry<TEntity> entry = null)
        {
            this.Entity = entity;
            this.Entry = entry;
        }

        public TEntity Entity { get; set; }

        public EntityEntry<TEntity> Entry { get; set; }

        public bool HasEntry => this.Entity != null;
    }
}
