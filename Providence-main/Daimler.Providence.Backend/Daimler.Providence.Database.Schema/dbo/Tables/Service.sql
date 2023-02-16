CREATE TABLE [dbo].[Service] (
    [Id]             INT            IDENTITY (1, 1) NOT NULL,
    [Name]           NVARCHAR (250) NOT NULL,
    [Description]    NVARCHAR (MAX) NULL,
	[EnvironmentRef] INT            NULL,
    [ElementId]      NVARCHAR (500) NULL,
    [EnvironmentId]  INT            NULL,
	[CreateDate]     DATETIME       NOT NULL DEFAULT '2020-01-01',
    CONSTRAINT [PK_Service] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Service_Environment] FOREIGN KEY ([EnvironmentRef]) REFERENCES [dbo].[Environment] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [UC_Service_ElementId_Environment] UNIQUE NONCLUSTERED ([ElementId] ASC, [EnvironmentRef] ASC)
);







