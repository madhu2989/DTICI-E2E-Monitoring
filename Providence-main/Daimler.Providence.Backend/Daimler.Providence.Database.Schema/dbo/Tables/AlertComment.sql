CREATE TABLE [dbo].[AlertComment] (
    [Id]					INT           IDENTITY (1, 1) NOT NULL,
    [User]					VARCHAR (MAX) NOT NULL,
    [Comment]				VARCHAR (MAX) NOT NULL,
	[State]					INT			  NOT NULL,
    [Timestamp]             DATETIME      NOT NULL,
	[StateTransitionId]		INT			  NOT NULL,
    CONSTRAINT [PK_AlertComment] PRIMARY KEY CLUSTERED ([Id] ASC),
	CONSTRAINT [FK_StateTransitionId] FOREIGN KEY ([StateTransitionId]) REFERENCES [dbo].[StateTransition] ([Id]) ON DELETE CASCADE
);