namespace GeekLearning.Bouncer.EntityFrameworkCore.Configuration
{
    public class BouncerContextOptions
    {
        public string SchemaName { get; set; }

        public string ConnectionString { get; set; }

        public BouncerSupportedProviders Provider { get; set; }
    }
}
