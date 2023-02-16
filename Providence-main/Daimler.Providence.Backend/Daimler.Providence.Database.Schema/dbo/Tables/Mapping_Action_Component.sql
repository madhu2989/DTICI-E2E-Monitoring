CREATE TABLE [dbo].[Mapping_Action_Component] (
    [ActionId]    INT NOT NULL,
    [ComponentId] INT NOT NULL,
    CONSTRAINT [PK_Mapping_Action_Resource] PRIMARY KEY CLUSTERED ([ActionId] ASC, [ComponentId] ASC),
    CONSTRAINT [FK_Mapping_Action_Component] FOREIGN KEY ([ComponentId]) REFERENCES [dbo].[Component] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Mapping_Action_Resource_Action] FOREIGN KEY ([ActionId]) REFERENCES [dbo].[Action] ([Id]) ON DELETE CASCADE
);

GO

CREATE NONCLUSTERED INDEX [nci_wi_Mapping_Action_Component_F1E4C30F90033A9CD7C110A2F0F3EE9A] 
	ON [dbo].[Mapping_Action_Component] ([ComponentId]) 
	WITH (ONLINE = ON)
