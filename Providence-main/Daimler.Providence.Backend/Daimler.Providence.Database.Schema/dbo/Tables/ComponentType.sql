CREATE TABLE [dbo].[ComponentType] (
    [Id]   INT            IDENTITY (1, 1) NOT NULL,
    [Name] NVARCHAR (500) NULL,
    CONSTRAINT [PK_ResourceType] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UC_Name] UNIQUE NONCLUSTERED ([Name] ASC)
);



