namespace Bouncer.EntityFrameworkCore.Caching
{
    using System;
    using System.Collections.Generic;

    public class Role
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public IEnumerable<string> Rights { get; set; }
    }
}
