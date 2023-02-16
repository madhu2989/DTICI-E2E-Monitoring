CREATE TABLE [dbo].[InternalJob] (
    [Id]                   INT            IDENTITY (1, 1) NOT NULL,
    [Type]                 INT            NOT NULL,
    [UserName]             VARCHAR (500)  NOT NULL,
    [EnvironmentId]        INT            NOT NULL,
    [State]                INT            NOT NULL,
    [StateInformation]     VARCHAR (MAX)  NOT NULL,
    [StartDate]            DATETIME       NULL,
    [EndDate]              DATETIME       NULL,
    [QueuedDate]           DATETIME       NULL,
    [FileName]             VARCHAR (MAX)  NULL,
    CONSTRAINT [PK_InternalJob] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_InternalJob_Environment] FOREIGN KEY ([EnvironmentId]) REFERENCES [dbo].[Environment] ([Id]) ON DELETE CASCADE,
);