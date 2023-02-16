CREATE TABLE [dbo].[Environment] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [Name]        NVARCHAR (250) NOT NULL,
    [Description] NVARCHAR (MAX) NULL,
    [ElementId]   NVARCHAR (500) NULL,
    [IsDemo]      BIT            CONSTRAINT [DF_Environment_IsDemo] DEFAULT ((0)) NULL,
	[CreateDate]  DATETIME       NOT NULL DEFAULT '2020-01-01',
    CONSTRAINT [PK_Environment] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [UC_Environment_ElementId] UNIQUE NONCLUSTERED ([ElementId] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [UC_Environment_Name] UNIQUE NONCLUSTERED ([Name] ASC) WITH (FILLFACTOR = 90)
);



