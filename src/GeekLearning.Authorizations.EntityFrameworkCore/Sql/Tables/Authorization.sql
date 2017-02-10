CREATE TABLE [Authorizations].[Authorization] (
    [Id]               UNIQUEIDENTIFIER CONSTRAINT [DF_Authorization_Id] DEFAULT (newid()) NOT NULL,
    [IsDeletable]      BIT              CONSTRAINT [DF_Authorization_IsDeletable] DEFAULT ((1)) NOT NULL,
    [RoleId]           UNIQUEIDENTIFIER NOT NULL,
    [ScopeId]          UNIQUEIDENTIFIER NOT NULL,
    [PrincipalId]      UNIQUEIDENTIFIER NOT NULL,
    [CreationDate]     DATETIME2 (7)    CONSTRAINT [DF_Authorization_CreationDate] DEFAULT (getutcdate()) NOT NULL,
    [CreationBy]       UNIQUEIDENTIFIER NOT NULL,
    [ModificationDate] DATETIME2 (7)    CONSTRAINT [DF_Authorization_ModificationDate] DEFAULT (getutcdate()) NOT NULL,
    [ModificationBy]   UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_Authorization] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Authorization_Principal] FOREIGN KEY ([PrincipalId]) REFERENCES [Authorizations].[Principal] ([Id]),
    CONSTRAINT [FK_Authorization_Role] FOREIGN KEY ([RoleId]) REFERENCES [Authorizations].[Role] ([Id]),
    CONSTRAINT [FK_Authorization_Scope] FOREIGN KEY ([ScopeId]) REFERENCES [Authorizations].[Scope] ([Id]),
    CONSTRAINT [UK_Authorization] UNIQUE NONCLUSTERED ([RoleId] ASC, [ScopeId] ASC, [PrincipalId] ASC)
);





