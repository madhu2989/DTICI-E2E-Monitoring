CREATE TABLE [dbo].[Changelog] (
    [Id]                 INT            NOT NULL IDENTITY (1, 1),
    [EnvironmentId]      INT            NOT NULL,
    [ElementId]          INT            NOT NULL,
    [ElementType]        INT            NOT NULL,
    [ChangeDate]         DATETIME       NOT NULL,
    [Username]           NVARCHAR (100) NOT NULL,
    [Operation]          INT            NULL,
    [ValueOld]           NVARCHAR (MAX) NULL,
    [ValueNew]           NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_Changelog] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Changelog_EnvironmentId] FOREIGN KEY (EnvironmentId) REFERENCES dbo.Environment(Id) ON UPDATE CASCADE ON DELETE CASCADE
);