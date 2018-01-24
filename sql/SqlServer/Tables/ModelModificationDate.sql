CREATE TABLE [Authorizations].[ModelModificationDate] (
    [Id]     TINYINT       NOT NULL,
    [Rights] DATETIME2 (7) NOT NULL,
    [Roles]  DATETIME2 (7) NOT NULL,
    [Scopes] DATETIME2 (7) NOT NULL,
    CONSTRAINT [PK_ModelModificationDate] PRIMARY KEY CLUSTERED ([Id] ASC)
);

