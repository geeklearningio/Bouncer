namespace GeekLearning.Authorizations.EntityFrameworkCore.Exceptions
{
    using System;

    public class ScopeLoopDetectedException : Exception
    {
        public ScopeLoopDetectedException(string scopeName) : base($"Scope {scopeName} is already part of the scope hierarchy. This indicates a loop in the scope model which is not permitted.")
        {
        }
    }
}
