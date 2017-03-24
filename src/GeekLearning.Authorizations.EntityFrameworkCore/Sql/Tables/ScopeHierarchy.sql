CREATE TABLE [Authorizations].[ScopeHierarchy] (
    [ParentId] UNIQUEIDENTIFIER NOT NULL,
    [ChildId]  UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_ScopeHierarchy] PRIMARY KEY CLUSTERED ([ParentId] ASC, [ChildId] ASC),
    CONSTRAINT [FK_ScopeHierarchy_ChildScope] FOREIGN KEY ([ChildId]) REFERENCES [Authorizations].[Scope] ([Id]),
    CONSTRAINT [FK_ScopeHierarchy_ParentScope] FOREIGN KEY ([ParentId]) REFERENCES [Authorizations].[Scope] ([Id])
);






GO
CREATE NONCLUSTERED INDEX [IX_ScopeHierarchy_ChildId]
    ON [Authorizations].[ScopeHierarchy]([ChildId] ASC)
    INCLUDE([ParentId]);



