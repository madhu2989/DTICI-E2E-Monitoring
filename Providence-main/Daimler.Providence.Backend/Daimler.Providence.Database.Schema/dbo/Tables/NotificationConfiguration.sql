CREATE TABLE [dbo].[NotificationConfiguration] (
    [Id]					INT           IDENTITY (1, 1) NOT NULL,
    [Environment]			INT           NOT NULL,
    [EmailAddresses]		VARCHAR (MAX) NULL,
    [IsActive]				BIT           NOT NULL,
	[NotificationInterval]  INT			  NOT NULL DEFAULT 0,
    CONSTRAINT [PK_NotificationConfiguration] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_NotificationConfiguration_Environment] FOREIGN KEY ([Environment]) REFERENCES [dbo].[Environment] ([Id]) ON DELETE CASCADE
);

