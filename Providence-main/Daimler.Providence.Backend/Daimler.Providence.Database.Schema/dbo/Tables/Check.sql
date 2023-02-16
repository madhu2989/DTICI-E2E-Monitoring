CREATE TABLE [dbo].[Check] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [Description]   NVARCHAR (MAX) NULL,
    [Name]          NVARCHAR (250) NULL,
    [VstsStory]     NVARCHAR (MAX) NULL,
    [Frequency]     INT            NULL,
    [ElementId]     NVARCHAR (500) NULL,
    [EnvironmentId] INT            NULL,
    CONSTRAINT [PK_Check] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Check_Environment] FOREIGN KEY ([EnvironmentId]) REFERENCES [dbo].[Environment] ([Id]) ON DELETE CASCADE,
	CONSTRAINT [CHK_Constraint_Frequency] CHECK ([frequency]>=(-1) AND [frequency]<=(2147483647)),
    CONSTRAINT [UC_Check_ElementId_Env] UNIQUE NONCLUSTERED ([ElementId] ASC, [EnvironmentId] ASC)
);



