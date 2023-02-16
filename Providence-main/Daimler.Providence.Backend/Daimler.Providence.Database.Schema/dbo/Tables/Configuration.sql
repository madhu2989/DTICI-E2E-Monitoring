CREATE TABLE [dbo].[Configuration] (
    [Id]               INT IDENTITY (1, 1) NOT NULL,
    [EnvironmentId]    INT            NOT NULL,
    [Key]              NVARCHAR (100) NOT NULL,
    [Value]            NVARCHAR (MAX) NULL,
    [Description]      NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_Configuration_Id] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Configuration_EnvironmentId] FOREIGN KEY ([EnvironmentId]) REFERENCES [dbo].[Environment] ([Id]) ON DELETE CASCADE,
);
