namespace GeekLearning.Authorizations.EntityFrameworkCore.Caching
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;

    public class Scope
    {
        [JsonProperty("i")]
        public Guid Id { get; set; }

        [JsonProperty("n")]
        public string Name { get; set; }

        [JsonProperty("p")]
        public IEnumerable<Guid> ParentIds { get; set; }

        [JsonProperty("c")]
        public IEnumerable<Guid> ChildIds { get; set; }
    }
}
