CREATE TABLE [Authorizations].[Role] (
    [Id]               UNIQUEIDENTIFIER CONSTRAINT [DF_Role_Id] DEFAULT (newid()) NOT NULL,
    [Name]             NVARCHAR (150)   NOT NULL,
    [IsDeletable]      BIT              CONSTRAINT [DF_Role_IsDeletable] DEFAULT ((1)) NOT NULL,
    [CreationDate]     DATETIME2 (7)    CONSTRAINT [DF_Role_CreationDate] DEFAULT (getutcdate()) NOT NULL,
    [CreationBy]       UNIQUEIDENTIFIER NOT NULL,
    [ModificationDate] DATETIME2 (7)    CONSTRAINT [DF_Role_ModificationDate] DEFAULT (getutcdate()) NOT NULL,
    [ModificationBy]   UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_Role] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UK_Role_Name] UNIQUE NONCLUSTERED ([Name] ASC)
);



