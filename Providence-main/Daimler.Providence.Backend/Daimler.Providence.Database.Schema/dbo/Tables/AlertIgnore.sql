CREATE TABLE [dbo].[AlertIgnore] (
    [Id]                        INT            IDENTITY (1, 1) NOT NULL,
    [Name]                      NVARCHAR (500) NOT NULL,
    [CreationDate]              DATETIME       NOT NULL,
    [ExpirationDate]            DATETIME       NOT NULL,
    [IgnoreCondition]           NVARCHAR (MAX) NOT NULL,
    [EnvironmentSubscriptionId] NVARCHAR (500) NOT NULL,
    CONSTRAINT [PK_AlertIgnore] PRIMARY KEY CLUSTERED ([Id] ASC),
	CONSTRAINT [FK_AlertIgnore_Environment] FOREIGN KEY ([EnvironmentSubscriptionId]) REFERENCES [dbo].[Environment] ([ElementId]) ON DELETE CASCADE,
);







