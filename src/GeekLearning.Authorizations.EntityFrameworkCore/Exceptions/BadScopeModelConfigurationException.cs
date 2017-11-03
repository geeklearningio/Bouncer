namespace GeekLearning.Authorizations.EntityFrameworkCore.Exceptions
{
    using System;

    public class BadScopeModelConfigurationException : Exception
    {
        public BadScopeModelConfigurationException(string scopeParentName, string scopeChildName, int scopeParentlevel, int scopeChildLevel) : base($"Scope {scopeChildName} is a child of scope {scopeParentName} but its hierarchy level is higher or equal ({scopeChildName} vs {scopeParentName}) which indicate a bad scope model configuration")
        {
        }

        public BadScopeModelConfigurationException(string scopeName) : base($"Scope {scopeName} has invalid links which indicate a bad scope model configuration")
        {
        }
    }
}
