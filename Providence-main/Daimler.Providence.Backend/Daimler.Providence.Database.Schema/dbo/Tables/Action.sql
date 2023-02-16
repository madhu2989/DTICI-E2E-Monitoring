CREATE TABLE [dbo].[Action] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [ServiceId]     INT            NULL,
    [Name]          NVARCHAR (250) NULL,
    [Description]   NVARCHAR (MAX) NULL,
    [ElementId]     NVARCHAR (500) NULL,
    [EnvironmentId] INT            NULL,
	[CreateDate]    DATETIME       NOT NULL DEFAULT '2020-01-01',
    CONSTRAINT [PK_Action] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Action_Service] FOREIGN KEY ([ServiceId]) REFERENCES [dbo].[Service] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [UC_Action_ElementId_Environment] UNIQUE NONCLUSTERED ([ElementId] ASC, [EnvironmentId] ASC)
);

GO

CREATE NONCLUSTERED INDEX [nci_wi_Action_C06ABD7A01FB77E8E6AC5F034B35BEE3] 
	ON [dbo].[Action] ([ServiceId]) 
	WITH (ONLINE = ON)
