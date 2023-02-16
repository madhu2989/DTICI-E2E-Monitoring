CREATE TABLE [dbo].[StateTransition] (
    [Id]                   INT            IDENTITY (1, 1) NOT NULL,
    [Environment]          INT            NULL,
    [SourceTimestamp]      DATETIME       NULL,
    [State]                INT            NULL,
    [ComponentType]        INT            NULL,
    [GeneratedTimestamp]   DATETIME       NULL,
    [Description]          NVARCHAR (MAX) NULL,
    [Customfield1]         NVARCHAR (MAX) NULL,
    [Customfield2]         NVARCHAR (MAX) NULL,
    [Customfield3]         NVARCHAR (MAX) NULL,
    [Customfield4]         NVARCHAR (MAX) NULL,
    [Customfield5]         NVARCHAR (MAX) NULL,
    [CheckId]              VARCHAR (500)  NULL,
    [Guid]                 NVARCHAR (500) NULL,
    [ElementId]            VARCHAR (500)  NULL,
    [AlertName]            VARCHAR (500)  NULL,
    [TriggeredByCheckId]   NVARCHAR (500) NULL,
    [TriggeredByElementId] NVARCHAR (500) NULL,
    [TriggeredByAlertName] NVARCHAR (500) NULL,
	[ProgressState]		   int NULL DEFAULT 0,
    CONSTRAINT [PK_StateTransition] PRIMARY KEY CLUSTERED ([Id] ASC),
    FOREIGN KEY ([ComponentType]) REFERENCES [dbo].[ComponentType] ([Id]) ON DELETE CASCADE,
    FOREIGN KEY ([Environment]) REFERENCES [dbo].[Environment] ([Id]) ON DELETE CASCADE,
    FOREIGN KEY ([State]) REFERENCES [dbo].[State] ([Id]) ON DELETE CASCADE
);

GO
CREATE NONCLUSTERED INDEX [IDX_SourceTimestamp]
    ON [dbo].[StateTransition]([SourceTimestamp] ASC);

GO
CREATE NONCLUSTERED INDEX [IDX_StateTransition_042F846098DC4328AD8C334A2BB3E6431]
    ON [dbo].[StateTransition]([Environment] ASC)
    INCLUDE([SourceTimestamp], [CheckId], [ElementId], [AlertName]);

GO
CREATE NONCLUSTERED INDEX [IDX_ElementId_CheckId_Alertname]
    ON [dbo].[StateTransition]([ElementId] ASC, [CheckId] ASC, [AlertName] ASC);

GO 
CREATE NONCLUSTERED INDEX [nci_wi_StateTransition_85BA8B1A62E2B22C0F5E162114C6DE83] 
	ON [dbo].[StateTransition] ([Guid]) 
	INCLUDE ([AlertName], [CheckId], [ComponentType], [Customfield1], [Customfield2], [Customfield3], [Customfield4], [Customfield5], [Description], [ElementId], [Environment], [GeneratedTimestamp], [ProgressState], [SourceTimestamp], [State], [TriggeredByAlertName], [TriggeredByCheckId], [TriggeredByElementId]) 
	WITH (ONLINE = ON)
