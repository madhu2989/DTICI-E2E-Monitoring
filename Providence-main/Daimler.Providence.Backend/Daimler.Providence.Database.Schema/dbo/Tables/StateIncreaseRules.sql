CREATE TABLE [dbo].[StateIncreaseRules] (
    [Id]                        INT            IDENTITY (1, 1) NOT NULL,
    [Name]                      NVARCHAR (500) NOT NULL,
	[Description]               NVARCHAR (MAX) NOT NULL,
	[EnvironmentSubscriptionId] NVARCHAR (500) NOT NULL,
    [CheckId]                   NVARCHAR (500) NOT NULL,
    [AlertName]                 NVARCHAR (500) NULL,
    [ComponentId]               NVARCHAR (500) NOT NULL,
    [TriggerTime]               INT			   NOT NULL,
    [IsActive]					BIT            NOT NULL,
    CONSTRAINT [PK_StateIncreaseRules] PRIMARY KEY CLUSTERED ([Id]),
	CONSTRAINT [FK_StateIncreaseRule_Environment] FOREIGN KEY ([EnvironmentSubscriptionId]) REFERENCES [dbo].[Environment] ([ElementId]) ON DELETE CASCADE,
);