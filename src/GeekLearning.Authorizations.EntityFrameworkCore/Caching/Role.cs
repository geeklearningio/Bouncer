namespace GeekLearning.Authorizations.EntityFrameworkCore.Caching
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;

    public class Role
    {
        [JsonProperty("i")]
        public Guid Id { get; set; }

        [JsonProperty("n")]
        public string Name { get; set; }

        [JsonProperty("r")]
        public IEnumerable<string> Rights { get; set; }
    }
}
