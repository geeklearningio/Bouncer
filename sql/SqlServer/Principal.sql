CREATE TABLE [Authorizations].[Principal] (
    [Id]               UNIQUEIDENTIFIER CONSTRAINT [DF_Principal_Id] DEFAULT (newid()) NOT NULL,
    [IsDeletable]      BIT              CONSTRAINT [DF_Principal_IsDeletable] DEFAULT ((1)) NOT NULL,
    [CreationDate]     DATETIME2 (7)    CONSTRAINT [DF_Principal_CreationDate] DEFAULT (getutcdate()) NOT NULL,
    [CreationBy]       UNIQUEIDENTIFIER NOT NULL,
    [ModificationDate] DATETIME2 (7)    CONSTRAINT [DF_Principal_ModificationDate] DEFAULT (getutcdate()) NOT NULL,
    [ModificationBy]   UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_Principal] PRIMARY KEY CLUSTERED ([Id] ASC)
);

