CREATE TABLE [Authorizations].[Group] (
    [Id]               UNIQUEIDENTIFIER CONSTRAINT [DF_Group_Id] DEFAULT (newid()) NOT NULL,
    [Name]             NVARCHAR (150)   NOT NULL,
    [IsDeletable]      BIT              CONSTRAINT [DF_Group_IsDeletable] DEFAULT ((1)) NOT NULL,
    [CreationDate]     DATETIME2 (7)    CONSTRAINT [DF_Group_CreationDate] DEFAULT (getutcdate()) NOT NULL,
    [CreationBy]       UNIQUEIDENTIFIER NOT NULL,
    [ModificationDate] DATETIME2 (7)    CONSTRAINT [DF_Group_ModificationDate] DEFAULT (getutcdate()) NOT NULL,
    [ModificationBy]   UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_Group] PRIMARY KEY CLUSTERED ([Id] ASC),
	CONSTRAINT [FK_Group_Principal] FOREIGN KEY ([Id]) REFERENCES [Authorizations].[Principal] ([Id]),
    CONSTRAINT [UK_Group_Name] UNIQUE NONCLUSTERED ([Name] ASC)
);