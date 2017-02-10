CREATE TABLE [Authorizations].[Scope] (
    [Id]               UNIQUEIDENTIFIER CONSTRAINT [DF_Scope_Id] DEFAULT (newid()) NOT NULL,
    [Name]             NVARCHAR (150)   NOT NULL,
    [Description]      NVARCHAR (300)   NOT NULL,
    [IsDeletable]      BIT              CONSTRAINT [DF_Scope_IsDeletable] DEFAULT ((1)) NOT NULL,
    [CreationDate]     DATETIME2 (7)    CONSTRAINT [DF_Scope_CreationDate] DEFAULT (getutcdate()) NOT NULL,
    [CreationBy]       UNIQUEIDENTIFIER NOT NULL,
    [ModificationDate] DATETIME2 (7)    CONSTRAINT [DF_Scope_ModificationDate] DEFAULT (getutcdate()) NOT NULL,
    [ModificationBy]   UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_Scope] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UK_Scope_Name] UNIQUE NONCLUSTERED ([Name] ASC)
);



