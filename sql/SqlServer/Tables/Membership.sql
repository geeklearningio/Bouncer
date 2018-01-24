CREATE TABLE [Authorizations].[Membership] (
    [PrincipalId]      UNIQUEIDENTIFIER NOT NULL,
	[GroupId]          UNIQUEIDENTIFIER NOT NULL,
    [IsDeletable]      BIT              CONSTRAINT [DF_Membership_IsDeletable] DEFAULT ((1)) NOT NULL,
    [CreationDate]     DATETIME2 (7)    CONSTRAINT [DF_Membership_CreationDate] DEFAULT (getutcdate()) NOT NULL,
    [CreationBy]       UNIQUEIDENTIFIER NOT NULL,
    [ModificationDate] DATETIME2 (7)    CONSTRAINT [DF_Membership_ModificationDate] DEFAULT (getutcdate()) NOT NULL,
    [ModificationBy]   UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_Membership] PRIMARY KEY CLUSTERED ([PrincipalId] ASC, [GroupId] ASC),
    CONSTRAINT [FK_Membership_Principal] FOREIGN KEY ([PrincipalId]) REFERENCES [Authorizations].[Principal] ([Id]),
    CONSTRAINT [FK_Membership_Group] FOREIGN KEY ([GroupId]) REFERENCES [Authorizations].[Group] ([Id])
);