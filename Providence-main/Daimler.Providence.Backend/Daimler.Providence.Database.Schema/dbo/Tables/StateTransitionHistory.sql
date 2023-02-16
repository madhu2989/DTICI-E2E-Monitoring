CREATE TABLE [dbo].[StateTransitionHistory] (
	[Id]              INT IDENTITY(1,1) NOT NULL,
	[EnvironmentId]   INT            NOT NULL,
	[ComponentType]   INT            NOT NULL,
	[ElementId]       NVARCHAR(500)  NOT NULL,
	[State]           INT            NOT NULL,
	[StartDate]       DATETIME       NULL,
	[EndDate]         DATETIME       NULL,	
	CONSTRAINT [PK_StateTransitionHistory_Id]            PRIMARY KEY ([Id]),
	CONSTRAINT [FK_StateTransitionHistory_EnvironmentId] FOREIGN KEY ([EnvironmentId]) REFERENCES [dbo].[Environment]([Id])   ON DELETE CASCADE,
	CONSTRAINT [FK_StateTransitionHistory_ComponentType] FOREIGN KEY ([ComponentType]) REFERENCES [dbo].[ComponentType]([Id]) ON DELETE CASCADE,
	CONSTRAINT [FK_StateTransitionHistory_State]         FOREIGN KEY ([State])         REFERENCES [dbo].[State]([Id])         ON DELETE CASCADE
);

GO
CREATE NONCLUSTERED INDEX [nci_wi_StateTransitionHistory_6EB18D29F379FB68637F9FA78B4071E1] 
	ON [dbo].[StateTransitionHistory] ([EnvironmentId], [ComponentType], [EndDate], [StartDate], [State]) 
	INCLUDE ([ElementId]) 
	WITH (ONLINE = ON)

GO
CREATE NONCLUSTERED INDEX [nci_wi_StateTransitionHistory_28059F6C2029E9D031919FBE15775ED4] 
	ON [dbo].[StateTransitionHistory] ([ElementId], [EnvironmentId]) 
	INCLUDE ([ComponentType], [EndDate], [StartDate], [State]) 
	WITH (ONLINE = ON)