CREATE TABLE [Authorizations].[Right] (
    [Id]               UNIQUEIDENTIFIER CONSTRAINT [DF_Right_Id] DEFAULT (newid()) NOT NULL,
    [Name]             NVARCHAR (150)   NOT NULL,
    [IsDeletable]      BIT              CONSTRAINT [DF_Right_IsDeletable] DEFAULT ((1)) NOT NULL,
    [CreationDate]     DATETIME2 (7)    CONSTRAINT [DF_Right_CreationDate] DEFAULT (getutcdate()) NOT NULL,
    [CreationBy]       UNIQUEIDENTIFIER NOT NULL,
    [ModificationDate] DATETIME2 (7)    CONSTRAINT [DF_Right_ModificationDate] DEFAULT (getutcdate()) NOT NULL,
    [ModificationBy]   UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_Right] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UK_Right_Name] UNIQUE NONCLUSTERED ([Name] ASC)
);



