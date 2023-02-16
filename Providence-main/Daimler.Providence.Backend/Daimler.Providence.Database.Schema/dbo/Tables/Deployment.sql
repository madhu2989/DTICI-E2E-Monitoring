CREATE TABLE [dbo].[Deployment] (
    [Id]                            INT IDENTITY (1, 1) NOT NULL,
    [EnvironmentId]                 INT            NOT NULL,
    [ElementIds]                    NVARCHAR (MAX) NULL,
    [Description]                   NVARCHAR (MAX) NULL,
    [ShortDescription]              NVARCHAR (50)  NULL,
    [CloseReason]                   NVARCHAR (MAX) NULL,
    [StartDate]                     DATETIME       NOT NULL,
    [EndDate]                       DATETIME       NULL,
    [ParentId]                      INT            NULL,
    [RepeatInformation]             NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_Deployment] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_EnvironmentId] FOREIGN KEY ([EnvironmentId]) REFERENCES [dbo].[Environment] ([Id]) ON DELETE CASCADE,
);
