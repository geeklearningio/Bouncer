namespace GeekLearning.Authorizations.EntityFrameworkCore.Exceptions
{
    using System;

    public class RootScopeNotFoundException : Exception
    {
        public RootScopeNotFoundException() : base($"No root scope has been found in the model. This indicates a bad scope model configuration")
        {
        }
    }
}
