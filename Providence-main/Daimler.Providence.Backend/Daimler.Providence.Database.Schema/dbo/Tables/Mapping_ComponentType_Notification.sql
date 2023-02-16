CREATE TABLE [dbo].[Mapping_ComponentType_Notification] (
    [ComponentTypeId] INT NOT NULL,
    [NotificationId]  INT NOT NULL,
    CONSTRAINT [PK_Mapping_ComponentType_Notification] PRIMARY KEY CLUSTERED ([ComponentTypeId] ASC, [NotificationId] ASC),
    CONSTRAINT [FK_Mapping_ComponentType_Notification_ComponentType] FOREIGN KEY ([ComponentTypeId]) REFERENCES [dbo].[ComponentType] ([Id]),
    CONSTRAINT [FK_Mapping_ComponentType_Notification_NotificationConfiguration] FOREIGN KEY ([NotificationId]) REFERENCES [dbo].[NotificationConfiguration] ([Id]) ON DELETE CASCADE
);

