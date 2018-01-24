CREATE TABLE [Authorizations].[RoleRight] (
    [RoleId]  UNIQUEIDENTIFIER NOT NULL,
    [RightId] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_RoleRight] PRIMARY KEY CLUSTERED ([RoleId] ASC, [RightId] ASC),
    CONSTRAINT [FK_RoleRight_Right] FOREIGN KEY ([RightId]) REFERENCES [Authorizations].[Right] ([Id]),
    CONSTRAINT [FK_RoleRight_Role] FOREIGN KEY ([RoleId]) REFERENCES [Authorizations].[Role] ([Id])
);

