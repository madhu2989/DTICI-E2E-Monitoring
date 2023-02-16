CREATE TABLE [dbo].[Component] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [Name]          NVARCHAR (250) NULL,
    [Description]   NVARCHAR (MAX) NULL,
    [ElementId]     NVARCHAR (500) NULL,
    [EnvironmentId] INT            NULL,
    [ComponentType] NVARCHAR (250) NULL,
	[CreateDate]    DATETIME       NOT NULL DEFAULT '2020-01-01',
    CONSTRAINT [PK_Component] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [UC_Component_ElementId_Env] UNIQUE NONCLUSTERED ([ElementId] ASC, [EnvironmentId] ASC) WITH (FILLFACTOR = 90)
);

GO

CREATE NONCLUSTERED INDEX [nci_wi_Component_00C436ABD5B627BB32086EEA0047ED07] 
	ON [dbo].[Component] ([EnvironmentId], [ElementId]) 
	WITH (ONLINE = ON)

